using UnityEngine;

[CreateAssetMenu(
    fileName = "CircularTrajectory",
    menuName = "SIH26169/Trajectory/Circular"
)]
public class CircularTrajectory : BeaconTrajectory
{
    [Header("Circle")]
    [SerializeField]
    private float radius = 10f;
//hi

    [SerializeField]
    private float angularSpeed = 0.5f;

    [Header("Plane")]
    [SerializeField]
    private Vector3 plane = Vector3.up;

    public override Vector3 Evaluate(
        float time,
        Vector3 initialPosition
    )
    {
        float angle = time * angularSpeed;

        float x = Mathf.Cos(angle) * radius;
        float y = Mathf.Sin(angle) * radius;

        return initialPosition + new Vector3(x, y, 0f);
    }
}
