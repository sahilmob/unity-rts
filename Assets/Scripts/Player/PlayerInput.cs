using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using RTS.Units;
using RTS.EventBus;
using RTS.Events;
using System.Collections.Generic;
using RTS.Commands;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.LowLevel;

namespace RTS.Player
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] new Camera camera;
        [SerializeField] private Rigidbody cameraTarget;
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private CameraConfig cameraConfig;
        [SerializeField] private LayerMask selectableUnitsLayers;
        [SerializeField] private LayerMask intractableUnitsLayers;
        [SerializeField] private LayerMask floorLayers;
        [SerializeField] private RectTransform selectionBox;
        [SerializeField][ColorUsage(showAlpha: true, hdr: true)] private Color errorTintColor = Color.red;
        [SerializeField][ColorUsage(showAlpha: true, hdr: true)] private Color errorFresnelColor = new(4, 1.7f, 0, 2);
        [SerializeField][ColorUsage(showAlpha: true, hdr: true)] private Color availableToPlaceTintColor = new(0.2f, 0.65f, 0, 2);
        [SerializeField][ColorUsage(showAlpha: true, hdr: true)] private Color availableToPlaceFresnelColor = new(4, 1.7f, 0, 2);

        private bool wasMouseDownOnUI;
        private BaseCommand activeCommand;
        private GameObject ghostInstance;
        private MeshRenderer ghostRenderer;
        private Vector2 startingMousePosition;
        private CinemachineFollow cinemachineFollow;
        private float zoomStartTime;
        private float rotationStartTime;
        private Vector3 startingFollowOffset;
        private float maxRotationAmount;
        private List<ISelectable> selectedUnits = new(12);
        private HashSet<AbstractUnit> addedUnits = new(24);
        private HashSet<AbstractUnit> aliveUnits = new(100);
        private static readonly int TINT = Shader.PropertyToID("_Tint");
        private static readonly int FRESNEL = Shader.PropertyToID("_FresnelColor");

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
            Bus<CommandSelectedEvent>.onEvent += HandleActionSelected;
            Bus<UnitDeathEvent>.onEvent += HandleUnitDeath;
        }

        private void HandleUnitDeath(UnitDeathEvent e)
        {
            Bus<UnitDeselectedEvent>.Raise(new(e.Unit));
            aliveUnits.Remove(e.Unit);
        }

        private void HandleActionSelected(CommandSelectedEvent e)
        {
            activeCommand = e.Command;
            if (!activeCommand.RequiresClickToActivate)
            {
                ActivateAction(new RaycastHit());
            }
            else if (activeCommand.GhostPrefab != null)
            {
                ghostInstance = Instantiate(activeCommand.GhostPrefab);
                ghostRenderer = ghostInstance.GetComponentInChildren<MeshRenderer>();
            }
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
            Bus<CommandSelectedEvent>.onEvent -= HandleActionSelected;
            Bus<UnitDeathEvent>.onEvent -= HandleUnitDeath;
        }

        private void HandleUnitDeselected(UnitDeselectedEvent e)
        {
            selectedUnits.Remove(e.Unit);
        }

        private void HandleUnitSelected(UnitSelectedEvent e)
        {
            if (!selectedUnits.Contains(e.Unit))
            {
                selectedUnits.Add(e.Unit);
            }
        }

        private void Update()
        {
            HandlePanning();
            HandleZooming();
            HandleRotation();
            HandleRightClick();
            HandleDragSelect();
            HandleGhost();
        }

        private void HandleGhost()
        {
            if (ghostInstance == null) return;
            Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Keyboard.current.escapeKey.wasReleasedThisFrame)
            {
                Destroy(ghostInstance);
                ghostInstance = null;
                activeCommand = null;
                return;
            }

            if (Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, floorLayers))
            {
                ghostInstance.transform.position = hit.point;
                bool allRestrictionsPassed = activeCommand.AllRestrictionsPassed(hit.point);

                ghostRenderer.material.SetColor(TINT, allRestrictionsPassed ? availableToPlaceTintColor : errorTintColor);
                ghostRenderer.material.SetColor(FRESNEL, allRestrictionsPassed ? availableToPlaceFresnelColor : errorFresnelColor);
            }
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
            if (!wasMouseDownOnUI && activeCommand == null && !Keyboard.current.leftCtrlKey.isPressed)
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
            if (activeCommand != null || wasMouseDownOnUI) return;
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
            wasMouseDownOnUI = EventSystem.current.IsPointerOverGameObject();
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
                && Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, floorLayers | intractableUnitsLayers)
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

                for (int i = 0; i < abstractUnits.Count; i++)
                {
                    CommandContext ctx = new(abstractUnits[i], hit, i, MouseButton.Right);
                    foreach (ICommand c in GethAvailableCommands(abstractUnits[i]))
                    {
                        if (c.CanHandle(ctx))
                        {
                            c.Handle(ctx);
                            if (c.IsSingleUnitCommand)
                            {
                                return;
                            }
                            break;
                        }
                    }
                }
            }
        }

        private List<BaseCommand> GethAvailableCommands(AbstractUnit unit)
        {
            OverrideCommandsCommand[] overrideCommandsCommands = unit.AvailableCommands
                .Where(c => c is OverrideCommandsCommand)
                .Cast<OverrideCommandsCommand>().ToArray();

            List<BaseCommand> allAvailableCommands = new();

            foreach (OverrideCommandsCommand overrideCommand in overrideCommandsCommands)
            {
                allAvailableCommands.AddRange(overrideCommand.Commands.Where(c => c is not OverrideCommandsCommand));
            }

            allAvailableCommands.AddRange(unit.AvailableCommands.Where(c => c is not OverrideCommandsCommand));

            return allAvailableCommands;
        }

        private void HandleLeftClick()
        {
            if (camera == null) return;
            Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (activeCommand == null && Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, selectableUnitsLayers)
            && hit.collider.TryGetComponent(out ISelectable selectable))
            {
                selectable.Select();
            }
            else if (activeCommand != null && !EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(cameraRay, out hit, float.MaxValue, floorLayers | intractableUnitsLayers))
            {
                ActivateAction(hit);
            }
        }

        private void ActivateAction(RaycastHit hit)
        {
            if (ghostInstance != null)
            {
                Destroy(ghostInstance);
                ghostInstance = null;
            }

            List<AbstractCommandable> abstractCommandables = selectedUnits.Where(unit => unit is AbstractCommandable).Cast<AbstractCommandable>().ToList();
            for (int i = 0; i < abstractCommandables.Count; i++)
            {
                CommandContext ctx = new(abstractCommandables[i], hit, i);
                if (activeCommand.CanHandle(ctx))
                {
                    activeCommand.Handle(ctx);
                    if (activeCommand.IsSingleUnitCommand)
                    {
                        break;
                    }
                }
            }
            activeCommand = null;
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

            if (mousePosition.x >= 0 && mousePosition.x <= cameraConfig.EdgePanSize)
            {
                moveAmount.x -= cameraConfig.MousePanSpeed;
            }
            else if (mousePosition.x <= screenWidth && mousePosition.x >= screenWidth - cameraConfig.EdgePanSize)
            {
                moveAmount.x += cameraConfig.MousePanSpeed;
            }

            if (mousePosition.y <= screenHight && mousePosition.y >= screenHight - cameraConfig.EdgePanSize)
            {
                moveAmount.y += cameraConfig.MousePanSpeed;
            }
            else if (mousePosition.y >= 0 && mousePosition.y <= cameraConfig.EdgePanSize)
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