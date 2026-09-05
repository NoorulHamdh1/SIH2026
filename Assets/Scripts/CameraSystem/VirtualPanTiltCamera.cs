using UnityEngine;
using UnityEngine.InputSystem;

public class VirtualPanTiltCamera : MonoBehaviour
{
    [Header("Pan Settings")]
    [SerializeField, Range(5f, 10f)]
    private float maxPanSpeed = 5f;

    [SerializeField]
    private float minPanAngle = -90f;

    [SerializeField]
    private float maxPanAngle = 90f;

    [Header("Tilt Settings")]
    [SerializeField, Range(5f, 10f)]
    private float maxTiltSpeed = 5f;

    [SerializeField]
    private float minTiltAngle = -45f;

    [SerializeField]
    private float maxTiltAngle = 45f;

    [Header("Control Update")]
    [SerializeField, Min(20f)]
    private float updateRateHz = 30f;

    private float currentPan;
    private float currentTilt;

    private float updateTimer;

    private void Start()
    {
        currentPan = 0f;
        currentTilt = 0f;
        updateTimer = 0f;

        ApplyRotation();
    }

    private void Update()
    {
        updateTimer += Time.deltaTime;

        float updateInterval = 1f / updateRateHz;

        while (updateTimer >= updateInterval)
        {
            updateTimer -= updateInterval;

            HandleKeyboardInput(updateInterval);
        }
    }

    private void HandleKeyboardInput(float deltaTime)
    {
        float panInput = 0f;
        float tiltInput = 0f;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard.aKey.isPressed)
            panInput = -1f;

        if (keyboard.dKey.isPressed)
            panInput = 1f;

        if (keyboard.sKey.isPressed)
            tiltInput = -1f;

        if (keyboard.wKey.isPressed)
            tiltInput = 1f;

        ApplyPanTiltDelta(
            panInput * maxPanSpeed * deltaTime,
            tiltInput * maxTiltSpeed * deltaTime
        );
    }

    /// <summary>
    /// Applies a pan/tilt correction in degrees.
    /// Positive pan = right
    /// Negative pan = left
    /// Positive tilt = up
    /// Negative tilt = down
    /// </summary>
    public void ApplyPanTiltDelta(float panDelta, float tiltDelta)
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