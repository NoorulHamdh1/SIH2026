using UnityEngine;
using UnityEngine.UI;

/// <summary>Read-only presentation layer for the existing FSOC simulation.</summary>
public sealed class FSOCControlCenterUI : MonoBehaviour
{
    // Intentionally restrained engineering palette: no gradients, glow, or decorative overlays.
    private readonly Color background = new Color(.047f, .057f, .068f, 1f);
    private readonly Color panel = new Color(.082f, .098f, .115f, 1f);
    private readonly Color panelDark = new Color(.057f, .069f, .081f, 1f);
    private readonly Color border = new Color(.22f, .27f, .31f, 1f);
    private readonly Color accent = new Color(.38f, .62f, .76f, 1f);
    private readonly Color muted = new Color(.57f, .64f, .69f, 1f);
    private readonly Color text = new Color(.90f, .92f, .93f, 1f);
    private readonly Color success = new Color(.39f, .66f, .49f, 1f);
    private readonly Color stop = new Color(.72f, .37f, .39f, 1f);
    private Sprite roundedPanelSprite;

    private VirtualPanTiltCamera panTilt;
    private BeaconTarget target;
    private GroundTruthProjection groundTruth;
    private UnityTCPServer tcpServer;
    private Camera sensorCamera;
    private RawImage cameraFeed;

    private Text runStatus, feedFps, simulationState, tcpState;
    private Text targetName, targetPosition, distance, speed, lockStatus;
    private Text pan, tilt, cameraPosition, panCorrection, tiltCorrection, trackingError;
    private Text telemetryFps, telemetryTcp, packetId;
    private Vector3 previousTargetPosition;
    private bool hasPreviousTargetPosition;
    private float targetSpeed;

    private void Start()
    {
        roundedPanelSprite = CreateRoundedPanelSprite();
        panTilt = FindFirstObjectByType<VirtualPanTiltCamera>();
        target = FindFirstObjectByType<BeaconTarget>();
        groundTruth = FindFirstObjectByType<GroundTruthProjection>();
        tcpServer = FindFirstObjectByType<UnityTCPServer>();
        GameObject sensor = GameObject.Find("SensorCamera");
        sensorCamera = sensor != null ? sensor.GetComponent<Camera>() : null;
        cameraFeed = GetComponentInChildren<RawImage>(true);
        BuildOverlay();
    }

