using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests
{
    public partial class TestEnvironment : BaseTest
    {
        // These tests fails in GitHub actions

        // [UnityTest]
        // public IEnumerator TestServerBuild()
        // {
        //     var task = Startup.BuildServer(force: true);
        //     while (!task.IsCompleted)
        //         yield return null;

        //     Assert.IsTrue(task.IsCompletedSuccessfully, "Server build failed");

        //     yield return null;
        // }

        // [UnityTest]
        // public IEnumerator TestDotNetInstall()
        // {
        //     var task = Startup.InstallDotNetIfNeeded(force: true);
        //     while (!task.IsCompleted)
        //         yield return null;

        //     Assert.IsTrue(task.IsCompletedSuccessfully, "DotNet installation failed");

        //     task = Startup.IsDotNetInstalled();
        //     while (!task.IsCompleted)
        //         yield return null;

        //     Assert.IsTrue(task.IsCompletedSuccessfully, "DotNet installation verification failed");

        //     yield return null;
        // }
    }
}