using UnityEngine;

/// <summary>
/// ��������� ������������ ������ ��� ������������ ��������� ������� �����.
/// ��� ��������� ����� ������������� ����� ���������.
/// </summary>
[RequireComponent(typeof(Light))]
public class LightController : MonoBehaviour
{
    [SerializeField] private Vector3 lightPosition = new Vector3(4.5f, 10f, 4.5f); // ������� �����
    [SerializeField] private Vector3 lightRotation = new Vector3(45f, 0f, 0f); // ������� �����
    [SerializeField] private float lightIntensity = 0.9f; // ������������� �����
    [SerializeField] private Color lightColor = new Color(1f, 0.95f, 0.9f); // ���� ����� (����� ����� �������)

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
    /// ����������� ������������ ���� ��� ������������ ��������� ����� ��� �����.
    /// </summary>
    private void SetupLighting()
    {
        // ������������� �������
        transform.position = lightPosition;

        // ������������� �������
        transform.rotation = Quaternion.Euler(lightRotation);

        // ����������� ��������� �����
        directionalLight.intensity = lightIntensity;
        directionalLight.color = lightColor;
        directionalLight.shadows = LightShadows.None; // ��������� ����

        Debug.Log($"LightController: Directional light set at Position: {lightPosition}, Rotation: {lightRotation}, Intensity: {lightIntensity}, Color: {lightColor}");
    }
}