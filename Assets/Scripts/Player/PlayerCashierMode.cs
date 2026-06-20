using UnityEngine;
using SupermarketSim.Gameplay;

namespace SupermarketSim.Player
{
    public class PlayerCashierMode : MonoBehaviour
    {
        private FpsPlayerController fpsController;
        private Interaction.PlayerInteractor interactor;

        private Vector3 savedPosition;
        private Quaternion savedRotation;
        private bool isInCashierMode;
        private CashierStationInteractable activeStation;

        public bool IsInCashierMode => isInCashierMode;

        private void Awake()
        {
            fpsController = GetComponent<FpsPlayerController>();
            interactor = GetComponent<Interaction.PlayerInteractor>();
        }

        public void EnterCashierDesk(Transform standPoint, Camera cashierCamera, CashierStationInteractable station = null)
        {
            if (isInCashierMode || standPoint == null || cashierCamera == null)
                return;

            isInCashierMode = true;
            activeStation = station;

            // Save state
            savedPosition = transform.position;
            savedRotation = transform.rotation;

            // Move to stand point
            transform.position = standPoint.position;
            transform.rotation = standPoint.rotation;

            // Disable FPS controls
            fpsController.SetGameplayInputSuspended(true);

            // Switch cameras
            if (fpsController.cameraTransform != null)
            {
                // The camera is a child of the pivot
                var mainCam = fpsController.cameraTransform.GetComponentInChildren<Camera>();
                if (mainCam != null)
                {
                    mainCam.gameObject.SetActive(false);
                    var fpsAudioListener = mainCam.GetComponent<AudioListener>();
                    if (fpsAudioListener != null) fpsAudioListener.enabled = false;
                }
            }

            cashierCamera.gameObject.SetActive(true);
            var cashierAudioListener = cashierCamera.GetComponent<AudioListener>();
            if (cashierAudioListener != null) cashierAudioListener.enabled = true;

            // Update interactor to use cashier camera for raycasts (if needed)
            interactor.SetRaycastCameraOverride(cashierCamera);
            
            // Re-enable cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void ExitCashierDesk(Camera cashierCamera)
        {
            if (!isInCashierMode)
                return;

            isInCashierMode = false;
            activeStation = null;

            // Restore position
            transform.position = savedPosition;
            transform.rotation = savedRotation;

            // Enable FPS controls
            fpsController.SetGameplayInputSuspended(false);

            // Switch cameras back
            cashierCamera.gameObject.SetActive(false);
            var cashierAudioListener = cashierCamera.GetComponent<AudioListener>();
            if (cashierAudioListener != null) cashierAudioListener.enabled = false;

            if (fpsController.cameraTransform != null)
            {
                var mainCam = fpsController.cameraTransform.GetComponentInChildren<Camera>(true);
                if (mainCam != null)
                {
                    mainCam.gameObject.SetActive(true);
                    var fpsAudioListener = mainCam.GetComponent<AudioListener>();
                    if (fpsAudioListener != null) fpsAudioListener.enabled = true;
                }
            }

            // Restore interactor raycast camera
            interactor.SetRaycastCameraOverride(null);
            
            // Re-lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public bool TryExitActiveStation()
        {
            if (!isInCashierMode)
                return false;

            if (activeStation != null)
            {
                activeStation.ForceExit(gameObject);
                return true;
            }

            return false;
        }
    }
}