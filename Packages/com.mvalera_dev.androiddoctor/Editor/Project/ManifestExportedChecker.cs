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
                result.Add(new ManifestComponent
                {
                    Tag = element.Name.LocalName,
                    Name = (string)element.Attribute(Android + "name") ?? "",
                    Line = lineInfo.HasLineInfo() ? lineInfo.LineNumber : 0,
                });
            }
            return result;
        }
    }
}
