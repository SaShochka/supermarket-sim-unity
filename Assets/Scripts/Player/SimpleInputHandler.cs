using UnityEngine;
using UnityEngine.InputSystem;
using SupermarketSim.Gameplay;

namespace SupermarketSim.Player
{
    [RequireComponent(typeof(FpsPlayerController))]
    [RequireComponent(typeof(Interaction.PlayerInteractor))]
    public class SimpleInputHandler : MonoBehaviour
    {
        private FpsPlayerController fpsController;
        private Interaction.PlayerInteractor interactor;
        private PlayerCashierMode cashierMode;

        private void Awake()
        {
            fpsController = GetComponent<FpsPlayerController>();
            interactor = GetComponent<Interaction.PlayerInteractor>();
            cashierMode = GetComponent<PlayerCashierMode>();
        }

        private void Update()
        {
            var openTerminal = GetOpenTerminal();
            if (cashierMode != null && cashierMode.IsInCashierMode)
            {
                fpsController.SetMoveInput(Vector2.zero);
                fpsController.SetLookInput(Vector2.zero);

                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                    interactor.Interact();

                if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                    cashierMode.TryExitActiveStation();

                return;
            }

            if (ComputerTerminalUI.AnyOpen || openTerminal != null)
            {
                fpsController.SetMoveInput(Vector2.zero);
                fpsController.SetLookInput(Vector2.zero);

                if (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame))
                {
                    if (openTerminal != null)
                        openTerminal.CloseUI();
                    return;
                }

                return;
            }

            // Move
            Vector2 moveInput = Vector2.zero;
            if (Keyboard.current.wKey.isPressed) moveInput.y += 1;
            if (Keyboard.current.sKey.isPressed) moveInput.y -= 1;
            if (Keyboard.current.aKey.isPressed) moveInput.x -= 1;
            if (Keyboard.current.dKey.isPressed) moveInput.x += 1;
            fpsController.SetMoveInput(moveInput.normalized);
            
            // Look
            Vector2 lookInput = Vector2.zero;
            if (Mouse.current != null)
            {
                lookInput = Mouse.current.delta.ReadValue();
                // Scale down mouse delta slightly for better feel
                lookInput *= 0.1f;
            }
            fpsController.SetLookInput(lookInput);

            if (EquipmentPlacementController.IsPlacing)
            {
                // Placement controller owns confirmation/cancel keys while placing bought equipment.
                return;
            }

            // Interact
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                // Avoid stacking OpenUI / blocked state if terminal is already open (toggle via E on monitor).
                foreach (var term in Object.FindObjectsByType<ComputerTerminalUI>(FindObjectsInactive.Include))
                {
                    if (term != null && term.uiCanvas != null && term.uiCanvas.activeSelf)
                    {
                        term.CloseUI();
                        return;
                    }
                }

                bool interacted = interactor.Interact();
                
                // If we didn't interact with anything (like a shelf or item), and we are carrying something, drop it on the floor
                if (!interacted)
                {
                    var carrier = GetComponent<PlayerCarry>();
                    if (carrier != null && carrier.IsCarrying)
                    {
                        carrier.Drop();
                    }
                }
            }
            
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                foreach (var term in Object.FindObjectsByType<ComputerTerminalUI>(FindObjectsInactive.Include))
                {
                    if (term != null && term.uiCanvas != null && term.uiCanvas.activeSelf)
                    {
                        term.CloseUI();
                        return;
                    }
                }
            }
        }

        private static ComputerTerminalUI GetOpenTerminal()
        {
            foreach (var term in Object.FindObjectsByType<ComputerTerminalUI>(FindObjectsInactive.Include))
            {
                if (term != null && term.uiCanvas != null && term.uiCanvas.activeSelf)
                    return term;
            }

            return null;
        }
    }
}