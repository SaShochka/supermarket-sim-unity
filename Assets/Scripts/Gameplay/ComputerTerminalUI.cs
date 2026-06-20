using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SupermarketSim.Player;

namespace SupermarketSim.Gameplay
{
    [System.Serializable]
    public class FoodCatalogItem
    {
        public string displayName;
        public GameObject prefab;
        public int price = 20;
        public Texture icon;
    }

    [System.Serializable]
    public class EquipmentCatalogItem
    {
        public string displayName;
        public int price;
        public Texture icon;
        public enum EquipmentKind { Shelf, CashRegister }
        public EquipmentKind kind;
    }

    public class ComputerTerminalUI : MonoBehaviour
    {
        public static bool AnyOpen { get; private set; }

        public GameObject uiCanvas;
        public Transform deliveryZone;

        public List<FoodCatalogItem> catalog = new List<FoodCatalogItem>();
        public List<EquipmentCatalogItem> equipmentCatalog = new List<EquipmentCatalogItem>();

        public GameObject foodTabRoot;
        public GameObject equipmentTabRoot;

        public Button tabFoodButton;
        public Button tabEquipmentButton;

        public Button btnClose;
        public Text statusText;
        public Text headerMoneyText;

        public GameObject cashRegisterPrefab;
        public Transform shelvesParentOverride;
        public Transform checkoutSpawnParentOverride;

        private FpsPlayerController playerController;
        private PlayerWallet wallet;
        private int _extraShelvesPlaced;
        private int _extraRegistersPlaced;
        private bool _runtimeLayoutReady;

        private void Start()
        {
            EnsureEventSystem();
            EnsureRuntimeShopLayout();
            BindStaticButtons();

            ShowTab(0);
            if (statusText != null) statusText.text = "";
            // Editor-added onClick with lambdas is NOT saved in the scene — wire buy buttons at runtime.
            WireShopBuyButtons();
        }

        private void BindStaticButtons()
        {
            if (btnClose != null)
            {
                btnClose.onClick.RemoveAllListeners();
                btnClose.onClick.AddListener(CloseUI);
            }

            if (tabFoodButton != null)
            {
                tabFoodButton.onClick.RemoveAllListeners();
                tabFoodButton.onClick.AddListener(() => ShowTab(0));
            }

            if (tabEquipmentButton != null)
            {
                tabEquipmentButton.onClick.RemoveAllListeners();
                tabEquipmentButton.onClick.AddListener(() => ShowTab(1));
            }
        }

        public void ShowTab(int index)
        {
            if (foodTabRoot != null) foodTabRoot.SetActive(index == 0);
            if (equipmentTabRoot != null) equipmentTabRoot.SetActive(index == 1);

            var imgF = tabFoodButton != null ? tabFoodButton.GetComponent<Image>() : null;
            var imgE = tabEquipmentButton != null ? tabEquipmentButton.GetComponent<Image>() : null;
            if (imgF != null) imgF.color = index == 0 ? new Color(0.25f, 0.45f, 0.85f) : new Color(0.15f, 0.15f, 0.2f);
            if (imgE != null) imgE.color = index == 1 ? new Color(0.25f, 0.45f, 0.85f) : new Color(0.15f, 0.15f, 0.2f);
        }

