using UnityEngine;

public class TempGroundTruthTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GroundTruthProjection groundTruth;
    [SerializeField] private VirtualPanTiltCamera panTiltCamera;

    [Header("Camera Parameters")]
    [SerializeField] private float horizontalFOV = 4f;
    [SerializeField] private float verticalFOV = 3f;

    [Header("Image Resolution")]
    [SerializeField] private float imageWidth = 640f;
    [SerializeField] private float imageHeight = 480f;

    [Header("Tracking")]
    [SerializeField] private bool trackingEnabled = false;
    [SerializeField] private float deadZonePixels = 2f;

    [Header("Temporary Tracker Speed")]
    [SerializeField] private float maxPanSpeed = 5f;
    [SerializeField] private float maxTiltSpeed = 5f;

    private void Update()
    {
        if (!trackingEnabled)
            return;

        if (groundTruth == null || panTiltCamera == null)
            return;

        if (!groundTruth.IsTargetVisible())
            return;

        Vector2 targetPixel =
            groundTruth.GetPixelPosition();

        // Image center
        float centerX = imageWidth * 0.5f;
        float centerY = imageHeight * 0.5f;

        // Pixel error
        float errorX = targetPixel.x - centerX;
        float errorY = targetPixel.y - centerY;

        // Dead zone
        if (Mathf.Abs(errorX) < deadZonePixels)
            errorX = 0f;

        if (Mathf.Abs(errorY) < deadZonePixels)
            errorY = 0f;

        // Convert pixel error to angular error
        float panError =
            (errorX / imageWidth) * horizontalFOV;

        float tiltError =
            (errorY / imageHeight) * verticalFOV;

        // Maximum movement allowed during this frame
        float maxPanDelta =
            maxPanSpeed * Time.deltaTime;

        float maxTiltDelta =
            maxTiltSpeed * Time.deltaTime;

        // Limit correction to camera slew speed
        float panDelta =
            Mathf.Clamp(
                panError,
                -maxPanDelta,
                maxPanDelta
            );

        float tiltDelta =
            Mathf.Clamp(
                tiltError,
                -maxTiltDelta,
                maxTiltDelta
            );

        // Apply correction
        panTiltCamera.ApplyPanTiltDelta(
            panDelta,
            tiltDelta
        );
    }
}