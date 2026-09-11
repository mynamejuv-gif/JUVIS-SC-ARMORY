using Android.App;
using Android.Content;
using Android.Widget;
using Juvis.Core;

namespace Juvis.AndroidApp;

public partial class MainActivity
{
    Task OpenStarterGuide(string name)
    {
        var clipboard = (ClipboardManager)GetSystemService(ClipboardService)!;
        clipboard.PrimaryClip = ClipData.NewPlainText("JUVIS item name", name);
        new AlertDialog.Builder(this).SetTitle("Item name copied")!
            .SetMessage($"{name}\n\nOpen Citizen Starter Guide and paste this exact name into its Blueprint Finder. Check the variant and patch shown by that source.")!
            .SetPositiveButton("Open guide", (_, _) => { try { OpenUrl(StarterGuide.Finder); } catch (Exception ex) { Error(ex); } })!
            .SetNegativeButton("Stay here", (_, _) => { })!.Show();
        return Task.CompletedTask;
    }

    void AddCommunitySource(string name, string key, Action redraw)
    {
        var generation = screenGeneration;
        var snapshot = armory.Catalog.Guide;
        var match = StarterGuide.Match(snapshot, name, key);
        var card = Card(); card.AddView(Label(StarterGuide.Source, 20, cyan));
        card.AddView(Label("Community blueprint and mission reports · checked separately from Wiki", 12, muted));
        if (snapshot.Blueprints.Count == 0)
            card.AddView(Label("Sync this source to read its mission information offline.", 14));
        else
        {
            card.AddView(Label($"Source build {snapshot.Version} · " + (armory.Catalog.Synced.TryGetValue(StarterGuide.Source, out var date) ? "updated " + date.ToLocalTime().ToString("g") : "update date unavailable"), 12, muted));
            if (match == null) card.AddView(Label("No unambiguous match for this exact entry. Check the guide before using another variant's information.", 14));
            else
            {
                card.AddView(Label(match.Name, 16));
                card.AddView(Label(string.IsNullOrWhiteSpace(key) ? "Matched by exact name" : "Matched by blueprint identity", 12, muted));
                if (match.CraftSeconds is { } seconds) card.AddView(Label($"Reported craft time: {seconds / 60:0.#} minutes", 13));
                card.AddView(Label(match.Missions.Count == 0 ? "Source checked: no missions listed for this exact blueprint. Unlock availability remains unconfirmed." : $"{match.Missions.Count} mission entries reported", 14));
                foreach (var mission in match.Missions.Take(3)) card.AddView(Label(mission.Title + (mission.Faction.Length > 0 ? " · " + mission.Faction : ""), 13, muted));
                if (match.Missions.Count > 0) card.AddView(Button("View mission reports  ›", () => { CommunityMissions(match, snapshot.Version, redraw); return Task.CompletedTask; }));
            }
        }
        card.AddView(Button("Refresh Citizen Starter Guide", async () => { await RunSync(StarterGuide.Source); if (generation == screenGeneration) redraw(); }));
        card.AddView(Button("Open Citizen Starter Guide", () => OpenStarterGuide(name)));
        body.AddView(card);
    }

    void CommunityMissions(GuideBlueprint blueprint, string version, Action returnTo)
    {
        Detail(blueprint.Name, $"Citizen Starter Guide · source build {version}\nCommunity reports; check the current patch and exact variant before planning.", returnTo);
        page = 0;
        var results = new LinearLayout(this) { Orientation = Orientation.Vertical };
        void Render() => Pager(results, blueprint.Missions, m => {
            var card = Card(); card.AddView(Label(m.Title, 19));
            card.AddView(Label("Faction: " + ApiParser.First(m.Faction, "not listed"), 14));
            card.AddView(Label("System: " + ApiParser.First(m.System, "not listed"), 14));
            if (m.Standing.Length > 0) card.AddView(Label("Reputation standing: " + m.Standing, 14));
            if (m.MinReputation is { } rep) card.AddView(Label($"Reported minimum reputation: {rep:N0}", 14));
            card.AddView(Label(m.Lawful == true ? "Reported lawful contract" : m.Lawful == false ? "Reported unlawful contract" : "Legality not listed", 13, muted));
            if (m.Event.Length > 0) card.AddView(Label("Event: " + m.Event, 13));
            results.AddView(card);
        }, Render);
        body.AddView(results); Render();
        body.AddView(Label("A listed mission is not a guaranteed drop. Check reward pools, reputation gates and availability on the source website.", 13, muted));
        body.AddView(Button("Open Citizen Starter Guide", () => OpenStarterGuide(blueprint.Name)));
    }
}
