using UnityEngine;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour
{
    // Ссылки на UI-элементы, которые задаются в инспекторе
    [SerializeField] private GameObject mainMenuPanel; // Панель главного меню
    [SerializeField] private GameObject settingsPanel; // Панель настроек
    [SerializeField] private GameObject placementPanel; // Панель расстановки фигур
    [SerializeField] private Button startGameButton; // Кнопка "Старт игры"
    [SerializeField] private Button selectCharButton; // Кнопка "Выбор персонажа"
    [SerializeField] private Button settingsButton; // Кнопка "Настройки"
    [SerializeField] private UIManualPlacement manualPlacement; // Ссылка на компонент UIManualPlacement

    private void Awake()
    {
        // Проверяем, что все UI-элементы заданы в инспекторе
        if (!mainMenuPanel || !settingsPanel || !placementPanel || !startGameButton ||
            !selectCharButton || !settingsButton || !manualPlacement)
        {
            Debug.LogError("UIMainMenu: One or more UI elements are not assigned in the Inspector!");
            return;
        }

        // Назначаем обработчики событий для кнопок
        startGameButton.onClick.AddListener(OnStartGame);
        selectCharButton.onClick.AddListener(OnSelectCharacter);
        settingsButton.onClick.AddListener(OnSettings);

        // Изначально показываем только главное меню
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        placementPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        // Очищаем обработчики событий при уничтожении объекта
        startGameButton.onClick.RemoveListener(OnStartGame);
        selectCharButton.onClick.RemoveListener(OnSelectCharacter);
        settingsButton.onClick.RemoveListener(OnSettings);
    }

    // Обработчик нажатия кнопки "Старт игры"
    private void OnStartGame()
    {
        mainMenuPanel.SetActive(false); // Скрываем главное меню
        placementPanel.SetActive(true); // Показываем панель расстановки
        manualPlacement.Initialize(4); // Инициализируем расстановку с 4 горами по умолчанию
    }

    // Обработчик нажатия кнопки "Выбор персонажа" (заглушка)
    private void OnSelectCharacter()
    {
        Debug.Log("UIMainMenu: Select Character - Not implemented yet.");
    }

    // Обработчик нажатия кнопки "Настройки"
    private void OnSettings()
    {
        mainMenuPanel.SetActive(false); // Скрываем главное меню
        settingsPanel.SetActive(true); // Показываем панель настроек
    }
}