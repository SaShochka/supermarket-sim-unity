using UnityEngine;
using UnityEngine.InputSystem;

namespace SupermarketSim.Interaction
{
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("Settings")]
        public float interactionDistance = 12f; // Scaled up 4x
        public LayerMask interactableLayer;

        [Header("References")]
        public Camera mainCamera;
        public UnityEngine.UI.Text promptText; // Added UI Text reference
        public UnityEngine.UI.Image crosshairImage;
        
        private IInteractable currentInteractable;
        private Camera activeCamera;

        private void Start()
        {
            EnsurePromptText();
            EnsureCrosshair();

            activeCamera = mainCamera;
            if (promptText != null) promptText.text = "";
        }

        private void EnsurePromptText()
        {
            if (promptText != null) return;

            var prompt = GameObject.Find("PromptText");
            if (prompt != null)
            {
                promptText = prompt.GetComponent<UnityEngine.UI.Text>();
                if (promptText != null) return;
            }

            var canvasObj = GameObject.Find("PlayerCanvas");
            if (canvasObj == null)
            {
                canvasObj = new GameObject("PlayerCanvas");
                canvasObj.transform.SetParent(transform, false);
                var canvas = canvasObj.AddComponent<UnityEngine.Canvas>();
                canvas.renderMode = UnityEngine.RenderMode.ScreenSpaceOverlay;
                var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            var promptObj = new GameObject("PromptText");
            promptObj.transform.SetParent(canvasObj.transform, false);
            promptText = promptObj.AddComponent<UnityEngine.UI.Text>();
            promptText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            promptText.fontSize = 28;
            promptText.fontStyle = FontStyle.Bold;
            promptText.color = new Color(1f, 0.8f, 0.2f);
            promptText.alignment = TextAnchor.LowerRight;

            var shadow = promptObj.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
            shadow.effectDistance = new Vector2(2f, -2f);

            var rect = promptText.rectTransform;
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-50f, 50f);
            rect.sizeDelta = new Vector2(600f, 100f);
        }

        private void EnsureCrosshair()
        {
            if (crosshairImage != null) return;

            var existing = GameObject.Find("Crosshair");
            if (existing != null)
            {
                crosshairImage = existing.GetComponent<UnityEngine.UI.Image>();
                if (crosshairImage != null)
                    return;
            }

            var canvasObj = GameObject.Find("PlayerCanvas");
            if (canvasObj == null)
            {
                canvasObj = new GameObject("PlayerCanvas");
                canvasObj.transform.SetParent(transform, false);
                var canvas = canvasObj.AddComponent<UnityEngine.Canvas>();
                canvas.renderMode = UnityEngine.RenderMode.ScreenSpaceOverlay;
                var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            var crosshairObj = new GameObject("Crosshair");
            crosshairObj.transform.SetParent(canvasObj.transform, false);
            crosshairImage = crosshairObj.AddComponent<UnityEngine.UI.Image>();
            crosshairImage.color = new Color(1f, 1f, 1f, 0.85f);

            var rect = crosshairImage.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(8f, 8f);
        }

        public void SetRaycastCameraOverride(Camera overrideCamera)
        {
            activeCamera = overrideCamera != null ? overrideCamera : mainCamera;
        }

        private void Update()
        {
            if (activeCamera == null) return;

            Ray ray = new Ray(activeCamera.transform.position, activeCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayer))
            {
                currentInteractable = FindInteractable(hit.collider);
                if (promptText != null)
                {
                    if (currentInteractable != null)
                    {
                        promptText.text = currentInteractable.GetInteractionPrompt();
                    }
                    else
                    {
                        promptText.text = "";
                    }
                }
            }
            else
            {
                currentInteractable = null;
                if (promptText != null)
                {
                    promptText.text = "";
                }
            }
        }

        private static IInteractable FindInteractable(Collider collider)
        {
            if (collider == null) return null;

            var beltItem = collider.GetComponent<SupermarketSim.Gameplay.CheckoutBeltItem>();
            if (beltItem != null) return beltItem;

            var interactable = collider.GetComponent<IInteractable>();
            if (interactable != null) return interactable;

            beltItem = collider.GetComponentInParent<SupermarketSim.Gameplay.CheckoutBeltItem>();
            if (beltItem != null) return beltItem;

            interactable = collider.GetComponentInParent<IInteractable>();
            if (interactable != null) return interactable;

            beltItem = collider.GetComponentInChildren<SupermarketSim.Gameplay.CheckoutBeltItem>();
            if (beltItem != null) return beltItem;

            return collider.GetComponentInChildren<IInteractable>();
        }

        public void ForceSetInteractable(IInteractable interactable)
        {
            currentInteractable = interactable;
        }

        public void ClearForcedInteractable(IInteractable interactable)
        {
            if (currentInteractable == interactable)
                currentInteractable = null;
        }

        public bool Interact()
        {
            if (currentInteractable != null)
            {
                currentInteractable.Interact(this.gameObject);
                return true;
            }
            return false;
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Interact();
            }
        }
    }
}