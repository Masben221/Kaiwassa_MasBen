using UnityEngine;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour
{
    // ������ �� UI-��������, ������� �������� � ����������
    [SerializeField] private GameObject mainMenuPanel; // ������ �������� ����
    [SerializeField] private GameObject settingsPanel; // ������ ��������
    [SerializeField] private GameObject placementPanel; // ������ ����������� �����
    [SerializeField] private Button startGameButton; // ������ "����� ����"
    [SerializeField] private Button selectCharButton; // ������ "����� ���������"
    [SerializeField] private Button settingsButton; // ������ "���������"
    [SerializeField] private UIManualPlacement manualPlacement; // ������ �� ��������� UIManualPlacement

    private void Awake()
    {
        // ���������, ��� ��� UI-�������� ������ � ����������
        if (!mainMenuPanel || !settingsPanel || !placementPanel || !startGameButton ||
            !selectCharButton || !settingsButton || !manualPlacement)
        {
            Debug.LogError("UIMainMenu: One or more UI elements are not assigned in the Inspector!");
            return;
        }

        // ��������� ����������� ������� ��� ������
        startGameButton.onClick.AddListener(OnStartGame);
        selectCharButton.onClick.AddListener(OnSelectCharacter);
        settingsButton.onClick.AddListener(OnSettings);

        // ���������� ���������� ������ ������� ����
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        placementPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        // ������� ����������� ������� ��� ����������� �������
        startGameButton.onClick.RemoveListener(OnStartGame);
        selectCharButton.onClick.RemoveListener(OnSelectCharacter);
        settingsButton.onClick.RemoveListener(OnSettings);
    }

    // ���������� ������� ������ "����� ����"
    private void OnStartGame()
    {
        mainMenuPanel.SetActive(false); // �������� ������� ����
        placementPanel.SetActive(true); // ���������� ������ �����������
        manualPlacement.Initialize(4); // �������������� ����������� � 4 ������ �� ���������
    }

    // ���������� ������� ������ "����� ���������" (��������)
    private void OnSelectCharacter()
    {
        Debug.Log("UIMainMenu: Select Character - Not implemented yet.");
    }

    // ���������� ������� ������ "���������"
    private void OnSettings()
    {
        mainMenuPanel.SetActive(false); // �������� ������� ����
        settingsPanel.SetActive(true); // ���������� ������ ��������
    }
}