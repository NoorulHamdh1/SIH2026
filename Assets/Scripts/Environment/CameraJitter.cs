using UnityEngine;

public class CameraJitter : MonoBehaviour
{
    [Header("Jitter")]
    [SerializeField] private bool jitterEnabled = false;

    [SerializeField, Range(0f, 20f)]
    private float jitterPixelsPerFrame = 2f;

    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Update Rate")]
    [SerializeField, Min(20f)]
    private float updateRateHz = 30f;

    private float updateTimer;

    private Vector3 baseLocalPosition;

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        if (targetCamera == null)
        {
            Debug.LogError("CameraJitter requires a Camera.");
            enabled = false;
            return;
        }

        baseLocalPosition = targetCamera.transform.localPosition;
        updateTimer = 0f;
    }

    private void Update()
    {
        if (!jitterEnabled)
        {
            targetCamera.transform.localPosition = baseLocalPosition;
            return;
        }

        updateTimer += Time.deltaTime;

        float updateInterval = 1f / updateRateHz;

        while (updateTimer >= updateInterval)
        {
            updateTimer -= updateInterval;
            ApplyJitter();
        }
    }

    private void ApplyJitter()
    {
        float horizontalFOV =
            2f * Mathf.Atan(
                Mathf.Tan(
                    targetCamera.fieldOfView *
                    Mathf.Deg2Rad *
                    0.5f
                ) * targetCamera.aspect
            );

        float verticalFOV =
            targetCamera.fieldOfView *
            Mathf.Deg2Rad;

        float horizontalPixels =
            jitterPixelsPerFrame *
            horizontalFOV *
            Mathf.Rad2Deg /
            640f;

        float verticalPixels =
            jitterPixelsPerFrame *
            verticalFOV *
            Mathf.Rad2Deg /
            480f;

        float jitterX =
            Random.Range(-jitterPixelsPerFrame,
                          jitterPixelsPerFrame)
            * horizontalPixels;

        float jitterY =
            Random.Range(-jitterPixelsPerFrame,
                          jitterPixelsPerFrame)
            * verticalPixels;

        targetCamera.transform.localPosition =
            baseLocalPosition +
            new Vector3(jitterX, jitterY, 0f);
    }
}
