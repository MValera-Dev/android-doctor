using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace AndroidDoctor.Project
{
    public sealed class ManifestComponent
    {
        public string Tag;   // "activity", "service" or "receiver"
        public string Name;  // android:name, or "" if absent
        public int Line;     // 1-based line of the element, 0 if unknown
        public bool SuggestedExported; // the value a fix would write
        public string Reason;          // "launcher", "external" or "private": why that value
    }

    /// <summary>
    /// Finds manifest components that declare an intent filter but no android:exported. Since Android 12
    /// (API 31) that combination fails the build. Pure: it only reads the text it is given.
    /// </summary>
    public static class ManifestExportedChecker
    {
        static readonly XNamespace Android = "http://schemas.android.com/apk/res/android";
        static readonly string[] Tags = { "activity", "service", "receiver" };

        /// <summary>Components missing android:exported. Empty if the text is not valid XML.</summary>
        public static List<ManifestComponent> FindMissing(string manifestXml)
        {
            var result = new List<ManifestComponent>();
            if (string.IsNullOrEmpty(manifestXml)) return result;

            XDocument doc;
            try { doc = XDocument.Parse(manifestXml, LoadOptions.SetLineInfo); }
            catch (XmlException) { return result; }

            foreach (var element in doc.Descendants())
            {
                if (System.Array.IndexOf(Tags, element.Name.LocalName) < 0) continue;
                if (element.Attribute(Android + "exported") != null) continue;
                if (element.Element("intent-filter") == null) continue;

                var lineInfo = (IXmlLineInfo)element;
                var component = new ManifestComponent
                {
                    Tag = element.Name.LocalName,
                    Name = (string)element.Attribute(Android + "name") ?? "",
                    Line = lineInfo.HasLineInfo() ? lineInfo.LineNumber : 0,
                };
                Suggest(element, component);
                result.Add(component);
            }
            return result;
        }

        /// <summary>
        /// Picks android:exported for a component. The launcher activity must be exported. Any other activity with an
        /// intent filter is meant to be opened from outside (links, shares). A service or receiver is exported only if
        /// it answers an action that is not a system one ("android.*"); system broadcasts reach it either way.
        /// </summary>
        static void Suggest(XElement element, ManifestComponent component)
        {
            var actions = new List<string>();
            var categories = new List<string>();
            foreach (var filter in element.Elements("intent-filter"))
            {
                foreach (var a in filter.Elements("action")) actions.Add((string)a.Attribute(Android + "name") ?? "");
                foreach (var c in filter.Elements("category")) categories.Add((string)c.Attribute(Android + "name") ?? "");
            }

            if (actions.Contains("android.intent.action.MAIN") && categories.Contains("android.intent.category.LAUNCHER"))
            {
                component.SuggestedExported = true; component.Reason = "launcher"; return;
            }
            if (component.Tag == "activity")
            {
                component.SuggestedExported = true; component.Reason = "external"; return;
            }
            bool custom = actions.Exists(a => a.Length > 0 && !a.StartsWith("android."));
            component.SuggestedExported = custom;
            component.Reason = custom ? "external" : "private";
        }
    }
}
