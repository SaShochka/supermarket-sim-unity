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
        public IEnumerator GetComponents()
        {
            var child = new GameObject(GO_ParentName).AddChild(GO_Child1Name);

            var meshRenderer = child.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

            var result = new Tool_GameObject().Find(
                gameObjectRef: new ReflectorNet.Model.Unity.GameObjectRef
                {
                    instanceID = child.GetInstanceID()
                },
                briefData: false);
            ResultValidation(result);

            Assert.IsTrue(result.Contains(GO_Child1Name), $"{GO_Child1Name} should be found in the path");
            yield return null;
        }
    }
}