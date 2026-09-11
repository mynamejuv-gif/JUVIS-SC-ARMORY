using System.Text.RegularExpressions;

namespace Juvis.Core;

public static class CatalogPresentation
{
    public static bool HasName(string? name) => !string.IsNullOrWhiteSpace(name) &&
        !Regex.IsMatch(name, @"^\s*[<=>\s]*PLACEHOLDER[<=>\s]*$", RegexOptions.IgnoreCase);

    public static string BlueprintName(Blueprint b) => HasName(b.Name) ? b.Name.Trim() :
        "Incomplete blueprint · " + b.Id[..Math.Min(8, b.Id.Length)];

    // Display aliases only: retain source categories and stable IDs in the cache and backups.
    public static string Category(string category) => category.Trim() switch
    {
        "Cooler" => "Coolers",
        "Power" => "Power Plants",
        "Quantum Drive" => "Quantum Drives",
        "Shield" => "Shield Generators",
        "Eyeware" => "Eyewear",
        "" => "Uncategorized",
        var other => other
    };
}
