# JUVIS SC ARMORY · Android 0.1.1

> Archived documentation for the separate full-image source package. References below to included images and deliverables describe that package, not this lightweight GitHub repository. See the root README for this repository's build instructions.

A native Android starter implementation in C# using .NET 10 for Android. The dark navy and cyan visual direction comes from the RC17A Windows screenshots. Large touch targets, a persistent five-destination navigation bar, searchable lists and focused detail pages replace the desktop window layout.

The original Windows source archive was not accessible in the referenced conversation or project mirror. This is a new implementation of the described workflows, not a binary conversion or a claim of complete desktop parity.

## Run the debug APK

1. Transfer the supplied `JUVIS-SC-ARMORY-0.1.1-full-images-debug.apk` to an Android 8.0+ device (ARM64 or x86-64).
2. Open it and allow that file manager to install the application when Android asks.
3. Launch **JUVIS SC ARMORY**. The partial starter catalog works immediately without a network connection.
4. Open **More** and sync the modules you need. Start with Commodities, Vehicles and Blueprints; UEX item sync and Wiki component sync can take longer.

The development AAB is for bundle tooling, not direct installation or Play Store submission. The APK contains the runtime and does not need a desktop connection. Export a backup outside the app before uninstalling.

## Bundled image folder

This source package now includes the images supplied in `BundledData (2).zip`:

```text
BundledData/
  Images/                  # 3,655 item images named by game UUID
    commodities/           # 75 commodity images named by UEX numeric ID
  image-index.json          # ID-to-asset lookup used by Android
  image-sha256.json         # Checksums of the original image bytes
  image-pack-report.json   # Counts and total bytes
```

All 3,730 original JPG, PNG and WebP files are included without resizing or recompression in the source folder. The image payload is 772,083,556 bytes (about 736 MiB), so the full-image APK/source archive are large. The Android project embeds these as read-only assets and decodes them directly; it does not unpack a second full copy into app storage. It downsamples display decoding to keep memory use bounded.

An item image is selected by its game UUID even if its remote URL is blank or has changed. Matching bundled images appear offline from first launch; missing matches fall back to the downloaded cache/network. Commodity images use a separate UEX ID namespace. **Clear downloaded images** affects only the 100 MB download cache, not this bundled pack.

The partial starter catalog has 367 matching item images out of 567 items, plus 75 matching commodity images. Other packed item images become useful after syncing their catalog records. The desktop ZIP also contained 3,711 JSON records marking no cached image; these are not pictures and are not presented as successful images.

To import a replacement pack into a new empty folder, use the included utility, then replace the project's `BundledData` folder deliberately:

```powershell
dotnet run --project tools/Juvis.ImagePack -- "C:/path/BundledData.zip" "C:/path/NewBundledData"
```

The utility validates item/commodity filenames and rejects unsafe paths, duplicate item IDs and oversized entries. Its output includes the lookup index and checksums. The provided full source archive already contains the prepared folder, so no import step is needed to rebuild it. Full-size bundles are development artifacts; a store release would need a separate asset-delivery plan.

## Included workflows

| Module | Android implementation |
|---|---|
| Catalog | Search names/manufacturers/categories, category filter, paged native cards, detailed specifications, reported prices and shops when supplied by Wiki/UEX. |
| Images | 3,730 bundled images, UUID/commodity lookup, direct offline asset decoding; network fallback with 5 MB file limit and 100 MB LRU cache. |
| Commodities | Searchable UEX commodity data, buy/sell reference prices per SCU and illegal-goods flag. |
| Blueprints | Search recipes and ingredients, quantities, craft time, owned state, default unlocks, fetch mission unlock details, full-source links for tiers. |
| Crafting hub | Add/remove recipe quantities, fractional SCU totals separated from item counts, resources → Used In, My Blueprints. |
| My Gear | Independent Owned, Need and Favorite states; filters; imported states stay visible before catalog sync. |
| Gemini | Item, commodity, recipe and vehicle-build prompts. Copy prompt, then open Gemini using its HTTPS app link; paste manually. No API key or automatic prompt submission. |
| Vehicle upgrades | Ship/ground/my-vehicle filters; real stock ports/components; fixed-port handling; nested port paths; type, size, subtype, tag and patch checks; proposed builds persisted per vehicle. |
| Backup | Android document picker export/import; schema validation; merge preview; atomic storage; pre-import recovery copy. Includes personal states/builds, excludes tokens/images/catalogs. |
| Sync | UEX per-category items, commodities and vehicles; Wiki blueprints, components, individual details and ports. Pagination, 20-second request timeouts, cancel/retry, independent timestamps, failed-sync cache retention. |

