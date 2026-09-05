using UnityEngine;

public class BeaconTarget : MonoBehaviour
{
    [Header("Trajectory")]
    [SerializeField] private BeaconTrajectory trajectory;

    [Header("Initial Position")]
    [SerializeField] private Vector3 initialPosition = new Vector3(0f, 0f, 100f);

    [Header("Random Spawn")]
    [SerializeField] private bool randomizeInitialPosition = false;
    [SerializeField] private float randomXRange = 20f;
    [SerializeField] private float randomYRange = 15f;

    [Header("Beacon Size")]
    [SerializeField, Range(5f, 20f)]
    private float beaconSizePixels = 10f;

    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Simulation")]
    [SerializeField] private bool playOnStart = true;

    private float simulationTime;

    private void Start()
    {
        simulationTime = 0f;

        if (randomizeInitialPosition)
        {
            initialPosition = new Vector3(
                Random.Range(-randomXRange, randomXRange),
                Random.Range(-randomYRange, randomYRange),
                initialPosition.z
            );
        }

        transform.position = initialPosition;

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        UpdateBeaconSize();
    }

    private void Update()
    {
        if (!playOnStart || trajectory == null)
            return;

        simulationTime += Time.deltaTime;

        transform.position =
            trajectory.Evaluate(
                simulationTime,
                initialPosition
            );
    }

    private void UpdateBeaconSize()
    {
        if (targetCamera == null)
            return;

        float distance = Vector3.Distance(
            targetCamera.transform.position,
            transform.position
        );

        float verticalFovRadians =
            targetCamera.fieldOfView * Mathf.Deg2Rad;

        float visibleHeight =
            2f * distance * Mathf.Tan(verticalFovRadians * 0.5f);

        float worldSize =
            visibleHeight * (beaconSizePixels / 480f);

        transform.localScale =
            new Vector3(worldSize, worldSize, 1f);
    }
}