        public void OpenUI(FpsPlayerController player)
        {
            if (uiCanvas == null)
                uiCanvas = gameObject;
            if (uiCanvas != null)
                uiCanvas.SetActive(true);

            EnsureEventSystem();
            EnsureRuntimeShopLayout();
            BindStaticButtons();

            playerController = player;
            wallet = EnsureWallet(player);

            if (playerController != null)
                playerController.SetGameplayInputSuspended(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (wallet != null)
            {
                wallet.EnsureMoneyLabel();
                wallet.OnMoneyChanged -= OnWalletMoneyChanged;
                wallet.OnMoneyChanged += OnWalletMoneyChanged;
                OnWalletMoneyChanged(wallet.Money);
            }
            else if (headerMoneyText != null)
                headerMoneyText.text = "";

            AnyOpen = true;
            if (statusText != null) statusText.text = "";
            ShowTab(0);
            WireShopBuyButtons();
        }

        private static PlayerWallet EnsureWallet(FpsPlayerController player)
        {
            GameObject playerObject = null;

            if (player != null)
                playerObject = player.gameObject;

            if (playerObject == null)
                playerObject = GameObject.Find("Player");

            if (playerObject == null) return null;

            var existing = playerObject.GetComponent<PlayerWallet>();
            if (existing != null)
            {
                existing.EnsureMoneyLabel();
                return existing;
            }

            var created = playerObject.AddComponent<PlayerWallet>();
            created.EnsureMoneyLabel();
            created.SetMoney(300);
            return created;
        }

        private static void EnsureEventSystem()
        {
            var eventSystem = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                var go = new GameObject("EventSystem");
                eventSystem = go.AddComponent<UnityEngine.EventSystems.EventSystem>();
            }

            var oldStandalone = eventSystem.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (oldStandalone != null)
                Destroy(oldStandalone);

            if (eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
                eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            eventSystem.SetSelectedGameObject(null);
        }

        private void EnsureRuntimeShopLayout()
        {
            if (uiCanvas == null)
                uiCanvas = gameObject;

            DisableLegacyLayout();

            if (_runtimeLayoutReady && foodTabRoot != null && equipmentTabRoot != null)
            {
                var existingRoot = transform.Find("ShopRoot");
                if (existingRoot != null)
                    existingRoot.gameObject.SetActive(true);
                return;
            }

            if (GetComponent<Canvas>() == null)
            {
                var canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 50;
            }

            if (GetComponent<CanvasScaler>() == null)
            {
                var scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
            }

            if (GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();

            if (deliveryZone == null)
                deliveryZone = GameObject.Find("DeliveryZone")?.transform;

            if (shelvesParentOverride == null)
                shelvesParentOverride = GameObject.Find("Shelves")?.transform;

            if (checkoutSpawnParentOverride == null)
                checkoutSpawnParentOverride = GameObject.Find("Checkout")?.transform;

            if (cashRegisterPrefab == null)
            {
                var existingRegister = Object.FindAnyObjectByType<CashierStationInteractable>();
                if (existingRegister != null)
                    cashRegisterPrefab = existingRegister.gameObject;
            }

            EnsureDefaultCatalogData();

            var oldRoot = transform.Find("ShopRoot");
            if (oldRoot != null)
            {
                oldRoot.gameObject.SetActive(false);
                Destroy(oldRoot.gameObject);
            }

            BuildRuntimeFullscreenShop();
            _runtimeLayoutReady = true;
        }

        private void DisableLegacyLayout()
        {
            var oldPanel = transform.Find("Panel");
            if (oldPanel == null) return;

            oldPanel.gameObject.SetActive(false);
            Destroy(oldPanel.gameObject);
        }

        private void EnsureDefaultCatalogData()
        {
            for (int i = 0; i < catalog.Count; i++)
            {
                if (catalog[i] == null) continue;
                if (catalog[i].price <= 0) catalog[i].price = 20;
                if (catalog[i].icon == null && catalog[i].prefab != null)
                    catalog[i].icon = GetIconTextureFromPrefab(catalog[i].prefab);
            }

            if (equipmentCatalog.Count == 0)
            {
                equipmentCatalog.Add(new EquipmentCatalogItem
                {
                    displayName = "Стеллаж",
                    price = 85,
                    kind = EquipmentCatalogItem.EquipmentKind.Shelf
                });
                equipmentCatalog.Add(new EquipmentCatalogItem
                {
                    displayName = "Касса",
                    price = 120,
                    icon = cashRegisterPrefab != null ? GetIconTextureFromPrefab(cashRegisterPrefab) : null,
                    kind = EquipmentCatalogItem.EquipmentKind.CashRegister
                });
            }
        }

        private void BuildRuntimeFullscreenShop()
        {
            var root = new GameObject("ShopRoot");
            root.transform.SetParent(transform, false);
            var rootImg = root.AddComponent<Image>();
            rootImg.color = new Color(0.06f, 0.07f, 0.1f, 0.97f);
            Stretch(rootImg.rectTransform);

            var header = NewPanel(root.transform, "Header", new Color(0.12f, 0.14f, 0.2f));
            var headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.pivot = new Vector2(0.5f, 1);
            headerRect.anchoredPosition = Vector2.zero;
            headerRect.sizeDelta = new Vector2(0, 92);

            var title = NewText(header.transform, "Title", "Магазин супермаркета", 40, TextAnchor.MiddleLeft, Color.white);
            title.fontStyle = FontStyle.Bold;
            var titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0, 0);
            titleRect.anchorMax = new Vector2(0.6f, 1);
            titleRect.offsetMin = new Vector2(28, 0);
            titleRect.offsetMax = Vector2.zero;

            headerMoneyText = NewText(header.transform, "HeaderMoney", "Баланс: 300 $", 30, TextAnchor.MiddleRight, new Color(1f, 0.92f, 0.45f));
            headerMoneyText.fontStyle = FontStyle.Bold;
            var moneyRect = headerMoneyText.rectTransform;
            moneyRect.anchorMin = new Vector2(0.6f, 0);
            moneyRect.anchorMax = new Vector2(1, 1);
            moneyRect.offsetMin = Vector2.zero;
            moneyRect.offsetMax = new Vector2(-28, 0);

            var tabBar = NewPanel(root.transform, "TabBar", new Color(0.1f, 0.11f, 0.16f));
            var tabBarRect = tabBar.GetComponent<RectTransform>();
            tabBarRect.anchorMin = new Vector2(0, 1);
            tabBarRect.anchorMax = new Vector2(1, 1);
            tabBarRect.pivot = new Vector2(0.5f, 1);
            tabBarRect.anchoredPosition = new Vector2(0, -92);
            tabBarRect.sizeDelta = new Vector2(0, 64);

            tabFoodButton = NewButton(tabBar.transform, "Tab_Products", "Продукты", new Color(0.25f, 0.45f, 0.85f));
            var foodBtnRect = tabFoodButton.GetComponent<RectTransform>();
            foodBtnRect.anchorMin = foodBtnRect.anchorMax = new Vector2(0.5f, 0.5f);
            foodBtnRect.anchoredPosition = new Vector2(-200, 0);
            foodBtnRect.sizeDelta = new Vector2(300, 48);

            tabEquipmentButton = NewButton(tabBar.transform, "Tab_Equipment", "Оборудование", new Color(0.15f, 0.15f, 0.2f));
            var eqBtnRect = tabEquipmentButton.GetComponent<RectTransform>();
            eqBtnRect.anchorMin = eqBtnRect.anchorMax = new Vector2(0.5f, 0.5f);
            eqBtnRect.anchoredPosition = new Vector2(200, 0);
            eqBtnRect.sizeDelta = new Vector2(300, 48);

            var body = new GameObject("Body");
            body.transform.SetParent(root.transform, false);
            var bodyRect = body.AddComponent<RectTransform>();
            bodyRect.anchorMin = Vector2.zero;
            bodyRect.anchorMax = Vector2.one;
            bodyRect.offsetMin = new Vector2(24, 112);
            bodyRect.offsetMax = new Vector2(-24, -156);

            foodTabRoot = NewTabRoot(body.transform, "FoodTab");
            var foodGrid = NewGrid(foodTabRoot.transform, "FoodGrid", new Vector2(210, 248), new Vector2(16, 16));
            foreach (var item in catalog)
                CreateRuntimeCard(foodGrid.transform, "Card_" + item.displayName, item.displayName, item.price, item.icon, () => TryBuyFood(item));

            equipmentTabRoot = NewTabRoot(body.transform, "EquipmentTab");
            var equipGrid = NewGrid(equipmentTabRoot.transform, "EquipGrid", new Vector2(260, 170), new Vector2(24, 24));
            foreach (var item in equipmentCatalog)
                CreateRuntimeEquipmentCard(equipGrid.transform, "EquipCard_" + item.displayName, item.displayName, item.price, () => TryBuyEquipment(item));

            statusText = NewText(root.transform, "Status", "", 22, TextAnchor.LowerLeft, new Color(0.85f, 0.9f, 1f));
            var statusRect = statusText.rectTransform;
            statusRect.anchorMin = new Vector2(0, 0);
            statusRect.anchorMax = new Vector2(0.65f, 0);
            statusRect.pivot = new Vector2(0, 0);
            statusRect.anchoredPosition = new Vector2(32, 72);
            statusRect.sizeDelta = new Vector2(800, 40);

            btnClose = NewButton(root.transform, "Btn_Close", "ЗАКРЫТЬ (E / ESC)", new Color(0.75f, 0.25f, 0.22f));
            var closeRect = btnClose.GetComponent<RectTransform>();
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(1, 0);
            closeRect.pivot = new Vector2(1, 0);
            closeRect.anchoredPosition = new Vector2(-32, 24);
            closeRect.sizeDelta = new Vector2(260, 56);
        }

        /// <summary>
        /// Re-binds product/equipment buy buttons. Required because listeners added in Editor scripts
        /// (lambdas) are not persisted when the scene is saved.
        /// </summary>
        private void WireShopBuyButtons()
        {
            if (foodTabRoot != null)
            {
                var grid = foodTabRoot.transform.Find("FoodGrid");
                if (grid != null)
                {
                    foreach (var item in catalog)
                    {
                        if (item == null || string.IsNullOrEmpty(item.displayName)) continue;
                        var card = grid.Find("Card_" + item.displayName);
                        if (card == null) continue;
                        var buy = card.Find("Buy");
                        var btn = buy != null ? buy.GetComponent<Button>() : null;
                        if (btn == null) continue;
                        btn.onClick.RemoveAllListeners();
                        var captured = item;
                        btn.onClick.AddListener(() => TryBuyFood(captured));
                    }
                }
            }

            if (equipmentTabRoot != null)
            {
                var eqGrid = equipmentTabRoot.transform.Find("EquipGrid");
                if (eqGrid != null)
                {
                    foreach (var item in equipmentCatalog)
                    {
                        if (item == null || string.IsNullOrEmpty(item.displayName)) continue;
                        var card = eqGrid.Find("EquipCard_" + item.displayName);
                        if (card == null) continue;
                        var buy = card.Find("Buy");
                        var btn = buy != null ? buy.GetComponent<Button>() : null;
                        if (btn == null) continue;
                        btn.onClick.RemoveAllListeners();
                        var captured = item;
                        btn.onClick.AddListener(() => TryBuyEquipment(captured));
                    }
                }
            }
        }

        private void OnWalletMoneyChanged(int value)
        {
            if (headerMoneyText != null)
                headerMoneyText.text = $"Баланс: {value} $";
        }

        public void CloseUI()
        {
            AnyOpen = false;

            if (wallet != null)
                wallet.OnMoneyChanged -= OnWalletMoneyChanged;

            if (playerController != null)
                playerController.SetGameplayInputSuspended(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (uiCanvas != null)
                uiCanvas.SetActive(false);
        }

        public void TryBuyFood(FoodCatalogItem item)
        {
            if (item == null || item.prefab == null) return;
            if (wallet == null)
            {
                SetStatus("Нет кошелька игрока.");
                return;
            }
            if (!wallet.TrySpend(item.price))
            {
                SetStatus("Недостаточно средств.");
                GameAudio.PlayError();
                return;
            }
            SpawnFood(item.prefab);
            GameAudio.PlayBuy();
            SetStatus($"Куплено: {item.displayName}");
        }

        public void TryBuyEquipment(EquipmentCatalogItem item)
        {
            if (item == null) return;
            if (wallet == null)
            {
                SetStatus("Нет кошелька игрока.");
                return;
            }
            if (!wallet.TrySpend(item.price))
            {
                SetStatus("Недостаточно средств.");
                GameAudio.PlayError();
                return;
            }

            GameAudio.PlayBuy();
            CloseUI();
            var playerObject = wallet != null ? wallet.gameObject : GameObject.Find("Player");
            if (playerObject == null)
            {
                wallet.AddMoney(item.price);
                return;
            }

            var placement = EquipmentPlacementController.GetOrCreate(playerObject);
            placement.BeginPlacement(item, wallet, shelvesParentOverride, checkoutSpawnParentOverride, cashRegisterPrefab);
        }

        private void SetStatus(string msg)
        {
            if (statusText != null)
                statusText.text = msg;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static GameObject NewPanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        private static Text NewText(Transform parent, string name, string value, int size, TextAnchor alignment, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            return text;
        }

        private static Button NewButton(Transform parent, string name, string label, Color color)
        {
            var go = NewPanel(parent, name, color);
            var button = go.AddComponent<Button>();

            var text = NewText(go.transform, "Text", label, 22, TextAnchor.MiddleCenter, Color.white);
            Stretch(text.rectTransform);
            return button;
        }

        private static GameObject NewTabRoot(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Stretch(go.AddComponent<RectTransform>());
            return go;
        }

        private static GameObject NewGrid(Transform parent, string name, Vector2 cellSize, Vector2 spacing)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(8, 8);
            rect.offsetMax = new Vector2(-8, -8);

            var grid = go.AddComponent<GridLayoutGroup>();
            grid.cellSize = cellSize;
            grid.spacing = spacing;
            grid.padding = new RectOffset(8, 8, 8, 8);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;
            return go;
        }

        private static void CreateRuntimeCard(Transform parent, string name, string displayName, int price, Texture icon, System.Action buyAction)
        {
            var card = NewPanel(parent, name, new Color(0.18f, 0.2f, 0.26f));
            var layout = card.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 6;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(card.transform, false);
            var raw = iconObj.AddComponent<RawImage>();
            raw.texture = icon;
            raw.color = icon != null ? Color.white : new Color(0.3f, 0.3f, 0.35f);
            var iconLayout = iconObj.AddComponent<LayoutElement>();
            iconLayout.preferredHeight = 120;
            iconLayout.preferredWidth = 120;
            iconLayout.minHeight = 120;

            var nameText = NewText(card.transform, "Name", displayName, 20, TextAnchor.MiddleCenter, Color.white);
            nameText.fontStyle = FontStyle.Bold;
            nameText.gameObject.AddComponent<LayoutElement>().preferredHeight = 28;

            var priceText = NewText(card.transform, "Price", $"Цена: {price} $", 18, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.35f));
            priceText.gameObject.AddComponent<LayoutElement>().preferredHeight = 24;

            var buyButton = NewButton(card.transform, "Buy", "Купить", new Color(0.25f, 0.55f, 0.3f));
            buyButton.gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => buyAction?.Invoke());
        }

