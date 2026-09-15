using System.Text.RegularExpressions;

namespace Juvis.Core;

public static class CatalogPresentation
{
    static readonly Regex Marker = new(@"^\s*[<=>\[\](){}\s]*(?:PLACEHOLDER|UNINITIALIZED|UNDEFINED|NULL|N/?A|UNKNOWN|UNNAMED)[<=>\[\](){}\s]*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    static readonly Regex InternalPrefix = new(@"^(?:(?:hardpoint|loadout|debug|dummy|internal|test_data)[\s_:/.-]|default__|item[_:]|placeholder[-_:]\d)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    static readonly Regex MachineToken = new(@"^[a-z0-9]+(?:_[a-z0-9]+){2,}$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    static readonly Regex GuidValue = new(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static bool HasName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        var value = Regex.Replace(name.Trim(), @"\s+", " ");
        return value.Length is >= 2 and <= 160 && value.Any(char.IsLetterOrDigit) &&
            !Marker.IsMatch(value) && !InternalPrefix.IsMatch(value) && !MachineToken.IsMatch(value) &&
            !GuidValue.IsMatch(value) && !(value.StartsWith('<') && value.EndsWith('>'));
    }

    public static string? Name(params string?[] candidates) => candidates.FirstOrDefault(HasName)?.Trim();

    public static string? ItemName(Item item)
    {
        var direct = Name(new[] { item.Name }.Concat(item.AlternativeNames ?? []).ToArray());
        if (direct != null) return direct;
        var model = Name(item.Model);
        var maker = Name(item.Manufacturer);
        return model == null ? null : maker != null && !model.StartsWith(maker, StringComparison.OrdinalIgnoreCase) ? $"{maker} {model}" : model;
    }

    public static string? VehicleName(Vehicle vehicle)
    {
        var direct = Name(new[] { vehicle.Name }.Concat(vehicle.AlternativeNames ?? []).ToArray());
        if (direct != null) return direct;
        var model = Name(vehicle.Model);
        var maker = Name(vehicle.Manufacturer);
        return model == null ? null : maker != null && !model.StartsWith(maker, StringComparison.OrdinalIgnoreCase) ? $"{maker} {model}" : model;
    }

    public static string? CommodityName(Commodity commodity) => Name(new[] { commodity.Name }.Concat(commodity.AlternativeNames ?? []).ToArray());

    public static string? BlueprintName(Blueprint b) => Name(b.Name);

    public static string PortName(Port port)
    {
        var display = Name(port.DisplayName);
        if (display != null && !Regex.IsMatch(display, @"^(?:[A-Z]\s+){2,}[A-Z0-9]$")) return display;
        var subtype = port.Types.SelectMany(t => t.SubTypes).Select(value => Name(value)).FirstOrDefault(n => n != null);
        var type = port.Types.Select(t => Name(t.Type)).FirstOrDefault(n => n != null);
        return Humanize(subtype ?? type ?? "Component") + " port";
    }

    static string Humanize(string value) => Regex.Replace(Regex.Replace(value, "([a-z])([A-Z])", "$1 $2"), @"[_-]+", " ").Trim();

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
