# Android Doctor

Finds why your Unity Android build fails or gets rejected by Google Play, and tells you how to fix it.

![Android Doctor: Run checks, the finding cards and a fix](Docs/media/android-doctor.gif)

Free, open source (MIT), no network calls. One window: **Window > Android Doctor**.

## Install

Window > Package Manager > + > Add package from git URL…

```text
https://github.com/MValera-Dev/android-doctor.git?path=/Packages/com.mvalera_dev.androiddoctor
```

Or download the `.zip` of the package from the GitHub Releases page, unzip it, and use *Add package from disk…* on its `package.json`.

## What it checks

Nothing runs until you press **Run checks**. Every finding has a stable id, so you can search for it.

| Group | Ids | What it looks at |
|---|---|---|
| Environment | ENV001–ENV009 | Android module, JDK, SDK, platform, build-tools, NDK, Gradle, environment variables that confuse Gradle, project path |
| Project | PRJ001–PRJ009 | Target API vs Google Play, Min API, ARM64 and IL2CPP, App Bundle, custom Gradle templates, `android:exported` in manifests, duplicate `.aar`/`.jar`, debug signing, default application id |
| 16 KB / 64-bit | NAT001–NAT005 | Every native `.so` (in `Assets`, packages and inside `.aar` files) for 16 KB `LOAD` alignment, RELRO alignment, 32-bit-only libraries, old Burst and App Bundle packages. It names the package that ships the bad library |
| Your build | BLD001 | The `.so` files inside an APK or AAB. The output of your last build is remembered, or pick a file with **Scan build…** |
| Gradle errors | GRD003–GRD020 | Paste a Gradle log, or analyse the last build, and get a probable cause and a fix instead of `Gradle build failed` |

Some findings have a **Fix** button (target API, Min API, ARM64). It always shows what will change (`before -> after`) and asks first. Nothing is changed behind your back.

## Reading a finding

Each finding is a card with a colour on its left edge: red is an error (the build will fail or Google Play will reject it), amber is a warning, blue is information. The grey text is the rule id, the bold text is the problem, and below it is what to do. *Evidence* holds the exact paths and versions it found. Depending on the finding you also get **Copy command** (for example an `sdkmanager` line) and **Docs**.

## Reporting a bug or a missing detection

Press **Copy report** and paste the result in an issue. It is Markdown, always in English, and holds your Unity, OS, JDK, NDK and Gradle versions plus the findings. If a Gradle error is not recognised, press **Copy for forum** in the *Gradle log* tab: it copies the versions and the original failure block.

A false positive becomes a test before it is fixed, and a new Gradle error becomes a pattern with its own fixture.

## Limitations

- Developed and tested on **Unity 6000.6.4f1 on Windows 11**. `package.json` declares Unity 2022.3 as the minimum, but it has not been run there yet. macOS and Linux are untested.
- The compatibility table (`Editor/Data/compat.json`) and the Google Play requirements (`play.json`) are dated 2026-11-06. For a Unity version newer than the table, the closest older row is used.
- The Gradle patterns were checked against real failed builds on Unity 6000.6.4f1. Other Unity versions print slightly different logs, so a pattern may miss there. That is what **Copy for forum** is for.
- Gradle errors are explained, not fixed: they depend on your project.
- Not done yet: the ZIP alignment of uncompressed `.so` files inside an APK (BLD002).
- It reads files and the Editor log. It never changes your project on its own and never opens a network connection.

## Contributing

Issues and pull requests are welcome. Everything is in English except the Spanish table in `Editor/Core/Messages.cs`. Tests are in `Packages/com.mvalera_dev.androiddoctor/Tests/Editor` (Window > General > Test Runner > EditMode). The design is in `Docs/GDD.md` (in Spanish).

## License

MIT. See `LICENSE`.
