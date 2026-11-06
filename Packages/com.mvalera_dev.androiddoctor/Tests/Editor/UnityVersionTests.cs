using NUnit.Framework;

namespace AndroidDoctor.Tests
{
    public class UnityVersionTests
    {
        [Test] public void Parse_ReadsMajorMinorPatch()
        {
            var v = UnityVersion.Parse("6000.3.2f1");
            Assert.AreEqual(6000, v.Major);
            Assert.AreEqual(3, v.Minor);
            Assert.AreEqual(2, v.Patch);
        }

        [Test] public void Parse_ReadsLegacyYearVersions()
        {
            var v = UnityVersion.Parse("2022.3.61f1");
            Assert.AreEqual(2022, v.Major);
            Assert.AreEqual(3, v.Minor);
            Assert.AreEqual(61, v.Patch);
        }

        [TestCase("garbage")]
        [TestCase("")]
        [TestCase(null)]
        public void Parse_InvalidInput_ReturnsZero(string input)
        {
            var v = UnityVersion.Parse(input);
            Assert.AreEqual(0, v.Major);
            Assert.AreEqual(0, v.Minor);
            Assert.AreEqual(0, v.Patch);
        }

        [Test] public void AtLeast_ComparesMajorThenMinor()
        {
            var v = UnityVersion.Parse("6000.5.0f1");
            Assert.IsTrue(v.AtLeast(6000, 5));
            Assert.IsTrue(v.AtLeast(6000, 0));
            Assert.IsTrue(v.AtLeast(2022, 9));
            Assert.IsFalse(v.AtLeast(6000, 6));
            Assert.IsFalse(v.AtLeast(6001, 0));
        }

        [Test] public void Key_IsMajorDotMinor()
        {
            Assert.AreEqual("6000.3", UnityVersion.Parse("6000.3.2f1").Key);
        }

        [Test] public void CompareTo_OrdersByMajorMinorPatch()
        {
            var a = UnityVersion.Parse("6000.3.2f1");
            var b = UnityVersion.Parse("6000.3.10f1");
            var c = UnityVersion.Parse("6000.4.0f1");
            Assert.Less(a.CompareTo(b), 0);
            Assert.Less(b.CompareTo(c), 0);
            Assert.Greater(c.CompareTo(a), 0);
            Assert.AreEqual(0, a.CompareTo(a));
        }
    }
}
