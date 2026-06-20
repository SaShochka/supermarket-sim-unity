using UnityEngine;

namespace SupermarketSim.Gameplay
{
    public class CheckoutBeltItem : MonoBehaviour, Interaction.IInteractable
    {
        private CashierStationInteractable station;
        private PickupableItem item;

        public void Initialize(CashierStationInteractable owner, PickupableItem placedItem)
        {
            station = owner;
            item = placedItem;
        }

        public string GetInteractionPrompt()
        {
            var nameText = item != null ? item.itemName : "товар";
            var price = station != null ? station.SalePrice : 15;
            return $"Нажмите E чтобы пробить {nameText} (+{price} $)";
        }

        public void Interact(GameObject interactor)
        {
            var cashierMode = interactor != null ? interactor.GetComponent<SupermarketSim.Player.PlayerCashierMode>() : null;
            if (cashierMode == null || !cashierMode.IsInCashierMode)
                return;

            station?.ScanBeltItem(this);
        }
    }
}
