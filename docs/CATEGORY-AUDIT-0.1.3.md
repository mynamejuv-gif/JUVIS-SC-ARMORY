# Category review — Android 0.1.3

Reviewed on 11 September 2026. Private review build; not approved for public release.

The complete five-source sync succeeded against UEX and Star Citizen Wiki. This audit checks catalog integrity across every populated category, plus Android category navigation and regression tests. It does not establish full Windows feature parity or verify every remote image, shop, mission, or vehicle loadout.

| Module | Records | Findings |
|---|---:|---|
| Item catalog | 7,566 across 55 source categories | No missing names, missing IDs or repeated IDs. |
| Blueprints | 1,606 | Three missing output names and one placeholder name; no missing IDs, repeated IDs, empty recipes, or unnamed/non-positive ingredients. |
| Commodities | 205 | No missing names or IDs, no repeated IDs; 82 have neither a positive buy nor sell price. |
| Vehicles | 280, including 50 ground vehicles | No missing names or IDs; two conflicting ID groups affect four variants. |

See [every item category and its data gaps](category-audit/categories.csv), [module totals](category-audit/modules.csv), [incomplete recipes](category-audit/incomplete-blueprints.csv), and [vehicle conflicts](category-audit/vehicle-conflicts.csv).

## Fixed in this build

- Named blueprints are shown by default. **Show incomplete source records (4)** reveals readable, individually identifiable labels. Cached records and saved plans remain intact. The same fallback appears in recipe details, owned blueprints and craft plans. Newly fetched recipes also check the nested output name.
- Equivalent component category labels are combined for browsing: Cooler/Coolers, Power/Power Plants, Quantum Drive/Quantum Drives, and Shield/Shield Generators. Eyeware is displayed as Eyewear. Source fields and record IDs remain unchanged. The current 55 source categories produce 54 browsing categories.
- Refreshing Wiki details retains the local item/vehicle key, preserving the link to owned states and proposed builds for records originally identified by UEX IDs.
- Vehicle records sharing an ID show an explicit conflict message and pause ownership/build editing. This prevents changes for one variant applying to another. Existing saves are retained and can be exported.
- The About text now reports version 0.1.3.

## Data limitations that still need review

The source repeats the Prowler ID for **Esperia Prowler / Esperia Prowler Utility**, and the Guardian QI ID for **Mirai Guardian QI / Mirai Guardian MX**. These variants need verified, distinct upstream identities before editing is enabled. This build does not guess which variant owns an existing shared save.

Direct recipe-detail requests confirmed the four missing/placeholder names also occur in the nested output data. Three are internal radar records; one is a cooler placeholder. Internal class names are not treated as verified product names or proof of live availability.

Of the item catalog, **3,911** records have neither a matching bundled image nor an image URL in this snapshot. **7,290** have no cached description and **3,180** have no cached manufacturer. The UEX list parser provides summary data; use **Refresh Wiki details** for records that have a Wiki match. These counts describe cached information, not proof that a source can never supply it. The full image pack still contains 3,730 images, including 75 commodity images. Seventeen vehicle records have no image URL.

Twenty-two item-name groups and fourteen blueprint-name groups repeat names (including blank recipe names). Different IDs can represent different game variants or source records, so they are retained. Names alone are insufficient evidence for merging saved states.

The item category called Commodities contains item records and is distinct from the 205-record trading module. Missing or zero reference prices display as **unavailable**, not as free items.

## Validation

- All five live sync modules completed, including all 66 UEX item-category requests and all 17 blueprint pages.
- Forty core tests passed, including recipe parsing, category aliases, refresh identity preservation, backup round trips, crafting totals, port compatibility, failed-sync retention, pagination and offline image lookup.
- Lightweight APK and full-image APK/AAB built successfully. The lightweight restore could not retrieve NuGet vulnerability metadata in the restricted environment (NU1900); the full-image build completed with zero warnings or errors.
- Android 16 emulator loaded the complete catalog. Blueprint filtering showed 1,602 named records by default and four identifiable incomplete records when enabled. The conflicting-vehicle detail screen prevented edits and offered backup export.
- Category navigation results are recorded in [Android category checks](category-audit/android-category-checks.csv). This checks filtered counts and access to a detail action; it is not a download test for every individual detail page.

## Reproduce the data audit

With PowerShell 7 and the project build prerequisites installed:

```powershell
dotnet run --project tests/Juvis.Core.Tests.csproj -c Release -- --live artifacts/live-catalog
./Audit-Catalog.ps1 -CatalogPath artifacts/live-catalog/catalog.json -ImageIndexPath .local-image-pack/image-index.json
```

Omit `-ImageIndexPath` when testing the lightweight edition. Use a fresh output directory for a fresh sync: the live test runner skips sources already successfully synced in that folder. Image availability totals depend on the supplied image index. API results can change after this audit.

Primary sources: [UEX API](https://api.uexcorp.uk/2.0/), [Star Citizen Wiki API](https://api.star-citizen.wiki/api/).
