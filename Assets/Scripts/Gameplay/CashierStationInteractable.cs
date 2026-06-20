using UnityEngine;
using SupermarketSim.Player;

namespace SupermarketSim.Gameplay
{
    public class CashierStationInteractable : MonoBehaviour, Interaction.IInteractable
    {
        public Transform playerStandPoint;
        public Camera cashierCamera;
        public Transform beltPoint;
        public Vector3 beltItemLocalOffset = new Vector3(0.35f, 0f, 0f);
        public int salePrice = 15;
        public int SalePrice => salePrice;

        private bool isOccupied = false;
        private PlayerCashierMode currentPlayer;
        private CustomerNpc waitingCustomer;
        private CheckoutBeltItem beltItem;
        private bool currentItemScanned;

        private void Awake()
        {
            RepairGeneratedRig();
        }

        private void RepairGeneratedRig()
        {
            if (playerStandPoint == null)
            {
                var point = new GameObject("PlayerStandPoint");
                playerStandPoint = point.transform;
            }

            if (playerStandPoint.parent == transform)
                playerStandPoint.SetParent(transform.parent, worldPositionStays: true);
            else if (playerStandPoint.parent == null && transform.parent != null)
                playerStandPoint.SetParent(transform.parent, worldPositionStays: true);

            playerStandPoint.position = transform.position + transform.rotation * new Vector3(0f, 0f, -2.4f);
            playerStandPoint.rotation = transform.rotation;

            if (beltPoint == null)
            {
                var beltObj = new GameObject("CheckoutBeltPoint");
                beltPoint = beltObj.transform;
            }

            if (beltPoint.parent == transform)
                beltPoint.SetParent(transform.parent, worldPositionStays: true);
            else if (beltPoint.parent == null && transform.parent != null)
                beltPoint.SetParent(transform.parent, worldPositionStays: true);

            beltPoint.position = transform.position + transform.rotation * new Vector3(0f, 1.05f, 0.35f);
            beltPoint.rotation = transform.rotation;
            EnsureBeltVisual();

            if (cashierCamera == null)
            {
                var camObj = new GameObject("CashRegisterCamera");
                cashierCamera = camObj.AddComponent<Camera>();
                var audioListener = camObj.AddComponent<AudioListener>();
                audioListener.enabled = false;
                camObj.SetActive(false);
            }

            if (cashierCamera.transform.parent == transform)
                cashierCamera.transform.SetParent(transform.parent, worldPositionStays: true);
            else if (cashierCamera.transform.parent == null && transform.parent != null)
                cashierCamera.transform.SetParent(transform.parent, worldPositionStays: true);

            cashierCamera.transform.position = transform.position + transform.rotation * new Vector3(0f, 2.25f, -1.8f);
            var lookTarget = beltPoint != null ? beltPoint.position : transform.position;
            cashierCamera.transform.rotation = Quaternion.LookRotation((lookTarget - cashierCamera.transform.position).normalized, Vector3.up);
        }

        private void EnsureBeltVisual()
        {
            if (beltPoint == null)
                return;

            var existing = beltPoint.Find("CheckoutBeltVisual");
            if (existing != null)
                return;

            var belt = GameObject.CreatePrimitive(PrimitiveType.Cube);
            belt.name = "CheckoutBeltVisual";
            belt.transform.SetParent(beltPoint, worldPositionStays: false);
            belt.transform.localPosition = new Vector3(0f, -0.13f, 0f);
            belt.transform.localRotation = Quaternion.identity;
            belt.transform.localScale = new Vector3(1.8f, 0.08f, 0.9f);

            var col = belt.GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;

            var renderer = belt.GetComponent<Renderer>();
            if (renderer != null)
            {
                var shader = Shader.Find("HDRP/Lit");
                if (shader == null || !shader.isSupported) shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null || !shader.isSupported) shader = Shader.Find("Standard");
                if (shader != null)
                {
                    var mat = new Material(shader);
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", new Color(0.03f, 0.03f, 0.035f));
                    if (mat.HasProperty("_Color")) mat.SetColor("_Color", new Color(0.03f, 0.03f, 0.035f));
                    renderer.sharedMaterial = mat;
                }
            }
        }

        public string GetInteractionPrompt()
        {
            return isOccupied ? "Esc чтобы встать с кассы" : "Нажмите E чтобы сесть за кассу";
        }

        public void Interact(GameObject interactor)
        {
            RepairGeneratedRig();

            if (playerStandPoint == null || cashierCamera == null)
            {
                Debug.LogWarning($"{name}: cashier station is missing stand point or camera.");
                return;
            }

            if (!isOccupied)
            {
                var playerMode = interactor.GetComponent<PlayerCashierMode>();
                if (playerMode != null)
                {
                    currentPlayer = playerMode;
                    isOccupied = true;
                    currentPlayer.EnterCashierDesk(playerStandPoint, cashierCamera, this);
                    GameAudio.PlayPlace();
                    
                    // Force the interactor to keep this as the current interactable so we can press E to exit
                    var playerInteractor = interactor.GetComponent<Interaction.PlayerInteractor>();
                    if (playerInteractor != null)
                    {
                        playerInteractor.ForceSetInteractable(this);
                    }
                }
            }
            else
            {
                // While seated, E is reserved for scanning belt items. Leaving the cashier uses Esc.
            }
        }

        public void ForceExit(GameObject interactor)
        {
            if (currentPlayer == null || currentPlayer.gameObject != interactor)
                return;

            currentPlayer.ExitCashierDesk(cashierCamera);
            var playerInteractor = interactor.GetComponent<Interaction.PlayerInteractor>();
            if (playerInteractor != null)
                playerInteractor.ClearForcedInteractable(this);

            currentPlayer = null;
            isOccupied = false;
            GameAudio.PlayPlace();
        }

        public bool TryPlaceCustomerItem(CustomerNpc customer, PickupableItem item)
        {
            RepairGeneratedRig();
            if (customer == null || item == null || beltPoint == null || beltItem != null)
                return false;

            waitingCustomer = customer;
            currentItemScanned = false;

            item.transform.SetParent(beltPoint, worldPositionStays: false);
            item.transform.localPosition = beltItemLocalOffset;
            item.transform.localRotation = Quaternion.identity;

            foreach (var col in item.GetComponentsInChildren<Collider>(true))
                col.enabled = true;

            var rb = item.GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true;

            beltItem = item.gameObject.AddComponent<CheckoutBeltItem>();
            beltItem.Initialize(this, item);
            return true;
        }

        public void ScanBeltItem(CheckoutBeltItem scannedItem)
        {
            if (scannedItem == null || scannedItem != beltItem)
                return;

            currentItemScanned = true;
            if (currentPlayer != null)
            {
                var wallet = currentPlayer.GetComponent<PlayerWallet>();
                if (wallet != null)
                    wallet.AddMoney(salePrice);
            }
            GameAudio.PlayScan();

            if (beltItem != null)
            {
                Destroy(beltItem.gameObject);
                beltItem = null;
            }
        }

        public bool HasWaitingCustomer(CustomerNpc customer)
        {
            return customer != null && waitingCustomer == customer && beltItem != null && !currentItemScanned;
        }

        public bool WasCustomerItemScanned(CustomerNpc customer)
        {
            return customer != null && waitingCustomer == customer && currentItemScanned;
        }

        public void ClearCustomer(CustomerNpc customer)
        {
            if (waitingCustomer != customer)
                return;

            waitingCustomer = null;
            currentItemScanned = false;
            beltItem = null;
        }
    }
}