using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public float zoomSensitivity = 5f;
    public float minDistance = 50f;
    public float maxDistance = 5000f;
    public bool canControlCamera = true; // Флаг, можно ли управлять камерой

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float currentDistance;
    private Transform target; // Цель, вокруг которой вращается камера

    void Start()
    {
        // Автоматически находим цель в центре сцены, если не указана
        if (target == null)
        {
            GameObject targetObject = GameObject.Find("TrajectoryCenter");
            if (targetObject == null)
            {
                targetObject = new GameObject("TrajectoryCenter");
                targetObject.transform.position = Vector3.zero;
            }
            target = targetObject.transform;
        }

        // Сохраняем начальную позицию и вращение камеры
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        // Рассчитываем начальное расстояние до цели
        currentDistance = Vector3.Distance(transform.position, target.position);
    }

    void Update()
    {
        HandleCameraControls();
    }

    void HandleCameraControls()
    {
        // Если камера заблокирована для управления, выходим
        if (!canControlCamera) return;

        // Сброс позиции камеры на ПКМ только если мы НЕ в режиме слежения
        if (Input.GetMouseButtonDown(1) && target != null)
        {
            ResetCameraPosition();
        }

        // Вращение камеры при удержании ЛКМ
        if (Input.GetMouseButton(0) && target != null)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // Вращаем камеру вокруг цели
            transform.RotateAround(target.position, Vector3.up, mouseX);
            transform.RotateAround(target.position, transform.right, -mouseY);
        }

        // Масштабирование колесом мыши
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0 && target != null)
        {
            currentDistance -= scroll * zoomSensitivity * currentDistance;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);

            // Перемещаем камеру вдоль ее направления взгляда
            Vector3 direction = (transform.position - target.position).normalized;
            transform.position = target.position + direction * currentDistance;
        }
    }

    public void ResetCameraPosition()
    {
        if (target != null)
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            currentDistance = Vector3.Distance(initialPosition, target.position);
        }
    }

    public void SetInitialPosition(Vector3 position, Quaternion rotation)
    {
        initialPosition = position;
        initialRotation = rotation;
        if (target != null)
        {
            currentDistance = Vector3.Distance(position, target.position);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null && Application.isPlaying)
        {
            currentDistance = Vector3.Distance(transform.position, target.position);
        }
    }
}