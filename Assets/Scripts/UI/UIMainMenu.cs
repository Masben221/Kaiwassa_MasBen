using UnityEngine;
using UnityEngine.UI;
using Zenject;

/// <summary>
/// Управляет UI главного меню для выбора режима расстановки и настройки числа гор.
/// </summary>
public class UIMainMenu : MonoBehaviour
{
    [Inject] private IGameManager gameManager; // Интерфейс для управления игрой

    // UI-элементы главного меню
    [SerializeField] private GameObject mainMenuPanel; // Панель выбора режима
    [SerializeField] private Button randomPlacementButton; // Кнопка случайной расстановки
    [SerializeField] private Button manualPlacementButton; // Кнопка ручной расстановки

    // UI-элементы настроек
    [SerializeField] private GameObject settingsPanel; // Панель настроек
    [SerializeField] private Slider mountainsSlider; // Слайдер для числа гор
    [SerializeField] private Text mountainsValueText; // Текст значения слайдера
    [SerializeField] private Button backButton; // Кнопка "Назад"
    [SerializeField] private Button startGameButton; // Кнопка "Начать игру"

    private int selectedMountains = 4; // Число гор по умолчанию
    private bool isRandomPlacement; // Выбран ли случайный режим
    private bool arePiecesPlaced; // Расставлены ли фигуры

    private void Awake()
    {
        // Проверяем, что все UI-элементы привязаны
        if (!mainMenuPanel || !randomPlacementButton || !manualPlacementButton ||
            !settingsPanel || !mountainsSlider || !mountainsValueText || !backButton || !startGameButton)
        {
            Debug.LogError("UIMainMenu: One or more UI elements are not assigned in the Inspector!");
            return;
        }

        // Настраиваем начальное состояние
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        startGameButton.interactable = false; // Кнопка неактивна
        arePiecesPlaced = false;

        // Настраиваем обработчики кнопок
        randomPlacementButton.onClick.AddListener(OnRandomPlacementSelected);
        manualPlacementButton.onClick.AddListener(OnManualPlacementSelected);
        backButton.onClick.AddListener(OnBackClicked);
        startGameButton.onClick.AddListener(OnStartGameClicked);

        // Настраиваем слайдер
        mountainsSlider.minValue = 0;
        mountainsSlider.maxValue = 8;
        mountainsSlider.wholeNumbers = true;
        mountainsSlider.value = selectedMountains;
        mountainsValueText.text = selectedMountains.ToString();
        mountainsSlider.onValueChanged.AddListener(OnMountainsSliderChanged);
    }

    private void OnDestroy()
    {
        // Очищаем обработчики
        randomPlacementButton.onClick.RemoveListener(OnRandomPlacementSelected);
        manualPlacementButton.onClick.RemoveListener(OnManualPlacementSelected);
        backButton.onClick.RemoveListener(OnBackClicked);
        startGameButton.onClick.RemoveListener(OnStartGameClicked);
        mountainsSlider.onValueChanged.RemoveListener(OnMountainsSliderChanged);
    }

    /// <summary>
    /// Выбор случайной расстановки.
    /// </summary>
    private void OnRandomPlacementSelected()
    {
        isRandomPlacement = true;
        arePiecesPlaced = true; // Для случайной расстановки считаем, что фигуры будут расставлены GameManager
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        startGameButton.interactable = true; // Активируем кнопку, так как режим выбран
        Debug.Log("UIMainMenu: Random placement selected.");
    }

    /// <summary>
    /// Выбор ручной расстановки (заглушка).
    /// </summary>
    private void OnManualPlacementSelected()
    {
        Debug.Log("Ручная расстановка не реализована");
        // Пока не переходим к настройкам, так как ручная расстановка не реализована
        // Можно раскомментировать, если позже добавим ручную расстановку
        // isRandomPlacement = false;
        // arePiecesPlaced = false; // Фигуры ещё не расставлены
        // mainMenuPanel.SetActive(false);
        // settingsPanel.SetActive(true);
    }

    /// <summary>
    /// Обновление числа гор через слайдер.
    /// </summary>
    private void OnMountainsSliderChanged(float value)
    {
        selectedMountains = Mathf.FloorToInt(value);
        mountainsValueText.text = selectedMountains.ToString();
        Debug.Log($"UIMainMenu: Mountains per side set to {selectedMountains}");
    }

    /// <summary>
    /// Возврат к выбору режима.
    /// </summary>
    private void OnBackClicked()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        startGameButton.interactable = false; // Деактивируем кнопку
        arePiecesPlaced = false;
        Debug.Log("UIMainMenu: Back to main menu.");
    }

    /// <summary>
    /// Запуск игры.
    /// </summary>
    private void OnStartGameClicked()
    {
        if (!arePiecesPlaced)
        {
            Debug.LogWarning("UIMainMenu: Cannot start game, pieces are not placed!");
            return;
        }

        // Запускаем игру с выбранным числом гор
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        gameManager.StartGame(selectedMountains); // Передаём число гор
        Debug.Log($"UIMainMenu: Game started with {selectedMountains} mountains per side.");
    }

    [Inject]
    public void Setup(IGameManager gameManager)
    {
        this.gameManager = gameManager;
    }
}