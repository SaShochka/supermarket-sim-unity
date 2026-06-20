using UnityEngine;
using Unity.AI.Navigation;

namespace SupermarketSim.Gameplay
{
    public class ProximityDoor : MonoBehaviour
    {
        public Transform leftDoor;
        public Transform rightDoor;
        public float openDistance = 1.5f;
        public float slideAmount = 1.2f;
        public float slideSpeed = 5f;
        
        private Vector3 leftClosedPos;
        private Vector3 rightClosedPos;
        private Vector3 leftOpenPos;
        private Vector3 rightOpenPos;
        
        private Transform player;

        private void Awake()
        {
            MakeDoorCollidersTriggers(leftDoor);
            MakeDoorCollidersTriggers(rightDoor);
        }

        private void Start()
        {
            MakeDoorCollidersTriggers(leftDoor);
            MakeDoorCollidersTriggers(rightDoor);

            if (leftDoor != null)
            {
                leftClosedPos = leftDoor.localPosition;
                leftOpenPos = leftClosedPos + new Vector3(-slideAmount, 0, 0);
            }
            if (rightDoor != null)
            {
                rightClosedPos = rightDoor.localPosition;
                rightOpenPos = rightClosedPos + new Vector3(slideAmount, 0, 0);
            }
            
            var pObj = GameObject.Find("Player");
            if (pObj != null) player = pObj.transform;
        }

        private static void MakeDoorCollidersTriggers(Transform door)
        {
            if (door == null) return;
            var modifier = door.GetComponent<NavMeshModifier>();
            if (modifier == null)
                modifier = door.gameObject.AddComponent<NavMeshModifier>();
            modifier.ignoreFromBuild = true;

            foreach (var col in door.GetComponentsInChildren<Collider>(true))
                col.isTrigger = true;
        }

        private void Update()
        {
            if (leftDoor == null || rightDoor == null) return;

            bool shouldOpen = IsNearPlayer() || IsNearCustomer();

            Vector3 targetLeft = shouldOpen ? leftOpenPos : leftClosedPos;
            Vector3 targetRight = shouldOpen ? rightOpenPos : rightClosedPos;

            leftDoor.localPosition = Vector3.Lerp(leftDoor.localPosition, targetLeft, Time.deltaTime * slideSpeed);
            rightDoor.localPosition = Vector3.Lerp(rightDoor.localPosition, targetRight, Time.deltaTime * slideSpeed);
        }

        private bool IsNearPlayer()
        {
            if (player == null)
            {
                var pObj = GameObject.Find("Player");
                if (pObj != null) player = pObj.transform;
            }

            return player != null && Vector3.Distance(transform.position, player.position) < openDistance;
        }

        private bool IsNearCustomer()
        {
            foreach (var customer in Object.FindObjectsByType<CustomerNpc>(FindObjectsInactive.Exclude))
            {
                if (customer != null && Vector3.Distance(transform.position, customer.transform.position) < openDistance + 1.25f)
                    return true;
            }

            return false;
        }
    }
}