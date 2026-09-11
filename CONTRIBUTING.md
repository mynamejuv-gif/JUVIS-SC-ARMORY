# Contributing

Start with the README build instructions and `docs/ARCHITECTURE.md`. Keep changes focused on one user-visible problem and explain the before/after behavior in your pull request.

## Before a pull request

1. Run `dotnet run --project tests/Juvis.Core.Tests.csproj -c Release`.
2. Build the Android app using `Build-Android.ps1`.
3. For screen changes, check the affected flow on an Android device or emulator and include a screenshot.
4. Update the user guide if button names or workflows change.

Keep the default `BundledData/image-index.json` empty. Use the ignored `.local-image-pack` folder or an external image-pack path for full-image builds. Do not commit build outputs, downloaded SDKs, personal backups, bearer tokens or signing keys.

## Reporting a problem

Use the bug-report form with the app version, Android version, build edition, steps to reproduce and expected behavior. For data or compatibility problems, include the item/vehicle name and displayed patch. Remove personal information from screenshots and logs.

## Project scope

JUVIS stores local plans and community reference data. It does not modify a player's in-game inventory or vehicle. Changes should preserve cautious compatibility checks, offline cache access and backup portability.

No open-source license has been selected yet. Public availability does not itself grant a redistribution license. Third-party game assets and data remain subject to their respective owners' terms.
