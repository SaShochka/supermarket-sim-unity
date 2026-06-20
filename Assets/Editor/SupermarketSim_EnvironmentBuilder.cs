using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

namespace SupermarketSim.Editor
{
    public static class SupermarketSim_EnvironmentBuilder
    {
        /// <summary>Child name under Environment: anything you place here survives a full wipe rebuild.</summary>
        public const string ManualEditsChildName = "ManualEdits";

        [MenuItem("Supermarket/Build Environment (Game Scene)")]
        public static void BuildEnvironment()
        {
            BuildEnvironmentCore(fullWipe: false);
        }

        [MenuItem("Supermarket/Danger: Full wipe and rebuild Environment")]
        public static void BuildEnvironmentFullWipe()
        {
            if (!EditorUtility.DisplayDialog(
                    "Полная пересборка",
                    "Будет удалён весь объект Environment и собран заново.\n\n" +
                    "Сохранятся только дочерние объекты с именем \"" + ManualEditsChildName + "\" (их лучше заранее положить туда).\n\n" +
                    "Улица и внутренний декор скриптом больше не создаются — только через пункты Optional в меню Supermarket.\n\n" +
                    "Продолжить?",
                    "Удалить и пересобрать",
                    "Отмена"))
                return;

            BuildEnvironmentCore(fullWipe: true);
        }

        [MenuItem("Supermarket/Optional: Rebuild outdoor Street (grass, road, buildings…)")]
        public static void RebuildOutdoorStreetOptional()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.name != "Game")
            {
                Debug.LogWarning("Please open the 'Game' scene first! (Assets/Scenes/Game.unity)");
                return;
            }

            var env = GameObject.Find("Environment");
            if (env == null)
            {
                Debug.LogWarning("No Environment root — run Supermarket/Build Environment first.");
                return;
            }

            var old = env.transform.Find("Street");
            if (old != null)
                Object.DestroyImmediate(old.gameObject);

