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

            // Rule runner
            { "runner.ruleFailed.title", "Rule {0} failed internally" },

            // Window
            { "ui.window.title", "Android Doctor" },
            { "ui.header", "Android Doctor {0}" },
            { "ui.subheader", "Unity {0}" },
            { "ui.run", "Run checks" },
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

            { "runner.ruleFailed.title", "La regla {0} falló internamente" },

            { "ui.header", "Android Doctor {0}" },
            { "ui.subheader", "Unity {0}" },
            { "ui.run", "Ejecutar comprobaciones" },
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
