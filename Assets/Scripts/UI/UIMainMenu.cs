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
    [SerializeField] private Text mountainsValueText; // “екст дл€ отображени€ количества гор
    [SerializeField] private UIManualPlacement manualPlacement;

    [Inject] private IGameManager gameManager;
    [Inject] private IBoardManager boardManager;

    private bool isRandomPlacement = true;
    private int selectedMountains = 4;

    private void Awake()
    {
        // ѕровер€ем наличие всех необходимых UI-элементов
        if (!mainMenuPanel || !settingsPanel || !startGameButton || !randomPlacementButton ||
            !manualPlacementButton || !mountainsSlider || !mountainsValueText || !manualPlacement)
        {
            Debug.LogError("UIMainMenu: One or more UI elements are not assigned in the Inspector!");
            return;
        }

        // Ќастраиваем слушатели кнопок и слайдера
        startGameButton.onClick.AddListener(OnStartGame);
        randomPlacementButton.onClick.AddListener(OnRandomPlacementSelected);
        manualPlacementButton.onClick.AddListener(OnManualPlacementSelected);

        // Ќастраиваем слайдер дл€ выбора количества гор (0Ц8, начальное значение 4)
        mountainsSlider.minValue = 0;
        mountainsSlider.maxValue = 8;
        mountainsSlider.value = selectedMountains;
        mountainsSlider.onValueChanged.AddListener(OnMountainsSliderChanged);

        // »нициализируем текст с текущим значением
        mountainsValueText.text = selectedMountains.ToString();

        // ѕоказываем settingsPanel, скрываем mainMenuPanel
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
    /// ѕоказывает главное меню с выбором типа расстановки.
    /// </summary>
    private void OnStartGame()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    /// <summary>
    /// «апускает игру с автоматической расстановкой фигур.
    /// </summary>
    private void OnRandomPlacementSelected()
    {
        isRandomPlacement = true;
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        gameManager.StartGame(selectedMountains, isRandomPlacement);
    }

    /// <summary>
    /// ѕереходит к ручной расстановке фигур.
    /// </summary>
    private void OnManualPlacementSelected()
    {
        isRandomPlacement = false;
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        boardManager.InitializeBoard(10);
        gameManager.IsInPlacementPhase = true; // ”станавливаем фазу расстановки
        manualPlacement.Initialize(selectedMountains);
    }

    /// <summary>
    /// ќбновл€ет количество гор и текст на основе значени€ слайдера.
    /// </summary>
    private void OnMountainsSliderChanged(float value)
    {
        selectedMountains = Mathf.FloorToInt(value);
        if (mountainsValueText != null)
            mountainsValueText.text = selectedMountains.ToString();
    }
}