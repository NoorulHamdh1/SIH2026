using UnityEngine;
using UnityEngine.UI;

public class TrackingOverlayUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform overlay;
    [SerializeField] private Camera sensorCamera;
    [SerializeField] private Transform beacon;

    [Header("Center Lines")]
    [SerializeField] private float lineThickness = 2f;
    [SerializeField] private float dashLength = 6f;
    [SerializeField] private float gapLength = 10f;

    [Header("Tracking Box")]
    [SerializeField] private float boxSize = 24f;
    [SerializeField] private float boxThickness = 2f;

    [Header("Colors")]
    [SerializeField]
    private Color centerLineColor =
        new Color(0.75f, 0.75f, 0.75f, 0.65f);

    [SerializeField]
    private Color trackingBoxColor =
        new Color(0.1f, 0.75f, 0.35f, 0.9f);

    private RectTransform trackingBox;

    private void Start()
    {
        if (overlay == null)
            overlay = GetComponent<RectTransform>();

        CreateCenterLines();
        CreateTrackingBox();
    }

    private void Update()
    {
        if (sensorCamera == null || beacon == null)
        {
            SetTrackingBoxVisible(false);
            return;
        }

        Vector3 viewport =
            sensorCamera.WorldToViewportPoint(beacon.position);

        bool visible =
            viewport.z > 0f &&
            viewport.x >= 0f &&
            viewport.x <= 1f &&
            viewport.y >= 0f &&
            viewport.y <= 1f;

        if (!visible)
        {
            SetTrackingBoxVisible(false);
            return;
        }

        SetTrackingBoxVisible(true);

        // Convert viewport coordinates (0-1)
        // into the overlay's local coordinates.
        float x =
            (viewport.x - 0.5f) *
            overlay.rect.width;

        float y =
            (viewport.y - 0.5f) *
            overlay.rect.height;

        trackingBox.anchoredPosition =
            new Vector2(x, y);
    }

    // =========================================================
    // CENTER DASHED LINES
    // =========================================================

    private void CreateCenterLines()
    {
        CreateHorizontalDashedLine();
        CreateVerticalDashedLine();
    }

    private void CreateHorizontalDashedLine()
    {
        float width = overlay.rect.width;

        for (float x = -width / 2f;
             x < width / 2f;
             x += dashLength + gapLength)
        {
            float remaining =
                width / 2f - x;

            float segmentWidth =
                Mathf.Min(dashLength, remaining);

            CreateLineSegment(
                "HorizontalDash",
                new Vector2(
                    segmentWidth,
                    lineThickness
                ),
                new Vector2(
                    x + segmentWidth / 2f,
                    0f
                )
            );
        }
    }

    private void CreateVerticalDashedLine()
    {
        float height = overlay.rect.height;

        for (float y = -height / 2f;
             y < height / 2f;
             y += dashLength + gapLength)
        {
            float remaining =
                height / 2f - y;

            float segmentHeight =
                Mathf.Min(dashLength, remaining);

            CreateLineSegment(
                "VerticalDash",
                new Vector2(
                    lineThickness,
                    segmentHeight
                ),
                new Vector2(
                    0f,
                    y + segmentHeight / 2f
                )
            );
        }
    }

    private void CreateLineSegment(
        string objectName,
        Vector2 size,
        Vector2 position)
    {
        GameObject segment =
            new GameObject(objectName);

        segment.transform.SetParent(
            overlay,
            false
        );

        RectTransform rect =
            segment.AddComponent<RectTransform>();

        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image image =
            segment.AddComponent<Image>();

        image.color = centerLineColor;
        image.raycastTarget = false;
    }

    // =========================================================
    // BEACON TRACKING BOX
    // =========================================================

    private void CreateTrackingBox()
    {
        GameObject box =
            new GameObject("BeaconTrackingBox");

        box.transform.SetParent(
            overlay,
            false
        );

        trackingBox =
            box.AddComponent<RectTransform>();

        trackingBox.sizeDelta =
            new Vector2(
                boxSize,
                boxSize
            );

        CreateBoxEdge(
            "Top",
            new Vector2(
                boxSize,
                boxThickness
            ),
            new Vector2(
                0f,
                boxSize / 2f
            )
        );

        CreateBoxEdge(
            "Bottom",
            new Vector2(
                boxSize,
                boxThickness
            ),
            new Vector2(
                0f,
                -boxSize / 2f
            )
        );

        CreateBoxEdge(
            "Left",
            new Vector2(
                boxThickness,
                boxSize
            ),
            new Vector2(
                -boxSize / 2f,
                0f
            )
        );

        CreateBoxEdge(
            "Right",
            new Vector2(
                boxThickness,
                boxSize
            ),
            new Vector2(
                boxSize / 2f,
                0f
            )
        );
    }

    private void CreateBoxEdge(
        string objectName,
        Vector2 size,
        Vector2 position)
    {
        GameObject edge =
            new GameObject(objectName);

        edge.transform.SetParent(
            trackingBox,
            false
        );

        RectTransform rect =
            edge.AddComponent<RectTransform>();

        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image image =
            edge.AddComponent<Image>();

        image.color = trackingBoxColor;
        image.raycastTarget = false;
    }

    private void SetTrackingBoxVisible(bool visible)
    {
        if (trackingBox != null)
        {
            trackingBox.gameObject.SetActive(visible);
        }
    }
}