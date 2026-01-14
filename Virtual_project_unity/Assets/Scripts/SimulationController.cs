using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SimulationController : MonoBehaviour
{
    public GameObject projectilePrefab;
    public GameObject startPointPrefab;
    public GameObject polygonPrefab;
    public GameObject targetPrefab;

    private GameObject projectile;
    private LineRenderer trajectoryLine;
    private SimulationParameters parameters;
    private Vector3[] trajectoryPoints;
    private float max_height;
    private float total_time;
    private Bounds trajectoryBounds;
    private LaunchResult currentResult;
    private float minX, maxX, minY, maxY;

    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private float trajectoryWidth = 2.0f;


    void Start()
    {
        parameters = SimulationData.Parameters;
        SetupScene();
        CalculateTrajectory();
        StartCoroutine(AnimateProjectile());

        currentResult = new LaunchResult
        {
            presetName = "Без названия",
            timestamp = DateTime.Now, // Это автоматически установит timestampTicks
            initialSpeed = parameters.initialSpeed,
            angle = parameters.angleDegrees,
            drag = parameters.dragCoefficient,
            mass = parameters.mass,
            caliber = parameters.caliberMm,
            flightTime = total_time,
            maxDistance = trajectoryPoints[^1].x,
            maxHeight = max_height,
            windSpeed = parameters.windSpeed,
            windDirection = parameters.windDirection,
            temperature = parameters.temperature,
            altitude = parameters.altitude,
            turbulenceLevel = parameters.turbulenceLevel.ToString()
        };

        //ShowResults();
    }

    void SetupScene()
    {
        Instantiate(startPointPrefab, Vector3.zero, Quaternion.identity);
        Instantiate(polygonPrefab,
            new Vector3(parameters.startToPolygonDistance * 1000, 0, 0),
            Quaternion.identity);
        Instantiate(targetPrefab,
            new Vector3((parameters.startToPolygonDistance + parameters.targetDistanceKm) * 1000, 0, 0),
            Quaternion.identity);

        projectile = Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity);

        if (projectile.GetComponent<TrailRenderer>() != null)
            Destroy(projectile.GetComponent<TrailRenderer>());

        trajectoryLine = projectile.AddComponent<LineRenderer>();
        trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
        trajectoryLine.startWidth = trajectoryWidth;
        trajectoryLine.endWidth = trajectoryWidth;
        trajectoryLine.startColor = Color.yellow;
        trajectoryLine.endColor = Color.red;
        trajectoryLine.positionCount = 0;
    }

    void CalculateTrajectory()
    {
        // Получаем параметры
        float initialSpeed = parameters.initialSpeed;
        float angleRad = parameters.angleDegrees * Mathf.Deg2Rad;
        float dragCoeff = parameters.dragCoefficient;
        float mass = parameters.mass;
        float caliber = parameters.caliberMm * 0.001f; // мм → м

        // Погодные параметры
        float windSpeed = parameters.windSpeed;
        float windDirectionRad = parameters.windDirection * Mathf.Deg2Rad;
        float temperature = parameters.temperature;
        float altitude = parameters.altitude;

        // Рассчитываем плотность воздуха с учётом температуры и высоты
        float airDensity = CalculateAirDensity(temperature, altitude);

        // Компоненты скорости ветра
        float windSpeedX = windSpeed * Mathf.Cos(windDirectionRad);
        float windSpeedY = windSpeed * Mathf.Sin(windDirectionRad);

        // Физические константы
        const float gravity = 9.81f; // м/с²
        const float timeStep = 0.01f; // с

        // Начальные условия
        float vx = initialSpeed * Mathf.Cos(angleRad);
        float vy = initialSpeed * Mathf.Sin(angleRad);

        // Площадь поперечного сечения снаряда
        float crossSectionalArea = Mathf.PI * (caliber * 0.5f) * (caliber * 0.5f);

        // Инициализация границ траектории
        minX = float.MaxValue;
        maxX = float.MinValue;
        minY = float.MaxValue;
        maxY = float.MinValue;

        // Список точек траектории
        var points = new System.Collections.Generic.List<Vector3>();

        float x = 0, y = 0;
        float totalTime = 0;
        float maxHeight = 0;

        while (y >= 0 && totalTime < 200) // Ограничение по времени для безопасности
        {
            // Относительная скорость снаряда относительно воздуха
            float relVx = vx - windSpeedX;
            float relVy = vy - windSpeedY;
            float relSpeed = Mathf.Sqrt(relVx * relVx + relVy * relVy);

            // Ускорение от сопротивления воздуха
            float dragAccel = 0f;
            if (relSpeed > 0.001f)
            {
                // Базовая сила сопротивления: F = 0.5 * Cd * ρ * A * v²
                float dragForce = 0.5f * dragCoeff * airDensity * crossSectionalArea * relSpeed * relSpeed;

                // Применяем турбулентность как случайный множитель
                float turbulenceMultiplier = 1f;
                switch (parameters.turbulenceLevel)
                {
                    case TurbulenceLevel.Low:
                        turbulenceMultiplier += UnityEngine.Random.Range(0f, 0.1f);
                        break;
                    case TurbulenceLevel.Medium:
                        turbulenceMultiplier += UnityEngine.Random.Range(0f, 0.3f);
                        break;
                    case TurbulenceLevel.High:
                        turbulenceMultiplier += UnityEngine.Random.Range(0f, 0.6f);
                        break;
                }
                dragForce *= turbulenceMultiplier;

                // Ускорение от сопротивления: a = F / m
                dragAccel = dragForce / mass;
            }

            // Обновляем компоненты скорости
            if (relSpeed > 0.001f)
            {
                float relDirX = relVx / relSpeed;
                float relDirY = relVy / relSpeed;
                vx -= dragAccel * relDirX * timeStep;
                vy -= (gravity + dragAccel * relDirY) * timeStep;
            }
            else
            {
                // Если относительная скорость почти нулевая — только гравитация
                vy -= gravity * timeStep;
            }

            // Обновляем позицию
            x += vx * timeStep;
            y += vy * timeStep;
            totalTime += timeStep;

            // Отслеживаем максимальную высоту
            if (y > maxHeight) maxHeight = y;

            // Добавляем точку траектории
            points.Add(new Vector3(x, y, 0));

            // Обновляем границы для камеры
            if (x < minX) minX = x;
            if (x > maxX) maxX = x;
            if (y < minY) minY = y;
            if (y > maxY) maxY = y;
        }

        // Сохраняем результаты
        trajectoryPoints = points.ToArray();
        total_time = totalTime;
        max_height = maxHeight;

        // Рассчитываем границы для камеры
        CalculateTrajectoryBounds();
    }

    void CalculateTrajectoryBounds()
    {
        // Центр траектории
        Vector3 center = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, 0);
        // Размеры области
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 0);

        trajectoryBounds = new Bounds(center, size);
    }

    // Метод для расчета плотности воздуха
    private float CalculateAirDensity(float temperature, float altitude)
    {
        // Стандартная плотность воздуха на уровне моря при 15°C
        const float seaLevelDensity = 1.225f; // кг/м³

        // Поправка на температуру (упрощенная формула)
        float temperatureFactor = (273.15f + 15f) / (273.15f + temperature);

        // Поправка на высоту (экспоненциальная модель)
        float altitudeFactor = Mathf.Exp(-altitude / 8500f); // 8500м - характерная высота

        return seaLevelDensity * temperatureFactor * altitudeFactor;
    }

    // Метод для расчета силы сопротивления воздуха с учетом погоды
    private float CalculateAirResistance(float vx, float vy, float dragCoeff, float airDensity,
                                         float crossSectionalArea, float windSpeedX, float windSpeedY,
                                         TurbulenceLevel turbulenceLevel)
    {
        // Относительная скорость относительно воздуха
        float relativeVx = vx - windSpeedX;
        float relativeVy = vy - windSpeedY;
        float relativeSpeed = Mathf.Sqrt(relativeVx * relativeVx + relativeVy * relativeVy);

        // Базовое сопротивление
        float baseResistance = 0.5f * dragCoeff * airDensity * crossSectionalArea * relativeSpeed * relativeSpeed;

        // Добавляем турбулентность
        float turbulenceFactor = 1f;
        switch (turbulenceLevel)
        {
            case TurbulenceLevel.Low:
                turbulenceFactor = 1f + UnityEngine.Random.Range(0f, 0.1f);
                break;
            case TurbulenceLevel.Medium:
                turbulenceFactor = 1f + UnityEngine.Random.Range(0f, 0.3f);
                break;
            case TurbulenceLevel.High:
                turbulenceFactor = 1f + UnityEngine.Random.Range(0f, 0.6f);
                break;
        }

        return baseResistance * turbulenceFactor;
    }

    IEnumerator AnimateProjectile()
    {
        if (trajectoryPoints == null || trajectoryPoints.Length == 0)
        {
            Debug.LogError("Траектория не рассчитана");
            yield break;
        }

        trajectoryLine.positionCount = trajectoryPoints.Length;

        SetupCamera();
        yield return null;

        for (int i = 0; i < trajectoryPoints.Length; i++)
        {
            projectile.transform.position = trajectoryPoints[i];

            trajectoryLine.positionCount = i + 1;
            trajectoryLine.SetPosition(i, projectile.transform.position);

            if (projectile.transform.position.y <= 0 && i > 0)
                break;

            yield return new WaitForSeconds(0.01f);
        }

        ShowResults();
    }

    void SetupCamera()
    {
        Camera.main.orthographic = true;

        float screenRatio = (float)Screen.width / Screen.height;
        float boundsWidth = trajectoryBounds.size.x;
        float boundsHeight = trajectoryBounds.size.y;

        if (screenRatio > boundsWidth / boundsHeight)
        {
            Camera.main.orthographicSize = boundsHeight * 0.55f;
        }
        else
        {
            float horizontalSize = boundsWidth / screenRatio * 0.55f;
            Camera.main.orthographicSize = horizontalSize;
        }

        Camera.main.transform.position = new Vector3(
            trajectoryBounds.center.x,
            trajectoryBounds.center.y,
            Camera.main.transform.position.z
        );
    }

    void ShowResults()
    {
        if (resultsPanel == null || resultText == null ||
        saveButton == null || exitButton == null)
        {
            Debug.LogError("UI элементы не назначены!");
            return;
        }

        resultsPanel.SetActive(true);

        string weatherInfo = $"Погодные условия:\n" +
                        $"Скорость ветра: {parameters.windSpeed:F1} м/с\n" +
                        $"Направление ветра: {parameters.windDirection:F0}°\n" +
                        $"Температура: {parameters.temperature:F1}°C\n" +
                        $"Высота над уровнем моря: {parameters.altitude:F0} м\n" +
                        $"Турбулентность: {parameters.turbulenceLevel}";

        resultText.text = $"Время полета: {total_time:F2} с\n" +
                          $"Дальность стрельбы: {trajectoryPoints[^1].x:F1} м\n" +
                          $"Максимальная высота: {max_height:F1} м";

        // Добавьте проверки на null
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveAndExit);
        if (exitButton != null)
            exitButton.onClick.AddListener(ReturnToMenu);
    }

    public void SaveAndExit()
    {
        if (ResultManager.Instance == null)
        {
            Debug.LogError("ResultManager не инициализирован!");
            return;
        }

        ResultManager.Instance.SaveResult(currentResult);
        SceneManager.LoadScene("MainMenu");
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}