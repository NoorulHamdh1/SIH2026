using UnityEngine;

public class AtmosphericConditions : MonoBehaviour
{
    public enum Condition
    {
        Clear,
        Haze,
        Fog,
        Rain,
        LowLight
    }

    [Header("Condition")]
    [SerializeField]
    private Condition currentCondition = Condition.Clear;

    [Header("Intensity")]
    [SerializeField, Range(0f, 1f)]
    private float intensity = 0.5f;

    [Header("Camera")]
    [SerializeField]
    private Camera targetCamera;

    private Color originalBackgroundColor;

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera != null)
            originalBackgroundColor =
                targetCamera.backgroundColor;

        ApplyCondition();
    }

    private void Update()
    {
        ApplyCondition();
    }

    private void ApplyCondition()
    {
        if (targetCamera == null)
            return;

        Color baseColor = originalBackgroundColor;

        switch (currentCondition)
        {
            case Condition.Clear:
                targetCamera.backgroundColor = baseColor;
                break;

            case Condition.Haze:
                targetCamera.backgroundColor =
                    Color.Lerp(
                        baseColor,
                        Color.gray,
                        0.15f * intensity
                    );
                break;

            case Condition.Fog:
                targetCamera.backgroundColor =
                    Color.Lerp(
                        baseColor,
                        Color.gray,
                        0.35f * intensity
                    );
                break;

            case Condition.Rain:
                targetCamera.backgroundColor =
                    Color.Lerp(
                        baseColor,
                        Color.gray,
                        0.20f * intensity
                    );
                break;

            case Condition.LowLight:
                targetCamera.backgroundColor =
                    baseColor * (1f - 0.7f * intensity);
                break;
        }
    }

    public void SetCondition(Condition condition)
    {
        currentCondition = condition;
        ApplyCondition();
    }

    public Condition GetCondition()
    {
        return currentCondition;
    }
}