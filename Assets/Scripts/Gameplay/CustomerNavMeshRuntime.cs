using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace SupermarketSim.Gameplay
{
    [DefaultExecutionOrder(-100)]
    public class CustomerNavMeshRuntime : MonoBehaviour
    {
        public static CustomerNavMeshRuntime Instance { get; private set; }

        private NavMeshSurface surface;

        private void Awake()
        {
            Instance = this;
            surface = GetComponent<NavMeshSurface>();
            if (surface == null)
                surface = gameObject.AddComponent<NavMeshSurface>();

            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.defaultArea = 0;
            surface.agentTypeID = 0;
        }

        private void Start()
        {
            Rebuild();
        }

        public void Rebuild()
        {
            if (surface == null) return;
            PlayerOnlyStoreBoundary.ConfigureManualBoundaryObjects();
            surface.BuildNavMesh();
        }

        public static Vector3 NearestNavMeshPoint(Vector3 point, float radius = 8f)
        {
            if (NavMesh.SamplePosition(point, out var hit, radius, NavMesh.AllAreas))
                return hit.position;
            return point;
        }
    }
}
