using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests
{
    public class BaseTest
    {
        [UnitySetUp]
        public virtual IEnumerator SetUp()
        {
            Debug.Log($"[{GetType().Name}] SetUp");

            McpPluginUnity.Init();

            yield return null;
        }
        [UnityTearDown]
        public virtual IEnumerator TearDown()
        {
            Debug.Log($"[{GetType().Name}] TearDown");

            DestroyAllGameObjectsInActiveScene();

            yield return null;
        }

        protected static void DestroyAllGameObjectsInActiveScene()
        {
            var scene = SceneManager.GetActiveScene();
            foreach (var go in scene.GetRootGameObjects())
                Object.DestroyImmediate(go);
        }
    }
}