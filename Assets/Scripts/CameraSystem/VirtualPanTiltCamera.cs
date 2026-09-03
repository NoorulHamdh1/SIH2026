using UnityEngine;
using UnityEngine.InputSystem;

public class VirtualPanTiltCamera : MonoBehaviour
{
    [Header("Pan Settings")]
    [SerializeField]
    private float maxPanSpeed = 5f;

    [SerializeField]
    private float minPanAngle = -90f;

    [SerializeField]
    private float maxPanAngle = 90f;

    [Header("Tilt Settings")]
    [SerializeField]
    private float maxTiltSpeed = 5f;

    [SerializeField]
    private float minTiltAngle = -45f;

    [SerializeField]
    private float maxTiltAngle = 45f;

    private float currentPan;
    private float currentTilt;

    private void Start()
    {
        currentPan = 0f;
        currentTilt = 0f;

        ApplyRotation();
    }

    private void Update()
    {
        HandleKeyboardInput();
    }

    private void HandleKeyboardInput()
    {
        float panInput = 0f;
        float tiltInput = 0f;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        // A / D = Pan left / right
        if (keyboard.aKey.isPressed)
            panInput = -1f;

        if (keyboard.dKey.isPressed)
            panInput = 1f;

        // S / W = Tilt down / up
        if (keyboard.sKey.isPressed)
            tiltInput = -1f;

        if (keyboard.wKey.isPressed)
            tiltInput = 1f;

        ApplyPanTiltDelta(
            panInput * maxPanSpeed * Time.deltaTime,
            tiltInput * maxTiltSpeed * Time.deltaTime
        );
    }

    /// <summary>
    /// Applies a pan/tilt correction in degrees.
    /// Positive pan  = right
    /// Negative pan  = left
    /// Positive tilt = up
    /// Negative tilt = down
    /// </summary>
    public void ApplyPanTiltDelta(
        float panDelta,
        float tiltDelta
    )
    {
        currentPan += panDelta;
        currentTilt += tiltDelta;

        currentPan = Mathf.Clamp(
            currentPan,
            minPanAngle,
            maxPanAngle
        );

        currentTilt = Mathf.Clamp(
            currentTilt,
            minTiltAngle,
            maxTiltAngle
        );

        ApplyRotation();
    }

    private void ApplyRotation()
    {
        transform.localRotation = Quaternion.Euler(
            -currentTilt,
            currentPan,
            0f
        );
    }

    public float GetPan()
    {
        return currentPan;
    }

    public float GetTilt()
    {
        return currentTilt;
    }
}