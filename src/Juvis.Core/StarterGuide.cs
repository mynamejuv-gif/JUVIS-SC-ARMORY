using System.Text.Json;
using System.Text.RegularExpressions;

namespace Juvis.Core;

public record GuideMission(string Title, string Type, string Faction, string System,
    string Standing, decimal? MinReputation, bool? Lawful, string Event);
public record GuideBlueprint(string Key, string Name, decimal? CraftSeconds, List<GuideMission> Missions);
public record GuideSnapshot(string Version, List<GuideBlueprint> Blueprints);

public static class StarterGuide
{
    public const string Source = "Citizen Starter Guide";
    public const string Home = "https://citizen-starter-guide.com/";
    public const string Finder = "https://citizen-starter-guide.com/star-citizen-blueprint-finder/";
    public const string DataUrl = "https://quattrobaje3na-png.github.io/Star-Citizen-Blueprint-Finder-Mission-Contract-Rewards-Guide/blueprint_explorer_data.json";

    public static GuideSnapshot Parse(JsonElement root)
    {
        if (root.Get("items").ValueKind != JsonValueKind.Array || string.IsNullOrWhiteSpace(root.S("_build")))
            throw new InvalidDataException("Citizen Starter Guide changed its data format. Previous guide data retained.");
        var entries = new List<GuideBlueprint>();
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in root.Get("items").Array())
        {
            var key = item.S("blueprint");
            if (string.IsNullOrWhiteSpace(key) || !CatalogPresentation.HasName(item.S("name")) || !keys.Add(key) ||
                item.Get("missions").ValueKind != JsonValueKind.Array)
                throw new InvalidDataException("Citizen Starter Guide returned incomplete or conflicting records. Previous guide data retained.");
            var missions = new List<GuideMission>();
            foreach (var m in item.Get("missions").Array())
            {
                if (!CatalogPresentation.HasName(m.S("title"))) throw new InvalidDataException("Guide mission title is not displayable; previous guide data retained.");
                missions.Add(new(m.S("title"), m.S("type"), m.S("faction"),
                    ApiParser.First(string.Join(", ", m.Get("systems").Strings().Distinct()), m.S("system")),
                    m.S("repStanding"), m.Get("minRep").Number(), m.Get("lawful").Bool(), m.S("event")));
            }
            // Material quantities omit units in this source; keep Wiki recipes authoritative for craft totals.
            entries.Add(new(key, item.S("name"), item.Get("craftTime").Number(), missions));
        }
        if (entries.Count == 0) throw new InvalidDataException("Citizen Starter Guide returned no blueprints. Previous guide data retained.");
        return new(root.S("_build"), entries);
    }

    public static GuideBlueprint? Match(GuideSnapshot snapshot, string name, string key = "")
    {
        var matches = string.IsNullOrWhiteSpace(key)
            ? snapshot.Blueprints.Where(b => Normalize(b.Name) == Normalize(name)).ToList()
            : snapshot.Blueprints.Where(b => string.Equals(b.Key, key, StringComparison.OrdinalIgnoreCase)).ToList();
        return matches.Count == 1 ? matches[0] : null;
    }

    static string Normalize(string name) => Regex.Replace(name.Trim(), @"\s+", " ").ToUpperInvariant();
}
