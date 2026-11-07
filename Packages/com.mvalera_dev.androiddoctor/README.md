# Android Doctor

Finds why your Unity Android build fails or gets rejected by Google Play, and tells you how to fix it.

Open it with **Window > Android Doctor** and press **Run checks**. It checks your JDK/SDK/NDK/Gradle setup, your Player Settings, every native library for the 16 KB page-size requirement (and tells you which package ships the bad one), and translates Gradle errors into a probable cause and a fix.

Free, MIT, no network calls. Nothing runs until you press a button, and nothing changes without asking.

Full documentation, the list of checks, known limitations and how to report a problem with **Copy report**: https://github.com/MValera-Dev/android-doctor
