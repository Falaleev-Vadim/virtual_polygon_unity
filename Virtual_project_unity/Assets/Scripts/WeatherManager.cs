using UnityEngine;
using System;

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance { get; private set; }

    // Основные параметры
    public float windSpeed = 0f; // м/с
    public float windDirection = 0f; // градусы (0 = север, 90 = восток)
    public float temperature = 15f; // °C
    public float altitude = 0f; // м над уровнем моря

    // Уровень турбулентности
    public TurbulenceLevel turbulenceLevel = TurbulenceLevel.Low;

    // Флаги случайных значений
    public bool isWindSpeedRandom = false;
    public bool isWindDirectionRandom = false;
    public bool isTemperatureRandom = false;
    public bool isAltitudeRandom = false;
    public bool isTurbulenceRandom = false;

    private const string SAVE_KEY = "weather_settings";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveSettings()
    {
        WeatherData data = new WeatherData
        {
            windSpeed = windSpeed,
            windDirection = windDirection,
            temperature = temperature,
            altitude = altitude,
            turbulenceLevel = turbulenceLevel,
            isWindSpeedRandom = isWindSpeedRandom,
            isWindDirectionRandom = isWindDirectionRandom,
            isTemperatureRandom = isTemperatureRandom,
            isAltitudeRandom = isAltitudeRandom,
            isTurbulenceRandom = isTurbulenceRandom,
        };

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            WeatherData data = JsonUtility.FromJson<WeatherData>(json);

            windSpeed = data.windSpeed;
            windDirection = data.windDirection;
            temperature = data.temperature;
            altitude = data.altitude;
            turbulenceLevel = data.turbulenceLevel;
            isWindSpeedRandom = data.isWindSpeedRandom;
            isWindDirectionRandom = data.isWindDirectionRandom;
            isTemperatureRandom = data.isTemperatureRandom;
            isAltitudeRandom = data.isAltitudeRandom;
            isTurbulenceRandom = data.isTurbulenceRandom;
        }
        else
        {
            // Значения по умолчанию
            SaveSettings();
        }
    }

    public WeatherParameters GetCurrentWeatherParameters()
    {
        WeatherParameters parameters = new WeatherParameters();

        // Если активирована случайная генерация - генерируем новые значения каждый раз
        if (isWindSpeedRandom)
            parameters.windSpeed = UnityEngine.Random.Range(0f, 50f); // 0-50 м/с
        else
            parameters.windSpeed = windSpeed;

        if (isWindDirectionRandom)
            parameters.windDirection = UnityEngine.Random.Range(0f, 360f); // 0-360 градусов
        else
            parameters.windDirection = windDirection;

        if (isTemperatureRandom)
            parameters.temperature = UnityEngine.Random.Range(-20f, 40f); // -20°C до 40°C
        else
            parameters.temperature = temperature;

        if (isAltitudeRandom)
            parameters.altitude = UnityEngine.Random.Range(0f, 3000f); // 0-3000 м
        else
            parameters.altitude = altitude;

        if (isTurbulenceRandom)
        {
            int randomIndex = UnityEngine.Random.Range(0, 3); // 0 = Low, 1 = Medium, 2 = High
            parameters.turbulenceLevel = (TurbulenceLevel)randomIndex;
        }
        else
        {
            parameters.turbulenceLevel = turbulenceLevel;
        }

        return parameters;
    }

    [System.Serializable]
    private class WeatherData
    {
        public float windSpeed;
        public float windDirection;
        public float temperature;
        public float altitude;
        public TurbulenceLevel turbulenceLevel;
        public bool isWindSpeedRandom;
        public bool isWindDirectionRandom;
        public bool isTemperatureRandom;
        public bool isAltitudeRandom;
        public bool isTurbulenceRandom;
    }
}