    // This presentation mask hides the existing IMGUI ground-truth debug text
    // without changing the tracking script that owns it.
    private void OnGUI()
    {
        if (!Application.isPlaying)
            return;

        GUI.depth = -1000;
        GUI.color = background;
        GUI.DrawTexture(new Rect(0f, 0f, 430f, 135f), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Draw the product identity above the mask that suppresses the existing
        // ground-truth debug readout in this same screen area.
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.RoundToInt(24f * Screen.height / 768f),
            fontStyle = FontStyle.Bold,
            normal = { textColor = text }
        };
        GUIStyle subtitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.RoundToInt(11f * Screen.height / 768f),
            fontStyle = FontStyle.Bold,
            normal = { textColor = muted }
        };
        GUI.Label(new Rect(20f, 14f, Screen.width - 40f, 34f), "FSOC SIMULATION", titleStyle);
        GUI.Label(new Rect(22f, 50f, Screen.width - 44f, 18f), "FREE SPACE OPTICAL COMMUNICATION", subtitleStyle);
    }

    private void Update()
    {
        if (target != null)
        {
            if (hasPreviousTargetPosition && Time.deltaTime > 0f)
                targetSpeed = Vector3.Distance(target.transform.position, previousTargetPosition) / Time.deltaTime;
            previousTargetPosition = target.transform.position;
            hasPreviousTargetPosition = true;
        }
        UpdateReadout();
    }

    private void BuildOverlay()
    {
        RectTransform root = Panel("FSOC Control Center", transform, background);
        Stretch(root, 0f, 0f, 1f, 1f);
        root.SetAsFirstSibling();
        BuildHeader(root);
        BuildCamera(root);
        BuildSidebar(root);
        Label("Footer Brand", root, "SIH2026  |  FSOC SIMULATOR", 9, muted, TextAnchor.MiddleLeft, 14f, .008f, .42f, .040f);
        Label("Footer Stack", root, "UNITY  |  PYTHON  |  OPENCV  |  KALMAN  |  PID", 9, muted, TextAnchor.MiddleRight, 0f, .008f, .985f, .040f);
    }

    private void BuildHeader(RectTransform root)
    {
        // Keep the header visually continuous with the background; the title is
        // the only top-level identity, with operational context in the sidebar.
        RectTransform header = Panel("Header", root, background);
        Stretch(header, 0f, .885f, 1f, 1f);
        Label("Product Name", header, "FSOC SIMULATION", 18, text, TextAnchor.MiddleLeft, 20f, .36f, .50f, .94f);
        Label("Product Subtitle", header, "FREE SPACE OPTICAL COMMUNICATION", 9, muted, TextAnchor.MiddleLeft, 22f, .12f, .50f, .40f);
    }

    private void BuildCamera(RectTransform root)
    {
        RectTransform section = Section("Live Camera Feed", root, .015f, .052f, .725f, .865f);
        Label("Feed Title", section, "LIVE CAMERA FEED", 11, text, TextAnchor.MiddleLeft, 12f, .925f, .55f, .99f);
        Label("Resolution", section, "640 × 480", 10, muted, TextAnchor.MiddleRight, 0f, .925f, .86f, .99f);
        feedFps = Label("Feed FPS", section, "FPS  --", 10, muted, TextAnchor.MiddleRight, 0f, .925f, .97f, .99f);
        RectTransform frame = Panel("Camera Feed Frame", section, Color.black);
        Stretch(frame, .012f, .018f, .988f, .910f);
        if (cameraFeed != null)
        {
            // The existing RawImage and RenderTexture are retained; only UI placement changes.
            cameraFeed.transform.SetParent(frame, false);
            Stretch(cameraFeed.rectTransform, 0f, 0f, 1f, 1f, 5f, 5f, -5f, -5f);
            cameraFeed.raycastTarget = false;
        }
    }

    private void BuildSidebar(RectTransform root)
    {
        RectTransform sidebar = Panel("System Status Sidebar", root, background);
        Stretch(sidebar, .745f, .052f, .985f, .865f);

        RectTransform system = Section("System Status", sidebar, 0f, .845f, 1f, 1f);
        Title(system, "SYSTEM STATUS & CONTROLS");
        Metric(system, "Target / Mode", "BEACON  |  AUTO", .47f);
        simulationState = Metric(system, "Environment / Simulation", "SPACE  |  RUNNING", .16f);

        RectTransform beacon = Section("Beacon Target", sidebar, 0f, .535f, 1f, .830f);
        Title(beacon, "BEACON / TARGET");
        targetName = Metric(beacon, "Target Name", "--", .70f);
        targetPosition = Metric(beacon, "Position (X, Y, Z)", "--", .55f);
        distance = Metric(beacon, "Distance", "--", .40f);
        Metric(beacon, "Trajectory", "--", .25f);
        speed = Metric(beacon, "Speed", "--", .10f);
        lockStatus = LabelAt("Lock Status", beacon, "○  TARGET SEARCHING", 8, muted, TextAnchor.MiddleLeft, .45f, .005f, .96f, .085f);

        RectTransform camera = Section("Camera Control", sidebar, 0f, .335f, 1f, .520f);
        Title(camera, "CAMERA CONTROL");
        pan = Metric(camera, "Pan", "--", .54f);
        tilt = Metric(camera, "Tilt", "--", .31f);
        cameraPosition = Metric(camera, "Camera Position (X, Y, Z)", "--", .08f);

        RectTransform tracking = Section("Tracking", sidebar, 0f, .155f, 1f, .320f);
        Title(tracking, "TRACKING");
        Metric(tracking, "Mode", "AUTO", .58f);
        panCorrection = Metric(tracking, "Pan Correction", "--", .40f);
        tiltCorrection = Metric(tracking, "Tilt Correction", "--", .22f);
        trackingError = Metric(tracking, "Tracking Error", "--", .04f);

        RectTransform telemetry = Section("Telemetry", sidebar, 0f, 0f, 1f, .140f);
        Title(telemetry, "TELEMETRY");
        telemetryFps = CompactMetric(telemetry, "FPS", "--", .71f);
        telemetryTcp = CompactMetric(telemetry, "TCP Connection", "--", .54f);
        CompactMetric(telemetry, "Latency", "--", .37f);
        CompactMetric(telemetry, "Frames Received", "--", .20f);
        packetId = CompactMetric(telemetry, "Packet ID", "--", .03f);
    }

    private void BuildControls(RectTransform root)
    {
        RectTransform controls = Section("Simulation Controls", root, .015f, .052f, .985f, .120f);
        Label("Controls Title", controls, "SIMULATION CONTROLS", 9, muted, TextAnchor.MiddleLeft, 12f, 0f, .22f, 1f);
        string[] names = { "START", "PAUSE", "STOP", "RESET", "SETTINGS" };
        for (int i = 0; i < names.Length; i++)
        {
            float minX = .27f + i * .115f;
            RectTransform button = Panel(names[i], controls, panelDark);
            Stretch(button, minX, .19f, minX + .098f, .81f);
            Color labelColor = names[i] == "STOP" ? stop : names[i] == "START" ? success : text;
            Label("Label", button, names[i], 9, labelColor, TextAnchor.MiddleCenter, 0f, 0f, 1f, 1f);
        }
    }

    private void UpdateReadout()
    {
        bool running = Application.isPlaying;
        bool connected = tcpServer != null && tcpServer.IsClientConnected();
        bool visible = groundTruth != null && groundTruth.IsTargetVisible();
        string fpsValue = (1f / Mathf.Max(Time.unscaledDeltaTime, .0001f)).ToString("F0");
        if (runStatus != null) runStatus.text = running ? "●  SIMULATION RUNNING" : "○  SIMULATION IDLE";
        if (simulationState != null) simulationState.text = running ? "SPACE  |  RUNNING" : "SPACE  |  IDLE";
        if (feedFps != null) feedFps.text = "FPS  " + fpsValue;
        if (targetName != null) targetName.text = target != null ? target.name : "--";
        if (targetPosition != null) targetPosition.text = target != null ? Vector(target.transform.position) : "--";
        if (distance != null) distance.text = target != null && sensorCamera != null ? Vector3.Distance(sensorCamera.transform.position, target.transform.position).ToString("F2") + " m" : "--";
        if (speed != null) speed.text = target != null ? targetSpeed.ToString("F2") + " m/s" : "--";
        SetStatus(lockStatus, visible ? "●  TARGET LOCKED" : "○  TARGET SEARCHING", visible);
        if (pan != null) pan.text = panTilt != null ? panTilt.GetPan().ToString("+0.00;-0.00;0.00") + "°" : "--";
        if (tilt != null) tilt.text = panTilt != null ? panTilt.GetTilt().ToString("+0.00;-0.00;0.00") + "°" : "--";
        if (cameraPosition != null) cameraPosition.text = sensorCamera != null ? Vector(sensorCamera.transform.position) : "--";
        if (panCorrection != null) panCorrection.text = panTilt != null ? panTilt.GetPan().ToString("F2") + "°" : "--";
        if (tiltCorrection != null) tiltCorrection.text = panTilt != null ? panTilt.GetTilt().ToString("F2") + "°" : "--";
        if (trackingError != null) trackingError.text = visible ? Vector2.Distance(groundTruth.GetPixelPosition(), new Vector2(320f, 240f)).ToString("F1") + " px" : "--";
        if (telemetryFps != null) telemetryFps.text = fpsValue;
        SetStatus(telemetryTcp, connected ? "CONNECTED" : "DISCONNECTED", connected);
        if (packetId != null) packetId.text = tcpServer != null ? tcpServer.GetFrameId().ToString() : "--";
    }

    private void SetStatus(Text label, string value, bool active)
    {
        if (label == null) return;
        label.text = value;
        label.color = active ? success : muted;
    }

    private Text Metric(RectTransform parent, string name, string value, float y)
    {
        LabelAt(name + " Label", parent, name, 9, muted, TextAnchor.MiddleLeft, .035f, y, .43f, y + .105f);
        return LabelAt(name + " Value", parent, value, 10, text, TextAnchor.MiddleLeft, .45f, y, .96f, y + .105f);
    }

    private Text CompactMetric(RectTransform parent, string name, string value, float y)
    {
        LabelAt(name + " Label", parent, name, 8, muted, TextAnchor.MiddleLeft, .035f, y, .48f, y + .12f);
        return LabelAt(name + " Value", parent, value, 9, text, TextAnchor.MiddleLeft, .50f, y, .96f, y + .12f);
    }

    private void Title(RectTransform section, string value)
    {
        Label("Section Title", section, value, 11, accent, TextAnchor.MiddleLeft, 10f, .87f, .96f, .99f);
    }

    private RectTransform Section(string name, Transform parent, float minX, float minY, float maxX, float maxY)
    {
        RectTransform outer = Panel(name, parent, border);
        Stretch(outer, minX, minY, maxX, maxY);
        RectTransform inner = Panel("Content", outer, panel);
        Stretch(inner, 0f, 0f, 1f, 1f, 1f, 1f, -1f, -1f);
        return inner;
    }

    private RectTransform Panel(string name, Transform parent, Color color)
    {
        GameObject item = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        item.transform.SetParent(parent, false);
        Image image = item.GetComponent<Image>();
        image.sprite = roundedPanelSprite;
        image.type = roundedPanelSprite != null ? Image.Type.Sliced : Image.Type.Simple;
        image.color = color;
        image.raycastTarget = false;
        return item.GetComponent<RectTransform>();
    }

    private Text Label(string name, Transform parent, string value, int size, Color color, TextAnchor alignment, float left, float minY, float maxX, float maxY)
    {
        GameObject item = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        item.transform.SetParent(parent, false);
        Text label = item.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = value;
        label.fontSize = Mathf.RoundToInt(size * 1.45f);
        label.fontStyle = FontStyle.Bold;
        label.color = color;
        label.alignment = alignment;
        label.raycastTarget = false;
        Stretch(item.GetComponent<RectTransform>(), 0f, minY, maxX, maxY, left, 0f, 0f, 0f);
        return label;
    }

    private Text LabelAt(string name, Transform parent, string value, int size, Color color, TextAnchor alignment, float minX, float minY, float maxX, float maxY)
    {
        GameObject item = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        item.transform.SetParent(parent, false);
        Text label = item.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = value;
        label.fontSize = Mathf.RoundToInt(size * 1.45f);
        label.fontStyle = FontStyle.Bold;
        label.color = color;
        label.alignment = alignment;
        label.raycastTarget = false;
        Stretch(item.GetComponent<RectTransform>(), minX, minY, maxX, maxY);
        return label;
    }

    private Sprite CreateRoundedPanelSprite()
    {
        const int size = 32;
        const float radius = 7f;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(radius - x, x - (size - 1 - radius), 0f);
                float dy = Mathf.Max(radius - y, y - (size - 1 - radius), 0f);
                texture.SetPixel(x, y, dx * dx + dy * dy <= radius * radius ? Color.white : Color.clear);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(.5f, .5f), 100f, 0u, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }

    private static void Stretch(RectTransform rect, float minX, float minY, float maxX, float maxY, float left = 0f, float bottom = 0f, float right = 0f, float top = 0f)
    {
        rect.anchorMin = new Vector2(minX, minY);
        rect.anchorMax = new Vector2(maxX, maxY);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(right, top);
    }

    private static string Vector(Vector3 value)
    {
        return value.x.ToString("F2") + "     " + value.y.ToString("F2") + "     " + value.z.ToString("F2");
    }
}
