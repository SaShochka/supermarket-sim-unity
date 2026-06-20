using System.Collections;
using UnityEngine;

namespace SupermarketSim.Gameplay
{
    public class CustomerSpawner : MonoBehaviour
    {
        public Vector3 spawnPoint = new Vector3(-18f, 0f, -46f);
        public Vector3 exitPoint = new Vector3(18f, 0f, -46f);
        public float firstSpawnDelay = 4f;
        public float spawnInterval = 18f;
        public int maxCustomers = 3;

        private void Start()
        {
            BindToRoadSpawnPoints();
            StartCoroutine(SpawnLoop());
        }

        private void BindToRoadSpawnPoints()
        {
            var spawnRoad = FindRoadByName("road-straight (22)");
            var exitRoad = FindRoadByName("road-straight (15)");

            if (spawnRoad != null)
                spawnPoint = GetRoadPoint(spawnRoad.transform, -2f);
            else
                spawnPoint = CustomerNavMeshRuntime.NearestNavMeshPoint(new Vector3(-6f, 0f, -26f), 30f);

            if (exitRoad != null)
                exitPoint = GetRoadPoint(exitRoad.transform, 2f);
            else if (spawnRoad != null)
                exitPoint = GetRoadPoint(spawnRoad.transform, 2f);
            else
                exitPoint = CustomerNavMeshRuntime.NearestNavMeshPoint(new Vector3(6f, 0f, -26f), 30f);
        }

        private static Vector3 GetRoadPoint(Transform road, float localXOffset)
        {
            var point = road.TransformPoint(new Vector3(localXOffset, 0f, 0f));
            point.y = 0f;
            return point;
        }

        private static GameObject FindRoadByName(string exactName)
        {
            var exact = GameObject.Find(exactName);
            if (exact != null) return exact;

            foreach (var transform in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
            {
                if (transform.name == exactName)
                    return transform.gameObject;
            }

            return null;
        }

        private IEnumerator SpawnLoop()
        {
            yield return new WaitForSeconds(firstSpawnDelay);
            if (CustomerNavMeshRuntime.Instance != null)
                CustomerNavMeshRuntime.Instance.Rebuild();

            while (true)
            {
                if (Object.FindObjectsByType<CustomerNpc>(FindObjectsInactive.Exclude).Length < maxCustomers)
                    SpawnCustomer();

                yield return new WaitForSeconds(spawnInterval);
            }
        }

        private void SpawnCustomer()
        {
            var customerObj = new GameObject("CustomerNpc");
            var desiredSpawn = spawnPoint + new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-2f, 2f));
            customerObj.transform.position = CustomerNavMeshRuntime.NearestNavMeshPoint(desiredSpawn, 15f);
            var customer = customerObj.AddComponent<CustomerNpc>();
            customer.Initialize(CustomerNavMeshRuntime.NearestNavMeshPoint(exitPoint, 15f));
        }
    }
}
