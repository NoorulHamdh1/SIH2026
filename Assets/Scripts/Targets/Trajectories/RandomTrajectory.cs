using UnityEngine;

[CreateAssetMenu(
    fileName = "RandomTrajectory",
    menuName = "SIH26169/Trajectory/Random"
)]
public class RandomTrajectory : BeaconTrajectory
{
    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float directionChangeInterval = 2f;
    [SerializeField] private float movementRange = 15f;

    private Vector3 currentDirection;
    private float lastDirectionChange = -999f;

    public override Vector3 Evaluate(float time, Vector3 initialPosition)
    {
        if (time - lastDirectionChange >= directionChangeInterval)
        {
            currentDirection = Random.onUnitSphere;
            currentDirection.z = 0f;

            if (currentDirection.sqrMagnitude < 0.001f)
                currentDirection = Vector3.right;

            currentDirection.Normalize();
            lastDirectionChange = time;
        }

        Vector3 offset = currentDirection * speed * time;

        offset.x = Mathf.Clamp(offset.x, -movementRange, movementRange);
        offset.y = Mathf.Clamp(offset.y, -movementRange, movementRange);

        return initialPosition + offset;
    }
}