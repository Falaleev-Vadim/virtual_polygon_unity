using System;

[Serializable]
public class LaunchResult
{
    public string id = Guid.NewGuid().ToString();
    public string presetName;
    public DateTime timestamp;
    public float initialSpeed;
    public float angle;
    public float drag;
    public float mass;
    public float caliber;
    public float flightTime;
    public float maxDistance;
    public float maxHeight;
}