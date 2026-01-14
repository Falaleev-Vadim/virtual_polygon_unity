using System;

[System.Serializable]
public class LaunchResult
{
    public string id = Guid.NewGuid().ToString();
    public string presetName;

    // Вместо DateTime храните Unix timestamp (количество секунд с 01.01.1970)
    public long timestampTicks;

    // Для совместимости добавьте свойство, которое конвертирует ticks в DateTime
    public DateTime timestamp
    {
        get { return new DateTime(1970, 1, 1).AddSeconds(timestampTicks); }
        set { timestampTicks = (long)(value - new DateTime(1970, 1, 1)).TotalSeconds; }
    }

    public float initialSpeed;
    public float angle;
    public float drag;
    public float mass;
    public float caliber;
    public float flightTime;
    public float maxDistance;
    public float maxHeight;

    // Параметры погоды
    public float windSpeed;
    public float windDirection;
    public float temperature;
    public float altitude;
    public string turbulenceLevel;
}