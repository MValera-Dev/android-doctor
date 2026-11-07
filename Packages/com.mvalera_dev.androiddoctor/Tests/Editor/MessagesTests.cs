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

        [Test] public void EnglishScope_ForcesEnglish_AndTheLanguageComesBackAfterwards()
        {
            string before = Messages.Get("ui.run");
            using (Messages.English())
            {
                Assert.AreEqual("Run checks", Messages.Get("ui.run"));
                using (Messages.English()) Assert.AreEqual("Run checks", Messages.Get("ui.run")); // scopes nest
                Assert.AreEqual("Run checks", Messages.Get("ui.run"));
            }
            Assert.AreEqual(before, Messages.Get("ui.run"));
        }

        [Test] public void EnglishScope_DisposedTwice_DoesNotLeakIntoTheNextCall()
        {
            string before = Messages.Get("ui.run");
            var scope = Messages.English();
            scope.Dispose();
            scope.Dispose();
            using (Messages.English()) { }
            Assert.AreEqual(before, Messages.Get("ui.run"));
        }

        [Test] public void Arguments_AreFormatted()
        {
            StringAssert.Contains("com.example.app", Messages.Get("PRJ009.evidence", "com.example.app"));
        }
    }
}
