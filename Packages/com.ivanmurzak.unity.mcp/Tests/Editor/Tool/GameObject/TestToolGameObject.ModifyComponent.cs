using System.Collections;
using com.IvanMurzak.ReflectorNet.Model.Unity;
using com.IvanMurzak.Unity.MCP.Editor.API;
using com.IvanMurzak.Unity.MCP.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using com.IvanMurzak.ReflectorNet.Model;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests
{
    public partial class TestToolGameObject : BaseTest
    {
        [UnityTest]
        public IEnumerator ModifyComponent_Vector3()
        {
            var child = new GameObject(GO_ParentName).AddChild(GO_Child1Name);
            var newPosition = new Vector3(1, 2, 3);

            var data = SerializedMember.FromValue(
                    name: child.name,
                    type: typeof(GameObject),
                    value: new ObjectRef(child.GetInstanceID()))
                .AddField(SerializedMember.FromValue(
                    name: nameof(child.transform),
                    type: typeof(Transform),
                    value: new ObjectRef(child.transform.GetInstanceID())
                )
                .AddProperty(SerializedMember.FromValue(name: nameof(child.transform.position),
                    value: newPosition)));

            var result = new Tool_GameObject().Modify(
                gameObjectDiffs: new SerializedMemberList(data),
                gameObjectRefs: new GameObjectRefList
                {
                    new GameObjectRef()
                    {
                        instanceID = child.GetInstanceID()
                    }
                });
            ResultValidation(result);

            Assert.IsTrue(result.Contains(GO_Child1Name), $"{GO_Child1Name} should be found in the path");
            Assert.AreEqual(child.transform.position, newPosition, "Position should be changed");
            Assert.AreEqual(child.GetInstanceID(), data.GetInstanceID(), "InstanceID should be the same");
            Assert.AreEqual(child.transform.GetInstanceID(), data.GetField(nameof(child.transform)).GetInstanceID(), "InstanceID should be the same");
            yield return null;
        }
        [UnityTest]
        public IEnumerator ModifyComponent_Material()
        {
            var sharedMaterial = new Material(Shader.Find("Standard"));

            var go = new GameObject(GO_ParentName);
            var component = go.AddComponent<MeshRenderer>();

            var data = SerializedMember.FromValue(
                    name: go.name,
                    type: typeof(GameObject),
                    value: new ObjectRef(go.GetInstanceID()))
                .AddField(SerializedMember.FromValue(name: "",
                    type: typeof(MeshRenderer),
                    value: new ObjectRef(component.GetInstanceID())
                )
                .AddProperty(SerializedMember.FromValue(name: nameof(component.sharedMaterial),
                    type: typeof(Material),
                    value: new ObjectRef(sharedMaterial.GetInstanceID()))));

            var result = new Tool_GameObject().Modify(
                gameObjectDiffs: new SerializedMemberList(data),
                gameObjectRefs: new GameObjectRefList
                {
                    new GameObjectRef()
                    {
                        instanceID = go.GetInstanceID()
                    }
                });
            ResultValidation(result);

            Assert.IsTrue(result.Contains(GO_ParentName), $"{GO_ParentName} should be found in the path");
            Assert.AreEqual(component.sharedMaterial.GetInstanceID(), sharedMaterial.GetInstanceID(), "Materials InstanceIDs should be the same.");
            yield return null;
        }
    }
}