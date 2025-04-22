using UnityEngine;

/// <summary>
/// ��������� ��������������� ������� ��� ������ ������� �����.
/// ������������� ��� �����-������ � �����������: ����� 1 (�������) �����, ����� 2 (�����) ������.
/// </summary>
public class CameraController : MonoBehaviour
{
    [SerializeField] private Vector3 cameraPosition = new Vector3(7f, 2.8f, 4.5f); // ������� ������
    [SerializeField] private Vector3 cameraRotation = new Vector3(45f, -90f, 0f); // ������� ������
    [SerializeField] private float orthographicSize = 3.8f; // ������ ��������������� ������
    [SerializeField] private float nearClippingPlane = -3f; // ������� ��������� ���������

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
    }

    /// <summary>
    /// ����������� ��������������� ������ � ��������� �����������.
    /// ����� 1 (z=0�4) �����, ����� 2 (z=5�9) ������.
    /// </summary>
    private void SetupCamera()
    {
        // ������������� �������
        transform.position = cameraPosition;

        // ������������� �������
        transform.rotation = Quaternion.Euler(cameraRotation);

        // ����������� ��������� ������
        mainCamera.orthographicSize = orthographicSize;
        mainCamera.nearClipPlane = nearClippingPlane;

        Debug.Log($"CameraController: Camera set at Position: {cameraPosition}, Rotation: {cameraRotation}, OrthographicSize: {orthographicSize}, NearClip: {nearClippingPlane}");
    }
}