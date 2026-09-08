using System.Collections;
using UnityEngine;

public class CameraFrameCapture : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private RenderTexture sourceTexture;

    [Header("Resolution")]
    [SerializeField] private int frameWidth = 640;
    [SerializeField] private int frameHeight = 480;

    [Header("Capture")]
    [SerializeField] private float captureRateHz = 30f;

    private Texture2D frameTexture;
    private UnityTCPServer tcpServer;

    private void Start()
    {
        tcpServer =
            FindFirstObjectByType<UnityTCPServer>();

        if (tcpServer == null)
        {
            Debug.LogError(
                "[FrameCapture] UnityTCPServer not found."
            );

            enabled = false;
            return;
        }

        if (sourceTexture == null)
        {
            Debug.LogError(
                "[FrameCapture] Source RenderTexture is missing."
            );

            enabled = false;
            return;
        }

        frameTexture = new Texture2D(
            frameWidth,
            frameHeight,
            TextureFormat.RGBA32,
            false
        );

        StartCoroutine(CaptureFrames());
    }

    private IEnumerator CaptureFrames()
    {
        float interval = 1f / captureRateHz;

        while (true)
        {
            yield return new WaitForEndOfFrame();

            CaptureFrame();

            yield return new WaitForSeconds(interval);
        }
    }

    private void CaptureFrame()
    {
        if (sourceTexture == null)
            return;

        RenderTexture previous =
            RenderTexture.active;

        RenderTexture.active =
            sourceTexture;

        frameTexture.ReadPixels(
            new Rect(
                0,
                0,
                frameWidth,
                frameHeight
            ),
            0,
            0
        );

        frameTexture.Apply();

        RenderTexture.active =
            previous;

        Color32[] pixels =
            frameTexture.GetPixels32();

        byte[] rgb =
            new byte[
                frameWidth *
                frameHeight *
                3
            ];

        for (int i = 0; i < pixels.Length; i++)
        {
            int rgbIndex = i * 3;

            rgb[rgbIndex] =
                pixels[i].r;

            rgb[rgbIndex + 1] =
                pixels[i].g;

            rgb[rgbIndex + 2] =
                pixels[i].b;
        }

        tcpServer.SubmitFrame(rgb);
    }

    private void OnDestroy()
    {
        if (frameTexture != null)
        {
            Destroy(frameTexture);
        }
    }
}