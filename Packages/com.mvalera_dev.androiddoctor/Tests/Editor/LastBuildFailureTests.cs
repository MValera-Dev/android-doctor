using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using AndroidDoctor.Gradle;

namespace AndroidDoctor.Tests
{
    /// <summary>A failure the user has already acted on must not keep showing as the problem.</summary>
    public class LastBuildFailureTests
    {
        string _captured, _addressed;

        [SetUp] public void SetUp()
        {
            _captured = SessionState.GetString(GradleErrorCapture.SessionKey, "");
            _addressed = SessionState.GetString("AndroidDoctor.AddressedFailure", "");
            GradleErrorCapture.Clear();
            LastBuildFailure.ForgetAddressed();
        }

        [TearDown] public void TearDown()
        {
            SessionState.SetString(GradleErrorCapture.SessionKey, _captured);
            SessionState.SetString("AndroidDoctor.AddressedFailure", _addressed);
        }

        static void Capture(string fixture) => SessionState.SetString(GradleErrorCapture.SessionKey, GradleFixtures.Read(fixture));

        [Test] public void ACapturedFailure_IsShown_UntilTheUserActsOnIt()
        {
            Capture("GRD009_1");
            Assert.IsNotNull(LastBuildFailure.Current());

            LastBuildFailure.MarkAddressed();

            Assert.IsNull(LastBuildFailure.Current(), "after a fix the old failure is not the news");
        }

        [Test] public void ANewFailure_ShowsAgain_EvenIfTheOldOneWasAddressed()
        {
            Capture("GRD009_1");
            LastBuildFailure.MarkAddressed();

            // The next build fails again: Unity logs the failure and the capture records it.
            GradleErrorCapture.OnLog(GradleFixtures.Read("GRD009_1"), "", LogType.Error); // logs a warning, which does not fail a test

            Assert.IsNotNull(LastBuildFailure.Current(), "the same error again after a rebuild is a real, new failure");
        }

        [Test] public void ADifferentFailure_IsNotHiddenByTheMarkOfAnotherOne()
        {
            Capture("GRD009_1");
            LastBuildFailure.MarkAddressed();
            Capture("GRD006_1");
            Assert.IsNotNull(LastBuildFailure.Current());
        }

        [Test] public void MarkingWithNoFailure_DoesNothing()
        {
            Assert.DoesNotThrow(() => LastBuildFailure.MarkAddressed()); // the Editor log may hold an old failure of its own
        }

        [Test] public void TheRule_FollowsTheSame()
        {
            Capture("GRD009_1");
            Assert.IsTrue(new LastBuildFailureRule().Run(null).GetEnumerator().MoveNext());
            LastBuildFailure.MarkAddressed();
            Assert.IsFalse(new LastBuildFailureRule().Run(null).GetEnumerator().MoveNext());
        }
    }
}
