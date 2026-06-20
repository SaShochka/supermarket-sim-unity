using UnityEngine;
using UnityEngine.SceneManagement;

namespace SupermarketSim.Gameplay
{
    public static class CustomerSystemBootstrap
    {
        private const string GameSceneName = "Game";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == GameSceneName)
                EnsureCustomerSpawner();
        }

        private static void EnsureCustomerSpawner()
        {
            if (Object.FindAnyObjectByType<CustomerNavMeshRuntime>() == null)
            {
                var navObj = new GameObject("CustomerNavMeshRuntime");
                navObj.AddComponent<CustomerNavMeshRuntime>();
            }

            if (Object.FindAnyObjectByType<CustomerSpawner>() != null)
                return;

            var spawnerObj = new GameObject("CustomerSpawner");
            spawnerObj.AddComponent<CustomerSpawner>();
        }
    }
}
