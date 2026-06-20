using UnityEngine;

namespace SupermarketSim.Interaction
{
    public interface IInteractable
    {
        string GetInteractionPrompt();
        void Interact(GameObject interactor);
    }
}