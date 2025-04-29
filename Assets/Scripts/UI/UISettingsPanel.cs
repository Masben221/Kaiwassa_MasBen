using UnityEngine;
using UnityEngine.UI;

public class UISettingsPanel : MonoBehaviour
{
    // Ссылки на UI-элементы, которые задаются в инспекторе
    [SerializeField] private GameObject settingsPanel; // Панель настроек
    [SerializeField] private GameObject mainMenuPanel; // Панель главного меню
    [SerializeField] private Button backButton; // Кнопка "Назад"

    private void Awake()
    {
        // Проверяем, что все UI-элементы заданы в инспекторе
        if (!settingsPanel || !mainMenuPanel || !backButton)
        {
            Debug.LogError("UISettingsPanel: One or more UI elements are not assigned in the Inspector!");
            return;
        }

        // Назначаем обработчик для кнопки "Назад"
        backButton.onClick.AddListener(OnBack);
    }

    private void OnDestroy()
    {
        // Очищаем обработчик событий при уничтожении объекта
        backButton.onClick.RemoveListener(OnBack);
    }

    // Обработчик нажатия кнопки "Назад"
    private void OnBack()
    {
        settingsPanel.SetActive(false); // Скрываем панель настроек
        mainMenuPanel.SetActive(true); // Показываем главное меню
    }
}