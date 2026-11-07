using System.Collections.Generic;

namespace AndroidDoctor
{
    // The texts of the one-click fixes and of the rules that offer them. Same rules as Messages.cs:
    // English first, Spanish second, a missing Spanish key falls back to English.
    public static partial class Messages
    {
        static readonly Dictionary<string, string> EnFixes = new Dictionary<string, string>
        {
            // Window
            { "ui.checkProject", "Check project" },
            { "ui.checkAgain", "Check again" },
            { "ui.verdict.idle.title", "Check your project" },
            { "ui.verdict.idle.sub", "Looks at your Android setup, your Player Settings and your last build. Nothing is changed until you say so." },
            { "ui.verdict.error.one", "1 problem will stop your build" },
            { "ui.verdict.error.many", "{0} problems will stop your build" },
            { "ui.verdict.fixable", "{0} can be fixed with one click." },
            { "ui.verdict.manual", "These need a decision from you." },
            { "ui.verdict.alsoWarnings", "{0} more worth fixing." },
            { "ui.verdict.warn", "Ready to build, but {0} thing(s) are worth fixing" },
            { "ui.verdict.ok.title", "Ready to build" },
            { "ui.verdict.ok.none", "No problems found." },
            { "ui.verdict.ok.notes", "{0} note(s) for your information." },
            { "ui.group.errors", "Stops your build ({0})" },
            { "ui.group.warnings", "Worth fixing ({0})" },
            { "ui.group.notes", "Notes ({0})" },

            // Fix all
            { "fix.all.header", "Android Doctor will make {0} change(s):" },
            { "fix.all.failed", "Could not apply:" },
            { "fix.all.done", "Applied {0} of {1} change(s)." },
            { "ui.fixAll", "Fix all ({0})" },

            // Tools installed with Unity
            { "fix.unityTools.button", "Use the {0} installed with Unity" },
            { "fix.unityTools.line", "{0}: {1}\n   -> the one installed with Unity ({2})" },
            { "fix.unityTools.nothing", "Everything already uses the tools installed with Unity." },
            { "fix.unityTools.done", "Now using the tools installed with Unity: {0}." },
            { "fix.unityTools.unavailable", "This Unity version does not expose its Android tool settings." },
            { "fix.externalTools.button", "Open External Tools" },

            // Gradle heap
            { "ENV010.title", "Gradle only gets {0} MB of memory" },
            { "ENV010.detail", "Gradle runs out of memory on larger projects and the build stops with \"Java heap space\". Unity passes this limit to Gradle." },
            { "ENV010.evidence", "Maximum JVM heap size: {0} MB" },
            { "ENV010.fix", "Give Gradle more memory" },
            { "fix.heap.describe", "Maximum JVM heap size (Preferences > External Tools): {0} MB -> {1} MB" },
            { "fix.heap.already", "The heap is already {0} MB, the most this machine can give." },
            { "fix.heap.done", "Gradle heap raised from {0} MB to {1} MB." },

            // Environment variables
            { "ENV008.fix", "Ignore {0} while Unity is open" },
            { "fix.env.unknown", "No variable was named." },
            { "fix.env.describe", "Ignore the environment variable {0} ({1}) while Unity is open.\nThe variable itself stays on your computer; it only stops reaching Gradle." },
            { "fix.env.notSet", "{0} is not set." },
            { "fix.env.done", "{0} is ignored until you close Unity. To remove it for good, delete it in Windows > Environment Variables and restart Unity Hub." },

            // App Bundle
            { "PRJ004.fix", "Turn on Build App Bundle" },
            { "fix.appBundle.describe", "Build App Bundle: off -> on\nThe build produces an .aab for Google Play instead of an .apk you can install on a device." },
            { "fix.appBundle.done", "Build App Bundle is on." },

            // Packages
            { "known.fix.update", "Update the package" },
            { "known.fix.open", "Open Package Manager" },
            { "fix.package.unknown", "No package was named." },
            { "fix.package.describe", "Package Manager will update {0} to the version Unity recommends for this Editor." },
            { "fix.package.started", "Updating {0}. Unity recompiles when it finishes; run the checks again after that." },
            { "fix.package.finished", "Android Doctor: {0} is now {1}." },
            { "fix.package.failed", "Android Doctor: could not update {0}. {1}" },
            { "fix.openPackage.describe", "Opens Package Manager on the package. Nothing is changed." },
            { "fix.openPackage.done", "Opened Package Manager." },
            { "fix.openSettings.describe", "Opens Project Settings. Nothing is changed." },
            { "fix.openSettings.done", "Opened Project Settings." },
            { "ui.openPlayerSettings", "Open Player Settings" },

            // Duplicate libraries
            { "PRJ007.keepNewest", "Keep {0} (newest), turn off the others" },
            { "PRJ007.keep", "Keep {0}, turn off the others" },
            { "fix.disablePlugin.unknown", "No plugin file was named." },
            { "fix.disablePlugin.describe", "Turn these files off for Android (they stay in the project):\n{0}\nTo undo it: select the file, then Inspector > Select platforms for plugin > Android." },
            { "fix.disablePlugin.done", "Turned off {0} file(s) for Android: {1}." },
            { "fix.disablePlugin.notFound", "Not found in the project: {0}" },

            // android:exported
            { "PRJ006.fix", "Add android:exported" },
            { "fix.exported.unknown", "The manifest was not found." },
            { "fix.exported.describe", "Add android:exported in {0}:\n{1}\nThe original is saved in Library/AndroidDoctor/Backups first." },
            { "fix.exported.reason.launcher", "the app's launcher" },
            { "fix.exported.reason.external", "other apps or links open it" },
            { "fix.exported.reason.private", "only the system calls it" },
            { "fix.exported.cannotEdit", "{0} could not be edited safely. Add android:exported by hand." },
            { "fix.exported.nothing", "{0} has nothing to change." },
            { "fix.exported.done", "Added android:exported to {0} component(s) in {1}. Original saved in {2}." },

            // Gradle templates and properties
            { "PRJ005.fix", "Back up and remove the custom templates" },
            { "fix.templates.unknown", "No template was named." },
            { "fix.templates.describe", "Remove these templates so Unity uses its own (copies are saved first):\n{0}" },
            { "fix.templates.done", "Removed {0}. Copies are in {1}." },
            { "PRJ010.title", "gradleTemplate.properties uses settings Gradle no longer accepts" },
            { "PRJ010.detail", "Unity 6 uses a newer Android Gradle plugin that removed these settings, so the build fails with \"Failed to apply plugin\". They usually come from a project created in Unity 2022." },
            { "PRJ010.fix", "Update the settings" },
            { "PRJ010.reset", "Use Unity's template instead" },
            { "fix.gradleProps.unknown", "gradleTemplate.properties was not found." },
            { "fix.gradleProps.describe", "In {0}, replace the removed setting with android.useFullClasspathForDexingTransform=true:\n{1}\nThe original is saved first." },
            { "fix.gradleProps.nothing", "{0} has nothing to change." },
            { "fix.gradleProps.done", "Updated {0} setting(s) in {1}. Original saved in {2}." },

            // Gradle errors
            { "GRD006.off", "Turn off {0}" },
            { "GRD006.resolver", "Reset the Android Resolver (EDM4U)" },
            { "GRD011.fix", "Set the minimum API level to {0}" },
            { "GRD000.clean", "Clean the generated Android project" },
            { "fix.resolver.describe", "Run the External Dependency Manager's \"Delete Resolved Libraries\" and then \"Force Resolve\". It downloads the libraries again, so it needs your connection." },
            { "fix.resolver.missing", "The External Dependency Manager menu was not found." },
            { "fix.resolver.done", "Asked the External Dependency Manager to resolve the libraries again." },

            // Build cache
            { "fix.cleanCache.describe", "Delete Unity's generated Android Gradle project (Library/Bee/Android and Temp/gradleOut). Unity creates it again on the next build; nothing of yours is touched." },
            { "fix.cleanCache.nothing", "There was no generated Android project to delete." },
            { "fix.cleanCache.done", "Deleted {0} folder(s). The next build starts clean." },
            { "fix.cleanCache.failed", "Could not delete (close other programs using the project):\n{0}" },
        };

