using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.ReflectorNet.Model.Unity;
using com.IvanMurzak.Unity.MCP.Common.Reflection.Convertor;
using com.IvanMurzak.Unity.MCP.Reflection.Convertor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using com.IvanMurzak.ReflectorNet;
using com.IvanMurzak.ReflectorNet.Convertor;
using com.IvanMurzak.ReflectorNet.Utils;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests
{
    public partial class TestSerializer : BaseTest
    {
        static void PrintSerializers<TTarget>()
        {
            Debug.Log($"Serialize <b>[{typeof(TTarget)}]</b> priority:\n" + string.Join("\n", Reflector.Instance.Convertors.GetAllSerializers()
                .Select(x => $"{x.GetType()}: Priority: {x.SerializationPriority(typeof(TTarget))}")
                .ToList()));
        }
        static void TestSerializerChain<TTarget, TSerializer>(int countOfSerializers)
        {
            PrintSerializers<TTarget>();
            TestSerializerChain(typeof(TTarget), typeof(TSerializer), countOfSerializers);

            PrintSerializers<IEnumerable<TTarget>>();
            TestSerializerChain(typeof(IEnumerable<TTarget>), typeof(RS_ArrayUnity), countOfSerializers);

            PrintSerializers<List<TTarget>>();
            TestSerializerChain(typeof(List<TTarget>), typeof(RS_ArrayUnity), countOfSerializers);

            PrintSerializers<TTarget[]>();
            TestSerializerChain(typeof(TTarget[]), typeof(RS_ArrayUnity), countOfSerializers);

            Debug.Log($"-------------------------------------------");
        }
        static void TestSerializerChain(Type type, Type serializerType, int countOfSerializers)
        {
            var serializers = Reflector.Instance.Convertors.BuildSerializersChain(type).ToList();
            Assert.AreEqual(countOfSerializers, serializers.Count, $"{type}: Only {countOfSerializers} serializer should be used.");
            Assert.AreEqual(serializerType, serializers[0].GetType(), $"{type}: The first serializer should be {serializerType}.");
        }
        static void TestPopulatorChain<TTarget, TSerializer>(int countOfSerializers)
        {
            PrintSerializers<TTarget>();
            TestPopulatorChain(typeof(TTarget), typeof(TSerializer), countOfSerializers);

            PrintSerializers<IEnumerable<TTarget>>();
            TestPopulatorChain(typeof(IEnumerable<TTarget>), typeof(RS_ArrayUnity), countOfSerializers);

            PrintSerializers<List<TTarget>>();
            TestPopulatorChain(typeof(List<TTarget>), typeof(RS_ArrayUnity), countOfSerializers);

            PrintSerializers<TTarget[]>();
            TestPopulatorChain(typeof(TTarget[]), typeof(RS_ArrayUnity), countOfSerializers);

            Debug.Log($"-------------------------------------------");
        }
        static void TestPopulatorChain(Type type, Type serializerType, int countOfSerializers)
        {
            var serializers = Reflector.Instance.Convertors.BuildPopulatorsChain(type).ToList();
            Assert.AreEqual(countOfSerializers, serializers.Count, $"{type.Name}: Only {countOfSerializers} serializer should be used.");
            Assert.AreEqual(serializerType, serializers[0].GetType(), $"{type.Name}: The first serializer should be {serializerType.Name}.");
        }

        [UnityTest]
        public IEnumerator RS_SerializersOrder()
        {
            TestSerializerChain<int, PrimitiveReflectionConvertor>(1);
            TestSerializerChain<float, PrimitiveReflectionConvertor>(1);
            TestSerializerChain<bool, PrimitiveReflectionConvertor>(1);
            TestSerializerChain<string, PrimitiveReflectionConvertor>(1);
            TestSerializerChain<DateTime, PrimitiveReflectionConvertor>(1);
            TestSerializerChain<CultureTypes, PrimitiveReflectionConvertor>(1); // enum
            TestSerializerChain<object, RS_GenericUnity<object>>(1);
            TestSerializerChain<ObjectRef, RS_GenericUnity<object>>(1);

            TestSerializerChain<UnityEngine.Object, RS_UnityEngineObject>(1);
            TestSerializerChain<UnityEngine.Vector3, RS_UnityEngineVector3>(1);
            TestSerializerChain<UnityEngine.Rigidbody, RS_UnityEngineComponent>(1);
            TestSerializerChain<UnityEngine.Animation, RS_UnityEngineComponent>(1);
            TestSerializerChain<UnityEngine.Material, RS_UnityEngineMaterial>(1);
            TestSerializerChain<UnityEngine.Transform, RS_UnityEngineTransform>(1);
            TestSerializerChain<UnityEngine.SpriteRenderer, RS_UnityEngineRenderer>(1);
            TestSerializerChain<UnityEngine.MeshRenderer, RS_UnityEngineRenderer>(1);

            yield return null;
        }

        [UnityTest]
        public IEnumerator SerializeMaterial()
        {
            var material = new Material(Shader.Find("Standard"));

            var serialized = Reflector.Instance.Serialize(material);
            var json = JsonUtils.Serialize(serialized);
            Debug.Log($"[{nameof(TestSerializer)}] Result:\n{json}");

            var glossinessValue = 1.0f;
            var colorValue = new Color(1.0f, 0.0f, 0.0f, 0.5f);

            serialized.SetPropertyValue("_Glossiness", glossinessValue);
            serialized.SetPropertyValue("_Color", colorValue);

            var objMaterial = (object)material;
            Reflector.Instance.Populate(ref objMaterial, serialized);

            Assert.AreEqual(glossinessValue, material.GetFloat("_Glossiness"), 0.001f, $"Material property '_Glossiness' should be {glossinessValue}.");
            Assert.AreEqual(colorValue, material.GetColor("_Color"), $"Material property '_Glossiness' should be {glossinessValue}.");

            yield return null;
        }


        [UnityTest]
        public IEnumerator SerializeMaterialArray()
        {
            var material1 = new Material(Shader.Find("Standard"));
            var material2 = new Material(Shader.Find("Standard"));

            var materials = new[] { material1, material2 };

            var serialized = Reflector.Instance.Serialize(materials, logger: McpPlugin.Instance.Logger);
            var json = JsonUtils.Serialize(serialized);
            Debug.Log($"[{nameof(TestSerializer)}] Result:\n{json}");

            // var glossinessValue = 1.0f;
            // var colorValue = new Color(1.0f, 0.0f, 0.0f, 0.5f);

            // serialized.SetPropertyValue("_Glossiness", glossinessValue);
            // serialized.SetPropertyValue("_Color", colorValue);

            // var objMaterial = (object)material;
            // Serializer.Populate(ref objMaterial, serialized);

            // Assert.AreEqual(glossinessValue, material.GetFloat("_Glossiness"), 0.001f, $"Material property '_Glossiness' should be {glossinessValue}.");
            // Assert.AreEqual(colorValue, material.GetColor("_Color"), $"Material property '_Glossiness' should be {glossinessValue}.");

            yield return null;
        }

        void Test_Serialize_Deserialize<T>(T sourceObj)
        {
            var type = typeof(T);
            var serializedObj = Reflector.Instance.Serialize(sourceObj, logger: McpPlugin.Instance.Logger);
            var deserializedObj = Reflector.Instance.Deserialize(serializedObj, McpPlugin.Instance.Logger);

            Debug.Log($"[{type.Name}] Source:\n```json\n{JsonUtils.Serialize(sourceObj)}\n```");
            Debug.Log($"[{type.Name}] Serialized:\n```json\n{JsonUtils.Serialize(serializedObj)}\n```");
            Debug.Log($"[{type.Name}] Deserialized:\n```json\n{JsonUtils.Serialize(deserializedObj)}\n```");

            Assert.AreEqual(sourceObj.GetType(), deserializedObj.GetType(), $"Object type should be {sourceObj.GetType()}.");

            foreach (var field in Reflector.Instance.GetSerializableFields(type) ?? Enumerable.Empty<FieldInfo>())
            {
                try
                {
                    var sourceValue = field.GetValue(sourceObj);
                    var targetValue = field.GetValue(deserializedObj);
                    Assert.AreEqual(sourceValue, targetValue, $"Field '{field.Name}' should be equal. Expected: {sourceValue}, Actual: {targetValue}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error getting field '{type.Name}{field.Name}'\n{ex}");
                    throw ex;
                }
            }
            foreach (var prop in Reflector.Instance.GetSerializableProperties(type) ?? Enumerable.Empty<PropertyInfo>())
            {
                try
                {
                    var sourceValue = prop.GetValue(sourceObj);
                    var targetValue = prop.GetValue(deserializedObj);
                    Assert.AreEqual(sourceValue, targetValue, $"Property '{prop.Name}' should be equal. Expected: {sourceValue}, Actual: {targetValue}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error getting property '{type.Name}{prop.Name}'\n{ex}");
                    throw ex;
                }
            }
        }

        [UnityTest]
        public IEnumerator Serialize_Deserialize()
        {
            Test_Serialize_Deserialize(100);
            Test_Serialize_Deserialize(true);
            Test_Serialize_Deserialize("hello world");
            Test_Serialize_Deserialize(new UnityEngine.Vector3(1, 2, 3));
            Test_Serialize_Deserialize(new UnityEngine.Color(1, 0.5f, 0, 1));
            Test_Serialize_Deserialize(new UnityEditor.Build.NamedBuildTarget());

            yield return null;
        }
    }
}