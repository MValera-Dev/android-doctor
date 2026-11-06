using System.Collections.Generic;
using UnityEngine;

namespace AndroidDoctor
{
    /// <summary>
    /// Every user-visible string lives here. English by default; Spanish when the system language is Spanish.
    /// A key missing in Spanish falls back to English; a key missing in both returns the key itself,
    /// so an omission is visible on screen instead of crashing.
    /// </summary>
    public static class Messages
    {
        static readonly Dictionary<string, string> En = new Dictionary<string, string>
        {
            // Rules
            { "PRJ009.title", "Default application identifier" },
            { "PRJ009.detail", "The identifier starts with com.DefaultCompany. You cannot change it after publishing to Google Play." },
            { "PRJ009.evidence", "Application identifier: {0}" },

            { "ENV001.title", "Android Build Support is not installed" },
            { "ENV001.detail", "Unity cannot build for Android because the Android module is missing from this editor. Open Unity Hub > Installs > your editor > Add modules, and install Android Build Support, OpenJDK and Android SDK & NDK Tools." },
            { "ENV001.evidence", "Unity {0}" },

            { "ENV002.missing.title", "JDK folder does not exist" },
            { "ENV002.missing.detail", "The JDK path configured in Unity does not exist, so Gradle cannot run. In Preferences > External Tools, tick 'JDK Installed with Unity (recommended)'." },
            { "ENV002.version.title", "JDK version differs from the one Unity expects" },
            { "ENV002.version.detail", "Unity {0} is tested with JDK {1}, but a custom JDK {2} is configured. A different major version can break Gradle. In Preferences > External Tools, tick 'JDK Installed with Unity (recommended)'." },
            { "ENV002.evidence.path", "JDK path: {0}" },
            { "ENV002.evidence.version", "JDK version: {0}" },
            { "ENV002.fix", "Open External Tools" },

            { "ENV003.missing.title", "Android SDK folder does not exist" },
            { "ENV003.missing.detail", "The SDK path configured in Unity does not exist, so the build cannot find the Android tools. In Preferences > External Tools, tick 'Android SDK Tools Installed with Unity (recommended)'." },
            { "ENV003.incomplete.title", "Android SDK is missing components" },
            { "ENV003.incomplete.detail", "The SDK has no {0}, which the build needs. Use the SDK that Unity installs: Preferences > External Tools > 'Android SDK Tools Installed with Unity (recommended)'." },
            { "ENV003.evidence.path", "SDK path: {0}" },
            { "ENV003.component.platformTools", "platform-tools" },
            { "ENV003.component.cmdlineTools", "cmdline-tools (or tools)" },
            { "ENV003.component.separator", " or " },

            { "ENV004.missing.title", "SDK platform android-{0} is not installed" },
            { "ENV004.missing.detail", "Your target API level is {0}, but the SDK has no platform for it, so the build will fail. Install it with the command below, or lower the target API level." },
            { "ENV004.none.title", "No SDK platform is installed" },
            { "ENV004.none.detail", "The SDK has no platforms, so the build will fail. Install at least one platform with sdkmanager." },
            { "ENV004.evidence.path", "SDK path: {0}" },
            { "ENV004.evidence.installed", "Installed platforms: {0}" },
            { "ENV004.evidence.none", "none" },

            { "ENV005.title", "SDK build-tools {0} are not installed" },
            { "ENV005.detail", "Unity {0} ships with build-tools {1}, but the SDK has: {2}. Unity does not require an exact version, so this is informational." },
            { "ENV005.evidence.path", "SDK path: {0}" },
            { "ENV005.evidence.none", "none" },

            { "ENV006.missing.title", "NDK folder does not exist" },
            { "ENV006.missing.detail", "IL2CPP builds need the NDK, but the configured path does not exist. In Preferences > External Tools, tick 'Android NDK Installed with Unity (recommended)'." },
            { "ENV006.version.title", "NDK version differs from the one Unity expects" },
            { "ENV006.version.detail", "Unity {0} is tested with NDK {1}, but NDK {2} is configured. In Preferences > External Tools, tick 'Android NDK Installed with Unity (recommended)'." },
            { "ENV006.evidence.path", "NDK path: {0}" },
            { "ENV006.evidence.version", "NDK revision: {0}" },

            { "ENV007.missing.title", "Gradle folder does not exist" },
            { "ENV007.missing.detail", "The custom Gradle path configured in Unity does not exist. In Preferences > External Tools, tick 'Gradle Installed with Unity (recommended)'." },
            { "ENV007.version.title", "Custom Gradle version differs from the one Unity expects" },
            { "ENV007.version.detail", "Unity {0} is tested with Gradle {1}, but the configured Gradle is {2}. In Preferences > External Tools, tick 'Gradle Installed with Unity (recommended)'." },
            { "ENV007.evidence.path", "Gradle path: {0}" },
            { "ENV007.evidence.version", "Gradle version: {0}" },

            { "ENV008.sdkHome.title", "ANDROID_SDK_HOME is set" },
            { "ENV008.sdkHome.detail", "This variable is deprecated and breaks the Android Gradle plugin in real Unity builds. Remove it or fix it, then restart Unity Hub and Unity." },
            { "ENV008.other.title", "{0} is set" },
            { "ENV008.other.detail", "Gradle and the Android tools may use this variable instead of the paths configured in Unity. If a build picks the wrong SDK, JDK or NDK, remove it and restart Unity Hub and Unity." },
            { "ENV008.evidence", "{0}={1}" },

            { "ENV009.long.title", "Project path is very long" },
            { "ENV009.long.detail", "On Windows, paths over 200 characters often break Gradle and native builds. Move the project to a shorter path." },
            { "ENV009.long.evidence", "Path ({0} characters): {1}" },
            { "ENV009.ascii.title", "Project path has non-ASCII characters" },
            { "ENV009.ascii.detail", "Some Android build tools fail on paths with accents or other non-ASCII characters. If a build fails with a strange path error, move the project to a plain ASCII path." },
            { "ENV009.ascii.evidence", "Path: {0}" },

            { "PRJ001.title", "Target API level {0} is below the Google Play minimum ({1})" },
            { "PRJ001.detail", "Google Play requires apps to target API level {0} or higher. Raise the target API level in Player Settings > Other Settings." },
            { "PRJ001.evidence.target", "Target API level: {0}" },
            { "PRJ001.evidence.auto", "Target API level: Auto (highest installed platform: android-{0})" },
            { "PRJ001.evidence.minimum", "Google Play minimum: {0}" },
            { "PRJ001.evidence.deadline", "Google Play deadline: {0}" },
            { "PRJ001.fix", "Set target API to {0}" },

            { "PRJ002.title", "Minimum API level is below what Unity requires" },
            { "PRJ002.detail", "Unity {0} needs a minimum API level of at least {1}. Raise it in Player Settings > Other Settings." },
            { "PRJ002.evidence.min", "Minimum API level: {0}" },
            { "PRJ002.evidence.unityMin", "Unity minimum: {0}" },
            { "PRJ002.fix", "Set minimum API to {0}" },

            { "PRJ003.noArm64.title", "ARM64 is not enabled" },
            { "PRJ003.noArm64.detail", "Google Play requires 64-bit libraries. Enable ARM64 in Player Settings > Other Settings > Target Architectures; it needs the IL2CPP scripting backend." },
            { "PRJ003.mono.title", "The Mono scripting backend cannot build 64-bit" },
            { "PRJ003.mono.detail", "ARM64 builds need IL2CPP. Switch the scripting backend to IL2CPP in Player Settings > Other Settings." },
            { "PRJ003.evidence.architectures", "Target architectures: {0}" },
            { "PRJ003.evidence.backend", "Scripting backend: {0}" },
            { "PRJ003.fix", "Enable ARM64 and IL2CPP" },

            { "PRJ004.title", "Android App Bundle is off" },
            { "PRJ004.detail", "Google Play requires an Android App Bundle (.aab) for new apps. If you plan to publish, enable Build App Bundle in the Android build settings." },
            { "PRJ004.evidence", "Build App Bundle: off" },

            { "PRJ005.title", "Custom Gradle templates found" },
            { "PRJ005.detail.versions", "Unity {0} builds with Gradle {1} and Android Gradle plugin {2}. Custom templates replace Unity's defaults and can break after an upgrade. Compare them with the defaults in PlaybackEngines/AndroidPlayer/Tools/GradleTemplates/." },
            { "PRJ005.detail.generic", "Custom templates replace Unity's defaults and can break after an upgrade. Compare them with the defaults in PlaybackEngines/AndroidPlayer/Tools/GradleTemplates/." },

            { "PRJ006.title", "<{0}> {1} declares an intent filter but not android:exported" },
            { "PRJ006.detail", "Since Android 12 (API 31), a component with an intent filter must set android:exported explicitly, or the build fails. Add android:exported=\"true\" if other apps must start it, otherwise \"false\"." },
            { "PRJ006.evidence", "{0}, line {1}: <{2} android:name=\"{3}\">" },
            { "PRJ006.unnamed", "(no name)" },

            { "PRJ007.title", "{0} is in the project in more than one version" },
            { "PRJ007.detail", "These files bring the same library in versions {0}. The build can fail with duplicate classes. Remove the old one, or run the EDM4U resolver if you use it." },
            { "PRJ007.fix", "Select in Project" },

            { "PRJ008.title", "Release builds will be signed with the debug key" },
            { "PRJ008.detail", "Google Play rejects apps signed with the debug key. Set up a keystore in Player Settings > Publishing Settings and enable Custom Keystore." },
            { "PRJ008.evidence.keystore", "Custom keystore: off" },
            { "PRJ008.evidence.development", "Development build: off" },

            { "NAT001.title", "Native libraries from {0} are not 16 KB aligned ({1})" },
            { "NAT001.detail", "Google Play requires 64-bit native libraries to be aligned to 16 KB. Ask the vendor for a 16 KB-compatible build, or rebuild the library with -Wl,-z,max-page-size=16384." },
            { "NAT001.detail.package", "Google Play requires 64-bit native libraries to be aligned to 16 KB. Installed: {0} {1}. Update the package or ask the vendor for a version with 16 KB support." },
            { "NAT001.evidence", "{0} ({1}) · LOAD align 0x{2} · {3}" },
            { "NAT001.unreadable.title", "{0} native libraries could not be read" },
            { "NAT001.unreadable.detail", "Their ELF header could not be read, so they were not checked. Each one is listed with the reason." },
            { "NAT001.unreadable.evidence", "{0}: {1}" },
            { "NAT001.32bit.title", "{0} 32-bit libraries are not evaluated" },
            { "NAT001.32bit.detail", "Google Play does not require 32-bit libraries to be 16 KB aligned, so they are only counted." },

            { "NAT002.title", "Native libraries from {0} have a misaligned RELRO segment ({1})" },
            { "NAT002.detail", "The GNU_RELRO segment of these 64-bit libraries does not end on a 16 KB boundary, which Google's 16 KB guide lists as a requirement. Rebuild them with -Wl,-z,max-page-size=16384 -Wl,-z,common-page-size=16384, or with NDK r28 or newer." },
            { "NAT002.evidence", "{0} ({1}) · RELRO end 0x{2} · {3}" },

            { "NAT003.title", "Native libraries from {0} have no 64-bit version ({1})" },
            { "NAT003.detail", "Google Play requires 64-bit libraries. These ship only 32-bit ABIs, so a 64-bit build cannot include them. Ask the vendor for an arm64-v8a version." },
            { "NAT003.evidence", "{0}: {1}" },

            { "known.evidence.installed", "Installed: {0} {1}" },
            { "known.evidence.minimum", "Minimum version: {0}" },
            { "known.evidence.reason", "Reason: {0}" },
            { "NAT004.title", "Burst {0} is older than {1}" },
            { "NAT004.detail", "Older versions of {0} ship native libraries that are not 16 KB aligned. Update it in Window > Package Manager." },
            { "NAT005.title", "App Bundle package {0} is older than {1}" },
            { "NAT005.detail", "Older versions of {0} bundle tools that may not produce 16 KB aligned builds. This comes from a community report, not an official requirement. Update it in Window > Package Manager." },

            { "BLD001.title", "{0}: {1} libraries in your build are not 16 KB aligned" },
            { "BLD001.detail.known", "These libraries are inside the APK/AAB you built and fail Google Play's 16 KB check. Update the package or ask the vendor for a version with 16 KB support." },
            { "BLD001.detail.unknown", "These libraries are in your build but not in your project, so they probably come from a transitive Gradle dependency or from Unity itself. Find the dependency that brings them and update it." },
            { "BLD001.evidence", "{0} ({1}) · LOAD align 0x{2} · {3}" },
            { "BLD001.owner.unknown", "not found in your project" },
            { "BLD001.unreadable.title", "The build file could not be read" },
            { "BLD001.unreadable.detail", "Android Doctor could not open the APK or AAB, so the build libraries were not checked." },
            { "BLD001.unreadable.evidence", "{0}: {1}" },

            { "build.postbuild.ok", "Android Doctor: build checked, no 16 KB problems." },
            { "build.postbuild.warning", "Android Doctor: {0} native libraries in your build are not 16 KB aligned. Open Window > Android Doctor." },
            { "build.postbuild.failed", "Android Doctor could not check the build: {0}" },

            // Fixes
            { "fix.dialog.title", "Apply fix?" },
            { "fix.dialog.apply", "Apply" },
            { "fix.dialog.cancel", "Cancel" },
            { "fix.apiLevel.unsupported", "This Unity version has no API level {0}." },
            { "fix.targetApi.describe", "Target API level: {0} -> {1}" },
            { "fix.targetApi.unknown", "The Google Play minimum target API is unknown." },
            { "fix.targetApi.done", "Target API level set to {0}." },
            { "fix.minApi.describe", "Minimum API level: {0} -> {1}" },
            { "fix.minApi.unknown", "The minimum API level Unity requires is unknown." },
            { "fix.minApi.done", "Minimum API level set to {0}." },
            { "fix.arm64.describe", "Target architectures: {0} -> {1}\nScripting backend: {2} -> {3}" },
            { "fix.arm64.done", "ARM64 and IL2CPP enabled." },
            { "fix.externalTools.describe", "Opens Preferences > External Tools. Nothing is changed." },
            { "fix.externalTools.done", "Opened External Tools." },
            { "fix.selectAsset.describe", "Selects {0} in the Project window. Nothing is changed." },
            { "fix.selectAsset.notFound", "Asset not found: {0}" },
            { "fix.selectAsset.done", "Selected {0}." },

            // Rule runner
            { "runner.ruleFailed.title", "Rule {0} failed internally" },

            // Window
            { "ui.window.title", "Android Doctor" },
            { "ui.header", "Android Doctor {0}" },
            { "ui.subheader", "Unity {0}" },
            { "ui.run", "Run checks" },
            { "ui.fix", "Fix..." },
            { "ui.scanBuild", "Scan build..." },
            { "ui.scanBuild.panel", "Select an APK or AAB" },
            { "ui.build.current", "Build file: {0}" },
            { "ui.idle", "Press Run checks." },
            { "ui.summary", "{0} errors, {1} warnings, {2} info" },
            { "ui.noFindings", "No findings. Everything looks fine." },
            { "ui.error.title", "Android Doctor hit an internal error" },
            { "ui.error.copy", "Copy error" },
            { "ui.progress.title", "Android Doctor" },
        };

