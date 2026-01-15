using System;
using System.Collections.Generic;

[Serializable]
public struct SimulationParameters
{
    public float initialSpeed;
    public float elevationAngle; // Угол возвышения (ранее angleDegrees)
    public float azimuthAngle; // Новый параметр - азимутальный угол
    public float dragCoefficient;
    public float mass;
    public float caliberMm;
    public float targetDistanceKm;
    public float startToPolygonDistance;

    // Погодные параметры
    public float windSpeed;
    public float windDirection;
    public float temperature;
    public float altitude;
    public TurbulenceLevel turbulenceLevel;
}

/*public static class SimulationData
{
    public static SimulationParameters Parameters { get; set; }
    public static Preset CurrentPreset { get; set; }
    public static LaunchResult CurrentResult { get; set; }
}*/
public static class SimulationData
{
    public static SimulationParameters Parameters { get; set; }
    public static List<SimulationParameters> SeriesParameters { get; set; }
    public static bool IsSeriesMode { get; set; }
    public static List<LaunchResult> SeriesResults { get; set; } = new List<LaunchResult>();
}