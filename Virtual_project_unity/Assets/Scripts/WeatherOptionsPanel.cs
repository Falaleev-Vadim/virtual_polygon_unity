using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class WeatherOptionsPanel : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField windSpeedInput;
    public TMP_InputField windDirectionInput;
    public TMP_InputField temperatureInput;
    public TMP_InputField altitudeInput;
    public TMP_Dropdown windTurbulenceDropdown;

    [Header("Random Toggles")]
    public Toggle windSpeedRandomToggle;
    public Toggle windDirectionRandomToggle;
    public Toggle temperatureRandomToggle;
    public Toggle altitudeRandomToggle;
    public Toggle windTurbulenceRandomToggle;

    [Header("Buttons")]
    public Button enableAllButton;
    public Button disableAllButton;
    public Button cancelButton;
    public Button saveButton;

    // Сохраняем исходные значения для отмены
    private WeatherParameters originalParameters;
    private List<Toggle> allToggles = new List<Toggle>();

    [Header("Menu")]
    [SerializeField] private GameObject weatherOptionsPanel;

    private void Awake()
    {
        // Сохраняем все тогглы для удобства
        allToggles.Add(windSpeedRandomToggle);
        allToggles.Add(windDirectionRandomToggle);
        allToggles.Add(temperatureRandomToggle);
        allToggles.Add(altitudeRandomToggle);
        allToggles.Add(windTurbulenceRandomToggle);

        // Настраиваем обработчики событий
        enableAllButton.onClick.AddListener(EnableAllRandom);
        disableAllButton.onClick.AddListener(DisableAllRandom);
        //cancelButton.onClick.AddListener(ClosePanel);
        saveButton.onClick.AddListener(SaveAndClose);

        // Настраиваем обработчики для тогглов
        SetupToggleListeners();

        // Загружаем текущие настройки
        LoadCurrentSettings();
    }

    public void OpenWeatherOptions()
    {
        weatherOptionsPanel.SetActive(true);
    }

    public void CloseWeatherOptions()
    {
        weatherOptionsPanel.SetActive(false);
    }

    private void SetupToggleListeners()
    {
        windSpeedRandomToggle.onValueChanged.AddListener(value => ToggleInputField(windSpeedInput, !value));
        windDirectionRandomToggle.onValueChanged.AddListener(value => ToggleInputField(windDirectionInput, !value));
        temperatureRandomToggle.onValueChanged.AddListener(value => ToggleInputField(temperatureInput, !value));
        altitudeRandomToggle.onValueChanged.AddListener(value => ToggleInputField(altitudeInput, !value));
        windTurbulenceRandomToggle.onValueChanged.AddListener(value =>
        {
            windTurbulenceDropdown.interactable = !value;
            windTurbulenceDropdown.GetComponent<Image>().color = value ? new Color(0.7f, 0.7f, 0.7f, 1f) : Color.white;
        });
    }

    private void ToggleInputField(TMP_InputField inputField, bool interactable)
    {
        inputField.interactable = interactable;
        inputField.GetComponent<Image>().color = interactable ? Color.white : new Color(0.7f, 0.7f, 0.7f, 1f);
    }

    private void LoadCurrentSettings()
    {
        if (WeatherManager.Instance == null)
        {
            Debug.LogError("WeatherManager не инициализирован! Создайте GameObject с компонентом WeatherManager на сцене.");
            return;
        }

        WeatherManager weatherManager = WeatherManager.Instance;
        originalParameters = new WeatherParameters
        {
            windSpeed = weatherManager.windSpeed,
            windDirection = weatherManager.windDirection,
            temperature = weatherManager.temperature,
            altitude = weatherManager.altitude,
            turbulenceLevel = weatherManager.turbulenceLevel,
            isWindSpeedRandom = weatherManager.isWindSpeedRandom,
            isWindDirectionRandom = weatherManager.isWindDirectionRandom,
            isTemperatureRandom = weatherManager.isTemperatureRandom,
            isAltitudeRandom = weatherManager.isAltitudeRandom,
            isTurbulenceRandom = weatherManager.isTurbulenceRandom,
        };

        // Заполняем поля значениями
        windSpeedInput.text = weatherManager.windSpeed.ToString("F1");
        windDirectionInput.text = weatherManager.windDirection.ToString("F1");
        temperatureInput.text = weatherManager.temperature.ToString("F1");
        altitudeInput.text = weatherManager.altitude.ToString("F0");
        windTurbulenceDropdown.value = (int)weatherManager.turbulenceLevel;

        // Устанавливаем состояния тогглов
        windSpeedRandomToggle.isOn = weatherManager.isWindSpeedRandom;
        windDirectionRandomToggle.isOn = weatherManager.isWindDirectionRandom;
        temperatureRandomToggle.isOn = weatherManager.isTemperatureRandom;
        altitudeRandomToggle.isOn = weatherManager.isAltitudeRandom;
        windTurbulenceRandomToggle.isOn = weatherManager.isTurbulenceRandom;

        // Обновляем доступность полей ввода
        ToggleInputField(windSpeedInput, !weatherManager.isWindSpeedRandom);
        ToggleInputField(windDirectionInput, !weatherManager.isWindDirectionRandom);
        ToggleInputField(temperatureInput, !weatherManager.isTemperatureRandom);
        ToggleInputField(altitudeInput, !weatherManager.isAltitudeRandom);
        windTurbulenceDropdown.interactable = !weatherManager.isTurbulenceRandom;
    }

