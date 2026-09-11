using System.Text.Json;
using System.Text.RegularExpressions;

namespace Juvis.Core;

/// <summary>Maps game UUIDs to immutable APK assets, independent of remote image URLs.</summary>
public sealed class BundledImageIndex
{
    readonly Dictionary<string, string> files;
    public int Count => files.Count;
    public BundledImageIndex(Dictionary<string, string> entries)
    {
        files = new(StringComparer.OrdinalIgnoreCase);
        foreach (var (id, name) in entries)
        {
            var key = KeyForFile(name);
            if (key is null || !string.Equals(key, id, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Bundled image filename must match its item key.");
            files.Add(key, name);
        }
    }
    public string? AssetFor(string id) => files.TryGetValue(id, out var name)
        ? "bundled-images/" + name : null;
    public static string? KeyForFile(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Contains('\\')) return null;
        var commodity = Regex.Match(name, @"^commodities/commodity-([1-9][0-9]*)\.(jpg|jpeg|png|webp)$", RegexOptions.CultureInvariant);
        if (commodity.Success) return "uex:commodity:" + commodity.Groups[1].Value;
        if (name.Contains('/') || Path.GetExtension(name).ToLowerInvariant() is not (".jpg" or ".jpeg" or ".png" or ".webp")) return null;
        return Guid.TryParseExact(Path.GetFileNameWithoutExtension(name), "D", out var uuid) ? uuid.ToString("D") : null;
    }
    public static async Task<BundledImageIndex> Load(Stream stream) => new(await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream)
        ?? throw new InvalidDataException("Missing bundled image index."));
}
