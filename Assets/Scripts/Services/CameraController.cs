using UnityEngine;

/// <summary>
/// Управляет ортографической камерой для обзора игровой доски.
/// Устанавливает вид сбоку-сверху с ориентацией: Игрок 1 (светлые) слева, Игрок 2 (тёмные) справа.
/// </summary>
public class CameraController : MonoBehaviour
{
    [SerializeField] private Vector3 cameraPosition = new Vector3(7f, 2.8f, 4.5f); // Позиция камеры
    [SerializeField] private Vector3 cameraRotation = new Vector3(45f, -90f, 0f); // Ротация камеры
    [SerializeField] private float orthographicSize = 3.8f; // Размер ортографической камеры
    [SerializeField] private float nearClippingPlane = -3f; // Ближняя плоскость отсечения

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
    }

    /// <summary>
    /// Настраивает ортографическую камеру с заданными параметрами.
    /// Игрок 1 (z=0–4) слева, Игрок 2 (z=5–9) справа.
    /// </summary>
    private void SetupCamera()
    {
        // Устанавливаем позицию
        transform.position = cameraPosition;

        // Устанавливаем ротацию
        transform.rotation = Quaternion.Euler(cameraRotation);

        // Настраиваем параметры камеры
        mainCamera.orthographicSize = orthographicSize;
        mainCamera.nearClipPlane = nearClippingPlane;

        Debug.Log($"CameraController: Camera set at Position: {cameraPosition}, Rotation: {cameraRotation}, OrthographicSize: {orthographicSize}, NearClip: {nearClippingPlane}");
    }
}