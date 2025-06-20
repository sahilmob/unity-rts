using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using System;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private float keyboardPanSpeed = 5;
    [SerializeField] private float zoomSpeed = 1;
    [SerializeField] private float rotationSpeed = 1;
    [SerializeField] private float minZoomDistance = 7.5f;
    private CinemachineFollow cinemachineFollow;
    private float zoomStartTime;
    private float rotationStartTime;
    private Vector3 startingFollowOffset;
    private float maxRotationAmount;

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
    }

    private void HandleRotation()
    {
        if (ShouldSetRotationStartTime())
        {
            rotationStartTime = Time.time;
        }

        float rotationTime = Mathf.Clamp01((Time.time - rotationStartTime) * rotationSpeed);
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

        float zoomTime = Mathf.Clamp01((Time.time - zoomStartTime) * zoomSpeed);

        if (Keyboard.current.altKey.isPressed)
        {
            targetFollowOffset = new Vector3(
            cinemachineFollow.FollowOffset.x,
            minZoomDistance,
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
        Vector2 moveAmount = Vector2.zero;

        if (Keyboard.current.upArrowKey.isPressed)
        {
            moveAmount.y += keyboardPanSpeed;
        }

        if (Keyboard.current.downArrowKey.isPressed)
        {
            moveAmount.y -= keyboardPanSpeed;
        }

        if (Keyboard.current.leftArrowKey.isPressed)
        {
            moveAmount.x -= keyboardPanSpeed;
        }

        if (Keyboard.current.rightArrowKey.isPressed)
        {
            moveAmount.x += keyboardPanSpeed;
        }

        moveAmount *= Time.deltaTime;
        cameraTarget.position += new Vector3(moveAmount.x, 0, moveAmount.y);
    }
}
