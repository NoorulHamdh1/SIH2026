using UnityEngine;

public abstract class BeaconTrajectory : ScriptableObject
{
    public abstract Vector3 Evaluate(
        float time,
        Vector3 initialPosition
    );
}