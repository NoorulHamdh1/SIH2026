using UnityEngine;

public class CameraConfiguration : MonoBehaviour
{
    [Header("Camera Resolution")]
    [SerializeField] private int resolutionWidth = 640;
    [SerializeField] private int resolutionHeight = 480;

    [Header("Camera FOV")]
    [SerializeField] private float horizontalFOV = 4f;

    [Header("Camera Update Rate")]
    [SerializeField] private float updateRateHz = 30f;

    [Header("Initial Camera Position")]
    [SerializeField] private Vector3 initialPosition = Vector3.zero;

    [Header("Initial Camera Rotation")]
    [SerializeField] private Vector3 initialRotation = Vector3.zero;

    private Camera cameraComponent;

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();

        if (cameraComponent == null)
        {
            Debug.LogError("CameraConfiguration requires a Camera component.");
            return;
        }

        ApplyConfiguration();
    }

    private void ApplyConfiguration()
    {
        // Set camera aspect ratio from the configured resolution.
        cameraComponent.aspect =
            (float)resolutionWidth / resolutionHeight;

        // Unity Camera.fieldOfView is vertical FOV.
        // Convert the requested horizontal FOV to vertical FOV.
        float horizontalFOVRadians =
            horizontalFOV * Mathf.Deg2Rad;

        float verticalFOVRadians =
            2f * Mathf.Atan(
                Mathf.Tan(horizontalFOVRadians * 0.5f) /
                cameraComponent.aspect
            );

        cameraComponent.fieldOfView =
            verticalFOVRadians * Mathf.Rad2Deg;

        // Set initial camera position and rotation.
        transform.position = initialPosition;
        transform.rotation = Quaternion.Euler(initialRotation);
    }

    public int GetResolutionWidth()
    {
        return resolutionWidth;
    }

    public int GetResolutionHeight()
    {
        return resolutionHeight;
    }

    public float GetHorizontalFOV()
    {
        return horizontalFOV;
    }

    public float GetVerticalFOV()
    {
        return cameraComponent != null
            ? cameraComponent.fieldOfView
            : 0f;
    }

    public float GetUpdateRateHz()
    {
        return updateRateHz;
    }
}