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
    private float minX, maxX, minY, maxY, minZ, maxZ;

    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private float trajectoryWidth = 2.0f;

    // Добавьте эти поля в класс
    private bool isFollowingProjectile = false; // Флаг режима слежения
    private Vector3 initialCameraPosition; // Сохраняем изначальную позицию камеры
    private Quaternion initialCameraRotation; // Сохраняем изначальное вращение камеры
    private CameraController cameraController; // Ссылка на контроллер камеры


    void Start()
    {
        isFollowingProjectile = false;

        parameters = SimulationData.Parameters;
        SetupScene();
        CalculateTrajectory();

        // Создаем объект для центра траектории
        GameObject trajectoryCenter = new GameObject("TrajectoryCenter");
        trajectoryCenter.transform.position = trajectoryBounds.center;

        // Устанавливаем цель для камеры
        CameraController cameraController = Camera.main.GetComponent<CameraController>();
        if (cameraController != null)
        {
            cameraController.SetTarget(trajectoryCenter.transform);
        }

        // Создаем визуальные элементы ПОСЛЕ расчета траектории
        CreateGround();
        CreateVisualizationElements();

        StartCoroutine(AnimateProjectile());

        currentResult = new LaunchResult
        {
            presetName = "Без названия",
            timestamp = DateTime.Now,
            initialSpeed = parameters.initialSpeed,
            angle = parameters.elevationAngle,
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
    }

    void Update()
    {
        // Переключение режимов камеры по правой кнопке мыши
        if (Input.GetMouseButtonDown(1))
        {
            ToggleCameraMode();
        }
    }

    private void ToggleCameraMode()
    {
        if (cameraController == null) return;

        isFollowingProjectile = !isFollowingProjectile;

        if (isFollowingProjectile)
        {
            // Режим слежения: блокируем управление камерой
            cameraController.canControlCamera = false;

            // Перемещаем камеру ближе к снаряду
            if (projectile != null)
            {
                Vector3 targetPosition = new Vector3(
                    projectile.transform.position.x - 500f,
                    Mathf.Max(projectile.transform.position.y + 300f, trajectoryBounds.center.y + trajectoryBounds.size.y * 0.6f),
                    projectile.transform.position.z - 300f
                );

                Camera.main.transform.position = Vector3.Lerp(
                    Camera.main.transform.position,
                    targetPosition,
                    0.5f
                );

                Camera.main.transform.LookAt(projectile.transform.position);
            }
        }
        else
        {
            // Статичный режим: разрешаем управление камерой
            cameraController.canControlCamera = true;

            // Возвращаем камеру в начальную позицию
            Camera.main.transform.position = initialCameraPosition;
            Camera.main.transform.rotation = initialCameraRotation;
        }

        // Выводим информацию о текущем режиме в консоль для отладки
        Debug.Log($"Режим камеры: {(isFollowingProjectile ? "Слежение за снарядом" : "Статичный просмотр всей траектории")}");
    }

    void SetupScene()
    {
        // Создаем начальную точку
        Instantiate(startPointPrefab, Vector3.zero, Quaternion.identity);

        // Рассчитываем позицию полигона с учетом азимутального угла
        float polygonDistance = parameters.startToPolygonDistance * 1000;
        float polygonX = polygonDistance * Mathf.Cos(parameters.azimuthAngle * Mathf.Deg2Rad);
        float polygonZ = polygonDistance * Mathf.Sin(parameters.azimuthAngle * Mathf.Deg2Rad);

        // Создаем полигон в 3D позиции
        Instantiate(polygonPrefab, new Vector3(polygonX, 0, polygonZ),
                    Quaternion.Euler(0, -parameters.azimuthAngle, 0)); // Поворот полигона

        // Аналогично для цели
        float targetDistance = (parameters.startToPolygonDistance + parameters.targetDistanceKm) * 1000;
        float targetX = targetDistance * Mathf.Cos(parameters.azimuthAngle * Mathf.Deg2Rad);
        float targetZ = targetDistance * Mathf.Sin(parameters.azimuthAngle * Mathf.Deg2Rad);

        Instantiate(targetPrefab, new Vector3(targetX, 0, targetZ),
                    Quaternion.Euler(0, -parameters.azimuthAngle, 0));

        // Создаем снаряд
        projectile = Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity);

        // Настраиваем линию траектории
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

    void CreateGround()
    {
        // Создаем сетку земли для лучшего восприятия глубины
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(5000, 1, 5000); // Реалистичный размер для сцены

        // Настраиваем материал для земли
        Material groundMaterial = new Material(Shader.Find("Standard"));
        groundMaterial.color = new Color(0.3f, 0.6f, 0.3f);
        groundMaterial.mainTexture = Resources.Load<Texture2D>("ground_texture");

        ground.GetComponent<MeshRenderer>().material = groundMaterial;

        // Позиционируем землю в центре траектории
        if (trajectoryBounds.center != Vector3.zero)
        {
            ground.transform.position = new Vector3(trajectoryBounds.center.x, -0.1f, trajectoryBounds.center.z);
        }
        else
        {
            ground.transform.position = new Vector3(0, -0.1f, 0);
        }
    }

    void CalculateTrajectory()
    {
        // Получаем параметры
        float initialSpeed = parameters.initialSpeed;
        float elevationRad = parameters.elevationAngle * Mathf.Deg2Rad;
        float azimuthRad = parameters.azimuthAngle * Mathf.Deg2Rad;

        float dragCoeff = parameters.dragCoefficient;
        float mass = parameters.mass;
        float caliber = parameters.caliberMm * 0.001f; // мм → м

        // Погодные параметры
        float windSpeed = parameters.windSpeed;
        float windDirectionRad = parameters.windDirection * Mathf.Deg2Rad;
        float temperature = parameters.temperature;
        float altitude = parameters.altitude;

        // Рассчитываем плотность воздуха
        float airDensity = CalculateAirDensity(temperature, altitude);

        // Компоненты скорости ветра в 3D
        float windSpeedX = windSpeed * Mathf.Cos(windDirectionRad);
        float windSpeedZ = windSpeed * Mathf.Sin(windDirectionRad);
        float windSpeedY = 0f; // Предполагаем, что ветер горизонтальный

        // Физические константы
        const float gravity = 9.81f;
        const float timeStep = 0.01f;

        // Начальные условия (3D компоненты скорости)
        float initialSpeedXY = initialSpeed * Mathf.Cos(elevationRad); // Горизонтальная скорость
        float vx = initialSpeedXY * Mathf.Cos(azimuthRad);
        float vz = initialSpeedXY * Mathf.Sin(azimuthRad);
        float vy = initialSpeed * Mathf.Sin(elevationRad);

        // Площадь поперечного сечения
        float crossSectionalArea = Mathf.PI * (caliber * 0.5f) * (caliber * 0.5f);

        // Инициализация границ траектории для 3D
        minX = float.MaxValue; maxX = float.MinValue;
        minY = float.MaxValue; maxY = float.MinValue;
        minZ = float.MaxValue; maxZ = float.MinValue; // Новое для Z

        // Список точек траектории
        var points = new System.Collections.Generic.List<Vector3>();

        float x = 0, y = 0, z = 0; // Теперь 3 координаты
        float totalTime = 0;
        float maxHeight = 0;

        while (y >= 0 && totalTime < 200)
        {
            // Относительная скорость снаряда относительно воздуха в 3D
            float relVx = vx - windSpeedX;
            float relVy = vy - windSpeedY;
            float relVz = vz - windSpeedZ;

            float relSpeed = Mathf.Sqrt(relVx * relVx + relVy * relVy + relVz * relVz);

            float dragAccel = 0f;
            if (relSpeed > 0.001f)
            {
                // Базовая сила сопротивления
                float dragForce = 0.5f * dragCoeff * airDensity * crossSectionalArea * relSpeed * relSpeed;

                // Турбулентность
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

                // Ускорение от сопротивления
                dragAccel = dragForce / mass;
            }

            // Обновляем компоненты скорости с учетом 3D
            if (relSpeed > 0.001f)
            {
                float relDirX = relVx / relSpeed;
                float relDirY = relVy / relSpeed;
                float relDirZ = relVz / relSpeed;

                vx -= dragAccel * relDirX * timeStep;
                vy -= (gravity + dragAccel * relDirY) * timeStep;
                vz -= dragAccel * relDirZ * timeStep;
            }
            else
            {
                vy -= gravity * timeStep;
            }

            // Обновляем позицию в 3D
            x += vx * timeStep;
            y += vy * timeStep;
            z += vz * timeStep;

            totalTime += timeStep;

            // Отслеживаем максимальную высоту
            if (y > maxHeight) maxHeight = y;

            // Добавляем точку траектории
            points.Add(new Vector3(x, y, z));

            // Обновляем границы для камеры
            if (x < minX) minX = x;
            if (x > maxX) maxX = x;
            if (y < minY) minY = y;
            if (y > maxY) maxY = y;
            if (z < minZ) minZ = z; // Новое для Z
            if (z > maxZ) maxZ = z; // Новое для Z
        }

        // Сохраняем результаты
        trajectoryPoints = points.ToArray();
        total_time = totalTime;
        max_height = maxHeight;

        // Рассчитываем границы для камеры в 3D
        CalculateTrajectoryBounds3D();
    }

/*    void CalculateTrajectoryBounds()
    {
        // Центр траектории
        Vector3 center = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, 0);
        // Размеры области
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 0);

        trajectoryBounds = new Bounds(center, size);
    }*/

    void CalculateTrajectoryBounds3D()
    {
        // Добавляем буфер для лучшего обзора (10% от размеров)
        float buffer = 0.1f;

        // Центр траектории
        Vector3 center = new Vector3(
            (minX + maxX) / 2,
            (minY + maxY) / 2,
            (minZ + maxZ) / 2
        );

        // Размеры области с учетом буфера
        Vector3 size = new Vector3(
            (maxX - minX) * (1 + buffer),
            (maxY - minY) * (1 + buffer),
            (maxZ - minZ) * (1 + buffer)
        );

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

        // Сохраняем изначальную позицию камеры
        initialCameraPosition = Camera.main.transform.position;
        initialCameraRotation = Camera.main.transform.rotation;

        // Получаем ссылку на контроллер камеры
        cameraController = Camera.main.GetComponent<CameraController>();

        SetupCamera3D();
        yield return null;

        for (int i = 0; i < trajectoryPoints.Length; i++)
        {
            // Устанавливаем позицию в 3D
            projectile.transform.position = trajectoryPoints[i];

            // Поворачиваем снаряд по направлению движения
            if (i < trajectoryPoints.Length - 1)
            {
                Vector3 direction = trajectoryPoints[i + 1] - trajectoryPoints[i];
                if (direction.magnitude > 0.001f)
                {
                    // Поворот снаряда в направлении движения
                    projectile.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                }
            }

            // Следуем за снарядом ТОЛЬКО в режиме слежения
            if (isFollowingProjectile && i % 5 == 0)
            {
                Vector3 targetPosition = new Vector3(
                    projectile.transform.position.x - 500f,
                    Mathf.Max(projectile.transform.position.y + 300f, trajectoryBounds.center.y + trajectoryBounds.size.y * 0.6f),
                    projectile.transform.position.z - 300f
                );

                Camera.main.transform.position = Vector3.Lerp(
                    Camera.main.transform.position,
                    targetPosition,
                    0.1f
                );

                Camera.main.transform.LookAt(projectile.transform.position);
            }

            trajectoryLine.positionCount = i + 1;
            trajectoryLine.SetPosition(i, projectile.transform.position);

            if (projectile.transform.position.y <= 0 && i > 0)
                break;

            yield return new WaitForSeconds(0.01f);
        }

        // После завершения полета возвращаем камеру в начальную позицию
        /*yield return new WaitForSeconds(1f);
        if (!isFollowingProjectile)
        {
            Camera.main.transform.position = initialCameraPosition;
            Camera.main.transform.rotation = initialCameraRotation;
        }*/

        yield return new WaitForSeconds(1f);
        ShowResults();
    }

    void SetupCamera3D()
    {
        // Проверяем, существует ли CameraController
        CameraController cameraController = Camera.main.GetComponent<CameraController>();
        if (cameraController == null)
        {
            Debug.LogError("CameraController не прикреплен к основной камере!");
            return;
        }

        // Определяем оптимальное расстояние камеры от центра сцены
        float boundsSize = Mathf.Max(trajectoryBounds.size.x, trajectoryBounds.size.y, trajectoryBounds.size.z);

        // Устанавливаем расстояние камеры с учетом размеров траектории
        float cameraDistance = boundsSize * 0.8f;
        cameraDistance = Mathf.Max(cameraDistance, 100f);

        // Оптимальная высота камеры над траекторией
        float cameraHeight = boundsSize * 0.6f;

        // Позиционируем камеру для лучшего обзора всей траектории
        Vector3 cameraPosition = new Vector3(
            trajectoryBounds.center.x - cameraDistance * 0.7f,
            trajectoryBounds.center.y + cameraHeight,
            trajectoryBounds.center.z - cameraDistance * 0.5f
        );

        Camera.main.transform.position = cameraPosition;

        // Направляем камеру на центр траектории
        Camera.main.transform.LookAt(trajectoryBounds.center);

        // Устанавливаем FOV для комфортного просмотра
        Camera.main.fieldOfView = 45f;

        // Настраиваем near/far clip plane для работы с дистанциями
        Camera.main.nearClipPlane = 0.3f;
        Camera.main.farClipPlane = Mathf.Max(2000f, cameraDistance * 2.5f);

        // Сохраняем начальную позицию для сброса
        cameraController.SetInitialPosition(cameraPosition, Camera.main.transform.rotation);
    }

    void SetupFlatTrajectoryCamera()
    {
        // Для плоских траекторий (соотношение высоты к длине < 0.15)
        float boundsSize = Mathf.Max(trajectoryBounds.size.x, trajectoryBounds.size.z);
        float cameraDistance = boundsSize * 0.9f;
        cameraDistance = Mathf.Max(cameraDistance, 150f);

        float cameraHeight = trajectoryBounds.size.y * 0.8f + 50f;

        Vector3 cameraPosition = new Vector3(
            trajectoryBounds.center.x,
            trajectoryBounds.center.y + cameraHeight,
            trajectoryBounds.center.z - cameraDistance
        );

        Camera.main.transform.position = cameraPosition;
        Camera.main.transform.LookAt(new Vector3(
            trajectoryBounds.center.x,
            trajectoryBounds.center.y,
            trajectoryBounds.center.z
        ));

        Camera.main.fieldOfView = 40f;
    }

    void SetupMediumTrajectoryCamera()
    {
        // Для средних траекторий (соотношение 0.15-0.4)
        float boundsSize = Mathf.Max(trajectoryBounds.size.x, trajectoryBounds.size.y, trajectoryBounds.size.z);
        float cameraDistance = boundsSize * 0.7f;
        cameraDistance = Mathf.Max(cameraDistance, 120f);

        Vector3 cameraPosition = new Vector3(
            trajectoryBounds.center.x - cameraDistance * 0.6f,
            trajectoryBounds.center.y + boundsSize * 0.5f,
            trajectoryBounds.center.z - cameraDistance * 0.4f
        );

        Camera.main.transform.position = cameraPosition;
        Camera.main.transform.LookAt(trajectoryBounds.center);

        Camera.main.fieldOfView = 45f;
    }

    void SetupHighTrajectoryCamera()
    {
        // Для высоких траекторий (соотношение > 0.4)
        float boundsSize = Mathf.Max(trajectoryBounds.size.x, trajectoryBounds.size.y, trajectoryBounds.size.z);
        float cameraDistance = boundsSize * 0.6f;
        cameraDistance = Mathf.Max(cameraDistance, 100f);

        Vector3 cameraPosition = new Vector3(
            trajectoryBounds.center.x - cameraDistance * 0.4f,
            trajectoryBounds.center.y + boundsSize * 0.7f,
            trajectoryBounds.center.z - cameraDistance * 0.3f
        );

        Camera.main.transform.position = cameraPosition;
        Camera.main.transform.LookAt(trajectoryBounds.center);

        Camera.main.fieldOfView = 50f;
    }

    /*void SetupCamera()
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
    }*/

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

    void CreateVisualizationElements()
    {
        // Создаем координатные оси для ориентации
        CreateCoordinateAxes();

        // Создаем маркеры расстояния
        CreateDistanceMarkers();
    }
    private enum TrajectoryType { Flat, Medium, High }
    private TrajectoryType GetTrajectoryType()
    {
        float flatRatio = trajectoryBounds.size.y / Mathf.Max(trajectoryBounds.size.x, trajectoryBounds.size.z);

        if (flatRatio < 0.15f) return TrajectoryType.Flat;
        if (flatRatio < 0.4f) return TrajectoryType.Medium;
        return TrajectoryType.High;
    }

    void CreateCoordinateAxes()
    {
        GameObject axes = new GameObject("CoordinateAxes");

        // Ось X (красная)
        GameObject xAxis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        xAxis.transform.parent = axes.transform;
        xAxis.transform.localScale = new Vector3(100, 0.5f, 0.5f);
        xAxis.transform.localPosition = new Vector3(50, 0, 0);
        xAxis.GetComponent<Renderer>().material.color = Color.red;

        // Ось Y (зеленая)
        GameObject yAxis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        yAxis.transform.parent = axes.transform;
        yAxis.transform.localScale = new Vector3(0.5f, 100, 0.5f);
        yAxis.transform.localPosition = new Vector3(0, 50, 0);
        yAxis.GetComponent<Renderer>().material.color = Color.green;

        // Ось Z (синяя)
        GameObject zAxis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        zAxis.transform.parent = axes.transform;
        zAxis.transform.localScale = new Vector3(0.5f, 0.5f, 100);
        zAxis.transform.localPosition = new Vector3(0, 0, 50);
        zAxis.GetComponent<Renderer>().material.color = Color.blue;
    }

    void CreateDistanceMarkers()
    {
        float maxDistance = trajectoryBounds.size.x;
        int markerCount = Mathf.CeilToInt(maxDistance / 1000f);

        for (int i = 1; i <= markerCount; i++)
        {
            float position = i * 1000f;

            GameObject marker = new GameObject($"DistanceMarker_{i}km");
            marker.transform.position = new Vector3(position, 0, 0);

            // Создаем визуальный маркер
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.parent = marker.transform;
            cube.transform.localScale = new Vector3(10, 10, 10);
            cube.GetComponent<Renderer>().material.color = Color.yellow;

            // Добавляем текст
            GameObject textObject = new GameObject("Text");
            textObject.transform.parent = marker.transform;
            textObject.transform.localPosition = new Vector3(0, 15, 0);

            TextMeshPro text = textObject.AddComponent<TextMeshPro>();
            text.text = $"{i} km";
            text.fontSize = 12;
            text.color = Color.white;
        }
    }
}