using UnityEngine;

/// <summary>
/// Управляет ортографической камерой для обзора игровой доски.
/// Обеспечивает вид строго сверху с ориентацией: Игрок 1 (светлые) слева, Игрок 2 (тёмные) справа.
/// </summary>
public class CameraController : MonoBehaviour
{
    [SerializeField] private float cameraHeight = 10f; // Высота камеры над доской
    [SerializeField] private Vector3 boardCenter = new Vector3(4.5f, 0f, 4.5f); // Центр доски 10x10
    [SerializeField] private float orthographicSize = 5.5f; // Размер ортографической камеры

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("CameraController: No Camera component found on this GameObject!");
            return;
        }
        mainCamera.orthographic = true; // Устанавливаем ортографическую камеру
    }

    private void Start()
    {
        SetupCamera();
        SetupLighting();
    }

    /// <summary>
    /// Настраивает ортографическую камеру для вида строго сверху.
    /// Игрок 1 (z=0–4) слева, Игрок 2 (z=5–9) справа.
    /// </summary>
    private void SetupCamera()
    {
        // Позиция камеры: над центром доски
        Vector3 cameraPosition = new Vector3(boardCenter.x, cameraHeight, boardCenter.z);
        transform.position = cameraPosition;

        // Ротация камеры: смотрит вниз с поворотом, чтобы z=0 был слева, z=9 справа
        transform.rotation = Quaternion.Euler(90f, -90f, 0f);

        // Настраиваем размер ортографической камеры
        mainCamera.orthographicSize = orthographicSize;

        Debug.Log($"CameraController: Camera set at {cameraPosition}, Rotation: {transform.rotation.eulerAngles}, OrthographicSize: {mainCamera.orthographicSize}");
    }

    /// <summary>
    /// Настраивает освещение для равномерного освещения доски без теней.
    /// </summary>
    private void SetupLighting()
    {
        // Находим или создаём направленный свет
        Light directionalLight = FindObjectOfType<Light>();
        if (directionalLight == null || directionalLight.type != LightType.Directional)
        {
            GameObject lightObject = new GameObject("DirectionalLight");
            directionalLight = lightObject.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
        }

        // Настраиваем свет
        directionalLight.transform.position = new Vector3(boardCenter.x, 10f, boardCenter.z);
        directionalLight.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Смотрит вниз
        directionalLight.intensity = 1.2f; // Яркость
        directionalLight.color = Color.white; // Белый свет
        directionalLight.shadows = LightShadows.None; // Отключаем тени

        Debug.Log("CameraController: Directional light set up above board center, no shadows.");
    }
}