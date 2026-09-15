# Upload JUVIS SC ARMORY to GitHub

## Upload the source

1. Extract `JUVIS-SC-ARMORY-GitHub-ready.zip` on your PC.
2. Create an empty GitHub repository, for example `JUVIS-SC-ARMORY`. Choose the visibility you want. Do not initialize it with another README.
3. Upload the contents of the extracted `JUVIS-SC-ARMORY` folder, including `.github`, `.gitignore` and `.gitattributes`. Upload the files, not the ZIP itself. The repository root should contain `README.md`, `src`, `tests`, and `.github`.
4. Commit to `main`.
5. Open **Actions → Android debug build**. After it succeeds, download the APK artifact at the bottom of the run page, unzip it, and transfer the signed APK to your phone.

The workflow builds the lightweight edition. Its first hosted run has not yet been verified. No access tokens or signing secrets are needed for this debug build.

If using Git instead of browser upload, open a terminal inside the extracted source folder. With your normal Git identity configured:

```powershell
git init -b main
git add .
git commit -m "Add JUVIS SC ARMORY Android source"
git remote add origin YOUR_REPOSITORY_URL
git push -u origin main
```

Replace `YOUR_REPOSITORY_URL` with your actual repository URL. Do not put tokens in that URL.

## Offer the full-image download

Create a draft release for version `v0.1.2` and attach the existing files from your original `deliverables` folder:

- `JUVIS-SC-ARMORY-0.1.2-full-images-debug.apk` — direct Android installation.
- `JUVIS-SC-ARMORY-0.1.2-full-images-development.aab` — development app bundle.
- `JUVIS-SC-ARMORY-Android-0.1.2-source-with-images.zip` — original full source with images.
- `CHECKSUMS-0.1.2.sha256` — checksums for those downloads.

Keep the large image pack and APK/AAB files as release attachments, outside ordinary Git history. The GitHub-ready ZIP does not contain those large files. Publishing the release is a separate step that you control.

## Build locally with all images

From the repository folder, import your original ZIP into an empty folder:

```powershell
dotnet run --project tools/Juvis.ImagePack -- "C:\path\BundledData (2).zip" .local-image-pack
./Build-Android.ps1 -ImagePackDirectory .local-image-pack `
  -AndroidSdkDirectory "C:\path\android-sdk" `
  -JavaSdkDirectory "C:\path\jdk"
```

Alternatively, point `-ImagePackDirectory` to the `BundledData` folder in your existing full source edition. The repository's empty default index remains unchanged.

## Install on Android

Transfer the signed APK to your phone, open it in the phone's Files app, allow that app to install unknown apps if prompted, and tap Install. Open JUVIS SC ARMORY when installation finishes. An AAB or source ZIP cannot be installed this way.

Debug signing keys can differ between local and GitHub builds. If Android rejects an update due to a signature mismatch, export an in-app backup from the installed app before uninstalling it, then install the new APK and import your backup. For regular releases, use a stable private signing key; never commit the key or its password.

Workflow references: [checkout](https://github.com/actions/checkout), [setup-dotnet](https://github.com/actions/setup-dotnet), [upload-artifact](https://github.com/actions/upload-artifact).