        private static void CreateRuntimeEquipmentCard(Transform parent, string name, string displayName, int price, System.Action buyAction)
        {
            var card = NewPanel(parent, name, new Color(0.16f, 0.18f, 0.24f));
            var layout = card.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 12, 12);
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            var nameText = NewText(card.transform, "Name", displayName, 24, TextAnchor.MiddleCenter, Color.white);
            nameText.fontStyle = FontStyle.Bold;
            nameText.gameObject.AddComponent<LayoutElement>().preferredHeight = 38;

            var buyButton = NewButton(card.transform, "Buy", "Купить", new Color(0.28f, 0.4f, 0.75f));
            buyButton.gameObject.AddComponent<LayoutElement>().preferredHeight = 52;
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => buyAction?.Invoke());
        }

        private static Texture GetIconTextureFromPrefab(GameObject prefab)
        {
            if (prefab == null) return null;

            foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
            {
                foreach (var mat in renderer.sharedMaterials)
                {
                    if (mat == null) continue;
                    if (mat.HasProperty("_BaseColorMap") && mat.GetTexture("_BaseColorMap") != null)
                        return mat.GetTexture("_BaseColorMap");
                    if (mat.HasProperty("_BaseMap") && mat.GetTexture("_BaseMap") != null)
                        return mat.GetTexture("_BaseMap");
                    if (mat.HasProperty("_MainTex") && mat.GetTexture("_MainTex") != null)
                        return mat.GetTexture("_MainTex");
                }
            }

            return null;
        }

        private void SpawnFood(GameObject prefab)
        {
            if (prefab == null || deliveryZone == null) return;

            Vector3 spawnPos = deliveryZone.position + new Vector3(Random.Range(-1f, 1f), 2f, Random.Range(-1f, 1f));
            var newFood = Instantiate(prefab, spawnPos, Random.rotation);

            float scaleMultiplier = 2f;
            string n = prefab.name.ToLower();

            if (n.Contains("cereal") || n.Contains("peanut") || n.Contains("syrup")) scaleMultiplier = 0.8f;
            if (n.Contains("cola") || n.Contains("grape") || n.Contains("guarana") || n.Contains("milk")) scaleMultiplier = 0.8f;

            if (n.Contains("ice_cream") || n.Contains("hazelnut")) scaleMultiplier *= 0.5f;
            if (n.Contains("nachos")) scaleMultiplier *= 0.7f;

            if (n.Contains("cola") || n.Contains("grape") || n.Contains("guarana") ||
                n.Contains("juice") || n.Contains("water") || n.Contains("milk"))
            {
                scaleMultiplier *= 0.7f;
            }

            newFood.transform.localScale = Vector3.one * scaleMultiplier;

            if (newFood.GetComponent<Collider>() == null)
                newFood.AddComponent<BoxCollider>();
            if (newFood.GetComponent<Rigidbody>() == null)
            {
                var rb = newFood.AddComponent<Rigidbody>();
                rb.mass = 1f;
            }
            if (newFood.GetComponent<PickupableItem>() == null)
                newFood.AddComponent<PickupableItem>();

            FixMaterials(newFood);
        }

        private void FixMaterials(GameObject obj)
        {
            Shader targetShader = Shader.Find("HDRP/Lit");
            if (targetShader == null || !targetShader.isSupported) targetShader = Shader.Find("Universal Render Pipeline/Lit");
            if (targetShader == null || !targetShader.isSupported) targetShader = Shader.Find("Standard");

            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                Material[] newMats = new Material[r.sharedMaterials.Length];
                for (int j = 0; j < r.sharedMaterials.Length; j++)
                {
                    var oldMat = r.sharedMaterials[j];
                    if (oldMat != null && oldMat.shader.name != targetShader.name)
                    {
                        var newMat = new Material(targetShader);

                        Texture tex = null;
                        if (oldMat.HasProperty("_MainTex")) tex = oldMat.GetTexture("_MainTex");
                        if (tex == null && oldMat.HasProperty("_BaseMap")) tex = oldMat.GetTexture("_BaseMap");
                        if (tex == null && oldMat.HasProperty("_BaseColorMap")) tex = oldMat.GetTexture("_BaseColorMap");

                        if (tex != null)
                        {
                            if (newMat.HasProperty("_BaseColorMap")) newMat.SetTexture("_BaseColorMap", tex);
                            if (newMat.HasProperty("_BaseMap")) newMat.SetTexture("_BaseMap", tex);
                            if (newMat.HasProperty("_MainTex")) newMat.SetTexture("_MainTex", tex);
                        }

                        Color matCol = Color.white;
                        bool foundColor = false;
                        if (oldMat.HasProperty("_Color")) { matCol = oldMat.GetColor("_Color"); foundColor = true; }
                        if (!foundColor && oldMat.HasProperty("_BaseColor")) { matCol = oldMat.GetColor("_BaseColor"); foundColor = true; }

                        if (foundColor)
                        {
                            if (newMat.HasProperty("_BaseColor")) newMat.SetColor("_BaseColor", matCol);
                            if (newMat.HasProperty("_Color")) newMat.SetColor("_Color", matCol);
                        }

                        newMats[j] = newMat;
                    }
                    else
                    {
                        newMats[j] = oldMat;
                    }
                }
                r.sharedMaterials = newMats;
            }
        }

        private void OnDestroy()
        {
            if (wallet != null)
                wallet.OnMoneyChanged -= OnWalletMoneyChanged;
        }
    }
}
