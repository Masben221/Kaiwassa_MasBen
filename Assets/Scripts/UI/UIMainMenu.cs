using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIMainMenu : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button randomPlacementButton;
    [SerializeField] private Button manualPlacementButton;
    [SerializeField] private Slider mountainsSlider;
    [SerializeField] private Text mountainsValueText; // ����� ��� ����������� ���������� ���
    [SerializeField] private UIManualPlacement manualPlacement;

    [Inject] private IGameManager gameManager;
    [Inject] private IBoardManager boardManager;

    private bool isRandomPlacement = true;
    private int selectedMountains = 4;

    private void Awake()
    {
        // ��������� ������� ���� ����������� UI-���������
        if (!mainMenuPanel || !settingsPanel || !startGameButton || !randomPlacementButton ||
            !manualPlacementButton || !mountainsSlider || !mountainsValueText || !manualPlacement)
        {
            Debug.LogError("UIMainMenu: One or more UI elements are not assigned in the Inspector!");
            return;
        }

        // ����������� ��������� ������ � ��������
        startGameButton.onClick.AddListener(OnStartGame);
        randomPlacementButton.onClick.AddListener(OnRandomPlacementSelected);
        manualPlacementButton.onClick.AddListener(OnManualPlacementSelected);

        // ����������� ������� ��� ������ ���������� ��� (0�8, ��������� �������� 4)
        mountainsSlider.minValue = 0;
        mountainsSlider.maxValue = 8;
        mountainsSlider.value = selectedMountains;
        mountainsSlider.onValueChanged.AddListener(OnMountainsSliderChanged);

        // �������������� ����� � ������� ���������
        mountainsValueText.text = selectedMountains.ToString();

        // ���������� settingsPanel, �������� mainMenuPanel
        settingsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        startGameButton.onClick.RemoveListener(OnStartGame);
        randomPlacementButton.onClick.RemoveListener(OnRandomPlacementSelected);
        manualPlacementButton.onClick.RemoveListener(OnManualPlacementSelected);
        mountainsSlider.onValueChanged.RemoveListener(OnMountainsSliderChanged);
    }

    /// <summary>
    /// ���������� ������� ���� � ������� ���� �����������.
    /// </summary>
    private void OnStartGame()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    /// <summary>
    /// ��������� ���� � �������������� ������������ �����.
    /// </summary>
    private void OnRandomPlacementSelected()
    {
        isRandomPlacement = true;
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        gameManager.StartGame(selectedMountains, isRandomPlacement);
    }

    /// <summary>
    /// ��������� � ������ ����������� �����.
    /// </summary>
    private void OnManualPlacementSelected()
    {
        isRandomPlacement = false;
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        boardManager.InitializeBoard(10);
        gameManager.IsInPlacementPhase = true; // ������������� ���� �����������
        manualPlacement.Initialize(selectedMountains);
    }

    /// <summary>
    /// ��������� ���������� ��� � ����� �� ������ �������� ��������.
    /// </summary>
    private void OnMountainsSliderChanged(float value)
    {
        selectedMountains = Mathf.FloorToInt(value);
        if (mountainsValueText != null)
            mountainsValueText.text = selectedMountains.ToString();
    }
}