## Build from source

Prerequisites: .NET 10 SDK, Android workload, JDK and Android SDK platform/build tools matching the installed workload. The project targets `net10.0-android`, minimum Android API 26. It uses only the framework and standard C# libraries; there are no third-party application NuGet dependencies.

On Windows PowerShell, from this folder:

```powershell
dotnet workload install android
./Build-Android.ps1 -AndroidSdkDirectory C:/Android/sdk -JavaSdkDirectory C:/Android/jdk -InstallDependencies
```

`-InstallDependencies` runs Microsoft's dependency installer and accepts the Android SDK licenses. If you already have the tools installed, omit that switch and provide your existing paths (or set `ANDROID_HOME` and `JAVA_HOME`). Install .NET 10 first if `dotnet` is unavailable. Use `-Dotnet C:/path/to/dotnet.exe` to select a private SDK.

```powershell
# Tests only; no Android SDK needed
dotnet run --project tests/Juvis.Core.Tests.csproj

# Debug APK, tests first
./Build-Android.ps1

# Development AAB; configure production signing separately
./Build-Android.ps1 -Configuration Release -Format aab

# Install onto one connected test device/emulator
adb install -r artifacts/app.juvis.scarmory-Signed.apk
```

Build outputs are copied to `artifacts/`. `*-Signed.apk` is the installable package. The build embeds managed assemblies, so fast-deployment files on a development machine are not required.

For public distribution, use your own protected Android signing key and configure `AndroidKeyStore`, `AndroidSigningKeyStore`, `AndroidSigningKeyAlias`, and password inputs in your build environment. Do not commit keys or passwords. This first version disables trimming/AOT in the helper script to keep reflection-based JSON models intact and iteration straightforward.

## Code map

- `src/Juvis.Core`: platform-independent models, parsers, HTTP adapters, compatibility, crafting, local state and backup logic.
- `src/Juvis.Android`: native Android activity and screen partials, file picker, Gemini handoff, theme and icon.
- `src/Juvis.Android/Assets/starter.json`: normalized partial public-data snapshot, 2026-09-11. It is not a complete game database.
- `BundledData/Images`: original uploaded image files; `image-index.json` connects them to Android records.
- `tools/Juvis.ImagePack`: safe image-pack importer and checksum generator.
- `tests`: dependency-free executable tests with small real API fixtures and fake HTTP responses.
- `docs`: architecture, API references, test checklist and build verification.

The Android core can later be shared with the existing Windows app or hosted behind MAUI views. Native .NET Android was selected here to keep an Android-only first version small in code and avoid a second UI framework.

## First-version limits

- Windows backup migration is intentionally rejected until its actual schema/source is available; guessing could misapply ownership to different items.
- The starter vehicle is the Wiki's Cutlass Black variant returned by its name route. Sync Vehicles and select your exact vehicle; full requests prefer UUIDs to avoid variant ambiguity.
- Stock data and a proposed build are supported. Tracking a separate user-installed current loadout is not implemented yet.
- Fit means the available mount constraints match. It is not a ship-wide power/cooling simulation or proof that the build is better. Unresolved subtypes/patches remain review-only and cannot be added as confirmed upgrades.
- Recipe totals use the base recipe. Tier, quality, station and unlock availability must be checked against the linked source. The app does not model all crafting modifiers.
- Sync runs while the activity is alive; it cancels when the activity is destroyed. Completed data and user state persist. UEX tokens are session-only.
- Artwork from the uploaded desktop image pack is bundled; items with no matching packed image still use on-demand network lookup. The app does not invent images for missing records.

See `docs/VERIFICATION.md` for the exact checks performed and remaining device checks.

## Sources and attribution

Unofficial fan companion. Star Citizen and related assets belong to their respective owners; this app is not affiliated with Cloud Imperium Games.

Data: [UEX API](https://uexcorp.space/api/documentation/) and [Star Citizen Wiki API](https://api.star-citizen.wiki/developers). Prices are community reports and can be stale. Review the providers' current use terms before public/commercial distribution.

Build reference: [Microsoft .NET for Android](https://learn.microsoft.com/dotnet/android/overview) and [build properties](https://learn.microsoft.com/dotnet/android/building-apps/build-properties).
