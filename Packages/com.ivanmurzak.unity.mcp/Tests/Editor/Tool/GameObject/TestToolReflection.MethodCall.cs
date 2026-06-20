using System.Collections;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Editor.API;
using NUnit.Framework;
using UnityEngine.TestTools;
using com.IvanMurzak.ReflectorNet.Model;
using com.IvanMurzak.ReflectorNet;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests
{
    public partial class TestToolReflection : BaseTest
    {
        [UnityTest]
        public IEnumerator MethodCall_UnityEditor_EditorUserBuildSettings_get_activeBuildTarget()
        {
            var classType = typeof(UnityEditor.EditorUserBuildSettings);
            var name = "get_" + nameof(UnityEditor.EditorUserBuildSettings.activeBuildTarget);
            var methodInfo = classType.GetMethod(name);

            ResultValidation(new Tool_Reflection().MethodCall(
                filter: new MethodPointerRef(methodInfo)));

            yield return null;
        }
        [UnityTest]
        public IEnumerator MethodCall_UnityEditor_Build_NamedBuildTarget_get_TargetName()
        {
            var classType = typeof(UnityEditor.Build.NamedBuildTarget);
            var name = "get_" + nameof(UnityEditor.Build.NamedBuildTarget.TargetName);
            var methodInfo = classType.GetMethod(name);

            var obj = new UnityEditor.Build.NamedBuildTarget();
            var serializedObj = Reflector.Instance.Serialize(obj);

            ResultValidation(new Tool_Reflection().MethodCall(
                filter: new MethodPointerRef(methodInfo),
                targetObject: serializedObj));

            yield return null;
        }
        [UnityTest]
        public IEnumerator MethodCall_UnityEngine_Application_get_platform()
        {
            var classType = typeof(UnityEngine.Application);
            var name = "get_" + nameof(UnityEngine.Application.platform);
            var methodInfo = classType.GetMethod(name);
            var methodPointerRef = new MethodPointerRef(methodInfo);

            UnityEngine.Debug.Log($"Input: {methodPointerRef}\n");

            ResultValidation(new Tool_Reflection().MethodCall(
                filter: methodPointerRef));

            ResultValidation(new Tool_Reflection().MethodCall(
                filter: methodPointerRef,
                executeInMainThread: true));

            ResultValidation(new Tool_Reflection().MethodCall(
                filter: methodPointerRef,
                executeInMainThread: true,
                methodNameMatchLevel: 6));

            yield return null;
        }
        [UnityTest]
        public IEnumerator MethodCall_UnityEngine_Transform_LookAt()
        {
            var classType = typeof(UnityEngine.Transform);
            var name = nameof(UnityEngine.Transform.LookAt);
            var methodInfo = classType.GetMethod(name, new[] { typeof(UnityEngine.Transform) });
            var methodPointerRef = new MethodPointerRef(methodInfo);

            UnityEngine.Debug.Log($"Input: {methodPointerRef}\n");

            var go1 = new UnityEngine.GameObject("1");
            var go2 = new UnityEngine.GameObject("2");

            go1.transform.position = new UnityEngine.Vector3(0, 0, 0);
            go2.transform.position = new UnityEngine.Vector3(1, 0, 0);

            var serializedTransform1 = Reflector.Instance.Serialize(go1.transform, logger: McpPlugin.Instance.Logger);
            var serializedTransform2 = Reflector.Instance.Serialize(go2.transform, logger: McpPlugin.Instance.Logger, name: "target");

            ResultValidation(new Tool_Reflection().MethodCall(
                filter: methodPointerRef,
                targetObject: serializedTransform1,
                inputParameters: new SerializedMemberList(serializedTransform2),
                executeInMainThread: true));

            Assert.LessOrEqual(
                UnityEngine.Vector3.Distance(UnityEngine.Vector3.right, go1.transform.forward),
                0.0001f,
                $"Transform.forward should be {UnityEngine.Vector3.right}.");
            go1.transform.rotation = UnityEngine.Quaternion.identity;

            ResultValidation(new Tool_Reflection().MethodCall(
                filter: methodPointerRef,
                targetObject: serializedTransform1,
                inputParameters: new SerializedMemberList(serializedTransform2),
                executeInMainThread: true,
                methodNameMatchLevel: 6));

            Assert.LessOrEqual(
                UnityEngine.Vector3.Distance(UnityEngine.Vector3.right, go1.transform.forward),
                0.0001f,
                $"Transform.forward should be {UnityEngine.Vector3.right}.");

            yield return null;
        }
    }
}