            BuildStreet(env.transform);
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("Outdoor Street rebuilt (optional). Save the scene (Ctrl+S) to keep it.");
        }

        [MenuItem("Supermarket/Optional: Rebuild store decorations (plants, freezers, carts…)")]
        public static void RebuildStoreDecorationsOptional()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.name != "Game")
            {
                Debug.LogWarning("Please open the 'Game' scene first! (Assets/Scenes/Game.unity)");
                return;
            }

            var env = GameObject.Find("Environment");
            if (env == null)
            {
                Debug.LogWarning("No Environment root — run Supermarket/Build Environment first.");
                return;
            }

            var old = env.transform.Find("StoreDecorations");
            if (old != null)
                Object.DestroyImmediate(old.gameObject);

            BuildStoreDecorations(env.transform);
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("Store decorations rebuilt (optional). Save the scene (Ctrl+S) to keep it.");
        }

        private static void BuildEnvironmentCore(bool fullWipe)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.name != "Game")
            {
                Debug.LogWarning("Please open the 'Game' scene first! (Assets/Scenes/Game.unity)");
                return;
            }

            Transform manualEdits = null;

            if (fullWipe)
            {
                var existingEnv = GameObject.Find("Environment");
                if (existingEnv != null)
                {
                    var m = existingEnv.transform.Find(ManualEditsChildName);
                    if (m != null)
                    {
                        manualEdits = m;
                        m.SetParent(null, worldPositionStays: true);
                    }

                    Object.DestroyImmediate(existingEnv);
                }

                var existingEventSystem = GameObject.Find("EventSystem");
                if (existingEventSystem != null)
                    Object.DestroyImmediate(existingEventSystem);
            }

            GameObject envObj = GameObject.Find("Environment");
            if (envObj == null)
                envObj = new GameObject("Environment");
            Transform root = envObj.transform;

            void ReplaceChild(string childName, System.Action<Transform> build)
            {
                var t = root.Find(childName);
                if (t != null)
                    Object.DestroyImmediate(t.gameObject);
                build(root);
            }

            ReplaceChild("Floor", BuildFloor);
            ReplaceChild("Walls", BuildWalls);
            ReplaceChild("EntranceDoors", BuildEntranceDoors);
            // Street and store props are no longer auto-spawned — edit the scene or use Optional menu items.

            ReplaceChild("Roof", BuildRoof);

            ReplaceChild("Shelves", BuildShelving);
            ReplaceChild("Checkout", BuildCheckout);
            ReplaceChild("DeliveryZone", BuildDeliveryZone);
            ReplaceChild("ComputerTerminal", BuildComputerTerminal);

            BuildLighting(root);

            SpawnTestFood();

            if (fullWipe || GameObject.Find("Player") == null)
                BuildPlayer();
            else
                Debug.Log("Skipping Player rebuild (incremental). Use Supermarket/Build Player (Game Scene) if you need a fresh player.");

            if (manualEdits != null)
            {
                manualEdits.SetParent(root, worldPositionStays: true);
                Debug.Log("Restored \"" + ManualEditsChildName + "\" under Environment after full wipe.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log(fullWipe
                ? "Environment full wipe + rebuild finished. Save the scene (Ctrl+S)."
                : "Environment incremental rebuild finished (Street/StoreDecorations untouched). Save the scene (Ctrl+S).");
        }

        private static void BuildDeliveryZone(Transform root)
        {
            var zoneObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            zoneObj.name = "DeliveryZone";
            zoneObj.transform.SetParent(root);
            zoneObj.transform.position = new Vector3(-15f, 0.01f, -15f); // In a corner, floor level
            zoneObj.transform.localScale = new Vector3(4f, 0.01f, 4f); // Large circle

            // Make it red
            var renderer = zoneObj.GetComponent<Renderer>();
            Shader targetShader = Shader.Find("HDRP/Lit");
            if (targetShader == null || !targetShader.isSupported) targetShader = Shader.Find("Universal Render Pipeline/Lit");
            if (targetShader == null || !targetShader.isSupported) targetShader = Shader.Find("Standard");
            
            Material redMat = new Material(targetShader);
            redMat.color = Color.red;
            if (redMat.HasProperty("_BaseColor")) redMat.SetColor("_BaseColor", Color.red);
            renderer.material = redMat;

            // Remove collider so player doesn't trip on it
            Object.DestroyImmediate(zoneObj.GetComponent<Collider>());
        }

        private static void BuildComputerTerminal(Transform root)
        {
            var terminalRoot = new GameObject("ComputerTerminal");
            terminalRoot.transform.SetParent(root);
            terminalRoot.transform.position = new Vector3(-15f, 0f, -10f); // Near delivery zone
            terminalRoot.transform.rotation = Quaternion.Euler(0, 90, 0); // Face the room

            // Load Kenney assets
            var deskPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParty/Kenney_FurniturePack/Models/desk.obj");
            var screenPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParty/Kenney_FurniturePack/Models/computerScreen.obj");
            var keyboardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParty/Kenney_FurniturePack/Models/computerKeyboard.obj");

            GameObject desk = null;
            GameObject monitor = null;

            if (deskPrefab != null && screenPrefab != null)
            {
                // Instantiate Desk
                desk = PrefabUtility.InstantiatePrefab(deskPrefab) as GameObject;
                desk.name = "Desk";
                desk.transform.SetParent(terminalRoot.transform);
                desk.transform.localPosition = Vector3.zero;
                desk.transform.localScale = Vector3.one * 0.5f; // Reduced from 2f to 0.5f (4x smaller)
                
                // Add collider to desk
                var deskCol = desk.AddComponent<BoxCollider>();
                // We will let Unity auto-calculate the BoxCollider size based on the mesh

                // Calculate the top of the desk
                float deskHeight = 0.35f; // fallback
                var meshFilter = desk.GetComponentInChildren<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    deskHeight = meshFilter.sharedMesh.bounds.max.y * 0.5f; // multiply by localScale
                }

                // Instantiate Monitor
                monitor = PrefabUtility.InstantiatePrefab(screenPrefab) as GameObject;
                monitor.name = "Monitor";
                monitor.transform.SetParent(terminalRoot.transform);
                monitor.transform.localPosition = new Vector3(0, deskHeight, 0.06f); // Placed exactly on top
                monitor.transform.localRotation = Quaternion.Euler(0, 180, 0); // Face player
                monitor.transform.localScale = Vector3.one * 0.5f;

                // Instantiate Keyboard
                if (keyboardPrefab != null)
                {
                    var keyboard = PrefabUtility.InstantiatePrefab(keyboardPrefab) as GameObject;
                    keyboard.name = "Keyboard";
                    keyboard.transform.SetParent(terminalRoot.transform);
                    keyboard.transform.localPosition = new Vector3(0, deskHeight, -0.06f); // Placed exactly on top
                    keyboard.transform.localRotation = Quaternion.Euler(0, 180, 0);
                    keyboard.transform.localScale = Vector3.one * 0.5f;
                }
            }
            else
            {
                // Fallback to cubes if assets not found
                desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
                desk.name = "Desk";
                desk.transform.SetParent(terminalRoot.transform);
                desk.transform.localPosition = new Vector3(0, 0.5f, 0);
                desk.transform.localScale = new Vector3(2f, 1f, 1f);

                monitor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                monitor.name = "Monitor";
                monitor.transform.SetParent(terminalRoot.transform);
                monitor.transform.localPosition = new Vector3(0, 1.2f, 0.2f);
                monitor.transform.localScale = new Vector3(0.8f, 0.6f, 0.1f);
                monitor.transform.localRotation = Quaternion.Euler(-10f, 0, 0);

                var renderer = monitor.GetComponent<Renderer>();
                Shader targetShader = Shader.Find("HDRP/Lit");
                if (targetShader == null || !targetShader.isSupported) targetShader = Shader.Find("Universal Render Pipeline/Lit");
                if (targetShader == null || !targetShader.isSupported) targetShader = Shader.Find("Standard");
                Material darkMat = new Material(targetShader);
                darkMat.color = Color.black;
                if (darkMat.HasProperty("_BaseColor")) darkMat.SetColor("_BaseColor", Color.black);
                renderer.material = darkMat;
            }

            // Add interaction to monitor
            var col = monitor.AddComponent<BoxCollider>();
            col.isTrigger = true; // Make it a trigger so player can get close
            col.size = new Vector3(3f, 3f, 4f); // Expand trigger area

            var interactable = monitor.AddComponent<SupermarketSim.Gameplay.ComputerTerminalInteractable>();
            
            // Build UI (fullscreen shop: tabs, images, prices, $ currency)
            var uiCanvasObj = new GameObject("ComputerUI");
            uiCanvasObj.transform.SetParent(terminalRoot.transform);
            var canvas = uiCanvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = uiCanvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            uiCanvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var rootPanel = new GameObject("ShopRoot");
            rootPanel.transform.SetParent(uiCanvasObj.transform, false);
            var rootImg = rootPanel.AddComponent<UnityEngine.UI.Image>();
            rootImg.color = new Color(0.06f, 0.07f, 0.1f, 0.97f);
            var rootRt = rootImg.rectTransform;
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            // Header
            var header = new GameObject("Header");
            header.transform.SetParent(rootPanel.transform, false);
            var headerImg = header.AddComponent<UnityEngine.UI.Image>();
            headerImg.color = new Color(0.12f, 0.14f, 0.2f);
            var headerRt = header.GetComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0, 1);
            headerRt.anchorMax = new Vector2(1, 1);
            headerRt.pivot = new Vector2(0.5f, 1);
            headerRt.anchoredPosition = Vector2.zero;
            headerRt.sizeDelta = new Vector2(0, 92);

            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(header.transform, false);
            var titleText = titleObj.AddComponent<UnityEngine.UI.Text>();
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.text = "Магазин супермаркета";
            titleText.fontSize = 40;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleLeft;
            titleText.color = Color.white;
            var titleRect = titleText.rectTransform;
            titleRect.anchorMin = new Vector2(0, 0);
            titleRect.anchorMax = new Vector2(0.55f, 1);
            titleRect.offsetMin = new Vector2(28, 0);
            titleRect.offsetMax = new Vector2(0, 0);

            var moneyHeaderObj = new GameObject("HeaderMoney");
            moneyHeaderObj.transform.SetParent(header.transform, false);
            var moneyHeaderTxt = moneyHeaderObj.AddComponent<UnityEngine.UI.Text>();
            moneyHeaderTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            moneyHeaderTxt.fontSize = 30;
            moneyHeaderTxt.fontStyle = FontStyle.Bold;
            moneyHeaderTxt.alignment = TextAnchor.MiddleRight;
            moneyHeaderTxt.color = new Color(1f, 0.92f, 0.45f);
            var moneyHeaderRt = moneyHeaderTxt.rectTransform;
            moneyHeaderRt.anchorMin = new Vector2(0.55f, 0);
            moneyHeaderRt.anchorMax = new Vector2(1, 1);
            moneyHeaderRt.offsetMin = new Vector2(0, 0);
            moneyHeaderRt.offsetMax = new Vector2(-28, 0);

            // Tab bar
            var tabBar = new GameObject("TabBar");
            tabBar.transform.SetParent(rootPanel.transform, false);
            var tabBarRt = tabBar.AddComponent<RectTransform>();
            tabBarRt.anchorMin = new Vector2(0, 1);
            tabBarRt.anchorMax = new Vector2(1, 1);
            tabBarRt.pivot = new Vector2(0.5f, 1);
            tabBarRt.anchoredPosition = new Vector2(0, -92);
            tabBarRt.sizeDelta = new Vector2(0, 64);
            var tabBarImg = tabBar.AddComponent<UnityEngine.UI.Image>();
            tabBarImg.color = new Color(0.1f, 0.11f, 0.16f);

            var tabFoodBtn = CreateTabButton(tabBar.transform, "Продукты", new Vector2(-200, 0));
            var tabEquipBtn = CreateTabButton(tabBar.transform, "Оборудование", new Vector2(200, 0));

            var body = new GameObject("Body");
            body.transform.SetParent(rootPanel.transform, false);
            var bodyRt = body.AddComponent<RectTransform>();
            bodyRt.anchorMin = new Vector2(0, 0);
            bodyRt.anchorMax = new Vector2(1, 1);
            bodyRt.offsetMin = new Vector2(24, 112);
            bodyRt.offsetMax = new Vector2(-24, -156);

            var foodTabRoot = new GameObject("FoodTab");
            foodTabRoot.transform.SetParent(body.transform, false);
            var foodTabRt = foodTabRoot.AddComponent<RectTransform>();
            foodTabRt.anchorMin = Vector2.zero;
            foodTabRt.anchorMax = Vector2.one;
            foodTabRt.offsetMin = Vector2.zero;
            foodTabRt.offsetMax = Vector2.zero;

            var foodGridHolder = new GameObject("FoodGrid");
            foodGridHolder.transform.SetParent(foodTabRoot.transform, false);
            var foodGridRt = foodGridHolder.AddComponent<RectTransform>();
            foodGridRt.anchorMin = Vector2.zero;
            foodGridRt.anchorMax = Vector2.one;
            foodGridRt.offsetMin = new Vector2(8, 8);
            foodGridRt.offsetMax = new Vector2(-8, -8);
            var foodGrid = foodGridHolder.AddComponent<UnityEngine.UI.GridLayoutGroup>();
            foodGrid.cellSize = new Vector2(210, 248);
            foodGrid.spacing = new Vector2(16, 16);
            foodGrid.padding = new RectOffset(8, 8, 8, 8);
            foodGrid.startCorner = UnityEngine.UI.GridLayoutGroup.Corner.UpperLeft;
            foodGrid.startAxis = UnityEngine.UI.GridLayoutGroup.Axis.Horizontal;
            foodGrid.childAlignment = TextAnchor.UpperCenter;
            foodGrid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.Flexible;

            var equipTabRoot = new GameObject("EquipmentTab");
            equipTabRoot.transform.SetParent(body.transform, false);
            var equipTabRt = equipTabRoot.AddComponent<RectTransform>();
            equipTabRt.anchorMin = Vector2.zero;
            equipTabRt.anchorMax = Vector2.one;
            equipTabRt.offsetMin = Vector2.zero;
            equipTabRt.offsetMax = Vector2.zero;

            var equipGridHolder = new GameObject("EquipGrid");
            equipGridHolder.transform.SetParent(equipTabRoot.transform, false);
            var equipGridRt = equipGridHolder.AddComponent<RectTransform>();
            equipGridRt.anchorMin = Vector2.zero;
            equipGridRt.anchorMax = Vector2.one;
            equipGridRt.offsetMin = new Vector2(8, 8);
            equipGridRt.offsetMax = new Vector2(-8, -8);
            var equipGrid = equipGridHolder.AddComponent<UnityEngine.UI.GridLayoutGroup>();
            equipGrid.cellSize = new Vector2(260, 170);
            equipGrid.spacing = new Vector2(24, 24);
            equipGrid.padding = new RectOffset(16, 16, 16, 16);
            equipGrid.childAlignment = TextAnchor.UpperCenter;

            var statusObj = new GameObject("Status");
            statusObj.transform.SetParent(rootPanel.transform, false);
            var statusTxt = statusObj.AddComponent<UnityEngine.UI.Text>();
            statusTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusTxt.fontSize = 22;
            statusTxt.color = new Color(0.85f, 0.9f, 1f);
            statusTxt.alignment = TextAnchor.LowerLeft;
            var statusRt = statusTxt.rectTransform;
            statusRt.anchorMin = new Vector2(0, 0);
            statusRt.anchorMax = new Vector2(0.65f, 0);
            statusRt.pivot = new Vector2(0, 0);
            statusRt.anchoredPosition = new Vector2(32, 72);
            statusRt.sizeDelta = new Vector2(800, 40);

            var closeBtn = CreateOrderButton(rootPanel.transform, "ЗАКРЫТЬ (ESC)", Vector2.zero, new Color(0.75f, 0.25f, 0.22f));
            var closeRect = closeBtn.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1, 0);
            closeRect.anchorMax = new Vector2(1, 0);
            closeRect.pivot = new Vector2(1, 0);
            closeRect.anchoredPosition = new Vector2(-32, 24);
            closeRect.sizeDelta = new Vector2(220, 56);

            var terminalUI = uiCanvasObj.AddComponent<SupermarketSim.Gameplay.ComputerTerminalUI>();
            terminalUI.uiCanvas = uiCanvasObj;
            terminalUI.foodTabRoot = foodTabRoot;
            terminalUI.equipmentTabRoot = equipTabRoot;
            terminalUI.tabFoodButton = tabFoodBtn.GetComponent<UnityEngine.UI.Button>();
            terminalUI.tabEquipmentButton = tabEquipBtn.GetComponent<UnityEngine.UI.Button>();
            terminalUI.btnClose = closeBtn.GetComponent<UnityEngine.UI.Button>();
            terminalUI.statusText = statusTxt;
            terminalUI.headerMoneyText = moneyHeaderTxt;

            var dZone = GameObject.Find("DeliveryZone");
            if (dZone != null) terminalUI.deliveryZone = dZone.transform;

            var shelvesT = GameObject.Find("Shelves")?.transform;
            terminalUI.shelvesParentOverride = shelvesT;
            var checkoutPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParty/Kenney_MiniMarket/Models/OBJ format/cash-register.obj");
            terminalUI.cashRegisterPrefab = checkoutPrefab;
            terminalUI.checkoutSpawnParentOverride = GameObject.Find("Checkout")?.transform;

            var foodData = new (string name, string file, int price)[] {
                ("Хлеб", "bread_and_cream.prefab", 12),
                ("Хлопья", "breakfast_cereal.prefab", 14),
                ("Шоколад", "chocolate_bar.prefab", 10),
                ("Пудинг", "flan.prefab", 16),
                ("Ореховая паста", "hazelnut_cream.prefab", 18),
                ("Мороженое", "ice_cream.prefab", 15),
                ("Сок", "juice_box.prefab", 12),
                ("Молоко", "milk_box.prefab", 13),
                ("Начос", "nachos.prefab", 11),
                ("Сироп", "pancake_syrup.prefab", 14),
                ("Арахис. паста", "peanut_butter.prefab", 15),
                ("Кола", "soda_bottle_cola.prefab", 9),
                ("Виногр. газ.", "soda_bottle_grape.prefab", 9),
                ("Гуарана", "soda_bottle_guarana.prefab", 9),
                ("Вода", "water_bottle.prefab", 8),
            };

            foreach (var data in foodData)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Low Poly Cartoon Food and Groceries Pack/Prefabs/" + data.file);
                if (prefab == null) continue;

                var item = new SupermarketSim.Gameplay.FoodCatalogItem {
                    displayName = data.name,
                    prefab = prefab,
                    price = data.price,
                    icon = GetIconTextureFromPrefab(prefab)
                };
                terminalUI.catalog.Add(item);
                CreateFoodShopCard(foodGridHolder.transform, terminalUI, item);
            }

            Texture shelfTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ThirdParty/Kenney_PrototypeTextures/PNG/Dark/texture_01.png");
            var shelfEquip = new SupermarketSim.Gameplay.EquipmentCatalogItem {
                displayName = "Стеллаж",
                price = 85,
                icon = shelfTex,
                kind = SupermarketSim.Gameplay.EquipmentCatalogItem.EquipmentKind.Shelf
            };
            var regEquip = new SupermarketSim.Gameplay.EquipmentCatalogItem {
                displayName = "Касса",
                price = 120,
                icon = checkoutPrefab != null ? GetIconTextureFromPrefab(checkoutPrefab) : shelfTex,
                kind = SupermarketSim.Gameplay.EquipmentCatalogItem.EquipmentKind.CashRegister
            };
            terminalUI.equipmentCatalog.Add(shelfEquip);
            terminalUI.equipmentCatalog.Add(regEquip);
            CreateEquipmentShopCard(equipGridHolder.transform, terminalUI, shelfEquip);
            CreateEquipmentShopCard(equipGridHolder.transform, terminalUI, regEquip);

            interactable.terminalUI = terminalUI;
            uiCanvasObj.SetActive(false);
        }

        private static UnityEngine.UI.Button CreateOrderButton(Transform parent, string text, Vector2 pos, Color? color = null)
        {
            var btnObj = new GameObject("Btn_" + text);
            btnObj.transform.SetParent(parent, false);
            var img = btnObj.AddComponent<UnityEngine.UI.Image>();
            img.color = color ?? new Color(0.2f, 0.6f, 0.2f);
            var btn = btnObj.AddComponent<UnityEngine.UI.Button>();

            var rect = img.rectTransform;
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(160, 60);

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            var txt = textObj.AddComponent<UnityEngine.UI.Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = text;
            txt.fontSize = 24;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            
            var txtRect = txt.rectTransform;
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.sizeDelta = Vector2.zero;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;

            return btn;
        }

        private static Texture GetIconTextureFromPrefab(GameObject prefab)
        {
            if (prefab == null) return null;
            foreach (var r in prefab.GetComponentsInChildren<Renderer>(true))
            {
                foreach (var mat in r.sharedMaterials)
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

        private static GameObject CreateTabButton(Transform parent, string label, Vector2 anchoredPosition)
        {
            var go = new GameObject("Tab_" + label);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0.15f, 0.15f, 0.2f);
            go.AddComponent<UnityEngine.UI.Button>();
            var rt = img.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = new Vector2(300, 48);

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(go.transform, false);
            var txt = textObj.AddComponent<UnityEngine.UI.Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = label;
            txt.fontSize = 22;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            var tr = txt.rectTransform;
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;
            return go;
        }

        private static void CreateFoodShopCard(Transform parent, SupermarketSim.Gameplay.ComputerTerminalUI ui, SupermarketSim.Gameplay.FoodCatalogItem item)
        {
            var card = new GameObject("Card_" + item.displayName);
            card.transform.SetParent(parent, false);
            card.AddComponent<UnityEngine.UI.Image>().color = new Color(0.18f, 0.2f, 0.26f);
            var pad = card.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            pad.padding = new RectOffset(10, 10, 10, 10);
            pad.spacing = 6;
            pad.childAlignment = TextAnchor.UpperCenter;
            pad.childControlHeight = true;
            pad.childForceExpandHeight = false;

            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(card.transform, false);
            var raw = iconGo.AddComponent<UnityEngine.UI.RawImage>();
            raw.texture = item.icon;
            raw.uvRect = new Rect(0, 0, 1, 1);
            raw.color = item.icon != null ? Color.white : new Color(0.3f, 0.3f, 0.35f);
            var leIcon = iconGo.AddComponent<UnityEngine.UI.LayoutElement>();
            leIcon.preferredHeight = 120;
            leIcon.preferredWidth = 120;
            leIcon.minHeight = 120;

            var nameGo = new GameObject("Name");
            nameGo.transform.SetParent(card.transform, false);
            var nt = nameGo.AddComponent<UnityEngine.UI.Text>();
            nt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nt.text = item.displayName;
            nt.fontSize = 20;
            nt.fontStyle = FontStyle.Bold;
            nt.color = Color.white;
            nt.alignment = TextAnchor.MiddleCenter;
            nameGo.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 28;

            var priceGo = new GameObject("Price");
            priceGo.transform.SetParent(card.transform, false);
            var pt = priceGo.AddComponent<UnityEngine.UI.Text>();
            pt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            pt.text = $"Цена: {item.price} $";
            pt.fontSize = 18;
            pt.color = new Color(1f, 0.85f, 0.35f);
            pt.alignment = TextAnchor.MiddleCenter;
            priceGo.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 24;

            var buyGo = new GameObject("Buy");
            buyGo.transform.SetParent(card.transform, false);
            buyGo.AddComponent<UnityEngine.UI.Image>().color = new Color(0.25f, 0.55f, 0.3f);
            var buyBtn = buyGo.AddComponent<UnityEngine.UI.Button>();
            buyGo.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 40;
            var bt = new GameObject("Txt");
            bt.transform.SetParent(buyGo.transform, false);
            var btx = bt.AddComponent<UnityEngine.UI.Text>();
            btx.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btx.text = "Купить";
            btx.fontSize = 20;
            btx.alignment = TextAnchor.MiddleCenter;
            btx.color = Color.white;
            var btr = btx.rectTransform;
            btr.anchorMin = Vector2.zero;
            btr.anchorMax = Vector2.one;
            btr.offsetMin = Vector2.zero;
            btr.offsetMax = Vector2.zero;

            buyBtn.onClick.AddListener(() => ui.TryBuyFood(item));
        }

        private static void CreateEquipmentShopCard(Transform parent, SupermarketSim.Gameplay.ComputerTerminalUI ui, SupermarketSim.Gameplay.EquipmentCatalogItem item)
        {
            var card = new GameObject("EquipCard_" + item.displayName);
            card.transform.SetParent(parent, false);
            card.AddComponent<UnityEngine.UI.Image>().color = new Color(0.16f, 0.18f, 0.24f);
            var pad = card.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            pad.padding = new RectOffset(12, 12, 12, 12);
            pad.spacing = 8;
            pad.childAlignment = TextAnchor.UpperCenter;

            var nameGo = new GameObject("Name");
            nameGo.transform.SetParent(card.transform, false);
            var nt = nameGo.AddComponent<UnityEngine.UI.Text>();
            nt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nt.text = item.displayName;
            nt.fontSize = 24;
            nt.fontStyle = FontStyle.Bold;
            nt.color = Color.white;
            nt.alignment = TextAnchor.MiddleCenter;
            nameGo.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 38;

            var buyGo = new GameObject("Buy");
            buyGo.transform.SetParent(card.transform, false);
            buyGo.AddComponent<UnityEngine.UI.Image>().color = new Color(0.28f, 0.4f, 0.75f);
            var buyBtn = buyGo.AddComponent<UnityEngine.UI.Button>();
            buyGo.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 52;
            var bt = new GameObject("Txt");
            bt.transform.SetParent(buyGo.transform, false);
            var btx = bt.AddComponent<UnityEngine.UI.Text>();
            btx.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btx.text = "Купить";
            btx.fontSize = 22;
            btx.alignment = TextAnchor.MiddleCenter;
            btx.color = Color.white;
            var btr = btx.rectTransform;
            btr.anchorMin = Vector2.zero;
            btr.anchorMax = Vector2.one;
            btr.offsetMin = Vector2.zero;
            btr.offsetMax = Vector2.zero;

            buyBtn.onClick.AddListener(() => ui.TryBuyEquipment(item));
        }

        private static void BuildFloor(Transform root)
        {
            var floorParent = new GameObject("Floor").transform;
            floorParent.SetParent(root);

            // Instead of individual tiles, use a single large plane with a nice texture
            var floorPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floorPlane.name = "StoreFloor";
            floorPlane.transform.SetParent(floorParent);
            floorPlane.transform.position = new Vector3(0, 0, 0);
            floorPlane.transform.localScale = new Vector3(4.4f, 1f, 4.4f); // 44x44 meters

            var renderer = floorPlane.GetComponent<Renderer>();
            Shader targetShader = Shader.Find("HDRP/Lit");
            if (targetShader == null || !targetShader.isSupported) targetShader = Shader.Find("Universal Render Pipeline/Lit");
            if (targetShader == null || !targetShader.isSupported) targetShader = Shader.Find("Standard");
            
            Material floorMat = new Material(targetShader);
            
            // Load nice dark prototype texture
            string texPath = "Assets/ThirdParty/Kenney_PrototypeTextures/PNG/Dark/texture_01.png";
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (tex != null)
            {
                if (floorMat.HasProperty("_BaseColorMap")) floorMat.SetTexture("_BaseColorMap", tex);
                if (floorMat.HasProperty("_BaseMap")) floorMat.SetTexture("_BaseMap", tex);
                if (floorMat.HasProperty("_MainTex")) floorMat.SetTexture("_MainTex", tex);
                
                // Tile the texture
                if (floorMat.HasProperty("_BaseColorMap")) floorMat.SetTextureScale("_BaseColorMap", new Vector2(11f, 11f));
                if (floorMat.HasProperty("_BaseMap")) floorMat.SetTextureScale("_BaseMap", new Vector2(11f, 11f));
                if (floorMat.HasProperty("_MainTex")) floorMat.SetTextureScale("_MainTex", new Vector2(11f, 11f));
            }
            else
            {
                if (floorMat.HasProperty("_BaseColor")) floorMat.SetColor("_BaseColor", new Color(0.2f, 0.2f, 0.2f));
                if (floorMat.HasProperty("_Color")) floorMat.SetColor("_Color", new Color(0.2f, 0.2f, 0.2f));
            }
            
            renderer.material = floorMat;
        }

        private static void BuildLighting(Transform root)
        {
            var lightObj = GameObject.Find("Directional Light");
            if (lightObj == null)
            {
                lightObj = new GameObject("Directional Light");
                var light = lightObj.AddComponent<Light>();
                light.type = LightType.Directional;
                light.shadows = LightShadows.Soft;
                light.color = new Color(1f, 0.95f, 0.9f);
                
                var pipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
                if (pipeline != null && pipeline.GetType().Name.Contains("HDRenderPipeline"))
                {
                    light.intensity = 10000f; // HDRP uses Lux
                }
                else
                {
                    light.intensity = 1.5f; // URP/Standard
                }
            }
            lightObj.transform.SetParent(root);
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.sun = lightObj.GetComponent<Light>();
            
            if (RenderSettings.skybox == null)
            {
                RenderSettings.skybox = Resources.GetBuiltinResource<Material>("Default-Skybox.mat");
            }
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;

            // Add Fog to hide the void
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.6f, 0.7f, 0.8f); // Sky-like color
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.025f;
        }

        private static void BuildWalls(Transform root)
        {
            var wallsParent = new GameObject("Walls").transform;
            wallsParent.SetParent(root);

            string wallPath = "Assets/ThirdParty/Kenney_MiniMarket/Models/OBJ format/wall.obj";
            var wallPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(wallPath);

            if (wallPrefab != null)
            {
                // Simple box walls
                for (int x = -5; x <= 5; x++)
                {
                    InstantiateWall(wallPrefab, wallsParent, new Vector3(x * 4f, 0, 22f), Quaternion.identity); // Back wall
                    
                    // Front wall - leave a gap for entrance
                    if (x < -1 || x > 1) 
                    {
                        InstantiateWall(wallPrefab, wallsParent, new Vector3(x * 4f, 0, -22f), Quaternion.Euler(0, 180, 0));
                    }
                }
                for (int z = -5; z <= 5; z++)
                {
                    InstantiateWall(wallPrefab, wallsParent, new Vector3(22f, 0, z * 4f), Quaternion.Euler(0, 90, 0)); // Right wall
                    InstantiateWall(wallPrefab, wallsParent, new Vector3(-22f, 0, z * 4f), Quaternion.Euler(0, -90, 0)); // Left wall
                }
            }
        }

        private static void BuildEntranceDoors(Transform root)
        {
            var doorRoot = new GameObject("EntranceDoors");
            doorRoot.transform.SetParent(root);
            doorRoot.transform.position = new Vector3(0, 0, -22f); // Center of the gap in the front wall

            string windowPath = "Assets/ThirdParty/Kenney_MiniMarket/Models/OBJ format/wall-window.obj";
            var windowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(windowPath);

            if (windowPrefab != null)
            {
                var leftDoor = PrefabUtility.InstantiatePrefab(windowPrefab) as GameObject;
                leftDoor.name = "LeftDoor";
                leftDoor.transform.SetParent(doorRoot.transform);
                leftDoor.transform.localPosition = new Vector3(-2f, 0, 0);
                leftDoor.transform.localRotation = Quaternion.Euler(0, 180, 0);
                leftDoor.transform.localScale = Vector3.one * 4f;
                
                var colL = leftDoor.AddComponent<BoxCollider>();
                colL.center = new Vector3(0, 0.5f, 0);
                colL.size = new Vector3(1f, 1f, 0.2f);

                var rightDoor = PrefabUtility.InstantiatePrefab(windowPrefab) as GameObject;
                rightDoor.name = "RightDoor";
                rightDoor.transform.SetParent(doorRoot.transform);
                rightDoor.transform.localPosition = new Vector3(2f, 0, 0);
                rightDoor.transform.localRotation = Quaternion.Euler(0, 180, 0);
                rightDoor.transform.localScale = Vector3.one * 4f;
                
                var colR = rightDoor.AddComponent<BoxCollider>();
                colR.center = new Vector3(0, 0.5f, 0);
                colR.size = new Vector3(1f, 1f, 0.2f);

                var prox = doorRoot.AddComponent<SupermarketSim.Gameplay.ProximityDoor>();
                prox.leftDoor = leftDoor.transform;
                prox.rightDoor = rightDoor.transform;
                prox.openDistance = 6f;
                prox.slideAmount = 3.5f;
            }
        }

        private static void BuildStreet(Transform root)
        {
            var streetRoot = new GameObject("Street").transform;
            streetRoot.SetParent(root);

            // 1. Grass
            var grass = GameObject.CreatePrimitive(PrimitiveType.Plane);
            grass.name = "Grass";
            grass.transform.SetParent(streetRoot);
            grass.transform.position = new Vector3(0, -0.01f, -50f);
            grass.transform.localScale = new Vector3(15f, 1f, 8f); // Large area outside
            
            var renderer = grass.GetComponent<Renderer>();
            Shader targetShader = Shader.Find("HDRP/Lit");
            if (targetShader == null || !targetShader.isSupported) targetShader = Shader.Find("Universal Render Pipeline/Lit");
            if (targetShader == null || !targetShader.isSupported) targetShader = Shader.Find("Standard");
            Material grassMat = new Material(targetShader);
            grassMat.color = new Color(0.3f, 0.6f, 0.3f);
            if (grassMat.HasProperty("_BaseColor")) grassMat.SetColor("_BaseColor", new Color(0.3f, 0.6f, 0.3f));
            renderer.material = grassMat;

            // 2. Road
            string roadPath = "Assets/ThirdParty/Kenney_CityKitRoads/Models/OBJ format/road-straight.obj";
            var roadPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(roadPath);
            if (roadPrefab != null)
            {
                // Only build road directly in front of the store, not extending infinitely
                for (int x = -5; x <= 5; x++)
                {
                    var road = PrefabUtility.InstantiatePrefab(roadPrefab) as GameObject;
                    road.transform.SetParent(streetRoot);
                    road.transform.position = new Vector3(x * 10f, 0.05f, -35f);
                    road.transform.localRotation = Quaternion.Euler(0, 90, 0);
                    road.transform.localScale = Vector3.one * 10f; // Scale up to match store
                }
            }

            // 3. Buildings across the street
            string[] buildingNames = new string[] { "building-a.obj", "building-b.obj", "building-c.obj", "building-d.obj" };
            for (int i = 0; i < 6; i++)
            {
                string bName = buildingNames[i % buildingNames.Length];
                string bPath = $"Assets/ThirdParty/Kenney_CityKitCommercial/Models/OBJ format/{bName}";
                var bPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(bPath);
                if (bPrefab != null)
                {
                    var bld = PrefabUtility.InstantiatePrefab(bPrefab) as GameObject;
                    bld.transform.SetParent(streetRoot);
                    bld.transform.position = new Vector3(-30f + i * 12f, 0.05f, -48f); // Across the road
                    bld.transform.localRotation = Quaternion.Euler(0, 0, 0); // Face the store
                    bld.transform.localScale = Vector3.one * 10f;
                }
            }

            BuildCityBackdrop(streetRoot);

            // 4. Street Decorations (Lights, Benches, Trashcans)
            string lightPath = "Assets/ThirdParty/Kenney_CityKitRoads/Models/OBJ format/light-curved.obj";
            string trashPath = "Assets/ThirdParty/Kenney_FurniturePack/Models/trashcan.obj";
            string benchPath = "Assets/ThirdParty/Kenney_FurniturePack/Models/bench.obj";
            
            var lightPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(lightPath);
            var trashPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(trashPath);
            var benchPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(benchPath);

            for (int x = -4; x <= 4; x += 2)
            {
                // Streetlights
                if (lightPrefab != null)
                {
                    var light = PrefabUtility.InstantiatePrefab(lightPrefab) as GameObject;
                    light.transform.SetParent(streetRoot);
                    light.transform.position = new Vector3(x * 10f, 0.05f, -28f); // On the sidewalk near the store
                    light.transform.localRotation = Quaternion.Euler(0, 180, 0); // Face the road
                    light.transform.localScale = Vector3.one * 10f;
                }

                // Benches
                if (benchPrefab != null)
                {
                    var bench = PrefabUtility.InstantiatePrefab(benchPrefab) as GameObject;
                    bench.transform.SetParent(streetRoot);
                    bench.transform.position = new Vector3(x * 10f + 5f, 0.05f, -26f); // Slightly offset from lights
                    bench.transform.localRotation = Quaternion.Euler(0, 0, 0); // Face the store
                    bench.transform.localScale = Vector3.one * 4f;
                }

                // Trashcans
                if (trashPrefab != null)
                {
                    var trash = PrefabUtility.InstantiatePrefab(trashPrefab) as GameObject;
                    trash.transform.SetParent(streetRoot);
                    trash.transform.position = new Vector3(x * 10f + 2f, 0.05f, -27f); // Next to lights
                    trash.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    trash.transform.localScale = Vector3.one * 4f;
                }
            }
        }

        private static void BuildCityBackdrop(Transform streetRoot)
        {
            var oldBackdrop = streetRoot.Find("CityBackdrop");
            if (oldBackdrop != null)
                Object.DestroyImmediate(oldBackdrop.gameObject);

            var texture1 = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/city_skyline_silhouette.png");
            var texture2 = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/city_skyline_silhouette_2.png");
            var texture3 = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/city_skyline_silhouette_3.png");
            if (texture1 == null)
                texture1 = CreateGeneratedCitySkylineTexture(0);
            if (texture2 == null)
                texture2 = CreateGeneratedCitySkylineTexture(1);
            if (texture3 == null)
                texture3 = CreateGeneratedCitySkylineTexture(2);

            var root = new GameObject("CityBackdrop").transform;
            root.SetParent(streetRoot);

            CreateCityBackdropPanel(root, "CityBackdrop_Front", new Vector3(0f, 12f, -84f), Quaternion.identity, new Vector3(150f, 30f, 1f), CreateCityBackdropMaterial(texture1));
            CreateCityBackdropPanel(root, "CityBackdrop_Left", new Vector3(-78f, 10f, -48f), Quaternion.Euler(0f, 90f, 0f), new Vector3(90f, 24f, 1f), CreateCityBackdropMaterial(texture2 != null ? texture2 : texture1));
            CreateCityBackdropPanel(root, "CityBackdrop_Right", new Vector3(78f, 10f, -48f), Quaternion.Euler(0f, -90f, 0f), new Vector3(90f, 24f, 1f), CreateCityBackdropMaterial(texture3 != null ? texture3 : texture1));
        }

        private static void CreateCityBackdropPanel(Transform root, string name, Vector3 position, Quaternion rotation, Vector3 scale, Material mat)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.SetParent(root);
            quad.transform.position = position;
            quad.transform.rotation = rotation;
            quad.transform.localScale = scale;

            var col = quad.GetComponent<Collider>();
            if (col != null)
                Object.DestroyImmediate(col);

            var renderer = quad.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = mat;
        }

        private static Material CreateCityBackdropMaterial(Texture texture)
        {
            Shader shader = Shader.Find("HDRP/Unlit");
            if (shader == null || !shader.isSupported) shader = Shader.Find("Unlit/Transparent");
            if (shader == null || !shader.isSupported) shader = Shader.Find("Standard");

            var mat = new Material(shader);
            if (mat.HasProperty("_BaseColorMap")) mat.SetTexture("_BaseColorMap", texture);
            if (mat.HasProperty("_UnlitColorMap")) mat.SetTexture("_UnlitColorMap", texture);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", texture);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);
            if (mat.HasProperty("_SurfaceType")) mat.SetFloat("_SurfaceType", 1f);
            if (mat.HasProperty("_BlendMode")) mat.SetFloat("_BlendMode", 0f);
            if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            return mat;
        }

        private static Texture2D CreateGeneratedCitySkylineTexture(int variant)
        {
            const int width = 1024;
            const int height = 432;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.name = "GeneratedCitySkyline_" + variant;
            var pixels = new Color32[width * height];
            var clear = new Color32(0, 0, 0, 0);
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = clear;

            var dark = new Color32(18, 24, 28, 240);
            var buildings = variant == 1
                ? new[] { new RectInt(0, 235, 95, 185), new RectInt(110, 150, 70, 270), new RectInt(195, 250, 90, 170), new RectInt(300, 205, 60, 215), new RectInt(380, 115, 85, 305), new RectInt(485, 245, 85, 175), new RectInt(590, 160, 60, 260), new RectInt(670, 220, 100, 200), new RectInt(790, 180, 85, 240), new RectInt(890, 260, 134, 160) }
                : variant == 2
                    ? new[] { new RectInt(0, 280, 130, 140), new RectInt(145, 210, 80, 210), new RectInt(240, 245, 75, 175), new RectInt(335, 160, 55, 260), new RectInt(410, 225, 115, 195), new RectInt(545, 130, 75, 290), new RectInt(640, 255, 80, 165), new RectInt(740, 190, 90, 230), new RectInt(850, 235, 90, 185), new RectInt(960, 170, 64, 250) }
                    : new[] { new RectInt(0, 260, 120, 160), new RectInt(130, 220, 80, 200), new RectInt(225, 170, 75, 250), new RectInt(310, 245, 85, 175), new RectInt(410, 135, 60, 285), new RectInt(485, 230, 80, 190), new RectInt(580, 185, 100, 235), new RectInt(700, 255, 105, 165), new RectInt(825, 205, 85, 215), new RectInt(930, 150, 80, 270) };

            foreach (var b in buildings)
                FillGeneratedSkylineRect(pixels, width, height, b.x, b.y, b.width, b.height, dark);
            FillGeneratedSkylineRect(pixels, width, height, 0, 420, width, 12, dark);
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private static void FillGeneratedSkylineRect(Color32[] pixels, int width, int height, int x, int y, int w, int h, Color32 color)
        {
            int xMin = Mathf.Clamp(x, 0, width);
            int xMax = Mathf.Clamp(x + w, 0, width);
            int yMin = Mathf.Clamp(y, 0, height);
            int yMax = Mathf.Clamp(y + h, 0, height);
            for (int yy = yMin; yy < yMax; yy++)
            {
                int row = yy * width;
                for (int xx = xMin; xx < xMax; xx++)
                    pixels[row + xx] = color;
            }
        }


        private static void BuildStoreDecorations(Transform root)
        {
            var decorRoot = new GameObject("StoreDecorations").transform;
            decorRoot.SetParent(root);

            // Load Prefabs
            var plantPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParty/Kenney_FurniturePack/Models/pottedPlant.obj");
            var freezerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParty/Kenney_MiniMarket/Models/OBJ format/freezers-standing.obj");
            var cartPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParty/Kenney_MiniMarket/Models/OBJ format/shopping-cart.obj");
            var lampPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParty/Kenney_FurniturePack/Models/lampSquareCeiling.obj");

            // 1. Plants in corners
            if (plantPrefab != null)
            {
                Vector3[] plantPositions = new Vector3[] {
                    new Vector3(-18f, 0, 18f),
                    new Vector3(18f, 0, 18f),
                    new Vector3(18f, 0, -18f)
                };

                foreach (var pos in plantPositions)
                {
                    var plant = PrefabUtility.InstantiatePrefab(plantPrefab) as GameObject;
                    plant.transform.SetParent(decorRoot);
                    plant.transform.position = pos;
                    plant.transform.localScale = Vector3.one * 5f; // Scale up
                    
                    var col = plant.AddComponent<BoxCollider>();
                    col.center = new Vector3(0, 0.5f, 0);
                    col.size = new Vector3(0.6f, 1f, 0.6f);
                }
            }

            // 2. Freezers along the left wall
            if (freezerPrefab != null)
            {
                for (int z = 0; z < 3; z++)
                {
                    var freezer = PrefabUtility.InstantiatePrefab(freezerPrefab) as GameObject;
                    freezer.transform.SetParent(decorRoot);
                    freezer.transform.position = new Vector3(-20f, 0, -5f + z * 8f);
                    freezer.transform.localRotation = Quaternion.Euler(0, 90, 0); // Face inward
                    freezer.transform.localScale = Vector3.one * 4f;

                    var col = freezer.AddComponent<BoxCollider>();
                    col.center = new Vector3(0, 0.5f, 0);
                    col.size = new Vector3(1f, 1f, 1f);
                }
            }

            // 3. Shopping Carts near entrance
            if (cartPrefab != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    var cart = PrefabUtility.InstantiatePrefab(cartPrefab) as GameObject;
                    cart.transform.SetParent(decorRoot);
                    cart.transform.position = new Vector3(-8f - (i * 1.5f), 0, -18f);
                    cart.transform.localRotation = Quaternion.Euler(0, 160 + Random.Range(-10f, 10f), 0);
                    cart.transform.localScale = Vector3.one * 4f;

                    var col = cart.AddComponent<BoxCollider>();
                    col.center = new Vector3(0, 0.5f, 0);
                    col.size = new Vector3(1f, 1f, 1f);
                }
            }

            // 4. Ceiling Lights
            if (lampPrefab != null)
            {
                for (int x = -10; x <= 10; x += 10)
                {
                    for (int z = -10; z <= 10; z += 10)
                    {
                        var lamp = PrefabUtility.InstantiatePrefab(lampPrefab) as GameObject;
                        lamp.transform.SetParent(decorRoot);
                        lamp.transform.position = new Vector3(x, 3.9f, z); // Just below ceiling
                        lamp.transform.localScale = Vector3.one * 4f;
                    }
                }
            }
        }

        private static void BuildRoof(Transform root)
        {
            var roofParent = new GameObject("Roof").transform;
            roofParent.SetParent(root);
            
            string roofPath = "Assets/ThirdParty/Kenney_BuildingKit/Models/OBJ format/roof-flat-center.obj";
            var roofPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(roofPath);

            if (roofPrefab != null)
            {
                for (int x = -5; x <= 5; x++)
                {
                    for (int z = -5; z <= 5; z++)
                    {
                        var tile = PrefabUtility.InstantiatePrefab(roofPrefab) as GameObject;
                        tile.transform.SetParent(roofParent);
                        tile.transform.position = new Vector3(x * 4f, 4f, z * 4f); // 4f is the height of the walls
                        tile.transform.localScale = Vector3.one * 2f; // The roof tile is 2x2, so scale by 2 makes it 4x4
                        tile.isStatic = true;
                        
                        // Fix pink material
                        var renderer = tile.GetComponentInChildren<Renderer>();
                        if (renderer != null)
                        {
                            Shader targetShader = Shader.Find("HDRP/Lit");
                            if (targetShader == null || !targetShader.isSupported) targetShader = Shader.Find("Universal Render Pipeline/Lit");
                            if (targetShader == null || !targetShader.isSupported) targetShader = Shader.Find("Standard");
                            
                            Material[] mats = renderer.sharedMaterials;
                            for (int m = 0; m < mats.Length; m++)
                            {
                                if (mats[m] != null && mats[m].shader.name != targetShader.name)
                                {
                                    Material newMat = new Material(targetShader);
                                    if (mats[m].HasProperty("_Color")) newMat.SetColor("_BaseColor", mats[m].GetColor("_Color"));
                                    if (mats[m].HasProperty("_BaseColor")) newMat.SetColor("_BaseColor", mats[m].GetColor("_BaseColor"));
                                    mats[m] = newMat;
                                }
                            }
                            renderer.sharedMaterials = mats;
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("Could not find roof prefab. Using primitive cubes instead.");
                var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
                roof.name = "Roof";
                roof.transform.SetParent(roofParent);
                roof.transform.position = new Vector3(0, 4.2f, 0);
                roof.transform.localScale = new Vector3(44f, 0.4f, 44f);
                
                var renderer = roof.GetComponent<Renderer>();
                Shader targetShader = Shader.Find("HDRP/Lit");
                if (targetShader == null || !targetShader.isSupported) targetShader = Shader.Find("Universal Render Pipeline/Lit");
                if (targetShader == null || !targetShader.isSupported) targetShader = Shader.Find("Standard");
                Material roofMat = new Material(targetShader);
                roofMat.color = new Color(0.2f, 0.2f, 0.2f);
                if (roofMat.HasProperty("_BaseColor")) roofMat.SetColor("_BaseColor", new Color(0.2f, 0.2f, 0.2f));
                renderer.material = roofMat;
            }
        }

        private static void InstantiateWall(GameObject prefab, Transform parent, Vector3 pos, Quaternion rot)
        {
            var wall = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            wall.transform.SetParent(parent);
            wall.transform.position = pos;
            wall.transform.rotation = rot;
            wall.transform.localScale = Vector3.one * 4f;
            wall.isStatic = true;
            
            var col = wall.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 0.5f, 0);
            col.size = new Vector3(1f, 1f, 1f);
        }

        private static void BuildShelving(Transform root)
        {
            var shelvesParent = new GameObject("Shelves").transform;
            shelvesParent.SetParent(root);

            // Create a basic material for the shelves so they aren't default white
            Material shelfMat = null;
            Shader shader = Shader.Find("HDRP/Lit");
            if (shader == null || !shader.isSupported) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null || !shader.isSupported) shader = Shader.Find("Standard");
            
            if (shader != null)
            {
                shelfMat = new Material(shader);
                if (shelfMat.HasProperty("_BaseColor"))
                    shelfMat.SetColor("_BaseColor", new Color(0.8f, 0.8f, 0.8f)); // HDRP/URP
                else if (shelfMat.HasProperty("_Color"))
                    shelfMat.SetColor("_Color", new Color(0.8f, 0.8f, 0.8f)); // Standard
            }

            int shelfIndex = 0;

            // Back wall (Z = 20) - Only keep Shelf_0 to Shelf_3
            for (int i = 0; i < 4; i++)
            {
                CreateShelf(shelvesParent, shelfMat, ref shelfIndex, new Vector3(-12f + i * 8f, 0, 20f), Quaternion.identity);
            }
        }

        private static void CreateShelf(Transform parent, Material mat, ref int index, Vector3 pos, Quaternion rot)
        {
            var shelfRoot = new GameObject($"Shelf_{index++}");
            shelfRoot.transform.SetParent(parent);
            shelfRoot.transform.position = pos;
            shelfRoot.transform.rotation = rot;
            shelfRoot.transform.localScale = Vector3.one * 1.96f; // Reduced by another 30% from 2.8f
            shelfRoot.isStatic = true;

            // ShelfLayer1
            var layer1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            layer1.name = "ShelfLayer1";
            layer1.transform.SetParent(shelfRoot.transform);
            layer1.transform.localPosition = new Vector3(0, 0.5f, 0);
            layer1.transform.localScale = new Vector3(2f, 0.05f, 0.3f);
            if (mat != null) layer1.GetComponent<Renderer>().sharedMaterial = mat;
            layer1.isStatic = true;

            // ShelfLayer2
            var layer2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            layer2.name = "ShelfLayer2";
            layer2.transform.SetParent(shelfRoot.transform);
            layer2.transform.localPosition = new Vector3(0, 1.0f, 0);
            layer2.transform.localScale = new Vector3(2f, 0.05f, 0.3f);
            if (mat != null) layer2.GetComponent<Renderer>().sharedMaterial = mat;
            layer2.isStatic = true;

            // ShelfLayer3
            var layer3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            layer3.name = "ShelfLayer3";
            layer3.transform.SetParent(shelfRoot.transform);
            layer3.transform.localPosition = new Vector3(0, 1.5f, 0);
            layer3.transform.localScale = new Vector3(2f, 0.05f, 0.3f);
            if (mat != null) layer3.GetComponent<Renderer>().sharedMaterial = mat;
            layer3.isStatic = true;

            // LeftSupport
            var leftSupport = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftSupport.name = "LeftSupport";
            leftSupport.transform.SetParent(shelfRoot.transform);
            leftSupport.transform.localPosition = new Vector3(-0.9f, 0.75f, 0);
            leftSupport.transform.localScale = new Vector3(0.1f, 1.5f, 0.3f);
            if (mat != null) leftSupport.GetComponent<Renderer>().sharedMaterial = mat;
            leftSupport.isStatic = true;

            // RightSupport
            var rightSupport = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightSupport.name = "RightSupport";
            rightSupport.transform.SetParent(shelfRoot.transform);
            rightSupport.transform.localPosition = new Vector3(0.9f, 0.75f, 0);
            rightSupport.transform.localScale = new Vector3(0.1f, 1.5f, 0.3f);
            if (mat != null) rightSupport.GetComponent<Renderer>().sharedMaterial = mat;
            rightSupport.isStatic = true;

            // Add Placement Points
            CreatePlacementPoints(layer1.transform);
            CreatePlacementPoints(layer2.transform);
            CreatePlacementPoints(layer3.transform);
        }

        private static void CreatePlacementPoints(Transform layer)
        {
            // The layer is scaled to (2, 0.05, 0.3) locally.
            // We want 4 points along the X axis.
            for (int i = 0; i < 4; i++)
            {
                var point = new GameObject($"PlacementPoint_{i+1}");
                point.transform.SetParent(layer);
                
                // Local X goes from -0.5 to 0.5 (because of the primitive cube scale).
                // We want 4 points evenly spaced.
                float xOffset = -0.375f + (i * 0.25f);
                
                // Local Y is 0.5 to be on top of the cube (cube center is 0, height is 1, so top is 0.5).
                point.transform.localPosition = new Vector3(xOffset, 0.5f, 0);
                point.transform.localRotation = Quaternion.identity;
                // Reset scale so it's not squashed by the layer's scale
                point.transform.localScale = new Vector3(1f/2f, 1f/0.05f, 1f/0.3f); 

                // Add a small invisible collider for raycasting
                var col = point.AddComponent<BoxCollider>();
                col.size = new Vector3(0.4f, 0.4f, 0.4f);
                col.center = new Vector3(0, 0.2f, 0);
                col.isTrigger = true; // Don't block physics, just raycasts
                
                point.AddComponent<SupermarketSim.Gameplay.ShelfPlacementPoint>();
                point.layer = LayerMask.NameToLayer("Default");
            }
        }

        [MenuItem("Supermarket/Build Player (Game Scene)")]
        public static void BuildPlayer()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.name != "Game")
            {
                Debug.LogWarning("Please open the 'Game' scene first! (Assets/Scenes/Game.unity)");
                return;
            }

            // Remove existing player if it exists
            var existingPlayer = GameObject.Find("Player");
            if (existingPlayer != null)
            {
                Object.DestroyImmediate(existingPlayer);
            }

            // Also remove any stray student models if they exist
            var strayModels = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
            foreach (var go in strayModels)
            {
                if (go.name.ToLower().Contains("student") || go.name.ToLower().Contains("chibi"))
                {
                    Object.DestroyImmediate(go);
                }
            }

            // 1. Create Player Root
            var playerObj = new GameObject("Player");
            playerObj.transform.position = new Vector3(0, 0.15f, 0); // Slightly above floor so CharacterController settles safely.
            
            // 2. Add Character Controller
            var cc = playerObj.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.3f;
            cc.center = new Vector3(0, 0.9f, 0);

            // 3. Create Camera Pivot (for vertical rotation)
            var camPivot = new GameObject("CameraPivot");
            camPivot.transform.SetParent(playerObj.transform);
            camPivot.transform.localPosition = new Vector3(0, 1.6f, 0); // Eye height

            // 4. Create Camera
            var camObj = new GameObject("Main Camera");
            camObj.transform.SetParent(camPivot.transform);
            camObj.transform.localPosition = Vector3.zero; // Inside pivot
            camObj.transform.localRotation = Quaternion.identity;
            var cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            camObj.tag = "MainCamera";

            // 5. Add Scripts
            var fpsController = playerObj.AddComponent<SupermarketSim.Player.FpsPlayerController>();
            fpsController.cameraTransform = camPivot.transform; // Pivot handles vertical rotation
            fpsController.characterController = cc;
            
            var interactor = playerObj.AddComponent<SupermarketSim.Interaction.PlayerInteractor>();
            interactor.mainCamera = cam;
            interactor.interactableLayer = ~0; // Everything
            
            var cashierMode = playerObj.AddComponent<SupermarketSim.Player.PlayerCashierMode>();
            
            var playerCarry = playerObj.AddComponent<SupermarketSim.Player.PlayerCarry>();
            var holdPoint = new GameObject("HoldPoint");
            holdPoint.transform.SetParent(camObj.transform);
            // Position slightly right, down, and forward from camera center
            holdPoint.transform.localPosition = new Vector3(0.5f, -0.4f, 0.8f);
            playerCarry.holdPoint = holdPoint.transform;
            
            var wallet = playerObj.AddComponent<SupermarketSim.Player.PlayerWallet>();
            
            // Temporary input handler until InputActions are fully set up
            playerObj.AddComponent<SupermarketSim.Player.SimpleInputHandler>();

            // --- UI Canvas for Crosshair and Prompt ---
            var canvasObj = new GameObject("PlayerCanvas");
            canvasObj.transform.SetParent(playerObj.transform);
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scalerPlayer = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scalerPlayer.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scalerPlayer.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Money (top-right)
            var moneyObj = new GameObject("MoneyText");
            moneyObj.transform.SetParent(canvasObj.transform, false);
            var moneyText = moneyObj.AddComponent<UnityEngine.UI.Text>();
            moneyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            moneyText.fontSize = 32;
            moneyText.fontStyle = FontStyle.Bold;
            moneyText.color = new Color(0.95f, 0.95f, 0.6f);
            moneyText.alignment = TextAnchor.UpperRight;
            var moneyRect = moneyText.rectTransform;
            moneyRect.anchorMin = new Vector2(1, 1);
            moneyRect.anchorMax = new Vector2(1, 1);
            moneyRect.pivot = new Vector2(1, 1);
            moneyRect.anchoredPosition = new Vector2(-28, -24);
            moneyRect.sizeDelta = new Vector2(420, 48);
            var moneyOutline = moneyObj.AddComponent<UnityEngine.UI.Outline>();
            moneyOutline.effectColor = new Color(0, 0, 0, 0.85f);
            moneyOutline.effectDistance = new Vector2(2, -2);
            wallet.BindLabel(moneyText);

            // Crosshair (Center dot)
            var crosshairObj = new GameObject("Crosshair");
            crosshairObj.transform.SetParent(canvasObj.transform, false);
            var crosshairImg = crosshairObj.AddComponent<UnityEngine.UI.Image>();
            crosshairImg.color = new Color(1f, 1f, 1f, 0.7f); // Semi-transparent white
            var crosshairRect = crosshairImg.rectTransform;
            crosshairRect.anchorMin = new Vector2(0.5f, 0.5f);
            crosshairRect.anchorMax = new Vector2(0.5f, 0.5f);
            crosshairRect.sizeDelta = new Vector2(6f, 6f); // Small dot

            // Prompt Text (Bottom Right)
            var promptObj = new GameObject("PromptText");
            promptObj.transform.SetParent(canvasObj.transform, false);
            var promptText = promptObj.AddComponent<UnityEngine.UI.Text>();
            promptText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            promptText.fontSize = 28;
            promptText.fontStyle = FontStyle.Bold;
            promptText.color = new Color(1f, 0.8f, 0.2f); // Nice golden/yellow color
            
            // Add a subtle shadow for better readability
            var shadow = promptObj.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.8f);
            shadow.effectDistance = new Vector2(2f, -2f);
            
            // Add an outline for even better readability
            var outline = promptObj.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.5f);
            outline.effectDistance = new Vector2(1f, -1f);

            promptText.alignment = TextAnchor.LowerRight;
            var promptRect = promptText.rectTransform;
            promptRect.anchorMin = new Vector2(1, 0);
            promptRect.anchorMax = new Vector2(1, 0);
            promptRect.pivot = new Vector2(1, 0);
            promptRect.anchoredPosition = new Vector2(-50f, 50f); // Offset from bottom right
            promptRect.sizeDelta = new Vector2(600f, 100f);

            // Link UI to interactor
            interactor.promptText = promptText;

            // Ensure EventSystem exists
            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                // Use the new Input System UI module instead of the old StandaloneInputModule
                eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            Debug.Log("First-Person Player built successfully!");
        }

        [MenuItem("Supermarket/Spawn Test Food")]
        public static void SpawnTestFood()
        {
            var env = GameObject.Find("Environment");
            if (env == null)
            {
                Debug.LogWarning("Could not find 'Environment' object in the scene. Please build environment first.");
                return;
            }

            var oldFood = env.transform.Find("TestFood");
            if (oldFood != null)
                Object.DestroyImmediate(oldFood.gameObject);

            string[] foodPrefabNames = new string[]
            {
                "bread_and_cream.prefab",
                "breakfast_cereal.prefab",
                "chocolate_bar.prefab",
                "flan.prefab",
                "hazelnut_cream.prefab",
                "ice_cream.prefab",
                "juice_box.prefab",
                "milk_box.prefab",
                "nachos.prefab",
                "pancake_syrup.prefab",
                "peanut_butter.prefab",
                "soda_bottle_cola.prefab",
                "soda_bottle_grape.prefab",
                "soda_bottle_guarana.prefab",
                "water_bottle.prefab"
            };

            string basePath = "Assets/Low Poly Cartoon Food and Groceries Pack/Prefabs/";

            var foodParent = new GameObject("TestFood");
            foodParent.transform.SetParent(env.transform);

            // Find target HDRP/URP shader for fixing pink materials
            Shader targetShader = Shader.Find("HDRP/Lit");
            if (targetShader == null || !targetShader.isSupported) targetShader = Shader.Find("Universal Render Pipeline/Lit");
            if (targetShader == null || !targetShader.isSupported) targetShader = Shader.Find("Standard");

            for (int i = 0; i < foodPrefabNames.Length; i++)
            {
                string fullPath = basePath + foodPrefabNames[i];
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
                if (prefab != null)
                {
                    var foodInst = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    
                    foodInst.transform.SetParent(foodParent.transform);
                    
                    // Fix pink materials (upgrade to HDRP/URP)
                    if (targetShader != null)
                    {
                        var renderers = foodInst.GetComponentsInChildren<Renderer>(true);
                        foreach (var r in renderers)
                        {
                            Material[] mats = r.sharedMaterials;
                            for (int m = 0; m < mats.Length; m++)
                            {
                                Material oldMat = mats[m];
                                if (oldMat != null && oldMat.shader.name != targetShader.name)
                                {
                                    Material newMat = new Material(targetShader);
                                    newMat.name = oldMat.name + "_Fixed";
                                    
                                    // Try to find ANY texture on the old material
                                    Texture tex = null;
                                    if (oldMat.HasProperty("_MainTex")) tex = oldMat.GetTexture("_MainTex");
                                    if (tex == null && oldMat.HasProperty("_BaseMap")) tex = oldMat.GetTexture("_BaseMap");
                                    if (tex == null && oldMat.HasProperty("_BaseColorMap")) tex = oldMat.GetTexture("_BaseColorMap");
                                    
                                    if (tex != null)
                                    {
                                        if (newMat.HasProperty("_BaseColorMap")) newMat.SetTexture("_BaseColorMap", tex); // HDRP
                                        if (newMat.HasProperty("_BaseMap")) newMat.SetTexture("_BaseMap", tex); // URP
                                        if (newMat.HasProperty("_MainTex")) newMat.SetTexture("_MainTex", tex); // Standard
                                    }
                                    
                                    // Try to find ANY color on the old material
                                    Color matCol = Color.white;
                                    bool foundColor = false;
                                    if (oldMat.HasProperty("_Color")) { matCol = oldMat.GetColor("_Color"); foundColor = true; }
                                    if (!foundColor && oldMat.HasProperty("_BaseColor")) { matCol = oldMat.GetColor("_BaseColor"); foundColor = true; }
                                    
                                    if (foundColor)
                                    {
                                        if (newMat.HasProperty("_BaseColor")) newMat.SetColor("_BaseColor", matCol); // HDRP/URP
                                        if (newMat.HasProperty("_Color")) newMat.SetColor("_Color", matCol); // Standard
                                    }
                                    
                                    mats[m] = newMat;
                                }
                            }
                            r.sharedMaterials = mats;
                        }
                    }

                    // Place on the floor in front of the shelves
                    float xOffset = -10f + (i * 1.5f);
                    
                    foodInst.transform.position = new Vector3(xOffset, 0.5f, 0); // On the floor
                    foodInst.transform.rotation = Quaternion.Euler(0, 180, 0);
                    
                    // Scale food appropriately
                    float scaleMultiplier = 2f;
                    string n = foodPrefabNames[i].ToLower();
                    
                    if (n.Contains("cereal") || n.Contains("peanut") || n.Contains("syrup")) scaleMultiplier = 0.8f;
                    if (n.Contains("cola") || n.Contains("grape") || n.Contains("guarana") || n.Contains("milk")) scaleMultiplier = 0.8f;

                    if (n.Contains("ice_cream") || n.Contains("hazelnut")) scaleMultiplier *= 0.5f; // Reduce by 50%
                    if (n.Contains("nachos")) scaleMultiplier *= 0.7f; // Reduce by 30%
                    
                    // All drinks by 30%
                    if (n.Contains("cola") || n.Contains("grape") || n.Contains("guarana") || 
                        n.Contains("juice") || n.Contains("water") || n.Contains("milk"))
                    {
                        scaleMultiplier *= 0.7f;
                    }
                    
                    foodInst.transform.localScale = Vector3.one * scaleMultiplier; 

                    // Add components for picking up
                    var rb = foodInst.GetComponent<Rigidbody>();
                    if (rb == null) rb = foodInst.AddComponent<Rigidbody>();
                    rb.mass = 1f;

                    var col = foodInst.GetComponent<Collider>();
                    if (col == null)
                    {
                        var boxCol = foodInst.AddComponent<BoxCollider>();
                        // Give it a generic size if it doesn't have one
                    }

                    var pickupable = foodInst.AddComponent<SupermarketSim.Gameplay.PickupableItem>();
                    pickupable.itemName = foodPrefabNames[i].Replace(".prefab", "").Replace("_", " ");
                    
                    // Ensure it's on the interactable layer
                    foodInst.layer = LayerMask.NameToLayer("Default");
                }
                else
                {
                    Debug.LogWarning($"Could not find food prefab at {fullPath}");
                }
            }

            Debug.Log("Spawned 5 test food items on the floor!");
        }

        private static void BuildCheckout(Transform root)
        {
            var checkoutParent = new GameObject("Checkout").transform;
            checkoutParent.SetParent(root);

            string checkoutPath = "Assets/ThirdParty/Kenney_MiniMarket/Models/OBJ format/cash-register.obj";
            var checkoutPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(checkoutPath);

            if (checkoutPrefab != null)
            {
                var checkout = PrefabUtility.InstantiatePrefab(checkoutPrefab) as GameObject;
                checkout.transform.SetParent(checkoutParent);
                checkout.transform.position = new Vector3(0, 0, -10f);
                checkout.transform.localScale = Vector3.one * 4f;
                checkout.isStatic = true;
                
                // Add collider
                var col = checkout.AddComponent<BoxCollider>();
                col.center = new Vector3(0, 0.5f, 0);
                col.size = new Vector3(1f, 1f, 1f);
                
                // Setup Cashier Station
                RigCashierDeskUnderCheckout(checkout);
            }
        }

        private static void RigCashierDeskUnderCheckout(GameObject checkout)
        {
            // 1. Create Stand Point
            var standPoint = new GameObject("PlayerStandPoint");
            standPoint.transform.SetParent(checkout.transform.parent, worldPositionStays: true);
            standPoint.transform.position = checkout.transform.position + checkout.transform.rotation * new Vector3(0, 0, -2.4f);
            standPoint.transform.rotation = checkout.transform.rotation;

            // 2. Create Cashier Camera
            var camObj = new GameObject("CashRegisterCamera");
            camObj.transform.SetParent(checkout.transform.parent, worldPositionStays: true);
            camObj.transform.position = checkout.transform.position + checkout.transform.rotation * new Vector3(0, 2.2f, -1.2f);
            camObj.transform.rotation = checkout.transform.rotation * Quaternion.Euler(20, 0, 0);
            var cam = camObj.AddComponent<Camera>();
            var al = camObj.AddComponent<AudioListener>(); // Add AudioListener to Cashier Camera too
            al.enabled = false;
            camObj.SetActive(false);

            // 3. Add Interactable script
            var interactable = checkout.AddComponent<SupermarketSim.Gameplay.CashierStationInteractable>();
            interactable.playerStandPoint = standPoint.transform;
            interactable.cashierCamera = cam;
            
            // Set layer for interaction
            checkout.layer = LayerMask.NameToLayer("Default"); // Ensure it's on a raycastable layer
        }
    }
}