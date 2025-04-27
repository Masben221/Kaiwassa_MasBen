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
    [SerializeField] private UIManualPlacement manualPlacement;

    [Inject] private IGameManager gameManager;
    [Inject] private IBoardManager boardManager;

    private bool isRandomPlacement = true;
    private int selectedMountains = 4;

    private void Awake()
    {
        if (!mainMenuPanel || !settingsPanel || !startGameButton || !randomPlacementButton || !manualPlacementButton || !mountainsSlider || !manualPlacement)
        {
            Debug.LogError("UIMainMenu: One or more UI elements are not assigned in the Inspector!");
            return;
        }

        startGameButton.onClick.AddListener(OnStartGame);
        randomPlacementButton.onClick.AddListener(OnRandomPlacementSelected);
        manualPlacementButton.onClick.AddListener(OnManualPlacementSelected);

        mountainsSlider.minValue = 0;
        mountainsSlider.maxValue = 8;
        mountainsSlider.value = selectedMountains;
        mountainsSlider.onValueChanged.AddListener(OnMountainsSliderChanged);

        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    private void OnDestroy()
    {
        startGameButton.onClick.RemoveListener(OnStartGame);
        randomPlacementButton.onClick.RemoveListener(OnRandomPlacementSelected);
        manualPlacementButton.onClick.RemoveListener(OnManualPlacementSelected);
        mountainsSlider.onValueChanged.RemoveListener(OnMountainsSliderChanged);
    }

    private void OnStartGame()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        Debug.Log("UIMainMenu: Start game selected.");
    }

    private void OnRandomPlacementSelected()
    {
        isRandomPlacement = true;
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        gameManager.StartGame(selectedMountains, isRandomPlacement);
        Debug.Log("UIMainMenu: Random placement selected.");
    }

    private void OnManualPlacementSelected()
    {
        isRandomPlacement = false;
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        boardManager.InitializeBoard(10);
        gameManager.IsInPlacementPhase = true; // Устанавливаем фазу расстановки
        manualPlacement.Initialize(selectedMountains);
        Debug.Log("UIMainMenu: Manual placement selected.");
    }

    private void OnMountainsSliderChanged(float value)
    {
        selectedMountains = Mathf.FloorToInt(value);
        Debug.Log($"UIMainMenu: Mountains per side set to {selectedMountains}");
    }
}