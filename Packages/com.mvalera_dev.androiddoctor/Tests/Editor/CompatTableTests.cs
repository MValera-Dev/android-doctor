using NUnit.Framework;

namespace AndroidDoctor.Tests
{
    public class CompatTableTests
    {
        const string Json = @"{
            ""asOf"": ""2026-11-06"",
            ""unity"": [
                { ""match"": ""2022.3"", ""jdk"": 11, ""ndk"": ""23.1.7779620"", ""buildTools"": ""34.0.0"", ""gradle"": ""7.2"", ""agp"": ""7.1.2"", ""minSdk"": -1 },
                { ""match"": ""6000"", ""jdk"": 17, ""ndk"": ""27.2.12479018"", ""buildTools"": ""36.0.0"", ""gradle"": null, ""agp"": null, ""minSdk"": -1 },
                { ""match"": ""6000.5"", ""jdk"": 17, ""ndk"": ""27.2.12479018"", ""buildTools"": ""36.0.0"", ""gradle"": ""9.1.0"", ""agp"": ""9.0.0"", ""minSdk"": 26 }
            ]
        }";

        static CompatTable Table()
        {
            var table = CompatTable.Parse(Json);
            Assert.IsNotNull(table);
            return table;
        }

        [Test] public void Find_ExactMinorWins()
        {
            var row = Table().Find(UnityVersion.Parse("2022.3.61f1"));
            Assert.IsNotNull(row);
            Assert.AreEqual(11, row.jdk);
        }

        [Test] public void Find_MostSpecificRowWins()
        {
            var row = Table().Find(UnityVersion.Parse("6000.5.1f1"));
            Assert.AreEqual("6000.5", row.match);
            Assert.AreEqual("9.1.0", row.gradle);
            Assert.AreEqual(26, row.minSdk);
        }

        [Test] public void Find_NewerMinorInheritsClosestOlderRow()
        {
            Assert.AreEqual("6000.5", Table().Find(UnityVersion.Parse("6000.6.4f1")).match);
        }

        [Test] public void Find_OlderMinorFallsBackToMajorRow()
        {
            Assert.AreEqual("6000", Table().Find(UnityVersion.Parse("6000.3.2f1")).match);
        }

        [Test] public void Find_UnknownMajor_ReturnsNull()
        {
            Assert.IsNull(Table().Find(UnityVersion.Parse("2023.2.0f1")));
        }

        [Test] public void Find_NeverUsesARowFromAnotherMajor()
        {
            Assert.IsNull(Table().Find(UnityVersion.Parse("2021.3.0f1")));
        }

        [Test] public void NullStrings_ReadAsUnknown()
        {
            var row = Table().Find(UnityVersion.Parse("6000.3.2f1"));
            Assert.IsTrue(string.IsNullOrEmpty(row.gradle));
            Assert.IsTrue(string.IsNullOrEmpty(row.agp));
        }

        [Test] public void Parse_InvalidOrEmptyText_ReturnsNull()
        {
            Assert.IsNull(CompatTable.Parse(null));
            Assert.IsNull(CompatTable.Parse(""));
            Assert.IsNull(CompatTable.Parse("{ not json"));
        }

        [Test] public void Load_ReadsTheShippedFile()
        {
            var table = CompatTable.Load();
            Assert.IsNotNull(table, "Editor/Data/compat.json must be readable from the package");
            Assert.IsNotNull(table.Find(UnityVersion.Parse("6000.6.4f1")));
        }
    }
}