        static readonly Dictionary<string, string> Es = new Dictionary<string, string>
        {
            { "PRJ009.title", "Identificador de aplicación por defecto" },
            { "PRJ009.detail", "El identificador empieza por com.DefaultCompany. No se puede cambiar después de publicar en Google Play." },
            { "PRJ009.evidence", "Identificador de aplicación: {0}" },

            { "ENV001.title", "Android Build Support no está instalado" },
            { "ENV001.detail", "Unity no puede compilar para Android porque este editor no tiene el módulo de Android. Abre Unity Hub > Installs > tu editor > Add modules e instala Android Build Support, OpenJDK y Android SDK & NDK Tools." },
            { "ENV001.evidence", "Unity {0}" },

            { "ENV002.missing.title", "La carpeta del JDK no existe" },
            { "ENV002.missing.detail", "La ruta del JDK configurada en Unity no existe, así que Gradle no puede ejecutarse. En Preferences > External Tools, marca «JDK Installed with Unity (recommended)»." },
            { "ENV002.version.title", "La versión del JDK no es la que espera Unity" },
            { "ENV002.version.detail", "Unity {0} se prueba con JDK {1}, pero hay un JDK propio {2}. Una versión mayor distinta puede romper Gradle. En Preferences > External Tools, marca «JDK Installed with Unity (recommended)»." },
            { "ENV002.evidence.path", "Ruta del JDK: {0}" },
            { "ENV002.evidence.version", "Versión del JDK: {0}" },
            { "ENV002.fix", "Abrir External Tools" },

            { "ENV003.missing.title", "La carpeta del SDK de Android no existe" },
            { "ENV003.missing.detail", "La ruta del SDK configurada en Unity no existe, así que el build no encuentra las herramientas de Android. En Preferences > External Tools, marca «Android SDK Tools Installed with Unity (recommended)»." },
            { "ENV003.incomplete.title", "Al SDK de Android le faltan componentes" },
            { "ENV003.incomplete.detail", "El SDK no tiene {0}, que el build necesita. Usa el SDK que instala Unity: Preferences > External Tools > «Android SDK Tools Installed with Unity (recommended)»." },
            { "ENV003.evidence.path", "Ruta del SDK: {0}" },
            { "ENV003.component.platformTools", "platform-tools" },
            { "ENV003.component.cmdlineTools", "cmdline-tools (o tools)" },
            { "ENV003.component.separator", " ni " },

            { "ENV004.missing.title", "La plataforma android-{0} del SDK no está instalada" },
            { "ENV004.missing.detail", "Tu target API level es {0}, pero el SDK no tiene esa plataforma y el build fallará. Instálala con el comando de abajo o baja el target API level." },
            { "ENV004.none.title", "No hay ninguna plataforma del SDK instalada" },
            { "ENV004.none.detail", "El SDK no tiene plataformas y el build fallará. Instala al menos una plataforma con sdkmanager." },
            { "ENV004.evidence.path", "Ruta del SDK: {0}" },
            { "ENV004.evidence.installed", "Plataformas instaladas: {0}" },
            { "ENV004.evidence.none", "ninguna" },

            { "ENV005.title", "Las build-tools {0} del SDK no están instaladas" },
            { "ENV005.detail", "Unity {0} trae build-tools {1}, pero el SDK tiene: {2}. Unity no exige una versión exacta, así que es solo informativo." },
            { "ENV005.evidence.path", "Ruta del SDK: {0}" },
            { "ENV005.evidence.none", "ninguna" },

            { "ENV006.missing.title", "La carpeta del NDK no existe" },
            { "ENV006.missing.detail", "Los builds con IL2CPP necesitan el NDK, pero la ruta configurada no existe. En Preferences > External Tools, marca «Android NDK Installed with Unity (recommended)»." },
            { "ENV006.version.title", "La versión del NDK no es la que espera Unity" },
            { "ENV006.version.detail", "Unity {0} se prueba con el NDK {1}, pero hay configurado el NDK {2}. En Preferences > External Tools, marca «Android NDK Installed with Unity (recommended)»." },
            { "ENV006.evidence.path", "Ruta del NDK: {0}" },
            { "ENV006.evidence.version", "Revisión del NDK: {0}" },

            { "ENV007.missing.title", "La carpeta de Gradle no existe" },
            { "ENV007.missing.detail", "La ruta de Gradle propio configurada en Unity no existe. En Preferences > External Tools, marca «Gradle Installed with Unity (recommended)»." },
            { "ENV007.version.title", "La versión de Gradle propio no es la que espera Unity" },
            { "ENV007.version.detail", "Unity {0} se prueba con Gradle {1}, pero el Gradle configurado es {2}. En Preferences > External Tools, marca «Gradle Installed with Unity (recommended)»." },
            { "ENV007.evidence.path", "Ruta de Gradle: {0}" },
            { "ENV007.evidence.version", "Versión de Gradle: {0}" },

            { "ENV008.sdkHome.title", "ANDROID_SDK_HOME está definida" },
            { "ENV008.sdkHome.detail", "Esta variable está obsoleta y rompe el plugin de Android de Gradle en builds reales de Unity. Quítala o corrígela y reinicia Unity Hub y Unity." },
            { "ENV008.other.title", "{0} está definida" },
            { "ENV008.other.detail", "Gradle y las herramientas de Android pueden usar esta variable en lugar de las rutas configuradas en Unity. Si un build usa el SDK, JDK o NDK equivocado, quítala y reinicia Unity Hub y Unity." },
            { "ENV008.evidence", "{0}={1}" },

            { "ENV009.long.title", "La ruta del proyecto es muy larga" },
            { "ENV009.long.detail", "En Windows, las rutas de más de 200 caracteres suelen romper Gradle y los builds nativos. Mueve el proyecto a una ruta más corta." },
            { "ENV009.long.evidence", "Ruta ({0} caracteres): {1}" },
            { "ENV009.ascii.title", "La ruta del proyecto tiene caracteres no ASCII" },
            { "ENV009.ascii.detail", "Algunas herramientas de Android fallan con rutas con tildes u otros caracteres no ASCII. Si un build falla con un error de ruta extraño, mueve el proyecto a una ruta solo con ASCII." },
            { "ENV009.ascii.evidence", "Ruta: {0}" },

            { "PRJ001.title", "El target API level {0} está por debajo del mínimo de Google Play ({1})" },
            { "PRJ001.detail", "Google Play exige que las apps apunten al API level {0} o superior. Sube el target API level en Player Settings > Other Settings." },
            { "PRJ001.evidence.target", "Target API level: {0}" },
            { "PRJ001.evidence.auto", "Target API level: Auto (plataforma más alta instalada: android-{0})" },
            { "PRJ001.evidence.minimum", "Mínimo de Google Play: {0}" },
            { "PRJ001.evidence.deadline", "Fecha límite de Google Play: {0}" },
            { "PRJ001.fix", "Poner target API {0}" },

            { "PRJ002.title", "El API level mínimo está por debajo de lo que exige Unity" },
            { "PRJ002.detail", "Unity {0} necesita un API level mínimo de al menos {1}. Súbelo en Player Settings > Other Settings." },
            { "PRJ002.evidence.min", "API level mínimo: {0}" },
            { "PRJ002.evidence.unityMin", "Mínimo de Unity: {0}" },
            { "PRJ002.fix", "Poner API mínimo {0}" },

            { "PRJ003.noArm64.title", "ARM64 no está activado" },
            { "PRJ003.noArm64.detail", "Google Play exige librerías de 64 bits. Activa ARM64 en Player Settings > Other Settings > Target Architectures; necesita el backend de scripting IL2CPP." },
            { "PRJ003.mono.title", "El backend de scripting Mono no puede compilar 64 bits" },
            { "PRJ003.mono.detail", "Los builds ARM64 necesitan IL2CPP. Cambia el backend de scripting a IL2CPP en Player Settings > Other Settings." },
            { "PRJ003.evidence.architectures", "Arquitecturas de destino: {0}" },
            { "PRJ003.evidence.backend", "Backend de scripting: {0}" },
            { "PRJ003.fix", "Activar ARM64 e IL2CPP" },

            { "PRJ004.title", "Android App Bundle está desactivado" },
            { "PRJ004.detail", "Google Play exige un Android App Bundle (.aab) para apps nuevas. Si vas a publicar, activa Build App Bundle en los ajustes de build de Android." },
            { "PRJ004.evidence", "Build App Bundle: desactivado" },

            { "PRJ005.title", "Hay plantillas Gradle personalizadas" },
            { "PRJ005.detail.versions", "Unity {0} compila con Gradle {1} y el plugin de Android de Gradle {2}. Las plantillas propias sustituyen las de Unity y pueden romperse tras una actualización. Compáralas con las de PlaybackEngines/AndroidPlayer/Tools/GradleTemplates/." },
            { "PRJ005.detail.generic", "Las plantillas propias sustituyen las de Unity y pueden romperse tras una actualización. Compáralas con las de PlaybackEngines/AndroidPlayer/Tools/GradleTemplates/." },

            { "PRJ006.title", "<{0}> {1} declara un intent filter pero no android:exported" },
            { "PRJ006.detail", "Desde Android 12 (API 31), un componente con intent filter debe indicar android:exported, o el build falla. Añade android:exported=\"true\" si otras apps deben poder iniciarlo y, si no, \"false\"." },
            { "PRJ006.evidence", "{0}, línea {1}: <{2} android:name=\"{3}\">" },
            { "PRJ006.unnamed", "(sin nombre)" },

            { "PRJ007.title", "{0} está en el proyecto en más de una versión" },
            { "PRJ007.detail", "Estos archivos traen la misma librería en las versiones {0}. El build puede fallar por clases duplicadas. Quita la antigua o ejecuta el resolvedor de EDM4U si lo usas." },
            { "PRJ007.fix", "Seleccionar en Project" },

            { "PRJ008.title", "Los builds de release se firmarán con la clave de debug" },
            { "PRJ008.detail", "Google Play rechaza las apps firmadas con la clave de debug. Configura una keystore en Player Settings > Publishing Settings y activa Custom Keystore." },
            { "PRJ008.evidence.keystore", "Custom keystore: desactivada" },
            { "PRJ008.evidence.development", "Development build: desactivado" },

            { "NAT001.title", "Librerías nativas de {0} no alineadas a 16 KB ({1})" },
            { "NAT001.detail", "Google Play exige que las librerías nativas de 64 bits estén alineadas a 16 KB. Pide al proveedor una versión compatible con 16 KB o recompila la librería con -Wl,-z,max-page-size=16384." },
            { "NAT001.detail.package", "Google Play exige que las librerías nativas de 64 bits estén alineadas a 16 KB. Instalado: {0} {1}. Actualiza el paquete o pide al proveedor una versión con soporte de 16 KB." },
            { "NAT001.evidence", "{0} ({1}) · LOAD align 0x{2} · {3}" },
            { "NAT001.unreadable.title", "No se pudieron leer {0} librerías nativas" },
            { "NAT001.unreadable.detail", "No se pudo leer su cabecera ELF, así que no se comprobaron. Cada una aparece con el motivo." },
            { "NAT001.unreadable.evidence", "{0}: {1}" },
            { "NAT001.32bit.title", "{0} librerías de 32 bits no se evalúan" },
            { "NAT001.32bit.detail", "Google Play no exige que las librerías de 32 bits estén alineadas a 16 KB, así que solo se cuentan." },

            { "NAT002.title", "Librerías nativas de {0} con el segmento RELRO desalineado ({1})" },
            { "NAT002.detail", "El segmento GNU_RELRO de estas librerías de 64 bits no termina en un límite de 16 KB, que la guía de 16 KB de Google lista como requisito. Recompílalas con -Wl,-z,max-page-size=16384 -Wl,-z,common-page-size=16384 o con NDK r28 o posterior." },
            { "NAT002.evidence", "{0} ({1}) · fin de RELRO 0x{2} · {3}" },

            { "NAT003.title", "Librerías nativas de {0} sin versión de 64 bits ({1})" },
            { "NAT003.detail", "Google Play exige librerías de 64 bits. Estas solo traen ABIs de 32 bits, así que un build de 64 bits no puede incluirlas. Pide al proveedor una versión arm64-v8a." },
            { "NAT003.evidence", "{0}: {1}" },

            { "known.evidence.installed", "Instalado: {0} {1}" },
            { "known.evidence.minimum", "Versión mínima: {0}" },
            { "known.evidence.reason", "Motivo: {0}" },
            { "NAT004.title", "Burst {0} es anterior a {1}" },
            { "NAT004.detail", "Las versiones antiguas de {0} traen librerías nativas que no están alineadas a 16 KB. Actualízalo en Window > Package Manager." },
            { "NAT005.title", "El paquete de App Bundle {0} es anterior a {1}" },
            { "NAT005.detail", "Las versiones antiguas de {0} incluyen herramientas que pueden no generar builds alineados a 16 KB. Es un dato de la comunidad, no un requisito oficial. Actualízalo en Window > Package Manager." },

            { "BLD001.title", "{0}: {1} librerías de tu build no están alineadas a 16 KB" },
            { "BLD001.detail.known", "Estas librerías están dentro del APK/AAB que has compilado y no pasan la comprobación de 16 KB de Google Play. Actualiza el paquete o pide al proveedor una versión con soporte de 16 KB." },
            { "BLD001.detail.unknown", "Estas librerías están en tu build pero no en tu proyecto, así que probablemente llegan de una dependencia transitiva de Gradle o de Unity. Busca la dependencia que las trae y actualízala." },
            { "BLD001.evidence", "{0} ({1}) · LOAD align 0x{2} · {3}" },
            { "BLD001.owner.unknown", "no está en tu proyecto" },
            { "BLD001.unreadable.title", "No se pudo leer el archivo del build" },
            { "BLD001.unreadable.detail", "Android Doctor no pudo abrir el APK o AAB, así que no se comprobaron las librerías del build." },
            { "BLD001.unreadable.evidence", "{0}: {1}" },

            { "build.postbuild.ok", "Android Doctor: build comprobado, sin problemas de 16 KB." },
            { "build.postbuild.warning", "Android Doctor: {0} librerías nativas de tu build no están alineadas a 16 KB. Abre Window > Android Doctor." },
            { "build.postbuild.failed", "Android Doctor no pudo comprobar el build: {0}" },

            { "fix.dialog.title", "¿Aplicar el arreglo?" },
            { "fix.dialog.apply", "Aplicar" },
            { "fix.dialog.cancel", "Cancelar" },
            { "fix.apiLevel.unsupported", "Esta versión de Unity no tiene el API level {0}." },
            { "fix.targetApi.describe", "Target API level: {0} -> {1}" },
            { "fix.targetApi.unknown", "Se desconoce el target API mínimo de Google Play." },
            { "fix.targetApi.done", "Target API level puesto a {0}." },
            { "fix.minApi.describe", "API level mínimo: {0} -> {1}" },
            { "fix.minApi.unknown", "Se desconoce el API level mínimo que exige Unity." },
            { "fix.minApi.done", "API level mínimo puesto a {0}." },
            { "fix.arm64.describe", "Arquitecturas de destino: {0} -> {1}\nBackend de scripting: {2} -> {3}" },
            { "fix.arm64.done", "ARM64 e IL2CPP activados." },
            { "fix.externalTools.describe", "Abre Preferences > External Tools. No se cambia nada." },
            { "fix.externalTools.done", "External Tools abierto." },
            { "fix.selectAsset.describe", "Selecciona {0} en la ventana Project. No se cambia nada." },
            { "fix.selectAsset.notFound", "No se encuentra el asset: {0}" },
            { "fix.selectAsset.done", "Seleccionado {0}." },

            { "runner.ruleFailed.title", "La regla {0} falló internamente" },

            { "ui.header", "Android Doctor {0}" },
            { "ui.subheader", "Unity {0}" },
            { "ui.run", "Ejecutar comprobaciones" },
            { "ui.fix", "Arreglar..." },
            { "ui.scanBuild", "Analizar build..." },
            { "ui.scanBuild.panel", "Selecciona un APK o AAB" },
            { "ui.build.current", "Archivo de build: {0}" },
            { "ui.idle", "Pulsa «Ejecutar comprobaciones»." },
            { "ui.summary", "{0} errores, {1} avisos, {2} info" },
            { "ui.noFindings", "Sin hallazgos. Todo parece correcto." },
            { "ui.error.title", "Android Doctor tuvo un error interno" },
            { "ui.error.copy", "Copiar error" },
        };

        public static string Get(string key, params object[] args)
        {
            string s;
            bool es = Application.systemLanguage == SystemLanguage.Spanish;
            if (!(es && Es.TryGetValue(key, out s)) && !En.TryGetValue(key, out s)) s = key;
            return args == null || args.Length == 0 ? s : string.Format(s, args);
        }
    }
}
