using UnityEngine;
using UnityEngine.UI;
using Zenject;

/// <summary>
/// ��������� UI �������� ���� ��� ������ ������ ����������� � ��������� ����� ���.
/// </summary>
public class UIMainMenu : MonoBehaviour
{
    [Inject] private IGameManager gameManager; // ��������� ��� ���������� �����

    // UI-�������� �������� ����
    [SerializeField] private GameObject mainMenuPanel; // ������ ������ ������
    [SerializeField] private Button randomPlacementButton; // ������ ��������� �����������
    [SerializeField] private Button manualPlacementButton; // ������ ������ �����������

    // UI-�������� ��������
    [SerializeField] private GameObject settingsPanel; // ������ ��������
    [SerializeField] private Slider mountainsSlider; // ������� ��� ����� ���
    [SerializeField] private Text mountainsValueText; // ����� �������� ��������
    [SerializeField] private Button backButton; // ������ "�����"
    [SerializeField] private Button startGameButton; // ������ "������ ����"

    private int selectedMountains = 4; // ����� ��� �� ���������
    private bool isRandomPlacement; // ������ �� ��������� �����
    private bool arePiecesPlaced; // ����������� �� ������

    private void Awake()
    {
        // ���������, ��� ��� UI-�������� ���������
        if (!mainMenuPanel || !randomPlacementButton || !manualPlacementButton ||
            !settingsPanel || !mountainsSlider || !mountainsValueText || !backButton || !startGameButton)
        {
            Debug.LogError("UIMainMenu: One or more UI elements are not assigned in the Inspector!");
            return;
        }

        // ����������� ��������� ���������
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        startGameButton.interactable = false; // ������ ���������
        arePiecesPlaced = false;

        // ����������� ����������� ������
        randomPlacementButton.onClick.AddListener(OnRandomPlacementSelected);
        manualPlacementButton.onClick.AddListener(OnManualPlacementSelected);
        backButton.onClick.AddListener(OnBackClicked);
        startGameButton.onClick.AddListener(OnStartGameClicked);

        // ����������� �������
        mountainsSlider.minValue = 0;
        mountainsSlider.maxValue = 8;
        mountainsSlider.wholeNumbers = true;
        mountainsSlider.value = selectedMountains;
        mountainsValueText.text = selectedMountains.ToString();
        mountainsSlider.onValueChanged.AddListener(OnMountainsSliderChanged);
    }

    private void OnDestroy()
    {
        // ������� �����������
        randomPlacementButton.onClick.RemoveListener(OnRandomPlacementSelected);
        manualPlacementButton.onClick.RemoveListener(OnManualPlacementSelected);
        backButton.onClick.RemoveListener(OnBackClicked);
        startGameButton.onClick.RemoveListener(OnStartGameClicked);
        mountainsSlider.onValueChanged.RemoveListener(OnMountainsSliderChanged);
    }

    /// <summary>
    /// ����� ��������� �����������.
    /// </summary>
    private void OnRandomPlacementSelected()
    {
        isRandomPlacement = true;
        arePiecesPlaced = true; // ��� ��������� ����������� �������, ��� ������ ����� ����������� GameManager
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        startGameButton.interactable = true; // ���������� ������, ��� ��� ����� ������
        Debug.Log("UIMainMenu: Random placement selected.");
    }

    /// <summary>
    /// ����� ������ ����������� (��������).
    /// </summary>
    private void OnManualPlacementSelected()
    {
        Debug.Log("������ ����������� �� �����������");
        // ���� �� ��������� � ����������, ��� ��� ������ ����������� �� �����������
        // ����� �����������������, ���� ����� ������� ������ �����������
        // isRandomPlacement = false;
        // arePiecesPlaced = false; // ������ ��� �� �����������
        // mainMenuPanel.SetActive(false);
        // settingsPanel.SetActive(true);
    }

    /// <summary>
    /// ���������� ����� ��� ����� �������.
    /// </summary>
    private void OnMountainsSliderChanged(float value)
    {
        selectedMountains = Mathf.FloorToInt(value);
        mountainsValueText.text = selectedMountains.ToString();
        Debug.Log($"UIMainMenu: Mountains per side set to {selectedMountains}");
    }

    /// <summary>
    /// ������� � ������ ������.
    /// </summary>
    private void OnBackClicked()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        startGameButton.interactable = false; // ������������ ������
        arePiecesPlaced = false;
        Debug.Log("UIMainMenu: Back to main menu.");
    }

    /// <summary>
    /// ������ ����.
    /// </summary>
    private void OnStartGameClicked()
    {
        if (!arePiecesPlaced)
        {
            Debug.LogWarning("UIMainMenu: Cannot start game, pieces are not placed!");
            return;
        }

        // ��������� ���� � ��������� ������ ���
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        gameManager.StartGame(selectedMountains); // ������� ����� ���
        Debug.Log($"UIMainMenu: Game started with {selectedMountains} mountains per side.");
    }

    [Inject]
    public void Setup(IGameManager gameManager)
    {
        this.gameManager = gameManager;
    }
}