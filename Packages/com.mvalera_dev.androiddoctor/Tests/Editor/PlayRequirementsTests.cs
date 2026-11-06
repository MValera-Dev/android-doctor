using NUnit.Framework;

namespace AndroidDoctor.Tests
{
    public class PlayRequirementsTests
    {
        [Test] public void Parse_ReadsFieldsAndKnownPackages()
        {
            var play = PlayRequirements.Parse(@"{
                ""asOf"": ""2026-11-06"", ""minTargetSdk"": 36, ""minTargetSdkDeadline"": ""2026-08-31"",
                ""require64Bit"": true, ""require16KB"": true,
                ""knownPackages"": [ { ""name"": ""com.unity.burst"", ""minVersion"": ""1.8.21"", ""severity"": ""Error"", ""reason"": ""r"" } ]
            }");

            Assert.IsNotNull(play);
            Assert.AreEqual(36, play.minTargetSdk);
            Assert.IsTrue(play.require16KB);
            Assert.AreEqual(1, play.knownPackages.Length);
            Assert.AreEqual("com.unity.burst", play.knownPackages[0].name);
        }

        [Test] public void Parse_InvalidOrEmptyText_ReturnsNull()
        {
            Assert.IsNull(PlayRequirements.Parse(null));
            Assert.IsNull(PlayRequirements.Parse(""));
            Assert.IsNull(PlayRequirements.Parse("{ not json"));
        }

        [Test] public void Load_ReadsTheShippedFile()
        {
            var play = PlayRequirements.Load();
            Assert.IsNotNull(play, "Editor/Data/play.json must be readable from the package");
            Assert.Greater(play.minTargetSdk, 0);
            Assert.Greater(play.knownPackages.Length, 0);
        }
    }
}
