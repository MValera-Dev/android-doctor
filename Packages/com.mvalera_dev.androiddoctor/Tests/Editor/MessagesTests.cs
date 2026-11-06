using NUnit.Framework;

namespace AndroidDoctor.Tests
{
    public class MessagesTests
    {
        [Test] public void MissingKey_ReturnsTheKeyItself()
        {
            Assert.AreEqual("does.not.exist", Messages.Get("does.not.exist"));
        }

        [Test] public void KnownKey_IsTranslatedNotEchoed()
        {
            Assert.AreNotEqual("PRJ009.title", Messages.Get("PRJ009.title"));
        }

        [Test] public void Arguments_AreFormatted()
        {
            StringAssert.Contains("com.example.app", Messages.Get("PRJ009.evidence", "com.example.app"));
        }
    }
}
