using UnityEngine;

[CreateAssetMenu(
    fileName = "RandomTrajectory",
    menuName = "SIH26169/Trajectory/Random"
)]
public class RandomTrajectory : BeaconTrajectory
{
    [Header("Movement")]
    [SerializeField] private float speed = 1f;
    [SerializeField] private float directionChangeInterval = 2f;
    [SerializeField] private float movementRange = 15f;

    private Vector3 currentPosition;
    private Vector3 currentDirection;

    private float previousTime = -1f;
    private float lastDirectionChangeTime = -1f;

    public override Vector3 Evaluate(
        float time,
        Vector3 initialPosition
    )
    {
        // First frame
        if (previousTime < 0f)
        {
            currentPosition = initialPosition;
            currentDirection = RandomDirection();

            previousTime = time;
            lastDirectionChangeTime = time;

            return currentPosition;
        }

        // Actual elapsed time since previous frame
        float deltaTime = time - previousTime;

        if (deltaTime <= 0f)
            return currentPosition;

        // Change direction every few seconds
        if (time - lastDirectionChangeTime >= directionChangeInterval)
        {
            currentDirection = RandomDirection();
            lastDirectionChangeTime = time;
        }

        // Move continuously
        currentPosition += currentDirection * speed * deltaTime;

        // Keep movement inside the allowed area
        Vector3 offset = currentPosition - initialPosition;

        if (Mathf.Abs(offset.x) >= movementRange)
        {
            currentDirection.x *= -1f;

            currentPosition.x =
                initialPosition.x +
                Mathf.Clamp(
                    offset.x,
                    -movementRange,
                    movementRange
                );
        }

        if (Mathf.Abs(offset.y) >= movementRange)
        {
            currentDirection.y *= -1f;

            currentPosition.y =
                initialPosition.y +
                Mathf.Clamp(
                    offset.y,
                    -movementRange,
                    movementRange
                );
        }

        previousTime = time;

        return currentPosition;
    }

    private Vector3 RandomDirection()
    {
        Vector2 direction = Random.insideUnitCircle.normalized;

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector2.right;

        return new Vector3(
            direction.x,
            direction.y,
            0f
        );
    }
}