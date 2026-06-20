using System.Collections;
using com.IvanMurzak.Unity.MCP.Editor.API;
using com.IvanMurzak.Unity.MCP.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests
{
    public partial class TestToolGameObject : BaseTest
    {
        [UnityTest]
        public IEnumerator FindByInstanceId()
        {
            var child = new GameObject(GO_ParentName).AddChild(GO_Child1Name);
            var result = new Tool_GameObject().Find(
                gameObjectRef: new ReflectorNet.Model.Unity.GameObjectRef
                {
                    instanceID = child.GetInstanceID()
                });
            ResultValidation(result);

            Assert.IsTrue(result.Contains(GO_Child1Name), $"{GO_Child1Name} should be found in the path");
            yield return null;
        }

        [UnityTest]
        public IEnumerator FindByPath()
        {
            var child = new GameObject(GO_ParentName).AddChild(GO_Child1Name);
            var result = new Tool_GameObject().Find(
                gameObjectRef: new ReflectorNet.Model.Unity.GameObjectRef
                {
                    path = $"{GO_ParentName}/{GO_Child1Name}"
                });
            ResultValidation(result);

            Assert.IsTrue(result.Contains(GO_Child1Name), $"{GO_Child1Name} should be found in the path");
            yield return null;
        }

        [UnityTest]
        public IEnumerator FindByName()
        {
            var child = new GameObject(GO_ParentName).AddChild(GO_Child1Name);
            var result = new Tool_GameObject().Find(
                gameObjectRef: new ReflectorNet.Model.Unity.GameObjectRef
                {
                    name = GO_Child1Name
                });
            ResultValidation(result);

            Assert.IsTrue(result.Contains(GO_Child1Name), $"{GO_Child1Name} should be found in the path");
            yield return null;
        }
    }
}