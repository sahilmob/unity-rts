using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using RTS.Units;
using System;
using RTS.EventBus;
using RTS.Events;
using System.Collections.Generic;

namespace RTS.Player
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] new Camera camera;
        [SerializeField] private Rigidbody cameraTarget;
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private CameraConfig cameraConfig;
        [SerializeField] private LayerMask selectableUnitsLayer;
        [SerializeField] private LayerMask floorLayers;
        [SerializeField] private RectTransform selectionBox;

        private Vector2 startingMousePosition;

        private CinemachineFollow cinemachineFollow;
        private float zoomStartTime;
        private float rotationStartTime;
        private Vector3 startingFollowOffset;
        private float maxRotationAmount;
        private List<ISelectable> selectedUnits = new(12);
        private HashSet<AbstractUnit> addedUnits = new(24);
        private HashSet<AbstractUnit> aliveUnits = new(100);

        private void Awake()
        {
            if (!cinemachineCamera.TryGetComponent(out cinemachineFollow))
            {
                Debug.LogError("Cinemachine camera didn't have CinemachineFollow. Zoom will not work!");
            }

            startingFollowOffset = cinemachineFollow.FollowOffset;
            maxRotationAmount = Mathf.Abs(cinemachineFollow.FollowOffset.z);
            Bus<UnitSelectedEvent>.onEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.onEvent += HandleUnitDeselected;
            Bus<UnitSpawnEvent>.onEvent += HandleUnitSpawn;
        }

        private void HandleUnitSpawn(UnitSpawnEvent e)
        {
            aliveUnits.Add(e.Unit);
        }

        private void OnDestroy()
        {
            if (selectedUnits.Count != 0)
            {
                DeselectAllUnits();
            }
            Bus<UnitSelectedEvent>.onEvent -= HandleUnitSelected;
            Bus<UnitDeselectedEvent>.onEvent -= HandleUnitDeselected;
            Bus<UnitSpawnEvent>.onEvent -= HandleUnitSpawn;
        }

        private void HandleUnitDeselected(UnitDeselectedEvent e)
        {
            selectedUnits.Remove(e.Unit);
        }

        private void HandleUnitSelected(UnitSelectedEvent e)
        {
            selectedUnits.Add(e.Unit);
        }

        private void Update()
        {
            HandlePanning();
            HandleZooming();
            HandleRotation();
            HandleRightClick();
            HandleDragSelect();
        }

        private void HandleDragSelect()
        {
            if (selectionBox == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleMouseDown();
            }
            else if (Mouse.current.leftButton.isPressed && !Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleMouseDrag();
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                HandleMouseRelease();
            }
        }

        private void HandleMouseRelease()
        {
            if (!Keyboard.current.leftCtrlKey.isPressed)
                DeselectAllUnits();
            HandleLeftClick();
            foreach (AbstractUnit u in addedUnits)
            {
                u.Select();
            }
            selectionBox.gameObject.SetActive(false);
        }

        private void HandleMouseDrag()
        {
            Bounds selectionBox = ResizeSelectionBox();
            foreach (AbstractUnit u in aliveUnits)
            {
                Vector2 unitPosition = camera.WorldToScreenPoint(u.transform.position);
                if (selectionBox.Contains(unitPosition))
                {
                    addedUnits.Add(u);
                }
            }
        }

        private void HandleMouseDown()
        {
            selectionBox.sizeDelta = Vector2.zero;
            selectionBox.gameObject.SetActive(true);
            startingMousePosition = Mouse.current.position.ReadValue();
            addedUnits.Clear();
        }

        private void DeselectAllUnits()
        {
            ISelectable[] currentlySelectedUnits = selectedUnits.ToArray();
            foreach (ISelectable s in currentlySelectedUnits)
            {
                s.Deselect();
            }
        }

        private Bounds ResizeSelectionBox()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            float width = mousePosition.x - startingMousePosition.x;
            float height = mousePosition.y - startingMousePosition.y;

            selectionBox.anchoredPosition = startingMousePosition + new Vector2(width / 2, height / 2);
            selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));

            return new Bounds(selectionBox.anchoredPosition, selectionBox.sizeDelta);
        }

        private void HandleRightClick()
        {
            if (selectedUnits.Count == 0) return;

            Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Mouse.current.rightButton.wasReleasedThisFrame
                && Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, floorLayers)
            )
            {
                List<AbstractUnit> abstractUnits = new(selectedUnits.Count);

                foreach (ISelectable selectable in selectedUnits)
                {
                    if (selectable is AbstractUnit unit)
                    {
                        abstractUnits.Add(unit);
                    }
                }

                int unitsOnLayer = 0;
                int maxUnitsOnLayer = 1;
                float circleRadius = 0;
                float radialOffset = 0;

                foreach (AbstractUnit u in abstractUnits)
                {
                    Vector3 targetPosition = new(
                        hit.point.x + circleRadius * Mathf.Cos(radialOffset * unitsOnLayer),
                        hit.point.y,
                        hit.point.z + circleRadius * Mathf.Sign(radialOffset * unitsOnLayer)
                    );

                    u.MoveTo(targetPosition);
                    unitsOnLayer++;

                    if (unitsOnLayer >= maxUnitsOnLayer)
                    {
                        unitsOnLayer = 0;
                        circleRadius += u.AgentRadius * 3.5f;
                        maxUnitsOnLayer = Mathf.FloorToInt(2 * Mathf.PI * circleRadius / (u.AgentRadius * 2));
                        radialOffset = 2 * Mathf.PI / maxUnitsOnLayer;
                    }
                }

                // foreach (AbstractUnit u in selectedUnits)
                // {
                //     if (u is not IMovable movable)
                //     {
                //         continue;
                //     }
                //     movable.MoveTo(hit.point);
                // }
            }
        }

        private void HandleLeftClick()
        {
            if (camera == null) return;
            Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, selectableUnitsLayer)
            && hit.collider.TryGetComponent(out ISelectable selectable))
            {
                selectable.Select();
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