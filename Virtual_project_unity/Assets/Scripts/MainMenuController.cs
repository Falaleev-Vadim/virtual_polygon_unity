using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour
{
    public TMP_InputField speedInput;
    public TMP_InputField angleInput;
    public TMP_InputField dragCoeffInput;
    public TMP_InputField massInput;
    public TMP_InputField caliberInput;
    public TMP_InputField targetDistanceInput;
    public TMP_InputField startToPolygonInput;
    //public TMP_InputField azimuthAngleInput;

    public WeatherManager weatherManager;

    [Header("Серия выстрелов")]
    public TMP_InputField shotCountInput; // Количество выстрелов (1-10)
    public TMP_InputField dispersionInput; // Величина отклонений в %
    public Button startSeriesButton;
    //public GameObject seriesPanel; // Панель для управления серией

    void Start()
    {
        // Добавьте задержку для инициализации WeatherManager
        StartCoroutine(InitializeAfterDelay());
        startSeriesButton.onClick.AddListener(StartShotSeries);
    }

    public void StartShotSeries()
    {
        if (!ValidateInputs()) return;

        if (!int.TryParse(shotCountInput.text, out int shotCount) || shotCount < 1 || shotCount > 10)
        {
            Debug.LogError("Количество выстрелов должно быть от 1 до 10");
            return;
        }

        if (!float.TryParse(dispersionInput.text, out float dispersion) || dispersion < 0 || dispersion > 50)
        {
            Debug.LogError("Величина отклонений должна быть от 0 до 50%");
            return;
        }

        // Сохраняем базовые параметры
        SimulationParameters baseParameters = new SimulationParameters
        {
            initialSpeed = float.Parse(speedInput.text),
            elevationAngle = float.Parse(angleInput.text),
            azimuthAngle = 300,//float.Parse(azimuthAngleInput.text),
            dragCoefficient = float.Parse(dragCoeffInput.text),
            mass = float.Parse(massInput.text),
            caliberMm = float.Parse(caliberInput.text),
            targetDistanceKm = float.Parse(targetDistanceInput.text),
            startToPolygonDistance = float.Parse(startToPolygonInput.text),
            windSpeed = WeatherManager.Instance.windSpeed,
            windDirection = WeatherManager.Instance.windDirection,
            temperature = WeatherManager.Instance.temperature,
            altitude = WeatherManager.Instance.altitude,
            turbulenceLevel = WeatherManager.Instance.turbulenceLevel
        };

        // Генерируем параметры для каждого выстрела с отклонениями
        var seriesParameters = new List<SimulationParameters>();

        for (int i = 0; i < shotCount; i++)
        {
            SimulationParameters shotParams = baseParameters;

            // Применяем случайные отклонения
            float speedDeviation = baseParameters.initialSpeed * (dispersion / 100f) * UnityEngine.Random.Range(-1f, 1f);
            float angleDeviation = baseParameters.elevationAngle * (dispersion / 100f) * UnityEngine.Random.Range(-1f, 1f);
            float azimuthDeviation = baseParameters.azimuthAngle * (dispersion / 100f) * UnityEngine.Random.Range(-1f, 1f);

            shotParams.initialSpeed += speedDeviation;
            shotParams.elevationAngle += angleDeviation;
            shotParams.azimuthAngle += azimuthDeviation;

            // Случайные отклонения ветра
            if (!WeatherManager.Instance.isWindSpeedRandom)
                shotParams.windSpeed += UnityEngine.Random.Range(-5f, 5f);

            if (!WeatherManager.Instance.isWindDirectionRandom)
                shotParams.windDirection += UnityEngine.Random.Range(-30f, 30f);

            seriesParameters.Add(shotParams);
        }

        // Сохраняем параметры серии в SimulationData
        SimulationData.SeriesParameters = seriesParameters;
        SimulationData.IsSeriesMode = true;

        SceneManager.LoadScene("Simulation", LoadSceneMode.Single);
    }

    IEnumerator InitializeAfterDelay()
    {
        yield return null; // Даем один кадр на инициализацию

        if (WeatherManager.Instance == null)
        {
            Debug.LogError("WeatherManager не инициализирован!");
            // Можно создать временный экземпляр для предотвращения ошибок
            GameObject tempWeatherManager = new GameObject("TempWeatherManager");
            tempWeatherManager.AddComponent<WeatherManager>();
        }

        // Продолжение инициализации
    }

    public void StartSimulation()
    {
        if (!ValidateInputs()) return;

        SimulationParameters parameters = new SimulationParameters
        {
            initialSpeed = float.Parse(speedInput.text),
            elevationAngle = float.Parse(angleInput.text),
            azimuthAngle = 300,//float.Parse(azimuthAngleInput.text), // Новое поле
            dragCoefficient = float.Parse(dragCoeffInput.text),
            mass = float.Parse(massInput.text),
            caliberMm = float.Parse(caliberInput.text),
            targetDistanceKm = float.Parse(targetDistanceInput.text),
            startToPolygonDistance = float.Parse(startToPolygonInput.text)
        };

        // Получаем текущие погодные условия
        WeatherParameters weather = weatherManager.GetCurrentWeatherParameters();

        parameters.windSpeed = weather.windSpeed;
        parameters.windDirection = weather.windDirection;
        parameters.temperature = weather.temperature;
        parameters.altitude = weather.altitude;
        parameters.turbulenceLevel = weather.turbulenceLevel;

        SimulationData.Parameters = parameters;
        SceneManager.LoadScene("Simulation", LoadSceneMode.Single);
    }

    private bool ValidateInputs()
    {
        if (!ValidateRange(speedInput, 100, 2000)) return false;
        if (!ValidateRange(angleInput, 0, 90)) return false;
        if (!ValidateRange(dragCoeffInput, 0.1f, 2)) return false;
        if (!ValidateRange(massInput, 0.1f, 1000)) return false;
        if (!ValidateRange(caliberInput, 1, 500)) return false;
        if (!ValidateRange(targetDistanceInput, 0.1f, 50)) return false;
        if (!ValidateRange(startToPolygonInput, 0.1f, 50)) return false;
        //if (!ValidateRange(azimuthAngleInput, 0, 360)) return false;
        return true;
    }

    private bool ValidateRange(TMP_InputField input, float min, float max)
    {
        if (!float.TryParse(input.text, out float value))
        {
            Debug.LogError("Некорректный ввод");
            return false;
        }
        if (value < min || value > max)
        {
            Debug.LogError($"Значение {input.name} должно быть между {min} и {max}");
            return false;
        }
        return true;
    }
}