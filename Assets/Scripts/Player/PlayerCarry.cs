using UnityEngine;
using SupermarketSim.Gameplay;

namespace SupermarketSim.Player
{
    public class PlayerCarry : MonoBehaviour
    {
        [Header("Settings")]
        public Transform holdPoint;
        public float bobSpeed = 12f;
        public float bobAmount = 0.05f;

        private PickupableItem carriedItem;
        private Vector3 originalHoldLocalPos;
        private CharacterController cc;

        public bool IsCarrying => carriedItem != null;

        private void Awake()
        {
            cc = GetComponent<CharacterController>();
        }

        private void Start()
        {
            if (holdPoint != null)
            {
                originalHoldLocalPos = holdPoint.localPosition;
            }
        }

        private void Update()
        {
            if (IsCarrying && holdPoint != null && cc != null)
            {
                // Simple bobbing effect based on movement speed
                float speed = cc.velocity.magnitude;
                if (speed > 0.1f && cc.isGrounded)
                {
                    float bobY = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
                    holdPoint.localPosition = originalHoldLocalPos + new Vector3(0, bobY, 0);
                }
                else
                {
                    holdPoint.localPosition = Vector3.Lerp(holdPoint.localPosition, originalHoldLocalPos, Time.deltaTime * 10f);
                }
            }
        }

        public void PickUp(PickupableItem item)
        {
            carriedItem = item;
            
            // Disable physics on the item if it has a rigidbody
            var rb = item.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            // Disable collider so it doesn't interfere with raycasts or player movement
            foreach (var col in item.GetComponentsInChildren<Collider>(true))
                col.enabled = false;

            // Move to hold point
            item.transform.SetParent(holdPoint);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
        }

        public PickupableItem GetCarriedItem() => carriedItem;

        public void RemoveItem()
        {
            carriedItem = null;
        }

        public void Drop()
        {
            if (carriedItem != null)
            {
                var rb = carriedItem.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = false;

                foreach (var col in carriedItem.GetComponentsInChildren<Collider>(true))
                    col.enabled = true;

                carriedItem.transform.SetParent(null);
                carriedItem = null;
            }
        }
    }
}