using UnityEngine;

/// <summary>
/// ��������� ��������������� ������� ��� ������ ������� �����.
/// ������������ ��� ������ ������ � �����������: ����� 1 (�������) �����, ����� 2 (�����) ������.
/// </summary>
public class CameraController : MonoBehaviour
{
    [SerializeField] private float cameraHeight = 10f; // ������ ������ ��� ������
    [SerializeField] private Vector3 boardCenter = new Vector3(4.5f, 0f, 4.5f); // ����� ����� 10x10
    [SerializeField] private float orthographicSize = 5.5f; // ������ ��������������� ������

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("CameraController: No Camera component found on this GameObject!");
            return;
        }
        mainCamera.orthographic = true; // ������������� ��������������� ������
    }

    private void Start()
    {
        SetupCamera();
        SetupLighting();
    }

    /// <summary>
    /// ����������� ��������������� ������ ��� ���� ������ ������.
    /// ����� 1 (z=0�4) �����, ����� 2 (z=5�9) ������.
    /// </summary>
    private void SetupCamera()
    {
        // ������� ������: ��� ������� �����
        Vector3 cameraPosition = new Vector3(boardCenter.x, cameraHeight, boardCenter.z);
        transform.position = cameraPosition;

        // ������� ������: ������� ���� � ���������, ����� z=0 ��� �����, z=9 ������
        transform.rotation = Quaternion.Euler(90f, -90f, 0f);

        // ����������� ������ ��������������� ������
        mainCamera.orthographicSize = orthographicSize;

        Debug.Log($"CameraController: Camera set at {cameraPosition}, Rotation: {transform.rotation.eulerAngles}, OrthographicSize: {mainCamera.orthographicSize}");
    }

    /// <summary>
    /// ����������� ��������� ��� ������������ ��������� ����� ��� �����.
    /// </summary>
    private void SetupLighting()
    {
        // ������� ��� ������ ������������ ����
        Light directionalLight = FindObjectOfType<Light>();
        if (directionalLight == null || directionalLight.type != LightType.Directional)
        {
            GameObject lightObject = new GameObject("DirectionalLight");
            directionalLight = lightObject.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
        }

        // ����������� ����
        directionalLight.transform.position = new Vector3(boardCenter.x, 10f, boardCenter.z);
        directionalLight.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // ������� ����
        directionalLight.intensity = 1.2f; // �������
        directionalLight.color = Color.white; // ����� ����
        directionalLight.shadows = LightShadows.None; // ��������� ����

        Debug.Log("CameraController: Directional light set up above board center, no shadows.");
    }
}