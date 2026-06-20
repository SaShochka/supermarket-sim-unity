using UnityEngine;

namespace SupermarketSim.Gameplay
{
    public class PickupableItem : MonoBehaviour, Interaction.IInteractable
    {
        public string itemName = "Item";
        public ShelfPlacementPoint currentPlacementPoint;

        public string GetInteractionPrompt()
        {
            return $"Нажмите E чтобы подобрать {itemName}";
        }

        public void Interact(GameObject interactor)
        {
            var carrier = interactor.GetComponent<Player.PlayerCarry>();
            if (carrier != null && !carrier.IsCarrying)
            {
                if (currentPlacementPoint != null)
                {
                    currentPlacementPoint.currentItem = null;
                    currentPlacementPoint = null;
                }
                carrier.PickUp(this);
                GameAudio.PlayPickup();
            }
        }
    }
}