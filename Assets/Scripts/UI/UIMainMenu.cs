using UnityEngine;
using UnityEngine.UI;
using Zenject;

/// <summary>
/// Управляет главным меню и настройками игры.
/// </summary>
public class UIMainMenu : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel; // Панель главного меню
    [SerializeField] private GameObject settingsPanel; // Панель настроек
    [SerializeField] private Button startGameButton; // Кнопка старта игры
    [SerializeField] private Button randomPlacementButton; // Кнопка случайной расстановки
    [SerializeField] private Button manualPlacementButton; // Кнопка ручной расстановки
    [SerializeField] private Slider mountainsSlider; // Слайдер для выбора количества гор
    [SerializeField] private UIManualPlacement manualPlacement; // UI для ручной расстановки

    [Inject] private IGameManager gameManager; // Интерфейс управления игрой
    [Inject] private IBoardManager boardManager; // Интерфейс управления доской

    private bool isRandomPlacement = true; // Режим расстановки (true = случайная)
    private int selectedMountains = 4; // Количество гор на сторону (по умолчанию)

    private void Awake()
    {
        // Проверка наличия UI-элементов
        if (!mainMenuPanel || !settingsPanel || !startGameButton || !randomPlacementButton || !manualPlacementButton || !mountainsSlider || !manualPlacement)
        {
            Debug.LogError("UIMainMenu: One or more UI elements are not assigned in the Inspector!");
            return;
        }

        // Настройка обработчиков кнопок
        startGameButton.onClick.AddListener(OnStartGame);
        randomPlacementButton.onClick.AddListener(OnRandomPlacementSelected);
        manualPlacementButton.onClick.AddListener(OnManualPlacementSelected);

        // Настройка слайдера
        mountainsSlider.minValue = 0;
        mountainsSlider.maxValue = 8;
        mountainsSlider.value = selectedMountains;
        mountainsSlider.onValueChanged.AddListener(OnMountainsSliderChanged);

        // Инициализация состояния
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    private void OnDestroy()
    {
        // Очистка обработчиков
        startGameButton.onClick.RemoveListener(OnStartGame);
        randomPlacementButton.onClick.RemoveListener(OnRandomPlacementSelected);
        manualPlacementButton.onClick.RemoveListener(OnManualPlacementSelected);
        mountainsSlider.onValueChanged.RemoveListener(OnMountainsSliderChanged);
    }

    /// <summary>
    /// Обработчик кнопки старта игры.
    /// </summary>
    private void OnStartGame()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        Debug.Log("UIMainMenu: Start game selected.");
    }

    /// <summary>
    /// Обработчик выбора случайной расстановки.
    /// </summary>
    private void OnRandomPlacementSelected()
    {
        isRandomPlacement = true;
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        gameManager.StartGame(selectedMountains, isRandomPlacement);
        Debug.Log("UIMainMenu: Random placement selected.");
    }

    /// <summary>
    /// Обработчик выбора ручной расстановки.
    /// </summary>
    private void OnManualPlacementSelected()
    {
        isRandomPlacement = false;
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        boardManager.InitializeBoard(10); // Инициализируем доску перед ручной расстановкой
        manualPlacement.Initialize(selectedMountains);
        Debug.Log("UIMainMenu: Manual placement selected.");
    }

    /// <summary>
    /// Обработчик изменения значения слайдера гор.
    /// </summary>
    private void OnMountainsSliderChanged(float value)
    {
        selectedMountains = Mathf.FloorToInt(value);
        Debug.Log($"UIMainMenu: Mountains per side set to {selectedMountains}");
    }
}