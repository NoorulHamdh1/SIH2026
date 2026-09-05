using System.Collections;
using UnityEngine;

public class ImageNoise : MonoBehaviour
{
    public enum NoiseType
    {
        None = 0,
        SaltAndPepper = 1,
        Gaussian = 2,
        Poisson = 3
    }

    [Header("Noise")]
    [SerializeField] private bool noiseEnabled = false;
    [SerializeField] private NoiseType noiseType = NoiseType.SaltAndPepper;

    [Header("Intensity")]
    [SerializeField, Range(0f, 20f)]
    private float noiseStandardDeviation = 10f;

    [Header("Coverage")]
    [SerializeField, Range(0f, 1f)]
    private float coverage = 0.10f;

    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Textures")]
    [SerializeField] private RenderTexture sourceTexture;
    [SerializeField] private RenderTexture outputTexture;

    [Header("Shader")]
    [SerializeField] private Shader noiseShader;

    private Material noiseMaterial;

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (sourceTexture == null && targetCamera != null)
            sourceTexture = targetCamera.targetTexture;

        if (noiseShader == null)
            noiseShader = Shader.Find("SIH26169/ImageNoise");

        if (noiseShader == null)
        {
            Debug.LogError("ImageNoise: Noise shader not found.");
            enabled = false;
            return;
        }

        if (sourceTexture == null)
        {
            Debug.LogError("ImageNoise: Source RenderTexture is missing.");
            enabled = false;
            return;
        }

        if (outputTexture == null)
        {
            Debug.LogError("ImageNoise: Output RenderTexture is missing.");
            enabled = false;
            return;
        }

        noiseMaterial = new Material(noiseShader);

        StartCoroutine(ProcessFrames());
    }

    private IEnumerator ProcessFrames()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();

            ProcessFrame();
        }
    }

    private void ProcessFrame()
    {
        if (sourceTexture == null || outputTexture == null)
            return;

        if (!noiseEnabled)
        {
            Graphics.Blit(
                sourceTexture,
                outputTexture
            );

            return;
        }

        noiseMaterial.SetFloat(
            "_NoiseType",
            (float)noiseType
        );

        noiseMaterial.SetFloat(
            "_Intensity",
            noiseStandardDeviation
        );

        noiseMaterial.SetFloat(
            "_Coverage",
            coverage
        );

        noiseMaterial.SetFloat(
            "_TimeSeed",
            Time.time
        );

        Graphics.Blit(
            sourceTexture,
            outputTexture,
            noiseMaterial
        );
    }

    private void OnDestroy()
    {
        if (noiseMaterial != null)
            Destroy(noiseMaterial);
    }

    public RenderTexture GetProcessedTexture()
    {
        return outputTexture;
    }

    public RenderTexture GetSourceTexture()
    {
        return sourceTexture;
    }

    public bool IsEnabled()
    {
        return noiseEnabled;
    }

    public NoiseType GetNoiseType()
    {
        return noiseType;
    }

    public float GetStandardDeviation()
    {
        return noiseStandardDeviation;
    }

    public float GetCoverage()
    {
        return coverage;
    }

    public void SetNoiseEnabled(bool enabled)
    {
        noiseEnabled = enabled;
    }

    public void SetNoiseType(NoiseType type)
    {
        noiseType = type;
    }

    public void SetIntensity(float value)
    {
        noiseStandardDeviation =
            Mathf.Clamp(value, 0f, 20f);
    }

    public void SetCoverage(float value)
    {
        coverage =
            Mathf.Clamp01(value);
    }
}