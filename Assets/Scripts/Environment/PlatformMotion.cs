using UnityEngine;

public class PlatformMotion : MonoBehaviour
{
    [Header("Motion")]
    [SerializeField] private bool motionEnabled = false;

    [SerializeField, Range(0f, 20f)]
    private float motionPixelsPerFrame = 5f;

    [SerializeField]
    private Vector2 motionDirection = Vector2.right;

    [Header("Reference")]
    [SerializeField] private Camera targetCamera;

    [SerializeField]
    private float referenceDistance = 100f;

    [Header("Update Rate")]
    [SerializeField, Min(20f)]
    private float updateRateHz = 30f;

    private float updateTimer;

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = GetComponentInChildren<Camera>();

        motionDirection =
            motionDirection.sqrMagnitude > 0.001f
                ? motionDirection.normalized
                : Vector2.right;

        updateTimer = 0f;
    }

    private void Update()
    {
        if (!motionEnabled || targetCamera == null)
            return;

        updateTimer += Time.deltaTime;

        float updateInterval = 1f / updateRateHz;

        while (updateTimer >= updateInterval)
        {
            updateTimer -= updateInterval;
            ApplyMotion(updateInterval);
        }
    }

    private void ApplyMotion(float deltaTime)
    {
        float horizontalFOV =
            GetHorizontalFOV();

        float aspect =
            targetCamera.aspect;

        float verticalFOVRadians =
            2f * Mathf.Atan(
                Mathf.Tan(horizontalFOV * Mathf.Deg2Rad * 0.5f)
                / aspect
            );

        float horizontalVisibleSize =
            2f *
            referenceDistance *
            Mathf.Tan(horizontalFOV * Mathf.Deg2Rad * 0.5f);

        float verticalVisibleSize =
            2f *
            referenceDistance *
            Mathf.Tan(verticalFOVRadians * 0.5f);

        float worldUnitsPerPixelX =
            horizontalVisibleSize / 640f;

        float worldUnitsPerPixelY =
            verticalVisibleSize / 480f;

        float pixelsThisFrame =
            motionPixelsPerFrame;

        float movementX =
            motionDirection.x *
            pixelsThisFrame *
            worldUnitsPerPixelX;

        float movementY =
            motionDirection.y *
            pixelsThisFrame *
            worldUnitsPerPixelY;

        transform.position +=
            new Vector3(
                movementX,
                movementY,
                0f
            );
    }

    private float GetHorizontalFOV()
    {
        float verticalFOV =
            targetCamera.fieldOfView * Mathf.Deg2Rad;

        float horizontalFOV =
            2f *
            Mathf.Atan(
                Mathf.Tan(verticalFOV * 0.5f) *
                targetCamera.aspect
            );

        return horizontalFOV * Mathf.Rad2Deg;
    }

    public void SetMotionEnabled(bool enabled)
    {
        motionEnabled = enabled;
    }
}