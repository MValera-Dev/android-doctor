using NUnit.Framework;

namespace AndroidDoctor.Tests
{
    public class VersionCompareTests
    {
        [TestCase("1.8.20", "1.8.21", true)]
        [TestCase("1.8.21", "1.8.21", false)]
        [TestCase("1.8.22", "1.8.21", false)]
        [TestCase("1.9", "1.9.0", false)]          // missing parts count as zero
        [TestCase("1.9.0", "1.9", false)]
        [TestCase("1.9", "1.9.1", true)]
        [TestCase("2.0.0", "1.8.21", false)]
        [TestCase("1.8.21", "2.0.0", true)]
        [TestCase("1.10.0", "1.9.0", false)]       // numeric, not alphabetical
        [TestCase("1.9.0", "1.10.0", true)]
        public void IsLess_ComparesNumbers(string a, string b, bool expected)
        {
            Assert.AreEqual(expected, VersionCompare.IsLess(a, b));
        }

        [TestCase("1.9.0-pre.1", "1.9.0", false)]   // a prerelease is treated as its release
        [TestCase("1.9.0", "1.9.0-pre.1", false)]
        [TestCase("1.8.0-preview.3", "1.9.0", true)]
        public void IsLess_IgnoresTheSuffixAfterTheNumbers(string a, string b, bool expected)
        {
            Assert.AreEqual(expected, VersionCompare.IsLess(a, b));
        }

        [TestCase("garbage", "1.0.0")]
        [TestCase("1.0.0", "garbage")]
        [TestCase("", "1.0.0")]
        [TestCase(null, "1.0.0")]
        [TestCase("1.0.0", null)]
        [TestCase("99999999999.0", "1.0.0")]
        public void IsLess_WhenEitherCannotBeParsed_IsFalse(string a, string b)
        {
            Assert.IsFalse(VersionCompare.IsLess(a, b));
        }
    }
}
