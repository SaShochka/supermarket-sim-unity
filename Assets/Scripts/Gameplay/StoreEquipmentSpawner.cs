using UnityEngine;

namespace SupermarketSim.Gameplay
{
    /// <summary>Runtime spawning for shelves and registers purchased from the terminal (mirrors editor shelf layout).</summary>
    public static class StoreEquipmentSpawner
    {
        private static int _boughtShelfSerial;

        public static GameObject SpawnShelfUnit(Transform shelvesParent, Vector3 worldPosition, Quaternion worldRotation, float unitScale = 1.96f)
        {
            var shelfRoot = new GameObject($"Shelf_Bought_{++_boughtShelfSerial}");
            if (shelvesParent != null)
                shelfRoot.transform.SetParent(shelvesParent);
            shelfRoot.transform.position = worldPosition;
            shelfRoot.transform.rotation = worldRotation;
            shelfRoot.transform.localScale = Vector3.one * unitScale;

            Material shelfMat = CreateShelfMaterial();

            var layer1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            layer1.name = "ShelfLayer1";
            layer1.transform.SetParent(shelfRoot.transform);
            layer1.transform.localPosition = new Vector3(0, 0.5f, 0);
            layer1.transform.localScale = new Vector3(2f, 0.05f, 0.3f);
            if (shelfMat != null) layer1.GetComponent<Renderer>().sharedMaterial = shelfMat;

            var layer2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            layer2.name = "ShelfLayer2";
            layer2.transform.SetParent(shelfRoot.transform);
            layer2.transform.localPosition = new Vector3(0, 1.0f, 0);
            layer2.transform.localScale = new Vector3(2f, 0.05f, 0.3f);
            if (shelfMat != null) layer2.GetComponent<Renderer>().sharedMaterial = shelfMat;

            var layer3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            layer3.name = "ShelfLayer3";
            layer3.transform.SetParent(shelfRoot.transform);
            layer3.transform.localPosition = new Vector3(0, 1.5f, 0);
            layer3.transform.localScale = new Vector3(2f, 0.05f, 0.3f);
            if (shelfMat != null) layer3.GetComponent<Renderer>().sharedMaterial = shelfMat;

            var leftSupport = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftSupport.name = "LeftSupport";
            leftSupport.transform.SetParent(shelfRoot.transform);
            leftSupport.transform.localPosition = new Vector3(-0.9f, 0.75f, 0);
            leftSupport.transform.localScale = new Vector3(0.1f, 1.5f, 0.3f);
            if (shelfMat != null) leftSupport.GetComponent<Renderer>().sharedMaterial = shelfMat;

            var rightSupport = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightSupport.name = "RightSupport";
            rightSupport.transform.SetParent(shelfRoot.transform);
            rightSupport.transform.localPosition = new Vector3(0.9f, 0.75f, 0);
            rightSupport.transform.localScale = new Vector3(0.1f, 1.5f, 0.3f);
            if (shelfMat != null) rightSupport.GetComponent<Renderer>().sharedMaterial = shelfMat;

            AddPlacementPoints(layer1.transform);
            AddPlacementPoints(layer2.transform);
            AddPlacementPoints(layer3.transform);

            return shelfRoot;
        }

        private static void AddPlacementPoints(Transform layer)
        {
            for (int i = 0; i < 4; i++)
            {
                var point = new GameObject($"PlacementPoint_{i + 1}");
                point.transform.SetParent(layer);

                float xOffset = -0.375f + (i * 0.25f);
                point.transform.localPosition = new Vector3(xOffset, 0.5f, 0);
                point.transform.localRotation = Quaternion.identity;
                point.transform.localScale = new Vector3(1f / 2f, 1f / 0.05f, 1f / 0.3f);

                var col = point.AddComponent<BoxCollider>();
                col.size = new Vector3(0.4f, 0.4f, 0.4f);
                col.center = new Vector3(0, 0.2f, 0);
                col.isTrigger = true;

                point.AddComponent<ShelfPlacementPoint>();
                point.layer = LayerMask.NameToLayer("Default");
            }
        }

        private static Material CreateShelfMaterial()
        {
            Shader shader = Shader.Find("HDRP/Lit");
            if (shader == null || !shader.isSupported) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null || !shader.isSupported) shader = Shader.Find("Standard");
            if (shader == null) return null;

            var shelfMat = new Material(shader);
            if (shelfMat.HasProperty("_BaseColor"))
                shelfMat.SetColor("_BaseColor", new Color(0.8f, 0.8f, 0.8f));
            else if (shelfMat.HasProperty("_Color"))
                shelfMat.SetColor("_Color", new Color(0.8f, 0.8f, 0.8f));
            return shelfMat;
        }

        public static GameObject SpawnCashRegister(GameObject prefab, Transform checkoutParent, Vector3 worldPosition, Quaternion worldRotation, float scale = 4f)
        {
            if (prefab == null || checkoutParent == null) return null;

            var checkout = Object.Instantiate(prefab, worldPosition, worldRotation, checkoutParent);
            checkout.name = "CashRegister_Bought";
            checkout.transform.localScale = Vector3.one * scale;

            RemoveGeneratedCashierChildren(checkout.transform);

            var col = checkout.GetComponent<Collider>();
            if (col == null)
                col = checkout.AddComponent<BoxCollider>();
            if (col is BoxCollider bc)
            {
                bc.center = new Vector3(0, 0.5f, 0);
                bc.size = new Vector3(1f, 1f, 1f);
            }

            RigCashierStation(checkout);
            return checkout;
        }

        private static void RemoveGeneratedCashierChildren(Transform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                var child = root.GetChild(i);
                if (child.name == "PlayerStandPoint" || child.name == "CashRegisterCamera" || child.name == "CheckoutBeltPoint" || child.name == "CheckoutBeltVisual")
                    Object.Destroy(child.gameObject);
            }

            var oldInteractable = root.GetComponent<CashierStationInteractable>();
            if (oldInteractable != null)
                Object.Destroy(oldInteractable);
        }

        private static void RigCashierStation(GameObject checkout)
        {
            var standPoint = new GameObject("PlayerStandPoint");
            standPoint.transform.SetParent(checkout.transform.parent, worldPositionStays: true);
            standPoint.transform.position = checkout.transform.position + checkout.transform.rotation * new Vector3(0, 0, -2.4f);
            standPoint.transform.rotation = checkout.transform.rotation;

            var camObj = new GameObject("CashRegisterCamera");
            camObj.transform.SetParent(checkout.transform.parent, worldPositionStays: true);
            camObj.transform.position = checkout.transform.position + checkout.transform.rotation * new Vector3(0, 2.2f, -1.2f);
            camObj.transform.rotation = checkout.transform.rotation * Quaternion.Euler(20, 0, 0);
            var cam = camObj.AddComponent<Camera>();
            var al = camObj.AddComponent<AudioListener>();
            al.enabled = false;
            camObj.SetActive(false);

            var existing = checkout.GetComponent<CashierStationInteractable>();
            if (existing != null)
                Object.Destroy(existing);

            var interactable = checkout.AddComponent<CashierStationInteractable>();
            interactable.playerStandPoint = standPoint.transform;
            interactable.cashierCamera = cam;
            checkout.layer = LayerMask.NameToLayer("Default");
        }
    }
}
