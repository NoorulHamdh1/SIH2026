using UnityEngine;

[CreateAssetMenu(
    fileName = "Figure8Trajectory",
    menuName = "SIH26169/Trajectory/Figure 8"
)]
public class Figure8Trajectory : BeaconTrajectory
{
    [Header("Figure 8")]
    [SerializeField]
    private float width = 10f;

    [SerializeField]
    private float height = 5f;

    [SerializeField]
    private float angularSpeed = 0.5f;

    public override Vector3 Evaluate(
        float time,
        Vector3 initialPosition
    )
    {
        float angle = time * angularSpeed;

        float x = Mathf.Sin(angle) * width;
        float y = Mathf.Sin(angle * 2f) * height;

        return initialPosition + new Vector3(x, y, 0f);
    }
}