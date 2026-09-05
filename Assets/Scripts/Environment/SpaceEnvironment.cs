using UnityEngine;

public class SpaceEnvironment : MonoBehaviour
{
    [Header("Star Field")]
    [SerializeField] private int starCount = 300;
    [SerializeField] private float starDistance = 500f;
    [SerializeField] private float starSpread = 300f;
    [SerializeField] private float starSize = 0.5f;

    [Header("Reference")]
    [SerializeField] private Camera targetCamera;

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        GenerateStars();
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