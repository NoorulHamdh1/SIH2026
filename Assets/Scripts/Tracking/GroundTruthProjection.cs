using UnityEngine;

public class GroundTruthProjection : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera sensorCamera;
    [SerializeField] private BeaconTarget target;

    [Header("Camera Resolution")]
    [SerializeField] private int imageWidth = 640;
    [SerializeField] private int imageHeight = 480;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private Vector2 currentPixel;
    private bool targetVisible;

    private void Update()
    {
        if (sensorCamera == null || target == null)
            return;

        Vector3 screenPosition =
            sensorCamera.WorldToScreenPoint(
                target.transform.position
            );

        targetVisible =
            screenPosition.z > 0f &&
            screenPosition.x >= 0f &&
            screenPosition.x <= imageWidth &&
            screenPosition.y >= 0f &&
            screenPosition.y <= imageHeight;

        if (targetVisible)
        {
            currentPixel = new Vector2(
                screenPosition.x,
                screenPosition.y
            );
        }
    }

    private void OnGUI()
    {
        if (!showDebugInfo)
            return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        string status =
            targetVisible ? "VISIBLE" : "OUT OF VIEW";

        GUI.Label(
            new Rect(20, 20, 400, 30),
            "GROUND TRUTH"
        );

        GUI.Label(
            new Rect(20, 50, 400, 30),
            $"Pixel X: {currentPixel.x:F1}"
        );

        GUI.Label(
            new Rect(20, 80, 400, 30),
            $"Pixel Y: {currentPixel.y:F1}"
        );

        GUI.Label(
            new Rect(20, 110, 400, 30),
            $"Target: {status}"
        );
    }

    public Vector2 GetPixelPosition()
    {
        return currentPixel;
    }

    public bool IsTargetVisible()
    {
        return targetVisible;
    }

    public float GetPixelX()
    {
        return currentPixel.x;
    }

    public float GetPixelY()
    {
        return currentPixel.y;
    }
}