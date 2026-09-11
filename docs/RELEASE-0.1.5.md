# Android 0.1.5 — Citizen Starter Guide

Private review build. Adds Citizen Starter Guide as an optional online sync source with offline mission reports and website links.

- New More → Sync Citizen Starter Guide action and per-item/per-blueprint refresh buttons.
- Separate community source cards show source build, update date, exact match, mission count and reported craft time. Mission pages show faction, system, reputation standing/threshold and reported legality when supplied.
- Blueprint keys match first; legacy records fall back only to a unique exact name, normalizing case and whitespace. Ambiguous matches and different variants are never silently substituted.
- Open Citizen Starter Guide copies the selected name and opens the verified Blueprint Finder page. The website currently has no supported search URL parameter.
- Wiki distinguishes unchecked mission details from a checked empty result. Refreshed empty data can clear old missions; mission details are not carried across Wiki patch changes.
- Gemini crafting prompts request the exact variant, patch and comparison of sources.

## Validation and limits

51 core tests passed. Live sync returned 1,602 guide records, source build 4.10.191.2241. Standard A03: eight community mission reports; A03 "Canuto": zero. The absence of a report does not establish in-game availability either way.

Android 16 emulator: upgraded the existing installation; synced all 1,602 guide records; verified Canuto did not inherit base-A03 missions; checked the exact-name copy dialog; disabled Wi-Fi/mobile data, restarted the app and read the eight base-A03 reports offline. Screenshot: [offline mission reports](community-sources/missions-offline.png).

Lightweight APK and full-image APK/AAB built successfully. Full-image build: zero warnings/errors. Lightweight restore could not fetch NuGet vulnerability metadata in the restricted environment (NU1900). The full-image package retains the original 3,730 images.

The community dataset is a public static file used by the source's own tool, not a guaranteed stable API. Invalid, empty or changed-format responses retain the previous cache and update date. A failed sync can be retried; the website remains available for manual research. No UEX token is sent to this source. Mission rewards are community reports, not guaranteed drops. Recipe materials from this source lack explicit quantity units, so they do not replace Wiki craft-plan quantities.

Source attribution: [Citizen Starter Guide](https://citizen-starter-guide.com/), [Blueprint Finder](https://citizen-starter-guide.com/star-citizen-blueprint-finder/), [public dataset](https://quattrobaje3na-png.github.io/Star-Citizen-Blueprint-Finder-Mission-Contract-Rewards-Guide/blueprint_explorer_data.json). Data and third-party content remain owned by their respective creators. JUVIS does not imply affiliation or ownership.

## Install and test

Export an in-app backup, then install the signed 0.1.5 APK over your current local build. In More, sync Citizen Starter Guide once. Browse an item or blueprint to compare its source card with Wiki data. An AAB is not directly installable.

Run core tests with `dotnet run --project tests/Juvis.Core.Tests.csproj -c Release`. Test a live guide download with `dotnet run --project tests/Juvis.Core.Tests.csproj -c Release -- --guide-live artifacts/guide-live`.
