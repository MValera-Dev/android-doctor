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
