using UnityEngine;
using SupermarketSim.Interaction;
using SupermarketSim.Player;

namespace SupermarketSim.Gameplay
{
    public class ComputerTerminalInteractable : MonoBehaviour, IInteractable
    {
        public ComputerTerminalUI terminalUI;

        public string GetInteractionPrompt()
        {
            return "Нажмите E чтобы открыть компьютер";
        }

        public void Interact(GameObject interactor)
        {
            if (terminalUI == null) return;

            var playerController = interactor.GetComponent<FpsPlayerController>();
            terminalUI.OpenUI(playerController);
        }
    }
}