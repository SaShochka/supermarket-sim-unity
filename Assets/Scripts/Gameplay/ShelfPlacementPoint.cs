using UnityEngine;
using SupermarketSim.Player;

namespace SupermarketSim.Gameplay
{
    public class ShelfPlacementPoint : MonoBehaviour, Interaction.IInteractable
    {
        public PickupableItem currentItem;

        public string GetInteractionPrompt()
        {
            if (currentItem != null) return currentItem.GetInteractionPrompt();
            return "Нажмите E чтобы положить";
        }

        public void Interact(GameObject interactor)
        {
            if (currentItem != null)
            {
                currentItem.Interact(interactor);
                return;
            }

            var carrier = interactor.GetComponent<PlayerCarry>();
            if (carrier != null && carrier.IsCarrying)
            {
                var item = carrier.GetCarriedItem();
                carrier.RemoveItem(); // Remove from hands without dropping physics
                PlaceItem(item);
                GameAudio.PlayPlace();
            }
        }

        public void PlaceItem(PickupableItem item)
        {
            currentItem = item;
            item.transform.SetParent(transform);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.Euler(0, 180, 0); // Face outward
            
            var rb = item.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            foreach (var col in item.GetComponentsInChildren<Collider>(true))
                col.enabled = true; // Needs to be interactable to pick up again
            
            item.currentPlacementPoint = this;
        }
    }
}