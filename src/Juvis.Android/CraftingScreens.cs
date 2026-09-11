using Android.Widget;
using Juvis.Core;
namespace Juvis.AndroidApp;

public partial class MainActivity
{
    void BlueprintsScreen()
    {
        Title("Blueprints", "Recipes, resources and mission unlocks.");
        var results = new LinearLayout(this) { Orientation = Orientation.Vertical };
        void Render() => Pager(results, armory.Catalog.Blueprints.Where(b => Matches(query, b.Name, string.Join(" ", b.Ingredients.Select(i => i.Name)))).OrderBy(b => b.Name).ToList(), b => {
            var card = Card(); card.AddView(Label(b.Name, 19));
            card.AddView(Label($"{b.Ingredients.Count} resources · {b.Seconds / 60:0.#} min · " + (armory.State.Blueprints.Contains(b.Id) ? "Owned" : b.DefaultUnlocked ? "Default unlock" : "Unlock required"), 12, muted));
            card.AddView(Button("Recipe & unlocks  ›", () => { BlueprintScreen(b, Draw); return Task.CompletedTask; })); results.AddView(card);
        }, Render);
        Search("Search blueprints or ingredients", _ => Render()); body.AddView(results); Render();
    }
    void BlueprintScreen(Blueprint b, Action returnTo)
    {
        Detail(b.Name, $"Blueprint · {b.Version}\nCraft time: {b.Seconds / 60:0.#} minutes", returnTo);
        var generation = screenGeneration;
        var card = Card(); card.AddView(Label("Resources per craft", 19));
        foreach (var i in b.Ingredients) card.AddView(Label($"{i.Name}   {i.Quantity:0.###} {i.Unit}", 15));
        if (b.Ingredients.Count == 0) card.AddView(Label("No recipe quantities available. Refresh before planning."));
        body.AddView(card);
        body.AddView(Button(armory.State.Blueprints.Contains(b.Id) ? "✓ Owned blueprint · tap to unmark" : "Mark blueprint owned", async () => {
            await armory.Change(s => { if (!s.Blueprints.Add(b.Id)) s.Blueprints.Remove(b.Id); }); BlueprintScreen(b, returnTo);
        }));
        body.AddView(Label("Mission unlocks", 19));
        body.AddView(Label(b.DefaultUnlocked ? "Available by default" : b.Missions.Count > 0 ? string.Join("\n", b.Missions) : "Mission details not cached. Refresh the recipe to load unlocks.", 14, muted));
        body.AddView(Button("Refresh recipe & mission unlocks", async () => { var next = await armory.Api.LoadBlueprint(b, lifetime.Token); await armory.RememberBlueprint(next); if (generation == screenGeneration) BlueprintScreen(next, returnTo); }));
        body.AddView(Button("+ Add one to craft plan", async () => {
            if (b.Ingredients.Count == 0) throw new InvalidOperationException("Recipe quantities are unavailable.");
            await armory.Change(s => s.CraftPlan[b.Id] = Math.Min(999, s.CraftPlan.GetValueOrDefault(b.Id) + 1));
            Toast.MakeText(this, "Added to craft plan", ToastLength.Short)!.Show();
        }, true));
        body.AddView(Button("✦ Ask Gemini about crafting", () => AskGemini($"Research crafting {b.Name} in Star Citizen. Cached recipe patch: {b.Version}. Verify live availability, mission unlocks, crafting station requirements and quality/tier effects. Cite sources. Ingredients: " + string.Join(", ", b.Ingredients.Select(i => $"{i.Name} {i.Quantity} {i.Unit}")))));
        if (b.WebUrl.Length > 0) body.AddView(Button("Open full recipe / tiers", () => { OpenUrl(b.WebUrl); return Task.CompletedTask; }));
    }
    void CraftScreen()
    {
        Title("Crafting hub", "A resource plan you can carry into the verse.");
        body.AddView(Button("Browse all blueprints  ›", () => { Navigate("Blueprints"); return Task.CompletedTask; }, true));
        body.AddView(Button("Commodities & prices  ›", () => { Navigate("Commodities"); return Task.CompletedTask; }));
        body.AddView(Label("Craft plan", 21));
        if (armory.State.CraftPlan.Count == 0) body.AddView(Label("Add a recipe from Blueprints to calculate your resource totals.", 14, muted));
        foreach (var (id, quantity) in armory.State.CraftPlan)
        {
            var b = armory.Catalog.Blueprints.FirstOrDefault(x => x.Id == id);
            var card = Card(); card.AddView(Label($"{b?.Name ?? id}  × {quantity}", 18));
            if (b == null) card.AddView(Label("Recipe not cached; sync Blueprints to include its resources.", 13, cyan));
            else if (!b.DefaultUnlocked && !armory.State.Blueprints.Contains(id)) card.AddView(Label("Blueprint unlock not marked owned", 12, cyan));
            card.AddView(Button("+ One more", async () => { await armory.Change(s => s.CraftPlan[id] = Math.Min(999, s.CraftPlan[id] + 1)); Draw(); }));
            card.AddView(Button("− One / remove", async () => { await armory.Change(s => { if (s.CraftPlan[id] > 1) s.CraftPlan[id]--; else s.CraftPlan.Remove(id); }); Draw(); })); body.AddView(card);
        }
        body.AddView(Label("Total resources", 21));
        foreach (var i in Crafting.Requirements(armory.Catalog.Blueprints, armory.State.CraftPlan))
        {
            var card = Card(); card.AddView(Label($"{i.Name}   {i.Quantity:0.###} {i.Unit}", 16));
            card.AddView(Button("Used in  ›", () => {
                Detail(i.Name, "Blueprints using this resource", Draw);
                foreach (var b in armory.Catalog.Blueprints.Where(b => b.Ingredients.Any(x => x.Id == i.Id)))
                    body.AddView(Button(b.Name, () => { BlueprintScreen(b, Draw); return Task.CompletedTask; }));
                return Task.CompletedTask;
            })); body.AddView(card);
        }
        body.AddView(Label("My blueprints", 21));
        var owned = armory.Catalog.Blueprints.Where(b => armory.State.Blueprints.Contains(b.Id)).ToList();
        if (owned.Count == 0) body.AddView(Label("Mark a blueprint owned from its recipe page.", 14, muted));
        foreach (var b in owned.Take(30)) body.AddView(Button(b.Name, () => { BlueprintScreen(b, Draw); return Task.CompletedTask; }));
        if (owned.Count > 30) body.AddView(Label($"Showing 30 of {owned.Count}. Search all in Blueprints."));
        body.AddView(Label("Totals use the cached base recipe. Review the source for station, quality and tier requirements.", 12, muted));
    }
}
