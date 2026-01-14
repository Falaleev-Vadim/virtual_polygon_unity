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

    public float windSpeed;
    public float windDirection;
    public float temperature;
    public float altitude;
    public TurbulenceLevel turbulenceLevel;
}

public static class SimulationData
{
    public static SimulationParameters Parameters { get; set; }
    public static Preset CurrentPreset { get; set; }
    public static LaunchResult CurrentResult { get; set; }
}