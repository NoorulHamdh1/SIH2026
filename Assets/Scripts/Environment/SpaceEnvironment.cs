using System.Collections.Generic;
using UnityEngine;

public class SpaceEnvironment : MonoBehaviour
{
    [Header("Environment")]
    [SerializeField] private bool environmentEnabled = true;

    [Header("Star Field")]
    [SerializeField] private int starCount = 300;
    [SerializeField] private float starDistance = 500f;
    [SerializeField] private float starSpread = 300f;
    [SerializeField] private float starSize = 0.5f;

    [Header("Reference")]
    [SerializeField] private Camera targetCamera;

    private readonly List<GameObject> stars = new List<GameObject>();
    private bool appliedEnvironmentEnabled;
    private GameObject environmentVisualRoot;

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        appliedEnvironmentEnabled = environmentEnabled;
        SetEnvironmentVisualState(environmentEnabled);
    }

    private void Update()
    {
        if (appliedEnvironmentEnabled == environmentEnabled)
            return;

        appliedEnvironmentEnabled = environmentEnabled;

        SetEnvironmentVisualState(environmentEnabled);
    }

    // The generated environment is isolated under one visual root. Toggling it
    // therefore removes the complete SpaceEnvironment visual contribution from
    // the camera rather than merely hiding individual star renderers.
    private void SetEnvironmentVisualState(bool enabled)
    {
        if (enabled && environmentVisualRoot == null)
        {
            environmentVisualRoot = new GameObject("SpaceEnvironmentVisuals");
            GenerateStars();
        }

        if (environmentVisualRoot != null)
            environmentVisualRoot.SetActive(enabled);
    }

    private void GenerateStars()
    {
        if (targetCamera == null)
        {
            Debug.LogError("SpaceEnvironment: No target camera assigned.");
            return;
        }

        for (int i = 0; i < starCount; i++)
        {
            GameObject star = GameObject.CreatePrimitive(
                PrimitiveType.Sphere
            );

            star.name = "Star_" + i;
            star.transform.SetParent(environmentVisualRoot.transform, true);
            stars.Add(star);

            Vector3 randomDirection =
                Random.insideUnitSphere.normalized;

            Vector3 position =
                targetCamera.transform.position +
                randomDirection * starDistance;

            star.transform.position = position;
            star.transform.localScale =
                Vector3.one * starSize;

            Renderer renderer = star.GetComponent<Renderer>();

            if (renderer != null)
            {
                renderer.material =
                    new Material(
                        Shader.Find("Universal Render Pipeline/Lit")
                    );

                renderer.material.color = Color.white;
            }

            Destroy(star.GetComponent<Collider>());
        }
    }
}
