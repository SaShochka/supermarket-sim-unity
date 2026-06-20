using UnityEngine;
using Unity.AI.Navigation;

namespace SupermarketSim.Gameplay
{
    public static class PlayerOnlyStoreBoundary
    {
        private const string RootName = "PlayerOnlyStoreBoundary";
        public const string LayerName = "PlayerOnlyBlocker";
        private static readonly string[] ManualBoundaryNames = { "GameObject", "GameObject (1)", "GameObject (2)" };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureBoundary()
        {
            if (ConfigureManualBoundaryObjects())
                return;

            if (GameObject.Find(RootName) != null)
                return;

            var root = new GameObject(RootName).transform;
            CreateBoundary(root, "BackBoundary", new Vector3(0f, 2.5f, 25.5f), new Vector3(54f, 5f, 1f));
            CreateBoundary(root, "LeftBoundary", new Vector3(-25.5f, 2.5f, 0f), new Vector3(1f, 5f, 52f));
            CreateBoundary(root, "RightBoundary", new Vector3(25.5f, 2.5f, 0f), new Vector3(1f, 5f, 52f));
        }

        public static void ConfigureNpcCollision(GameObject npc)
        {
            if (npc == null) return;

            ConfigureManualBoundaryObjects();

            foreach (var npcCollider in npc.GetComponentsInChildren<Collider>(true))
            {
                if (npcCollider == null) continue;
                foreach (var blocker in Object.FindObjectsByType<PlayerOnlyBoundaryMarker>(FindObjectsInactive.Exclude))
                {
                    if (blocker == null) continue;
                    foreach (var blockerCollider in blocker.GetComponentsInChildren<Collider>(true))
                    {
                        if (blockerCollider != null)
                            Physics.IgnoreCollision(npcCollider, blockerCollider, true);
                    }
                }
            }
        }

        public static bool ConfigureManualBoundaryObjects()
        {
            bool configuredAny = false;
            foreach (var boundaryName in ManualBoundaryNames)
            {
                var obj = GameObject.Find(boundaryName);
                if (obj == null)
                    continue;

                ConfigureBoundaryObject(obj);
                configuredAny = true;
            }

            return configuredAny;
        }

        private static void ConfigureBoundaryObject(GameObject obj)
        {
            if (obj == null) return;

            int layer = LayerMask.NameToLayer(LayerName);
            if (layer >= 0)
                SetLayerRecursive(obj, layer);

            foreach (var collider in obj.GetComponentsInChildren<Collider>(true))
                collider.isTrigger = false;

            IgnoreFromNavMeshBuild(obj);

            if (obj.GetComponent<PlayerOnlyBoundaryMarker>() == null)
                obj.AddComponent<PlayerOnlyBoundaryMarker>();
        }

        private static void SetLayerRecursive(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
                SetLayerRecursive(child.gameObject, layer);
        }

        private static void CreateBoundary(Transform root, string name, Vector3 position, Vector3 scale)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(root, false);
            obj.transform.position = position;
            obj.transform.localScale = scale;

            int layer = LayerMask.NameToLayer(LayerName);
            if (layer >= 0)
                obj.layer = layer;

            var collider = obj.AddComponent<BoxCollider>();
            collider.isTrigger = false;
            IgnoreFromNavMeshBuild(obj);
            obj.AddComponent<PlayerOnlyBoundaryMarker>();
        }

        private static void IgnoreFromNavMeshBuild(GameObject obj)
        {
            foreach (var transform in obj.GetComponentsInChildren<Transform>(true))
            {
                var modifier = transform.GetComponent<NavMeshModifier>();
                if (modifier == null)
                    modifier = transform.gameObject.AddComponent<NavMeshModifier>();

                modifier.ignoreFromBuild = true;
            }
        }
    }

    public class PlayerOnlyBoundaryMarker : MonoBehaviour
    {
    }
}
