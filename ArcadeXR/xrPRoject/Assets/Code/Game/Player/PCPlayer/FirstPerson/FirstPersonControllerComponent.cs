using System.Numerics;
using UnityEngine;

using UnityEngine.InputSystem;

/// <summary>
/// component to control a player with mouse and keyboard
/// </summary>
namespace Player.FirstPerson //use this to have include paths like in cpp!, includes all classes!
{
    class FirstPersonControllerComponent
    {
        public float moveSpeed = 6f;
        public float jumpHeight = 2f;
        public float gravity = -9.81f;
        public float mouseSensitivity = 2f;
        public Transform cameraTransform;
        private CharacterController controller;
        private UnityEngine.Vector3 velocity;
        private float xRotation = 0f;

        private GameObject playerPtr = null;
        private Camera camera = null;
        public InputData input;


        public FirstPersonControllerComponent(
            GameObject player
        )
        {
            SetupPlayer(player);
        }

        protected void SetupPlayer(GameObject player)
        {
            input = new InputData();
            if (player)
            {
                playerPtr = player;
                controller = player.GetComponent<CharacterController>();
                if (!controller)
                {
                    controller = player.gameObject.AddComponent<CharacterController>();
                }
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;


                camera = player.GetComponentInChildren<Camera>();
                if (camera)
                {
                    cameraTransform = camera.transform;
                }
                
            }
        }



        public void Tick()
        {
            HandleMouseLook();
            HandleMouseInput();
            HandleMovement();
        }
        private void HandleMouseLook()
        {
            if (!playerPtr)
            {
                return;
            }

            UnityEngine.Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            float mouseX = mouseDelta.x;
            float mouseY = mouseDelta.y;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            cameraTransform.localRotation = UnityEngine.Quaternion.Euler(xRotation, 0f, 0f);

            playerPtr.transform.Rotate(UnityEngine.Vector3.up * mouseX);

        }

        private void HandleMouseInput()
        {
            input.UpdateMouse(
                Mouse.current.leftButton.isPressed || Mouse.current.leftButton.wasPressedThisFrame,
                Mouse.current.rightButton.isPressed || Mouse.current.rightButton.wasPressedThisFrame
            );
        }

        private void HandleMovement()
        {
            if (!playerPtr) { return; }

            float x = 0f;
            float z = 0f;

            if (Keyboard.current.aKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed) x += 1f;
            if (Keyboard.current.wKey.isPressed) z += 1f;
            if (Keyboard.current.sKey.isPressed) z -= 1f;


            UnityEngine.Vector3 move = playerPtr.transform.right * x + playerPtr.transform.forward * z;
            controller.Move(move * moveSpeed * Time.deltaTime);
            if (controller.isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
            if (Keyboard.current.spaceKey.isPressed && controller.isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }



        public UnityEngine.Vector3 CameraPosition()
        {
            return cameraTransform.position;
        }

        public UnityEngine.Vector3 LookDir()
        {
            return cameraTransform.forward;
        }

        public  UnityEngine.Vector3 LookDirHorizontalPlane()
        {
            UnityEngine.Vector3 dir = LookDir();
            dir.y = 0.0f;
            return dir;
        }




    }
}