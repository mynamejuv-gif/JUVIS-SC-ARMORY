# Verification · 11 September 2026

## 0.1.1 bundled-image update

- Imported all 3,730 image files from the user-supplied `BundledData (2).zip`: 3,655 item images and 75 commodity images, totaling 772,083,556 bytes. Original image bytes are retained in `BundledData/Images`; lookup index, checksums and import report are alongside it.
- 35 automated tests pass, including UUID lookup without remote URLs, commodity ID namespaces, path/mismatched-ID rejection and stable image keys for UUID-backed commodities.
- Debug APK and Release development AAB builds pass with zero warnings and zero errors. App version is 0.1.1 / version code 2.
- Verified all 3,730 images inside both the signed APK and signed AAB against the source SHA-256 manifest. APK v2/v3 signature verification and AAB JAR verification pass; the AAB has the expected development-certificate warning.
- Fresh installation tested on the Android 16 emulator after clearing its previous test app data and disabling both Wi-Fi and mobile data (both settings read back as 0).
- FR-76 loaded its bundled WebP image offline. Venture Arms loaded a bundled image despite having no remote image URL. Agricium displayed its bundled commodity image. The screens explicitly reported `Bundled image · available offline from first launch`.
- Clearing downloaded images did not remove bundled imagery: FR-76 still rendered offline afterward. No AndroidRuntime crash appeared during the observed image tests.
- New screenshots: `bundled-fr76-offline.png`, `bundled-venture-offline.png`, `bundled-commodity-offline.png`.

The original 0.1.0 checks below establish the unchanged core workflows; the image-specific checks above apply to the new full-image packages. The full image payload makes these development packages much larger. No store-distribution asset-delivery workflow is claimed.

## Build environment

- Windows x64; project-local .NET SDK 10.0.401.
- .NET Android workload 36.1.2; Android SDK/build tools 36.0.0.
- Microsoft OpenJDK 17.0.14.
- Native Android API 36 / Android 16 x86-64 emulator; hardware acceleration available.
- APK supports ARM64 and x86-64, with minimum Android API 26 (Android 8).

## Completed checks

- Debug APK build: passed, zero warnings, zero errors.
- Release development AAB build: passed, zero warnings, zero errors. Uses development signing; not a Play Store release.
- Packaged PowerShell build script executed successfully.
- 31 automated core tests passed. Coverage includes full personal-state backup round trip, malformed/future schemas, quantity validation, atomic and concurrent writes, independent SCU/item totals, nested hardpoint IDs, fixed/type/size/subtype/tag/patch compatibility, API fixture parsing, auth host separation, pagination, empty UEX categories and failed-sync retention.
- Live sync passed: 205 commodities, 280 vehicles, 1,606 blueprints, 276 Wiki component records. UEX item sync across all categories produced a merged catalog of 7,566 items with Wiki records preserved for shared UUIDs.
- Live tests exposed duplicate Wiki page parameters; pagination now reconstructs requests from validated metadata and checks final record counts. A regression test covers this behavior.
- Live tests exposed UEX `status: ok, data: null` for empty categories; these are accepted as empty while actual API errors still fail safely.
- Debug APK installed and launched on Android 16. No AndroidRuntime crash was reported during the observed checks.
- Search `FR-76` returned one matching shield. Owned and Favorite toggles persisted in the app's state file and appeared in My Gear.
- Omnisky III Cannon blueprint ownership and craft plan quantity 3 persisted. The automated resource test checks 1.08 SCU and 21-unit aggregation.
- Backup export through Android's document picker created a 509-byte JSON file containing the test gear, blueprint and craft plan state.
- Backup import through the picker displayed its merge summary, completed successfully and saved the pre-import recovery copy. Personal state also survived installing the updated debug APK.
- APK signature verification passed (v2 and v3). AAB JAR signature verification passed with the expected self-signed development-certificate warning.
- Final APK vehicle flow passed: owned Cutlass Black → filter ports by `shield` → compatible upgrades → add `6MA 'Kozane'` → proposed build. The exact vehicle UUID, shield port, item UUID and patch were saved in the state file.

Screenshots in `screenshots/` are from the actual Android emulator, not mockups. Early screenshots may show the initial 620-entry seed before internal placeholders were removed. The final bundled seed contains 567 items, 205 commodities, 30 blueprints and one 238-port vehicle snapshot.

## Remaining release acceptance

Complete `TEST-CHECKLIST.md` on physical devices before public distribution. In particular: Android 8 behavior, ARM64 physical-device execution, large-font accessibility, prolonged background/process death, weak-network retries on a phone, full uninstall/reinstall restoration, and verification of current-patch game behavior. A successful emulator launch and automated tests do not establish all-device or in-game compatibility.

Windows backup migration and desktop source sharing were not validated because the RC17A Windows source/schema was unavailable. The Android app rejects unknown backup formats instead of guessing their ownership mappings.