//    private void ClosePanel()
//    {
//        // Восстанавливаем исходные настройки
//        ApplyParameters(originalParameters);
//        gameObject.SetActive(false);
//    }

    public void SaveAndClose()
    {
        // Сохраняем новые настройки
        SaveSettings();
        CloseWeatherOptions();
    }

    private void SaveSettings()
    {
        WeatherManager weatherManager = WeatherManager.Instance;

        // Сохраняем только базовые настройки, но не текущие значения случайных параметров
        if (!weatherManager.isWindSpeedRandom && float.TryParse(windSpeedInput.text, out float windSpeed))
            weatherManager.windSpeed = windSpeed;

        if (!weatherManager.isWindDirectionRandom && float.TryParse(windDirectionInput.text, out float windDirection))
            weatherManager.windDirection = windDirection;

        if (!weatherManager.isTemperatureRandom && float.TryParse(temperatureInput.text, out float temperature))
            weatherManager.temperature = temperature;

        if (!weatherManager.isAltitudeRandom && float.TryParse(altitudeInput.text, out float altitude))
            weatherManager.altitude = altitude;

        if (!weatherManager.isTurbulenceRandom)
            weatherManager.turbulenceLevel = (TurbulenceLevel)windTurbulenceDropdown.value;

        // Сохраняем только флаги случайной генерации
        weatherManager.isWindSpeedRandom = windSpeedRandomToggle.isOn;
        weatherManager.isWindDirectionRandom = windDirectionRandomToggle.isOn;
        weatherManager.isTemperatureRandom = temperatureRandomToggle.isOn;
        weatherManager.isAltitudeRandom = altitudeRandomToggle.isOn;
        weatherManager.isTurbulenceRandom = windTurbulenceRandomToggle.isOn;

        weatherManager.SaveSettings();
    }

    private void ApplyParameters(WeatherParameters parameters)
    {
        WeatherManager weatherManager = WeatherManager.Instance;

        weatherManager.windSpeed = parameters.windSpeed;
        weatherManager.windDirection = parameters.windDirection;
        weatherManager.temperature = parameters.temperature;
        weatherManager.altitude = parameters.altitude;
        weatherManager.turbulenceLevel = parameters.turbulenceLevel;
        weatherManager.isWindSpeedRandom = parameters.isWindSpeedRandom;
        weatherManager.isWindDirectionRandom = parameters.isWindDirectionRandom;
        weatherManager.isTemperatureRandom = parameters.isTemperatureRandom;
        weatherManager.isAltitudeRandom = parameters.isAltitudeRandom;
        weatherManager.isTurbulenceRandom = parameters.isTurbulenceRandom;

        weatherManager.SaveSettings();
    }

    private void EnableAllRandom()
    {
        foreach (var toggle in allToggles)
        {
            toggle.isOn = true;
        }
    }

    private void DisableAllRandom()
    {
        foreach (var toggle in allToggles)
        {
            toggle.isOn = false;
        }
    }
}

[System.Serializable]
public class WeatherParameters
{
    public float windSpeed = 0f;
    public float windDirection = 0f;
    public float temperature = 15f; // Стандартная температура по умолчанию
    public float altitude = 0f; // Уровень моря по умолчанию

    public TurbulenceLevel turbulenceLevel = TurbulenceLevel.Low;

    public bool isWindSpeedRandom = false;
    public bool isWindDirectionRandom = false;
    public bool isTemperatureRandom = false;
    public bool isAltitudeRandom = false;
    public bool isTurbulenceRandom = false;
}

public enum TurbulenceLevel
{
    Low,
    Medium,
    High
}