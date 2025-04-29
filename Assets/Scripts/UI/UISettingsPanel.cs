using UnityEngine;
using UnityEngine.UI;

public class UISettingsPanel : MonoBehaviour
{
    // ������ �� UI-��������, ������� �������� � ����������
    [SerializeField] private GameObject settingsPanel; // ������ ��������
    [SerializeField] private GameObject mainMenuPanel; // ������ �������� ����
    [SerializeField] private Button backButton; // ������ "�����"

    private void Awake()
    {
        // ���������, ��� ��� UI-�������� ������ � ����������
        if (!settingsPanel || !mainMenuPanel || !backButton)
        {
            Debug.LogError("UISettingsPanel: One or more UI elements are not assigned in the Inspector!");
            return;
        }

        // ��������� ���������� ��� ������ "�����"
        backButton.onClick.AddListener(OnBack);
    }

    private void OnDestroy()
    {
        // ������� ���������� ������� ��� ����������� �������
        backButton.onClick.RemoveListener(OnBack);
    }

    // ���������� ������� ������ "�����"
    private void OnBack()
    {
        settingsPanel.SetActive(false); // �������� ������ ��������
        mainMenuPanel.SetActive(true); // ���������� ������� ����
    }
}