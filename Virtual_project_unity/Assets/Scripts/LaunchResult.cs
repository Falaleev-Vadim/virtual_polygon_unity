using System;
using UnityEngine;

[System.Serializable]
public class LaunchResult
{
    public string id = Guid.NewGuid().ToString();
    public string presetName;
    public DateTime timestamp;
    public long timestampTicks; // Для сериализации

    // Параметры снаряда
    public float initialSpeed;
    public float angle;
    public float drag;
    public float mass;
    public float caliber;

    // Результаты
    public float flightTime;
    public float maxDistance;
    public float maxHeight;

    // Параметры погоды
    public float windSpeed;
    public float windDirection;
    public float temperature;
    public float altitude;
    public string turbulenceLevel;

    // Свойство для обратной совместимости
    public DateTime Timestamp
    {
        get { return new DateTime(1970, 1, 1).AddSeconds(timestampTicks); }
        set { timestampTicks = (long)(value - new DateTime(1970, 1, 1)).TotalSeconds; }
    }
}