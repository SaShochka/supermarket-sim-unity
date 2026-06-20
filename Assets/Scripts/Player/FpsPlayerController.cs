using UnityEngine;
using UnityEngine.InputSystem;

namespace SupermarketSim.Player
{
    public class FpsPlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 10f; // Adjusted for 2x scale player
        public float mouseSensitivity = 2f;

        [Header("References")]
        public Transform cameraTransform;
        public CharacterController characterController;

        private float verticalLookRotation = 0f;
        private Vector2 moveInput;
        private Vector2 lookInput;
        private float verticalVelocity;
        private bool isSuspended = false;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void SetGameplayInputSuspended(bool suspended)
        {
            isSuspended = suspended;
            if (suspended)
            {
                moveInput = Vector2.zero;
                lookInput = Vector2.zero;
            }
        }

        public void SetMoveInput(Vector2 input)
        {
            if (isSuspended) return;
            moveInput = input;
        }

        public void SetLookInput(Vector2 input)
        {
            if (isSuspended) return;
            lookInput = input;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            SetMoveInput(context.ReadValue<Vector2>());
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            SetLookInput(context.ReadValue<Vector2>());
        }

        private void Update()
        {
            if (isSuspended) return;

            if (transform.position.y < -5f)
            {
                if (characterController != null)
                    characterController.enabled = false;
                transform.position = new Vector3(0f, 1f, 0f);
                verticalVelocity = 0f;
                if (characterController != null)
                    characterController.enabled = true;
                return;
            }

            // Look
            transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);
            verticalLookRotation -= lookInput.y * mouseSensitivity;
            // Clamp pitch to avoid looking upside down
            verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
            
            if (cameraTransform != null)
            {
                cameraTransform.localEulerAngles = Vector3.right * verticalLookRotation;
            }

            // Move
            Vector3 horizontalMove = (transform.right * moveInput.x + transform.forward * moveInput.y) * moveSpeed;

            if (characterController != null && characterController.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;
            else
                verticalVelocity += Physics.gravity.y * Time.deltaTime;

            Vector3 motion = horizontalMove + Vector3.up * verticalVelocity;
            characterController.Move(motion * Time.deltaTime);
        }
    }
}