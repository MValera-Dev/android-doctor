# Changelog

All notable changes to this package are documented in this file.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses [Semantic Versioning](https://semver.org/).

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

### Known limitations

- Tested on Unity 6000.6.4f1 on Windows only; Unity 2022.3 is declared as the minimum but has not been run.
- Most findings explain the problem and what to do; one-click fixes exist only for target API, Min API and ARM64. More fixes are coming before 1.0.0.
- ZIP alignment of uncompressed `.so` files in an APK (BLD002) is not checked.

## [0.1.0] - 2026-11-06

### Added

- Initial package scaffold.
