# Changelog

All notable changes to this package are documented in this file.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses [Semantic Versioning](https://semver.org/).

## [1.0.0] - 2026-11-07

### Added

- **Fix all**: one confirmation that lists every change, then every fix that needs no decision. Findings can now offer several buttons, for example one per version of a duplicated library.
- One-click fixes: put a custom JDK, SDK, NDK or Gradle back to the one installed with Unity; raise the Gradle heap; set the target and minimum API, ARM64 and IL2CPP; add `android:exported` to a manifest; rewrite a setting Gradle removed in `gradleTemplate.properties`; set the exact minimum API a library declares; ignore `ANDROID_SDK_HOME` while Unity is open; update an old Burst through Package Manager.
- Fixes that offer options: turn off all but one version of a duplicated library or native plugin for Android (reversible, nothing is deleted); back up and remove custom Gradle templates; reset the External Dependency Manager; clean the generated Android project.
- ENV010 (Gradle heap too small), PRJ010 (a removed setting in `gradleTemplate.properties`, found before the build) and GRD021 (the same, recognised from a real failed build).
- The last failed build is part of **Check project**, with the buttons of its cause. A failure the user has already acted on is not shown again, and a build that works clears it.
- Files a fix edits are copied to `Library/AndroidDoctor/Backups` first.

### Changed

- The window was redesigned: one plain sentence about the whole project, findings grouped by how much they matter (stops your build, worth fixing, notes) as flat rows with the Console's own icons, and one strong button at a time. It follows the Editor's light and dark skins.
- The Gradle log tab is only for pasted logs; its fix buttons work too.

### Known limitations

- Tested on Unity 6000.6.4f1 on Windows only; Unity 2022.3 is declared as the minimum but has not been run.
- Missing SDK platforms and build-tools are explained and the `sdkmanager` command is copied, but they are not installed.
- Updating Burst and resetting the External Dependency Manager were not exercised against a real registry or a real EDM4U.
- ZIP alignment of uncompressed `.so` files in an APK (BLD002) is not checked.

## [0.9.0] - 2026-11-07 (pre-release)

### Added

- Window > Android Doctor with a Checks tab and a Gradle log tab. Nothing runs until you press a button.
- Environment rules ENV001–ENV009: Android module, JDK, SDK, platform, build-tools, NDK, Gradle, environment variables and project path.
- Project rules PRJ001–PRJ009: target and minimum API, ARM64 and IL2CPP, App Bundle, custom Gradle templates, `android:exported`, duplicate libraries, debug signing and default application id.
- Native library rules NAT001–NAT005 with a dependency-free ELF reader: 16 KB `LOAD` alignment, RELRO alignment, 32-bit-only libraries, old Burst and App Bundle packages. Findings name the package that ships the library.
- BLD001 and **Scan build…**: reads the `.so` files inside an APK or AAB. The output of your last build is remembered automatically.
- Gradle translator GRD003–GRD020, built from real failed-build logs, with a parser for pasted logs, the last build and Unity's console.
- Fix buttons with a confirmation dialog (`before -> after`) for target API, Min API and ARM64.
- **Copy report** (Markdown, always in English, at most 8000 characters) and **Copy for forum**.
- English and Spanish interface; Spanish is used when the system language is Spanish.

## [0.1.0] - 2026-11-06

### Added

- Initial package scaffold.
