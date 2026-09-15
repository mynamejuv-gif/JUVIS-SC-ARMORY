# JUVIS SC ARMORY Android 0.1.7

## Architecture verified

This remains a native C# .NET 10 Android application, not MAUI. `Juvis.Android` contains Activity-based native Android screens. `Juvis.Core` owns source parsing, cached catalogs, compatibility, crafting, personal state and schema-v1 backup merge logic. Catalog snapshots and personal state are written separately and atomically. UEX, Star Citizen Wiki and Citizen Starter Guide remain the external sources.

## Changes

- One shared resolver now rejects blank names, placeholder/uninitialized markers, GUID-only names and common internal tokens. It checks primary, alternate/localized and manufacturer/model candidates in that order. Every listing/detail/search flow uses the resolver.
- Records without a trustworthy name are hidden, not deleted from persistent personal state. Existing gear/build/plan IDs remain exportable in unchanged `juvis-android` schema-v1 backups.
- Weapon records now model ammunition class/type, caliber, magazine type/name, capacity, compatible magazines, compatible-ammunition names when supplied, and energy capacity/regeneration.
- Magazine compatibility uses the source's equipped UUID or the same size and bidirectional tag restrictions used for mount checks. It never treats all magazines as compatible.
- Catalog search indexes ammunition text. Existing filter controls now include Ballistic, Energy, Magazine-fed and Ammo data unavailable.
- A new atomic sync source refreshes personal weapons, ship/vehicle guns and magazines together. Partial responses do not replace the previous cache.

## Ammunition coverage

Checked on 15 September 2026 against the Star Citizen Wiki API list records for:

- `WeaponPersonal` (five API pages)
- `WeaponGun` (two API pages)
- `WeaponAttachment` with subtype `Magazine` (one API page)
- the bundled UEX-derived weapon-category records and existing stock-loadout components

The sanitized bundled result contains 586 displayable weapon-category items. Of these, 524 are ammunition-applicable ranged weapons and all 524 have at least one reliable structured ammunition field. The remaining 62 are knives/blades, grenades/flares, gadgets or utility/mining/tractor/repair-style entries and are marked **Ammunition not applicable**.

Weapons for which reliable ammunition data could not be found: **none within the scope above**.

Caliber is shown only when explicitly supplied by structured description data or an unambiguous source sentence about rounds/cartridges/shells. Internal ammunition UUIDs are retained only for relationships and never displayed.

## Verification

- 61 core tests pass, including compatibility constraints, bundled-data quality and ammunition coverage, a real bundled vehicle/upgrade match, backup round-trip/merge/backward compatibility, malformed backup rejection, name hiding without state loss, ammunition parsing, magazine relationships, energy capacitors, pagination and failed-sync retention.
- Release APK and AAB compile with zero warnings and zero errors using .NET SDK 10.0.401, Android API/build tools 36 and the supplied JDK.
- APK manifest: `app.juvis.scarmory`, versionCode 8, versionName 0.1.7, minSdk 26, targetSdk 36.
- APK signature verification passes v2 and v3 with the development debug certificate. The AAB JAR signature verifies with the expected self-signed development-certificate warning.
- Full-image APK and AAB each contain all 3,730 indexed offline images.
- Bundled catalog audit found no blank or placeholder item names, blueprint names, manufacturers or installed-component names.

No emulator/ADB device or configured AVD was available on this host. Navigation, Catalog, Weapons, Vehicles, My Gear and document-picker Import/Export were therefore not rerun interactively for 0.1.7; their behavior is covered by compilation and core tests only. Earlier screenshots and emulator claims in other documents apply to earlier releases.

## Known remaining issues

- Physical-device and emulator interaction is still required before public release, especially Android 8, ARM64 hardware, tablets, large fonts, document-picker import/export and process-death behavior.
- Current-patch in-game fit and performance cannot be established from a successful mount-data check; candidates with incomplete restrictions remain review-only, and Gemini suggestions still require source verification.
- Caliber and human-readable cartridge names are absent for some weapons even when class/capacity/projectile data exists; the app leaves those individual fields unknown instead of inferring them.
- The supplied APK/AAB are development-signed. A stable private release key is required for production upgrades or store publication.
- Windows desktop backup migration remains unsupported until its actual schema and fixtures are available. Android schema-v1 import/export remains backward compatible.
