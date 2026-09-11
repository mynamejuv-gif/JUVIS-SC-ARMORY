using Android.Widget;
using Juvis.Core;
namespace Juvis.AndroidApp;

public partial class MainActivity
{
    string vehicleFilter = "All vehicles", goal = "Balanced";
    void VehiclesScreen()
    {
        Title("Ship & ground upgrades", "Choose a vehicle. Inspect its ports. Save a proposed build.");
        var results = new LinearLayout(this) { Orientation = Orientation.Vertical };
        void Render() => Pager(results, armory.Catalog.Vehicles.Where(v => Matches(query, v.Name, v.Manufacturer) && (vehicleFilter switch {
            "My vehicles" => armory.State.Vehicles.Contains(v.Id), "Ships" => !v.Ground, "Ground" => v.Ground, _ => true })).OrderBy(v => v.Name).ToList(), v => {
            var card = Card(); card.AddView(Label(v.Ground ? "GROUND VEHICLE" : "SHIP", 10, cyan)); card.AddView(Label(v.Name, 20));
            card.AddView(Label(v.Manufacturer + (armory.State.Vehicles.Contains(v.Id) ? " · ✓ Owned" : ""), 12, muted));
            card.AddView(Button("Loadout & upgrades  ›", () => { VehicleScreen(v); return Task.CompletedTask; })); results.AddView(card);
        }, Render);
        Search("Search ships and ground vehicles", _ => Render());
        Choice(body, ["All vehicles", "My vehicles", "Ships", "Ground"], vehicleFilter, c => { vehicleFilter = c; page = 0; Render(); });
        body.AddView(results); Render();
    }
    void VehicleScreen(Vehicle v, bool proposed = false)
    {
        Detail(v.Name, $"{(v.Ground ? "Ground vehicle" : "Ship")} / {v.Manufacturer}\nLoadout patch: {ApiParser.First(v.Ports.FirstOrDefault()?.Version ?? "", v.Version, "unknown")}", Draw);
        var generation = screenGeneration;
        if (armory.Catalog.Vehicles.Count(other => other.Id == v.Id) > 1)
        {
            body.AddView(Label("Source identity conflict: multiple vehicle variants share this ID. Ownership and build editing are paused for these variants to prevent changes applying to the wrong vehicle. Existing saves are retained in your backup.", 15, cyan));
            body.AddView(Button("Export existing saves", () => { ExportBackup(); return Task.CompletedTask; }));
            return;
        }
        body.AddView(Button(armory.State.Vehicles.Contains(v.Id) ? "✓ Owned vehicle · tap to unmark" : "Mark vehicle owned", async () => {
            await armory.Change(s => { if (!s.Vehicles.Add(v.Id)) s.Vehicles.Remove(v.Id); }); VehicleScreen(v, proposed);
        }));
        Choice(body, ["Balanced", "Combat", "Defense", "Fast Travel", "Stealth", "Industrial / Mining", "Budget"], goal, g => goal = g);
        body.AddView(Button("✦ Ask Gemini for upgrade suggestions", () => AskGemini(GeminiPrompt.Vehicle(v, armory.State.Builds.GetValueOrDefault(v.Id) ?? [], goal)), true));
        body.AddView(Button("Refresh stock loadout / retry", async () => {
            status.Text = "Loading ports and installed components… (20-second timeout)";
            var loaded = await armory.Api.LoadVehicle(v, lifetime.Token);
            await armory.RememberVehicle(loaded); if (generation == screenGeneration) { VehicleScreen(loaded, proposed); status.Text = $"{loaded.Ports.Count} ports cached"; }
        }));
        var tabs = Row();
        tabs.AddView(Button("Stock loadout", () => { VehicleScreen(v); return Task.CompletedTask; }, !proposed), new LinearLayout.LayoutParams(0, Dp(50), 1));
        tabs.AddView(Button("Proposed build", () => { VehicleScreen(v, true); return Task.CompletedTask; }, proposed), new LinearLayout.LayoutParams(0, Dp(50), 1));
        body.AddView(tabs);
        if (proposed)
        {
            var build = armory.State.Builds.GetValueOrDefault(v.Id) ?? [];
            if (build.Count == 0) body.AddView(Label("Open a stock port, choose a compatible candidate, then add it to your build.", 15, muted));
            foreach (var entry in build)
            {
                var port = v.Ports.FirstOrDefault(p => p.Id == entry.PortId);
                var item = armory.Catalog.Items.FirstOrDefault(i => i.Id == entry.ItemId);
                var fit = port != null && item != null ? Compatibility.Check(port, item) : new FitResult(Fit.CheckRequired, "Refresh vehicle and component data");
                var card = Card(); card.AddView(Label(entry.ItemName, 19)); card.AddView(Label(entry.PortId, 11, muted));
                card.AddView(Label($"Was: {port?.Installed?.Name ?? "Unknown"}\n{fit.Fit}: {fit.Reason}", 13, cyan));
                if (item != null) card.AddView(Button("Component details", () => { ItemScreen(item, () => VehicleScreen(v, true)); return Task.CompletedTask; }));
                card.AddView(Button("Remove from proposed build", async () => { await armory.Change(s => s.Builds[v.Id].RemoveAll(e => e.PortId == entry.PortId)); VehicleScreen(v, true); }));
                body.AddView(card);
            }
            body.AddView(Label("Planning only: this does not modify your in-game ship. Refresh data after a patch to recheck saved candidates.", 13, muted));
            return;
        }
        if (v.Ports.Count == 0) body.AddView(Label("No loadout cached yet. Tap Refresh stock loadout. If the request fails, cached data remains available and the same button retries.", 15, muted));
        var portSearch = new EditText(this) { Hint = "Filter ports: shield, cooler, weapon…", TextSize = 15 };
        portSearch.SetTextColor(ink); portSearch.SetHintTextColor(muted); portSearch.SetSingleLine(true); portSearch.SetMinHeight(Dp(52));
        body.AddView(portSearch);
        var portCards = new LinearLayout(this) { Orientation = Orientation.Vertical };
        body.AddView(portCards);
        void RenderPorts(string text)
        {
            portCards.RemoveAllViews();
            var visible = v.Ports.Where(p => (p.Installed != null || p.Editable == true) && Matches(text, p.Id, p.Installed?.Name ?? "", string.Join(" ", p.Types.Select(t => t.Type))))
                .OrderByDescending(p => p.Editable == true).ThenBy(p => p.Id).ToList();
            portCards.AddView(Label($"{visible.Count} ports · editable first", 12, muted));
            foreach (var port in visible.Take(80))
            {
                var card = Card(); card.AddView(Label(ApiParser.First(port.Installed?.Name ?? "", "Empty port"), 18));
                card.AddView(Label(port.Id, 11, muted)); card.AddView(Label($"Allowed S{port.MinSize?.ToString() ?? "?"}–S{port.MaxSize?.ToString() ?? "?"} · " + string.Join(", ", port.Types.Select(t => t.Type)), 12, cyan));
                if (port.Editable == true) card.AddView(Button("Show compatible upgrades  ›", () => { CompatibleScreen(v, port); return Task.CompletedTask; }));
                else card.AddView(Label(port.Editable == false ? "Bespoke / fixed" : "Editability unknown", 13, muted));
                portCards.AddView(card);
            }
            if (visible.Count > 80) portCards.AddView(Label("Showing 80 ports. Filter by name or type to see a specific component.", 13, muted));
        }
        portSearch.TextChanged += (_, _) => RenderPorts(portSearch.Text ?? "");
        RenderPorts("");
    }
    void CompatibleScreen(Vehicle v, Port port)
    {
        Detail("Compatible upgrades", $"{v.Name}\n{port.Id}\nCurrent: {port.Installed?.Name ?? "Empty"}", () => VehicleScreen(v));
        var generation = screenGeneration;
        body.AddView(Label("Candidates with incomplete data are shown for review; only confirmed matches can be added. Mount fit does not guarantee a performance improvement.", 13, muted));
        body.AddView(Button("Sync candidates for this port / retry", async () => {
            var types = string.Join(",", port.Types.Select(t => t.Type));
            if (types.Length == 0) throw new InvalidOperationException("Port type is unavailable; refresh the stock loadout first.");
            var route = "items?filter[type]=" + Uri.EscapeDataString(types);
            if (port.Version.Length > 0) route += "&version=" + Uri.EscapeDataString(port.Version);
            var items = await armory.Api.WikiPages(route, ApiParser.WikiItem, new Progress<string>(s => status.Text = s), lifetime.Token);
            await armory.Cache(c => { var map = c.Items.ToDictionary(i => i.Id); foreach (var i in items) map[i.Id] = i; c.Items = map.Values.ToList(); });
            if (generation == screenGeneration) CompatibleScreen(v, port);
        }));
        var candidates = armory.Catalog.Items.Select(i => (Item: i, Result: Compatibility.Check(port, i)))
            .Where(x => port.Types.Any(t => t.Type.Equals(x.Item.Type, StringComparison.OrdinalIgnoreCase)) && x.Result.Fit is Fit.Direct or Fit.CheckRequired)
            .OrderBy(x => x.Result.Fit).ThenBy(x => x.Item.Name).ToList();
        if (candidates.Count == 0) body.AddView(Label("No candidate matches in this cache. Sync candidates for this port.", 16));
        foreach (var (item, fit) in candidates.Take(150))
        {
            var card = Card(); card.AddView(Label(item.Name, 19)); card.AddView(Label($"S{item.Size} · {item.Grade} {item.Class}", 13, muted));
            card.AddView(Label("Reported buy: " + Price(item.BuyPrice), 12, muted));
            card.AddView(Label(fit.Fit == Fit.Direct ? "✓ Mount fit confirmed" : "△ Restriction check required", 13, cyan)); card.AddView(Label(fit.Reason, 12, muted));
            if (port.Installed != null)
                foreach (var stat in item.Stats) card.AddView(Label($"{stat.Key}: {port.Installed.Stats.GetValueOrDefault(stat.Key, "?")} → {stat.Value}", 12));
            card.AddView(Button("Details / refresh restrictions", () => { ItemScreen(item, () => CompatibleScreen(v, port)); return Task.CompletedTask; }));
            bool parentChanged = (armory.State.Builds.GetValueOrDefault(v.Id) ?? []).Any(e => port.Id.StartsWith(e.PortId + "/", StringComparison.Ordinal));
            if (parentChanged) card.AddView(Label("Parent mount has a proposed replacement. Remove that replacement before planning stock child ports.", 13, cyan));
            if (fit.Fit == Fit.Direct && !parentChanged) card.AddView(Button("+ Add to proposed build", async () => {
                if (Compatibility.Check(port, item).Fit != Fit.Direct) throw new InvalidOperationException("Compatibility needs rechecking.");
                await armory.Change(s => {
                    if (!s.Builds.TryGetValue(v.Id, out var build)) s.Builds[v.Id] = build = [];
                    // Changing a mount invalidates any saved children under that mount.
                    build.RemoveAll(e => e.PortId == port.Id || e.PortId.StartsWith(port.Id + "/", StringComparison.Ordinal));
                    build.Add(new(port.Id, item.Id, item.Name, item.Version));
                }); VehicleScreen(v, true);
            }, true));
            body.AddView(card);
        }
        if (candidates.Count > 150) body.AddView(Label("Showing the first 150 candidates. Narrow this port's type/size in the catalog to inspect more."));
    }
}
