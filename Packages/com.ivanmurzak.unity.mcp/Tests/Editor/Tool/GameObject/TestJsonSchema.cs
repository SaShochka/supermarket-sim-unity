using System;
using System.Collections;
using System.Globalization;
using System.Text.Json.Nodes;
using com.IvanMurzak.ReflectorNet.Model.Unity;
using NUnit.Framework;
using UnityEngine.TestTools;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.ReflectorNet.Model;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests
{
    public class TestJsonSchema : BaseTest
    {
        static void ValidateType<T>() => ValidateType(typeof(T));
        static void ValidateType(Type type)
        {
            var schema = JsonUtils.GetSchema(type);
            UnityEngine.Debug.Log($"Schema for '{type.FullName}': {schema}");

            Assert.IsNotNull(schema, $"Schema for '{type.FullName}' is null");

            var typeNodes = JsonUtils.FindAllProperties(schema, "type");
            foreach (var typeNode in typeNodes)
            {
                switch (typeNode)
                {
                    case JsonValue value:
                        var typeValue = value.ToString();
                        Assert.IsFalse(string.IsNullOrEmpty(typeValue), $"Type node for '{type.FullName}' is empty");
                        Assert.IsFalse(typeValue == "null", $"Type node for '{type.FullName}' is \"null\" string");
                        break;
                    default:
                        if (typeNode is JsonObject typeObject)
                        {
                            if (typeObject.TryGetPropertyValue("enum", out var enumValue))
                                continue; // Skip enum types
                        }
                        Assert.Fail($"Unexpected type node for '{type.FullName}'.\nThe 'type' node has the type '{typeNode.GetType().Name}':\n{typeNode}");
                        break;
                }
            }
        }

        [UnityTest]
        public IEnumerator Primitives()
        {
            ValidateType<int>();
            ValidateType<float>();
            ValidateType<bool>();
            ValidateType<string>();
            ValidateType<CultureTypes>(); // enum

            yield return null;
        }

        [UnityTest]
        public IEnumerator Classes()
        {
            // ValidateType<object>();
            ValidateType<ObjectRef>();

            ValidateType<GameObjectData>();
            ValidateType<GameObjectRef>();
            ValidateType<GameObjectRefList>();

            ValidateType<ComponentData>();
            ValidateType<ComponentDataLight>();
            ValidateType<ComponentRef>();
            ValidateType<ComponentRefList>();

            ValidateType<MethodDataRef>();
            ValidateType<MethodPointerRef>();

            yield return null;
        }

        [UnityTest]
        public IEnumerator Structs()
        {
            ValidateType<DateTime>();
            ValidateType<TimeSpan>();

            yield return null;
        }

        [UnityTest]
        public IEnumerator UnityStructs()
        {
            ValidateType<UnityEngine.Color32>();
            ValidateType<UnityEngine.Color>();
            ValidateType<UnityEngine.Vector3>();
            ValidateType<UnityEngine.Vector3Int>();
            ValidateType<UnityEngine.Vector2>();
            ValidateType<UnityEngine.Vector2Int>();
            ValidateType<UnityEngine.Quaternion>();
            ValidateType<UnityEngine.Matrix4x4>();

            yield return null;
        }

        [UnityTest]
        public IEnumerator Unity()
        {
            ValidateType<UnityEngine.Object>();
            ValidateType<UnityEngine.Rigidbody>();
            ValidateType<UnityEngine.Animation>();
            ValidateType<UnityEngine.Material>();
            ValidateType<UnityEngine.Transform>();
            ValidateType<UnityEngine.SpriteRenderer>();
            ValidateType<UnityEngine.MeshRenderer>();

            yield return null;
        }
    }
}