        static readonly Dictionary<string, string> EsFixes = new Dictionary<string, string>
        {
            { "ui.checkProject", "Revisar proyecto" },
            { "ui.checkAgain", "Revisar otra vez" },
            { "ui.verdict.idle.title", "Revisa tu proyecto" },
            { "ui.verdict.idle.sub", "Mira tu configuración de Android, los Player Settings y tu último build. No se cambia nada hasta que tú lo digas." },
            { "ui.verdict.error.one", "1 problema va a parar tu build" },
            { "ui.verdict.error.many", "{0} problemas van a parar tu build" },
            { "ui.verdict.fixable", "{0} se pueden arreglar con un clic." },
            { "ui.verdict.manual", "Esto necesita una decisión tuya." },
            { "ui.verdict.alsoWarnings", "Y {0} más que conviene arreglar." },
            { "ui.verdict.warn", "Listo para compilar, pero conviene arreglar {0} cosa(s)" },
            { "ui.verdict.ok.title", "Listo para compilar" },
            { "ui.verdict.ok.none", "No se han encontrado problemas." },
            { "ui.verdict.ok.notes", "{0} nota(s) informativa(s)." },
            { "ui.group.errors", "Paran tu build ({0})" },
            { "ui.group.warnings", "Conviene arreglar ({0})" },
            { "ui.group.notes", "Notas ({0})" },

            { "fix.all.header", "Android Doctor va a hacer {0} cambio(s):" },
            { "fix.all.failed", "No se pudo aplicar:" },
            { "fix.all.done", "Aplicados {0} de {1} cambio(s)." },
            { "ui.fixAll", "Arreglar todo ({0})" },

            { "fix.unityTools.button", "Usar el {0} que instala Unity" },
            { "fix.unityTools.line", "{0}: {1}\n   -> el que instala Unity ({2})" },
            { "fix.unityTools.nothing", "Todo ya usa las herramientas que instala Unity." },
            { "fix.unityTools.done", "Ahora se usan las herramientas que instala Unity: {0}." },
            { "fix.unityTools.unavailable", "Esta versión de Unity no expone los ajustes de las herramientas de Android." },
            { "fix.externalTools.button", "Abrir External Tools" },

            { "ENV010.title", "Gradle solo dispone de {0} MB de memoria" },
            { "ENV010.detail", "Gradle se queda sin memoria en proyectos grandes y el build se para con «Java heap space». Unity le pasa este límite." },
            { "ENV010.evidence", "Maximum JVM heap size: {0} MB" },
            { "ENV010.fix", "Dar más memoria a Gradle" },
            { "fix.heap.describe", "Maximum JVM heap size (Preferences > External Tools): {0} MB -> {1} MB" },
            { "fix.heap.already", "El heap ya es de {0} MB, lo máximo que puede dar este equipo." },
            { "fix.heap.done", "Memoria de Gradle subida de {0} MB a {1} MB." },

            { "ENV008.fix", "Ignorar {0} mientras Unity esté abierto" },
            { "fix.env.unknown", "No se indicó ninguna variable." },
            { "fix.env.describe", "Ignorar la variable de entorno {0} ({1}) mientras Unity esté abierto.\nLa variable sigue en tu equipo; solo deja de llegar a Gradle." },
            { "fix.env.notSet", "{0} no está definida." },
            { "fix.env.done", "{0} se ignora hasta que cierres Unity. Para quitarla del todo, bórrala en Windows > Variables de entorno y reinicia Unity Hub." },

            { "PRJ004.fix", "Activar Build App Bundle" },
            { "fix.appBundle.describe", "Build App Bundle: desactivado -> activado\nEl build genera un .aab para Google Play en vez de un .apk que puedas instalar en el móvil." },
            { "fix.appBundle.done", "Build App Bundle activado." },

            { "known.fix.update", "Actualizar el paquete" },
            { "known.fix.open", "Abrir Package Manager" },
            { "fix.package.unknown", "No se indicó ningún paquete." },
            { "fix.package.describe", "Package Manager actualizará {0} a la versión que Unity recomienda para este Editor." },
            { "fix.package.started", "Actualizando {0}. Unity recompila al terminar; ejecuta las comprobaciones otra vez después." },
            { "fix.package.finished", "Android Doctor: {0} ahora es {1}." },
            { "fix.package.failed", "Android Doctor: no se pudo actualizar {0}. {1}" },
            { "fix.openPackage.describe", "Abre Package Manager en el paquete. No se cambia nada." },
            { "fix.openPackage.done", "Package Manager abierto." },
            { "fix.openSettings.describe", "Abre Project Settings. No se cambia nada." },
            { "fix.openSettings.done", "Project Settings abierto." },
            { "ui.openPlayerSettings", "Abrir Player Settings" },

            { "PRJ007.keepNewest", "Quedarse con la {0} (la más nueva) y desactivar las demás" },
            { "PRJ007.keep", "Quedarse con la {0} y desactivar las demás" },
            { "fix.disablePlugin.unknown", "No se indicó ningún archivo de plugin." },
            { "fix.disablePlugin.describe", "Desactivar estos archivos para Android (siguen en el proyecto):\n{0}\nPara deshacerlo: selecciona el archivo y, en el Inspector, marca Android en «Select platforms for plugin»." },
            { "fix.disablePlugin.done", "Desactivados {0} archivo(s) para Android: {1}." },
            { "fix.disablePlugin.notFound", "No están en el proyecto: {0}" },

            { "PRJ006.fix", "Añadir android:exported" },
            { "fix.exported.unknown", "No se encontró el manifest." },
            { "fix.exported.describe", "Añadir android:exported en {0}:\n{1}\nAntes se guarda el original en Library/AndroidDoctor/Backups." },
            { "fix.exported.reason.launcher", "lanzador de la app" },
            { "fix.exported.reason.external", "otras apps o enlaces lo abren" },
            { "fix.exported.reason.private", "solo lo llama el sistema" },
            { "fix.exported.cannotEdit", "{0} no se pudo editar con seguridad. Añade android:exported a mano." },
            { "fix.exported.nothing", "{0} no tiene nada que cambiar." },
            { "fix.exported.done", "Añadido android:exported a {0} componente(s) en {1}. Original guardado en {2}." },

            { "PRJ005.fix", "Guardar copia y quitar las plantillas personalizadas" },
            { "fix.templates.unknown", "No se indicó ninguna plantilla." },
            { "fix.templates.describe", "Quitar estas plantillas para que Unity use las suyas (antes se guarda una copia):\n{0}" },
            { "fix.templates.done", "Quitadas: {0}. Las copias están en {1}." },
            { "PRJ010.title", "gradleTemplate.properties usa ajustes que Gradle ya no acepta" },
            { "PRJ010.detail", "Unity 6 usa un plugin de Android Gradle más nuevo que retiró estos ajustes, así que el build falla con «Failed to apply plugin». Suelen venir de un proyecto creado en Unity 2022." },
            { "PRJ010.fix", "Actualizar los ajustes" },
            { "PRJ010.reset", "Usar la plantilla de Unity" },
            { "fix.gradleProps.unknown", "No se encontró gradleTemplate.properties." },
            { "fix.gradleProps.describe", "En {0}, cambiar el ajuste retirado por android.useFullClasspathForDexingTransform=true:\n{1}\nAntes se guarda el original." },
            { "fix.gradleProps.nothing", "{0} no tiene nada que cambiar." },
            { "fix.gradleProps.done", "Actualizados {0} ajuste(s) en {1}. Original guardado en {2}." },

            { "GRD006.off", "Desactivar {0}" },
            { "GRD006.resolver", "Reiniciar el Android Resolver (EDM4U)" },
            { "GRD011.fix", "Poner el API level mínimo en {0}" },
            { "GRD000.clean", "Limpiar el proyecto de Android generado" },
            { "fix.resolver.describe", "Ejecutar «Delete Resolved Libraries» y después «Force Resolve» del External Dependency Manager. Vuelve a descargar las librerías, así que necesita tu conexión." },
            { "fix.resolver.missing", "No se encontró el menú del External Dependency Manager." },
            { "fix.resolver.done", "Se pidió al External Dependency Manager que resuelva las librerías otra vez." },

            { "fix.cleanCache.describe", "Borrar el proyecto Gradle de Android que genera Unity (Library/Bee/Android y Temp/gradleOut). Unity lo crea de nuevo en el siguiente build; no se toca nada tuyo." },
            { "fix.cleanCache.nothing", "No había ningún proyecto de Android generado que borrar." },
            { "fix.cleanCache.done", "Borradas {0} carpeta(s). El siguiente build empieza limpio." },
            { "fix.cleanCache.failed", "No se pudo borrar (cierra otros programas que usen el proyecto):\n{0}" },
        };
    }
}
