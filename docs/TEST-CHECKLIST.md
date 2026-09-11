# Device acceptance checklist

Use a disposable test install and keep exported backups outside app storage.

1. First launch offline: starter data appears without an API key, all navigation destinations respond, system Back returns to the prior section.
2. Catalog: search `shield`, apply a category, page forward/back, open details, mark Owned/Need/Favorite independently. Restart and verify all three persist.
3. Images: disable network before first launch. Open FR-76 and verify the caption says "Bundled image". Browse Agricium in Commodities and verify its bundled picture. Clear downloaded images, relaunch offline and confirm both remain. Check an unmatched item online for normal network fallback; no personal state should be lost.
4. Commodities: search Agricium; confirm prices are labeled per SCU and unavailable values are not displayed as free goods.
5. Blueprint: open Omnisky III Cannon; inspect base resources, refresh mission unlocks, mark owned, add to plan three times. Crafting should show 1.08 SCU Agricium and 21 units each of Hadanite and Dolivine for this snapshot.
6. Crafting: Used In opens recipes, My Blueprints opens the owned recipe, reducing count to zero removes the entry.
7. Gemini: copy a prompt, open Gemini, paste manually. Confirm selected item/build context and no full backup/token content.
8. Vehicle: select the exact Cutlass variant, refresh stock data, inspect a shield port, sync candidates. Fixed ports cannot be changed. Only confirmed fits offer Add to proposed build. Open Proposed Build, remove an entry, restart and check persistence.
9. Failure: disconnect during a sync. Error is visible and the prior cache remains. Reconnect, retry. Cancel a long sync. Rapid navigation must not crash the app.
10. Export backup through the Android document picker. Change one state, import the file, inspect the merge summary, confirm and verify restored states. Try a malformed/future-version file; no states should change.
11. Reinstall recovery: export, uninstall, reinstall the same APK, import. Gear, blueprints, craft plan, owned vehicles and builds should return; catalogs can be synced separately.
12. Test portrait/landscape, tablet width, large Android font size, screen reader labels, keyboard visibility, Android 8 and Android 16 navigation insets. Verify no content is hidden under system bars.

The automated tests do not substitute for this full physical-device checklist.
