param(
    [Parameter(Mandatory=$true)][string]$CatalogPath,
    [string]$ImageIndexPath,
    [string]$OutputDirectory = 'artifacts/category-audit'
)
$ErrorActionPreference = 'Stop'
$catalog = Get-Content -LiteralPath $CatalogPath -Raw | ConvertFrom-Json
$index = @{}
if ($ImageIndexPath) { $index = Get-Content -LiteralPath $ImageIndexPath -Raw | ConvertFrom-Json -AsHashtable }
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$rows = foreach ($group in ($catalog.Items | Group-Object Category | Sort-Object Name)) {
    [pscustomobject]@{
        Category = $group.Name
        Records = $group.Count
        MissingNames = @($group.Group | Where-Object { [string]::IsNullOrWhiteSpace($_.Name) }).Count
        MissingIds = @($group.Group | Where-Object { [string]::IsNullOrWhiteSpace($_.Id) }).Count
        WithoutBundledImageOrUrl = @($group.Group | Where-Object { !$index.ContainsKey($_.Id) -and !$_.ImageUrl }).Count
        DescriptionsNotCached = @($group.Group | Where-Object { !$_.Description }).Count
        ManufacturerNotCached = @($group.Group | Where-Object { !$_.Manufacturer }).Count
    }
}
$rows | Export-Csv -LiteralPath (Join-Path $OutputDirectory 'categories.csv') -NoTypeInformation
$summary = foreach ($module in @('Items','Blueprints','Commodities','Vehicles')) {
    $records = @($catalog.$module)
    [pscustomobject]@{
        Module = $module; Records = $records.Count
        MissingNames = @($records | Where-Object { [string]::IsNullOrWhiteSpace($_.Name) }).Count
        PlaceholderNames = @($records | Where-Object { $_.Name -match '^\s*[<=>\s]*PLACEHOLDER[<=>\s]*$' }).Count
        MissingIds = @($records | Where-Object { [string]::IsNullOrWhiteSpace($_.Id) }).Count
        DuplicateIdGroups = @($records | Group-Object Id | Where-Object Count -gt 1).Count
        DuplicateNameGroups = @($records | Group-Object Name | Where-Object Count -gt 1).Count
    }
}
$summary | Export-Csv -LiteralPath (Join-Path $OutputDirectory 'modules.csv') -NoTypeInformation
$catalog.Vehicles | Group-Object Id | Where-Object Count -gt 1 | ForEach-Object { $_.Group } |
    Select-Object Id,Name | Export-Csv -LiteralPath (Join-Path $OutputDirectory 'vehicle-conflicts.csv') -NoTypeInformation
$catalog.Blueprints | Where-Object { !$_.Name -or $_.Name -match 'PLACEHOLDER' } |
    Select-Object Id,Name,WebUrl | Export-Csv -LiteralPath (Join-Path $OutputDirectory 'incomplete-blueprints.csv') -NoTypeInformation
$badIngredients = @($catalog.Blueprints.Ingredients | Where-Object { !$_.Id -or !$_.Name -or $_.Quantity -le 0 }).Count
$emptyRecipes = @($catalog.Blueprints | Where-Object { $_.Ingredients.Count -eq 0 }).Count
$summary | Format-Table
Write-Output "Checked $($rows.Count) item categories; $badIngredients invalid ingredients; $emptyRecipes empty recipes."
Write-Output 'Image counts describe available references, not verified remote downloads. Duplicate names are retained because IDs can represent different variants.'
