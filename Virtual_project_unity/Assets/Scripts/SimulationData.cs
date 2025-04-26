using System;

[Serializable]
public struct SimulationParameters
{
    public float initialSpeed;
    public float angleDegrees;
    public float dragCoefficient;
    public float mass;
    public float caliberMm;
    public float targetDistanceKm;
    public float startToPolygonDistance;
}

public static class SimulationData
{
    public static SimulationParameters Parameters { get; set; }
}