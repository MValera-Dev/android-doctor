## Android Doctor 1.0.0
Unity 6000.3.2f1 · Windows 11 · target API 36 · IL2CPP · ARM64
JDK 17.0.18 (embedded) · NDK 27.2.12479018 (embedded) · Gradle unknown

### Errors (2)
- **NAT001** Native libraries from Packages/com.vendor.sdk are not 16 KB aligned (4). Update the package or ask the vendor for a version with 16 KB support.
  - libfoo.so (arm64-v8a) · LOAD align 0x1000 · Packages/com.vendor.sdk/Plugins/arm64-v8a/libfoo.so
  - libbar.so (arm64-v8a) · LOAD align 0x1000 · Packages/com.vendor.sdk/Plugins/arm64-v8a/libbar.so
  - libbaz.so (x86_64) · LOAD align 0x1000 · Packages/com.vendor.sdk/Plugins/x86_64/libbaz.so
- **ENV004** SDK platform android-36 is not installed. Your target API level is 36, but the SDK has no platform for it, so the build will fail.
  - SDK path: C:/sdk
  - Run: `sdkmanager "platforms;android-36"`

### Warnings (1)
- **PRJ005** Custom Gradle templates found. Custom templates replace Unity's defaults and can break after an upgrade.
  - Assets/Plugins/Android/mainTemplate.gradle

### Info (1)
- **PRJ004** Android App Bundle is off. Google Play requires an Android App Bundle (.aab) for new apps.
  - Build App Bundle: off
