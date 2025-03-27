using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public TMP_InputField speedInput;
    public TMP_InputField angleInput;
    public TMP_InputField cdInput;
    public TMP_InputField massInput;
    public TMP_InputField caliberInput;
    public TMP_InputField distanceInput;
    public GameObject projectile;
    public GameObject target;
    public TrailRenderer trail;

    private List<Vector3> trajectory = new List<Vector3>();

    public void StartSimulation()
    {
        float v0 = float.Parse(speedInput.text);
        float angle = float.Parse(angleInput.text);
        float cD = float.Parse(cdInput.text);
        float mass = float.Parse(massInput.text);
        float caliberMm = float.Parse(caliberInput.text);
        float distanceKm = float.Parse(distanceInput.text);

        // Расчет траектории
        trajectory = CalculateTrajectory(v0, angle, cD, mass, caliberMm);

        // Обновление позиции цели
        target.transform.position = new Vector3(distanceKm * 1000, 25, 0);

        // Запуск анимации
        StartCoroutine(AnimateProjectile());
    }

    private List<Vector3> CalculateTrajectory(float v0, float angleDeg, float cD, float mass, float caliberMm)
    {
        float g = 9.80665f;
        float airDensity = 1.225f;
        float dt = 0.01f;
        float angle = Mathf.Deg2Rad * angleDeg;
        float caliber = caliberMm / 1000f;
        float radius = caliber / 2f;
        float area = Mathf.PI * radius * radius;

        List<Vector3> path = new List<Vector3>();
        Vector3 position = Vector3.zero;
        Vector3 velocity = new Vector3(v0 * Mathf.Cos(angle), v0 * Mathf.Sin(angle), 0);

        while (position.y >= 0)
        {
            float v = velocity.magnitude;
            float fDrag = 0.5f * cD * airDensity * area * v * v;
            Vector3 aDrag = (fDrag / mass) * (-velocity.normalized);
            Vector3 acceleration = new Vector3(aDrag.x, -g + aDrag.y, 0);

            velocity += acceleration * dt;
            position += velocity * dt;

            path.Add(position);
        }

        return path;
    }

    private IEnumerator AnimateProjectile()
    {
        projectile.SetActive(true);
        trail.Clear();

        foreach (var point in trajectory)
        {
            projectile.transform.position = point;
            yield return new WaitForSeconds(0.01f);
        }
    }
}