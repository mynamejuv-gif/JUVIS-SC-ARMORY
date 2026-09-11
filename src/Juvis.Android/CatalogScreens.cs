using Android.Graphics;
using Android.Widget;
using Juvis.Core;

namespace Juvis.AndroidApp;

public partial class MainActivity
{
    void CatalogScreen(bool gear = false)
    {
        Title(gear ? "My Gear" : "Explore the armory", gear ? "Your owned, needed and favorite equipment." : "Find equipment. Plan your next loadout.");
        if (gear)
        {
            body.AddView(Button("Export backup", () => { ExportBackup(); return Task.CompletedTask; }));
            body.AddView(Button("Import backup", () => { ImportBackup(); return Task.CompletedTask; }));
        }
        var results = new LinearLayout(this) { Orientation = Orientation.Vertical };
        void Render()
        {
            var all = armory.Catalog.Items.AsEnumerable();
            if (gear)
            {
                // Keep imported states visible even when their catalog has not been synced yet.
                var known = all.Select(i => i.Id).ToHashSet();
                all = all.Concat(armory.State.Gear.Where(x => !known.Contains(x.Key)).Select(x => new Item { Id = x.Key, Name = x.Value.Name.Length > 0 ? x.Value.Name : x.Key, Category = "Awaiting catalog sync" }));
                all = all.Where(i => armory.State.Gear.TryGetValue(i.Id, out var s) && (gearFilter switch { "Owned" => s.Owned, "Need" => s.Need, "Favorites" => s.Favorite, _ => s.Owned || s.Need || s.Favorite }));
            }
            else if (category != "All categories") all = all.Where(i => CatalogPresentation.Category(i.Category) == category);
            var filtered = all.Where(i => Matches(query, i.Name, i.Category, CatalogPresentation.Category(i.Category), i.Manufacturer)).OrderBy(i => i.Name).ToList();
            Pager(results, filtered, i => {
                var card = Card();
                card.AddView(Label(CatalogPresentation.Category(i.Category).ToUpperInvariant(), 10, cyan));
                card.AddView(Label(i.Name, 19));
                card.AddView(Label(ApiParser.First(i.Manufacturer, "Manufacturer unavailable") + (i.Size != null ? $" · S{i.Size}" : ""), 12, muted));
                if (armory.State.Gear.TryGetValue(i.Id, out var s)) card.AddView(Label(string.Join("  ·  ", new[] { s.Owned ? "✓ Owned" : "", s.Need ? "+ Need" : "", s.Favorite ? "★ Favorite" : "" }.Where(x => x.Length > 0)), 12, cyan));
                card.AddView(Button("Details & gear states  ›", () => { ItemScreen(i, Draw); return Task.CompletedTask; }));
                results.AddView(card);
            }, Render);
        }
        Search("Search names, categories, manufacturers", _ => Render());
        if (gear) Choice(body, ["All saved", "Owned", "Need", "Favorites"], gearFilter, c => { gearFilter = c; page = 0; Render(); });
        else Choice(body, new[] { "All categories" }.Concat(armory.Catalog.Items.Select(i => CatalogPresentation.Category(i.Category)).Distinct().Order()).ToArray(), category, c => { category = c; page = 0; Render(); });
        body.AddView(results); Render();
    }
    void ItemScreen(Item i, Action returnTo)
    {
        Detail(i.Name, $"{i.Category} · {i.Source}\nPatch: {ApiParser.First(i.Version, "unknown")}", returnTo);
        var generation = screenGeneration;
        var bundledAsset = bundledImages.AssetFor(i.Id);
        if (bundledAsset != null || i.ImageUrl.Length > 0)
        {
            var picture = new ImageView(this); picture.SetScaleType(ImageView.ScaleType.FitCenter);
            picture.ContentDescription = i.Name;
            body.AddView(picture, new LinearLayout.LayoutParams(-1, Dp(190)));
            var caption = Label("Loading image…", 11, muted); body.AddView(caption);
            _ = LoadPicture(picture, caption, i.ImageUrl, bundledAsset);
        }
        else body.AddView(Label("Image unavailable · refresh Wiki details to check", 12, muted));
        var card = Card();
        card.AddView(Label($"{i.Manufacturer}   /   Size {i.Size?.ToString() ?? "?"}   /   {i.Grade} {i.Class}", 14));
        card.AddView(Label(ApiParser.First(i.Description, "Refresh Wiki details for specifications."), 14, muted));
        foreach (var (key, value) in i.Stats) card.AddView(Label(key + ": " + value, 14));
        card.AddView(Label("Reported buy price: " + Price(i.BuyPrice), 15, cyan));
        foreach (var shop in i.Shops) card.AddView(Label(shop, 12, muted));
        body.AddView(card);
        AddCommunitySource(i.Name, "", () => ItemScreen(i, returnTo));
        var state = armory.State.Gear.GetValueOrDefault(i.Id) ?? new();
        foreach (var flag in new[] { "Owned", "Need", "Favorite" })
        {
            var check = new CheckBox(this) { Text = flag, Checked = flag == "Owned" ? state.Owned : flag == "Need" ? state.Need : state.Favorite };
            check.SetTextColor(ink); check.SetMinHeight(Dp(48));
            check.CheckedChange += async (_, e) => {
                check.Enabled = false;
                try { await armory.Change(s => { var old = s.Gear.GetValueOrDefault(i.Id) ?? new(); s.Gear[i.Id] = flag switch { "Owned" => old with { Owned = e.IsChecked, Name = i.Name }, "Need" => old with { Need = e.IsChecked, Name = i.Name }, _ => old with { Favorite = e.IsChecked, Name = i.Name } }; }); }
                catch (Exception ex) { Error(ex); ItemScreen(i, returnTo); }
                finally { check.Enabled = true; }
            };
            body.AddView(check);
        }
        body.AddView(Button("✦ Ask Gemini about this item", () => AskGemini(GeminiPrompt.Item(i)), true));
        body.AddView(Button("Refresh Wiki details", async () => {
            var refreshed = await armory.Api.LoadItem(i, lifetime.Token);
            if (refreshed.Id.Length == 0) throw new InvalidDataException("Wiki did not return a matching item.");
            await armory.RememberItem(refreshed);
            if (generation == screenGeneration) ItemScreen(refreshed, returnTo);
        }));
        if (i.WebUrl.Length > 0) body.AddView(Button("Open source / availability", () => { OpenUrl(i.WebUrl); return Task.CompletedTask; }));
    }
    async Task LoadPicture(ImageView view, TextView caption, string url, string? bundledAsset = null)
    {
        try
        {
            if (bundledAsset != null)
            {
                var bundledBitmap = await Task.Run(() => {
                    using var boundsStream = Assets!.Open(bundledAsset);
                    var options = new BitmapFactory.Options { InJustDecodeBounds = true };
                    BitmapFactory.DecodeStream(boundsStream, null, options);
                    int sample = 1;
                    while (options.OutWidth / sample > 900 || options.OutHeight / sample > 900) sample *= 2;
                    using var contentStream = Assets.Open(bundledAsset);
                    return BitmapFactory.DecodeStream(contentStream, null, new BitmapFactory.Options { InSampleSize = sample });
                }, lifetime.Token);
                if (lifetime.IsCancellationRequested || view.IsDisposed()) { bundledBitmap?.Dispose(); return; }
                if (bundledBitmap != null)
                {
                    view.SetImageBitmap(bundledBitmap);
                    caption.Text = "Bundled image · available offline from first launch";
                    return;
                }
            }
            var file = await images.Get(url, lifetime.Token);
            if (lifetime.IsCancellationRequested || view.IsDisposed()) return;
            if (file == null) { caption.Text = "Image unavailable offline or at source."; return; }
            var bounds = new BitmapFactory.Options { InJustDecodeBounds = true };
            await BitmapFactory.DecodeFileAsync(file, bounds);
            int sample = 1;
            while (bounds.OutWidth / sample > 900 || bounds.OutHeight / sample > 900) sample *= 2;
            var bitmap = await BitmapFactory.DecodeFileAsync(file, new BitmapFactory.Options { InSampleSize = sample });
            if (bitmap != null && !view.IsDisposed()) { view.SetImageBitmap(bitmap); caption.Text = "Cached image · available offline"; }
            else caption.Text = "Image format unavailable.";
        }
        catch (Exception ex) when (ex is not OutOfMemoryException) { if (!caption.IsDisposed()) caption.Text = "Image unavailable."; }
    }
    void CommoditiesScreen()
    {
        Title("Commodities", "UEX reference prices per SCU · community reports, not live quotes.");
        var results = new LinearLayout(this) { Orientation = Orientation.Vertical };
        void Render() => Pager(results, armory.Catalog.Commodities.Where(c => Matches(query, c.Name, c.Code)).OrderBy(c => c.Name).ToList(), c => {
            var card = Card(); card.AddView(Label(c.Name, 20)); card.AddView(Label(c.Code + (c.Illegal ? " · Illegal commodity" : ""), 12, cyan));
            var asset = bundledImages.AssetFor(ApiParser.First(c.ImageKey, c.Id));
            if (asset != null)
            {
                var picture = new ImageView(this); picture.SetScaleType(ImageView.ScaleType.FitCenter); picture.ContentDescription = c.Name;
                card.AddView(picture, new LinearLayout.LayoutParams(-1, Dp(100)));
                var caption = Label("Loading bundled image…", 11, muted); card.AddView(caption);
                _ = LoadPicture(picture, caption, "", asset);
            }
            card.AddView(Label($"Buy {Price(c.Buy)}    /    Sell {Price(c.Sell)}", 15));
            card.AddView(Button("✦ Ask Gemini", () => AskGemini($"Research {c.Name} trading in Star Citizen. Verify current patch, legality by jurisdiction, buy/sell locations and prices per SCU. Cite sources; cached UEX prices may be stale.")));
            results.AddView(card);
        }, Render);
        Search("Search commodities", _ => Render()); body.AddView(results); Render();
    }
    static string Price(decimal? value) => value is > 0 ? value.Value.ToString("N0") + " aUEC" : "unavailable";
}
