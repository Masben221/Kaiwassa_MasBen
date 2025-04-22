using UnityEngine;

/// <summary>
/// Управляет направленным светом для равномерного освещения игровой доски.
/// Все параметры света настраиваются через инспектор.
/// </summary>
[RequireComponent(typeof(Light))]
public class LightController : MonoBehaviour
{
    [SerializeField] private Vector3 lightPosition = new Vector3(4.5f, 10f, 4.5f); // Позиция света
    [SerializeField] private Vector3 lightRotation = new Vector3(45f, 0f, 0f); // Ротация света
    [SerializeField] private float lightIntensity = 0.9f; // Интенсивность света
    [SerializeField] private Color lightColor = new Color(1f, 0.95f, 0.9f); // Цвет света (лёгкий тёплый оттенок)

    private Light directionalLight;

    private void Awake()
    {
        directionalLight = GetComponent<Light>();
        if (directionalLight == null || directionalLight.type != LightType.Directional)
        {
            Debug.LogError("LightController: Component must be a Directional Light!");
            return;
        }
    }

    private void Start()
    {
        SetupLighting();
    }

    /// <summary>
    /// Настраивает направленный свет для равномерного освещения доски без теней.
    /// </summary>
    private void SetupLighting()
    {
        // Устанавливаем позицию
        transform.position = lightPosition;

        // Устанавливаем ротацию
        transform.rotation = Quaternion.Euler(lightRotation);

        // Настраиваем параметры света
        directionalLight.intensity = lightIntensity;
        directionalLight.color = lightColor;
        directionalLight.shadows = LightShadows.None; // Отключаем тени

        Debug.Log($"LightController: Directional light set at Position: {lightPosition}, Rotation: {lightRotation}, Intensity: {lightIntensity}, Color: {lightColor}");
    }
}