using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private float trajectoryWidth = 2.0f;

    void Start()
    {
        parameters = SimulationData.Parameters;
        SetupScene();
        CalculateTrajectory();
        StartCoroutine(AnimateProjectile());
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
        float g = 9.80665f;
        float dt = 0.01f;
        float angleRad = Mathf.Deg2Rad * parameters.angleDegrees;
        float caliber = parameters.caliberMm / 1000f;
        float radius = caliber / 2f;
        float area = Mathf.PI * radius * radius;

        float vx = parameters.initialSpeed * Mathf.Cos(angleRad);
        float vy = parameters.initialSpeed * Mathf.Sin(angleRad);

        var points = new System.Collections.Generic.List<Vector3>();
        float x = 0, y = 0;
        float time = 0;

        float minX = 0, maxX = 0, minY = 0, maxY = 0;

        while (y >= 0)
        {
            float v = Mathf.Sqrt(vx * vx + vy * vy);
            float F_drag = 0.5f * parameters.dragCoefficient * 1.225f * area * v * v;
            float a_drag = F_drag / parameters.mass;

            float ax = -a_drag * (vx / v);
            float ay = -g - a_drag * (vy / v);

            vx += ax * dt;
            vy += ay * dt;

            x += vx * dt;
            y += vy * dt;
            time += dt;

            points.Add(new Vector3(x, y, 0));

            if (y > max_height) max_height = y;

            if (x < minX) minX = x;
            if (x > maxX) maxX = x;
            if (y > maxY) maxY = y;
            minY = Mathf.Min(minY, y);
        }

        trajectoryBounds = new Bounds(
        new Vector3((minX + maxX) / 2, (minY + maxY) / 2, 0),
        new Vector3(maxX - minX, maxY - minY, 0)
        );

        total_time = time;
        trajectoryPoints = points.ToArray();
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
        if (resultsPanel == null || resultText == null)
        {
            Debug.LogError("UI элементы не назначены!");
            return;
        }

        resultsPanel.SetActive(true);

        resultText.text = $"Время полета: {total_time:F2} с\n" +
                          $"Дальность стрельбы: {trajectoryPoints[^1].x:F1} м\n" +
                          $"Максимальная высота: {max_height:F1} м";
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}