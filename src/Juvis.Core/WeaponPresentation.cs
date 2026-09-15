namespace Juvis.Core;

public sealed record WeaponAmmunitionView(
    string AmmoType, string Caliber, string MagazineType, string MagazineName, int? Capacity,
    IReadOnlyList<string> CompatibleMagazines, IReadOnlyList<string> CompatibleAmmunition,
    string EnergySource, int? EnergyCapacity, decimal? EnergyRegenerationPerSecond, bool NotApplicable)
{
    public bool HasData => new[] { AmmoType, Caliber, MagazineType, MagazineName, EnergySource }.Any(CatalogPresentation.HasName) ||
        Capacity > 0 || EnergyCapacity > 0 || EnergyRegenerationPerSecond > 0 || CompatibleMagazines.Count > 0 || CompatibleAmmunition.Count > 0;
    public string SearchText => string.Join(" ", new[] { AmmoType, Caliber, MagazineType, MagazineName, EnergySource }
        .Concat(CompatibleMagazines).Concat(CompatibleAmmunition).Where(CatalogPresentation.HasName));
}

public static class WeaponPresentation
{
    public static bool IsWeapon(Item item) => item.Type.Equals("WeaponPersonal", StringComparison.OrdinalIgnoreCase) ||
        item.Type.Equals("WeaponGun", StringComparison.OrdinalIgnoreCase) ||
        item.Category.Contains("weapon", StringComparison.OrdinalIgnoreCase) && !item.SubType.Equals("Magazine", StringComparison.OrdinalIgnoreCase);

    public static WeaponAmmunitionView Ammunition(Item weapon, IEnumerable<Item> catalog)
    {
        var source = weapon.Ammunition ?? new();
        var candidates = catalog.Where(item => IsCompatibleMagazine(source, item)).ToList();
        var names = source.CompatibleMagazineNames.Concat(candidates.Select(item => CatalogPresentation.ItemName(item)))
            .Append(source.MagazineName).Where(CatalogPresentation.HasName).Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();
        var calibers = candidates.Select(i => i.Ammunition?.Caliber).Where(CatalogPresentation.HasName).Cast<string>().Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var types = candidates.Select(i => i.Ammunition?.AmmoType).Where(CatalogPresentation.HasName).Cast<string>().Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        return new(
            CatalogPresentation.Name(source.AmmoType, types.Count == 1 ? types[0] : null) ?? "",
            CatalogPresentation.Name(source.Caliber, calibers.Count == 1 ? calibers[0] : null) ?? "",
            CatalogPresentation.Name(source.MagazineType, names.Count > 0 ? "Magazine" : null) ?? "",
            CatalogPresentation.Name(source.MagazineName) ?? "", source.Capacity,
            names, source.CompatibleAmmunitionNames.Where(CatalogPresentation.HasName).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList(),
            CatalogPresentation.Name(source.EnergySource) ?? "", source.EnergyCapacity, source.EnergyRegenerationPerSecond, source.NotApplicable);
    }

    public static bool MatchesFilter(Item item, IEnumerable<Item> catalog, string filter)
    {
        if (filter == "All ammunition") return true;
        if (!IsWeapon(item)) return false;
        var ammo = Ammunition(item, catalog);
        return filter switch
        {
            "Ballistic" => ammo.AmmoType.Contains("ballistic", StringComparison.OrdinalIgnoreCase),
            "Energy" => ammo.AmmoType.Contains("energy", StringComparison.OrdinalIgnoreCase),
            "Magazine-fed" => ammo.CompatibleMagazines.Count > 0 || CatalogPresentation.HasName(ammo.MagazineType),
            "Ammo data unavailable" => !ammo.HasData && !ammo.NotApplicable,
            _ => true
        };
    }

    static bool IsCompatibleMagazine(AmmunitionInfo source, Item candidate)
    {
        if (source.CompatibleMagazineIds.Contains(candidate.Id, StringComparer.OrdinalIgnoreCase)) return true;
        if (!candidate.Type.Equals("WeaponAttachment", StringComparison.OrdinalIgnoreCase) ||
            !candidate.SubType.Equals("Magazine", StringComparison.OrdinalIgnoreCase) || candidate.Size is null ||
            source.MagazineMinSize is null || source.MagazineMaxSize is null ||
            candidate.Size < source.MagazineMinSize || candidate.Size > source.MagazineMaxSize) return false;
        if (source.MagazinePortRequiredTags.Length == 0 && source.MagazinePortTags.Length == 0) return false;
        if (!source.MagazinePortRequiredTags.All(required => candidate.Tags.Any(tag => tag.Equals(required, StringComparison.OrdinalIgnoreCase)))) return false;
        if (!candidate.RequiredTags.All(required => source.MagazinePortTags.Any(tag => tag.Equals(required, StringComparison.OrdinalIgnoreCase)))) return false;
        return candidate.RestrictionsKnown;
    }
}
