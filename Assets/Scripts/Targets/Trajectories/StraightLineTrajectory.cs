using UnityEngine;

[CreateAssetMenu(
    fileName = "StraightLineTrajectory",
    menuName = "SIH26169/Trajectory/Straight Line"
)]
public class StraightLineTrajectory : BeaconTrajectory
{
    [Header("Velocity")]
    public Vector3 velocity = new Vector3(5f, 0f, 0f);

    public override Vector3 Evaluate(
        float time,
        Vector3 initialPosition
    )
    {
        return initialPosition + velocity * time;
    }
}
