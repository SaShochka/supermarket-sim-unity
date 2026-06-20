using SupermarketSim.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SupermarketSim.Gameplay
{
    public class EquipmentPlacementController : MonoBehaviour
    {
        public static bool IsPlacing { get; private set; }

        private EquipmentCatalogItem pendingItem;
        private PlayerWallet wallet;
        private Transform shelvesParent;
        private Transform checkoutParent;
        private GameObject cashRegisterPrefab;
        private Camera playerCamera;
        private GameObject preview;
        private GameObject hintCanvas;
        private float yaw;

        public static EquipmentPlacementController GetOrCreate(GameObject player)
        {
            var controller = player.GetComponent<EquipmentPlacementController>();
            if (controller == null)
                controller = player.AddComponent<EquipmentPlacementController>();
            return controller;
        }

        public void BeginPlacement(
            EquipmentCatalogItem item,
            PlayerWallet sourceWallet,
            Transform shelvesParentOverride,
            Transform checkoutParentOverride,
            GameObject registerPrefab)
        {
            CancelPlacement(refund: false);

            pendingItem = item;
            wallet = sourceWallet;
            shelvesParent = shelvesParentOverride != null ? shelvesParentOverride : GameObject.Find("Shelves")?.transform;
            checkoutParent = checkoutParentOverride != null ? checkoutParentOverride : GameObject.Find("Checkout")?.transform;
            if (checkoutParent == null)
                checkoutParent = GameObject.Find("Environment")?.transform;
            cashRegisterPrefab = registerPrefab;
            playerCamera = Camera.main;
            yaw = transform.eulerAngles.y;

            IsPlacing = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            CreatePreview();
            CreateHint();
        }

        private void Update()
        {
            if (!IsPlacing || pendingItem == null) return;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.qKey.isPressed)
                    yaw -= 120f * Time.deltaTime;
                if (Keyboard.current.eKey.isPressed)
                    yaw += 120f * Time.deltaTime;
                if (Keyboard.current.rKey.wasPressedThisFrame)
                    yaw += 15f;
            }

            if (Mouse.current != null)
            {
                var scroll = Mouse.current.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                    yaw += scroll * 0.08f;
            }

            UpdatePreviewTransform();

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                Place();

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                CancelPlacement(refund: true);
        }

        private void CreateHint()
        {
            hintCanvas = new GameObject("PlacementHintCanvas");
            var canvas = hintCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hintCanvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            hintCanvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var textObj = new GameObject("PlacementHintText");
            textObj.transform.SetParent(hintCanvas.transform, false);
            var text = textObj.AddComponent<UnityEngine.UI.Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = "ЛКМ - поставить   Q/E или колесо - вращать   R - +15°   ESC - отменить";
            text.fontSize = 28;
            text.fontStyle = FontStyle.Bold;
            text.color = new Color(1f, 0.9f, 0.45f);
            text.alignment = TextAnchor.LowerCenter;

            var shadow = textObj.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            shadow.effectDistance = new Vector2(2f, -2f);

            var rect = text.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 38f);
            rect.sizeDelta = new Vector2(900f, 60f);
        }

        private void CreatePreview()
        {
            if (pendingItem.kind == EquipmentCatalogItem.EquipmentKind.Shelf)
            {
                preview = StoreEquipmentSpawner.SpawnShelfUnit(null, Vector3.zero, Quaternion.identity, 1.96f);
            }
            else if (pendingItem.kind == EquipmentCatalogItem.EquipmentKind.CashRegister && cashRegisterPrefab != null)
            {
                preview = Instantiate(cashRegisterPrefab);
                preview.name = "PlacementPreview_CashRegister";
                preview.transform.localScale = Vector3.one * 4f;
            }

            if (preview == null) return;

            preview.name = "PlacementPreview_" + pendingItem.displayName;
            SetPreviewVisuals(preview);
            UpdatePreviewTransform();
        }

        private void SetPreviewVisuals(GameObject obj)
        {
            foreach (var collider in obj.GetComponentsInChildren<Collider>(true))
                collider.enabled = false;

            foreach (var behaviour in obj.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour != this)
                    behaviour.enabled = false;
            }

            var shader = Shader.Find("HDRP/Lit");
            if (shader == null || !shader.isSupported) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null || !shader.isSupported) shader = Shader.Find("Standard");

            foreach (var renderer in obj.GetComponentsInChildren<Renderer>(true))
            {
                var mat = shader != null ? new Material(shader) : new Material(renderer.sharedMaterial);
                var color = new Color(0.35f, 0.75f, 1f, 0.45f);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
                renderer.sharedMaterial = mat;
            }
        }

        private void UpdatePreviewTransform()
        {
            if (preview == null) return;

            Vector3 position = transform.position + transform.forward * 5f;
            if (playerCamera != null && Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out var hit, 30f, ~0, QueryTriggerInteraction.Ignore))
                position = hit.point;

            position.y = 0f;
            preview.transform.position = position;
            preview.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        private void Place()
        {
            if (pendingItem == null) return;

            var pos = preview != null ? preview.transform.position : transform.position + transform.forward * 5f;
            var rot = preview != null ? preview.transform.rotation : Quaternion.Euler(0f, yaw, 0f);

            if (preview != null)
                Destroy(preview);
            if (hintCanvas != null)
                Destroy(hintCanvas);

            if (pendingItem.kind == EquipmentCatalogItem.EquipmentKind.Shelf)
            {
                if (shelvesParent == null)
                    shelvesParent = GameObject.Find("Shelves")?.transform;

                if (shelvesParent == null)
                {
                    RefundAndStop("Нет родителя для полок.");
                    return;
                }

                StoreEquipmentSpawner.SpawnShelfUnit(shelvesParent, pos, rot, 1.96f);
            }
            else if (pendingItem.kind == EquipmentCatalogItem.EquipmentKind.CashRegister)
            {
                if (cashRegisterPrefab == null)
                {
                    RefundAndStop("Префаб кассы не настроен.");
                    return;
                }

                if (checkoutParent == null)
                {
                    RefundAndStop("Нет родителя для кассы.");
                    return;
                }

                StoreEquipmentSpawner.SpawnCashRegister(cashRegisterPrefab, checkoutParent, pos, rot, 4f);
            }

            if (CustomerNavMeshRuntime.Instance != null)
                CustomerNavMeshRuntime.Instance.Rebuild();

            StopPlacement();
        }

        private void CancelPlacement(bool refund)
        {
            if (!IsPlacing && preview == null) return;
            if (preview != null)
                Destroy(preview);
            if (hintCanvas != null)
                Destroy(hintCanvas);
            if (refund && wallet != null && pendingItem != null)
                wallet.AddMoney(pendingItem.price);
            StopPlacement();
        }

        private void RefundAndStop(string message)
        {
            if (wallet != null && pendingItem != null)
                wallet.AddMoney(pendingItem.price);
            Debug.LogWarning(message);
            StopPlacement();
        }

        private void StopPlacement()
        {
            IsPlacing = false;
            pendingItem = null;
            wallet = null;
            preview = null;
            hintCanvas = null;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
