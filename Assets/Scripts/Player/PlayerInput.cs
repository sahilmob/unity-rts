using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using System;
using RTS.Units;

namespace RTS.Player
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] new Camera camera;
        [SerializeField] private Rigidbody cameraTarget;
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private CameraConfig cameraConfig;
        private CinemachineFollow cinemachineFollow;
        private float zoomStartTime;
        private float rotationStartTime;
        private Vector3 startingFollowOffset;
        private float maxRotationAmount;
        private ISelectable selectedUnit;


        private void Awake()
        {
            if (!cinemachineCamera.TryGetComponent(out cinemachineFollow))
            {
                Debug.LogError("Cinemachine camera didn't have CinemachineFollow. Zoom will not work!");
            }

            startingFollowOffset = cinemachineFollow.FollowOffset;
            maxRotationAmount = Mathf.Abs(cinemachineFollow.FollowOffset.z);
        }

        private void Update()
        {
            HandlePanning();
            HandleZooming();
            HandleRotation();
            HandleLeftClick();
        }

        private void HandleLeftClick()
        {
            if (camera == null) return;
            Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                if (Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, LayerMask.GetMask("Default"))
                && hit.collider.TryGetComponent(out ISelectable selectable))
                {

                    selectable.Select();
                    if (selectedUnit != null)
                    {
                        selectedUnit.Deselect();
                    }
                    selectedUnit = selectable;
                }
                else if (selectedUnit != null)
                {
                    selectedUnit.Deselect();
                    selectedUnit = null;
                }
            }
        }

        private void HandleRotation()
        {
            if (ShouldSetRotationStartTime())
            {
                rotationStartTime = Time.time;
            }

            float rotationTime = Mathf.Clamp01((Time.time - rotationStartTime) * cameraConfig.RotationSpeed);
            Vector3 targetFollowOffset;

            if (Keyboard.current.rightShiftKey.isPressed)
            {
                targetFollowOffset = new Vector3(
                    maxRotationAmount,
                    cinemachineFollow.FollowOffset.y,
                    0
                );
            }
            else if (Keyboard.current.leftShiftKey.isPressed)
            {
                targetFollowOffset = new Vector3(
                -maxRotationAmount,
                cinemachineFollow.FollowOffset.y,
                0
            );
            }
            else
            {
                targetFollowOffset = new Vector3(
                    startingFollowOffset.x,
                    cinemachineFollow.FollowOffset.y,
                    startingFollowOffset.z
                );
            }

            cinemachineFollow.FollowOffset = Vector3.Slerp(
                cinemachineFollow.FollowOffset,
                targetFollowOffset,
                rotationTime
            );
        }

        private bool ShouldSetRotationStartTime()
        {
            return Keyboard.current.leftShiftKey.wasPressedThisFrame
            || Keyboard.current.rightShiftKey.wasPressedThisFrame || Keyboard.current.leftShiftKey.wasReleasedThisFrame
            || Keyboard.current.rightShiftKey.wasReleasedThisFrame;
        }

        private void HandleZooming()
        {
            if (ShouldSetZoomStartTime())
            {
                zoomStartTime = Time.time;
            }

            Vector3 targetFollowOffset;

            float zoomTime = Mathf.Clamp01((Time.time - zoomStartTime) * cameraConfig.ZoomSpeed);

            if (Keyboard.current.altKey.isPressed)
            {
                targetFollowOffset = new Vector3(
                cinemachineFollow.FollowOffset.x,
                cameraConfig.MinZoomDistance,
                cinemachineFollow.FollowOffset.z
                );
            }
            else
            {
                targetFollowOffset = new Vector3(
                    cinemachineFollow.FollowOffset.x,
                    startingFollowOffset.y,
                    cinemachineFollow.FollowOffset.z
                );
            }

            cinemachineFollow.FollowOffset = Vector3.Slerp(
                 cinemachineFollow.FollowOffset,
                 targetFollowOffset,
                 zoomTime
            );
        }

        private bool ShouldSetZoomStartTime()
        {
            return Keyboard.current.altKey.wasPressedThisFrame || Keyboard.current.altKey.wasReleasedThisFrame;

        }

        private void HandlePanning()
        {
            Vector2 moveAmount = GetKeyboardMoveAmount();
            moveAmount += GetMouseMoveAmount();

            cameraTarget.linearVelocity = new Vector3(moveAmount.x, 0, moveAmount.y);
        }

        private Vector2 GetMouseMoveAmount()
        {
            Vector2 moveAmount = Vector2.zero;

            if (!cameraConfig.EnableEdgePan) return moveAmount;

            Vector2 mousePosition = Mouse.current.position.ReadValue();

            int screenWidth = Screen.width;
            int screenHight = Screen.height;

            if (mousePosition.x <= cameraConfig.EdgePanSize)
            {
                moveAmount.x -= cameraConfig.MousePanSpeed;
            }
            else if (mousePosition.x >= screenWidth - cameraConfig.EdgePanSize)
            {
                moveAmount.x += cameraConfig.MousePanSpeed;
            }

            if (mousePosition.y >= screenHight - cameraConfig.EdgePanSize)
            {
                moveAmount.y += cameraConfig.MousePanSpeed;
            }
            else if (mousePosition.y <= cameraConfig.EdgePanSize)
            {
                moveAmount.y -= cameraConfig.MousePanSpeed;
            }

            return moveAmount;
        }

        private Vector2 GetKeyboardMoveAmount()
        {
            Vector2 moveAmount = Vector2.zero;

            if (Keyboard.current.upArrowKey.isPressed)
            {
                moveAmount.y += cameraConfig.KeyboardPanSpeed;
            }

            if (Keyboard.current.downArrowKey.isPressed)
            {
                moveAmount.y -= cameraConfig.KeyboardPanSpeed;
            }

            if (Keyboard.current.leftArrowKey.isPressed)
            {
                moveAmount.x -= cameraConfig.KeyboardPanSpeed;
            }

            if (Keyboard.current.rightArrowKey.isPressed)
            {
                moveAmount.x += cameraConfig.KeyboardPanSpeed;
            }

            return moveAmount;
        }
    }
}