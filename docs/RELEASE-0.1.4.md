# Android 0.1.4 — ADP Arms refresh fix

Private review build, 12 September 2026.

Version 0.1.3 reproduced the reported Android.OS.NetworkOnMainThreadException when refreshing the first Arms entry, ADP Arms (uex:item:2624). The shared API and image-cache services now keep request startup, streamed response reading and disposal on background threads. Screen callbacks still resume on the Android UI thread; Android's network-thread protections remain enabled.

The network fix exposed the real source response: Wiki returns HTTP 404 for the generic ADP Arms entry. The app now explains that no matching Wiki record is available and that cached details and saved states were retained. It does not recommend a UEX token for a Wiki error or substitute a differently named armor variant.

Validation:

- 43 core tests, including a regression that fails before the threading fix, streamed transport/disposal guards, image-cache reuse and the Wiki 404 message.
- Android 16 emulator: reproduced the old error on ADP Arms; the new network implementation instead surfaces the upstream 404.
- A matching item (FR-76) refreshed successfully and downloaded an image into the cache without the thread error.
- Version code 5, display version 0.1.4; same application ID and local debug signing identity as earlier local packages.

Install the signed APK over your current app. Export a backup first. Do not uninstall the existing app. The full-image build retains 3,730 bundled images. AAB files are not directly installable.

Background: [Android's network-on-main-thread exception](https://developer.android.com/reference/android/os/NetworkOnMainThreadException), [Android threading guidance](https://developer.android.com/topic/performance/threads).
