using UnityEngine;
using UnityEngine.UI;
using Zenject;

/// <summary>
/// Управляет UI игрового процесса, включая панель игры, отображение текущего хода и кнопку "Назад".
/// </summary>
public class UIGameManager : MonoBehaviour
{
    // Зависимости, инъектируемые через Zenject (из GameInstaller)
    [Inject] private IGameManager gameManager; // Менеджер игры для управления фазами и получения событий смены хода
    [Inject] private IBoardManager boardManager; // Менеджер доски для очистки фигур

    // Сериализируемые поля для UI-компонентов, задаются в инспекторе
    [SerializeField] private GameObject gamePanel; // Панель игрового процесса
    [SerializeField] private Button backButton; // Кнопка "Назад"
    [SerializeField] private UIManualPlacement uiManualPlacement; // Панель расстановки для возврата в меню
    [SerializeField] private Text currentTurnText; // Текст для отображения текущего хода (например, "Ход игрока 1")

    private void Awake()
    {
        // Проверяем, что все необходимые компоненты заданы в инспекторе
        if (gamePanel == null)
        {
            Debug.LogError("UIGameManager: GamePanel is not assigned in the inspector!");
            return;
        }

        if (backButton == null)
        {
            Debug.LogError("UIGameManager: BackButton is not assigned in the inspector!");
            return;
        }

        if (uiManualPlacement == null)
        {
            Debug.LogError("UIGameManager: UIManualPlacement is not assigned in the inspector!");
            return;
        }

        if (currentTurnText == null)
        {
            Debug.LogError("UIGameManager: CurrentTurnText is not assigned in the inspector!");
            return;
        }

        // Назначаем обработчик для кнопки "Назад"
        backButton.onClick.AddListener(OnBack);

        // Инициализируем стиль текста для отображения текущего хода
        SetupTurnTextStyle();
    }

    private void OnDestroy()
    {
        // Очищаем обработчик кнопки при уничтожении объекта
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBack);
        }

        // Отписываемся от события смены хода, чтобы избежать утечек памяти
        if (gameManager != null)
        {
            gameManager.OnTurnChanged -= UpdateTurnText;
        }
    }

    /// <summary>
    /// Инициализирует UI игрового процесса, показывая панель игры и подписываясь на события смены хода.
    /// </summary>
    public void Initialize()
    {
        gamePanel.SetActive(true); // Показываем панель игрового процесса

        // Подписываемся на событие смены хода из GameManager
        gameManager.OnTurnChanged += UpdateTurnText;

        // Устанавливаем начальный текст для текущего хода
        UpdateTurnText(gameManager.IsPlayer1Turn);

        Debug.Log("UIGameManager: Game UI initialized.");
    }

    /// <summary>
    /// Настраивает стиль текста для отображения текущего хода.
    /// </summary>
    private void SetupTurnTextStyle()
    {
        // Устанавливаем шрифт (если не задан, используем стандартный шрифт Unity)
        currentTurnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Устанавливаем размер шрифта
        currentTurnText.fontSize = 28;

        // Устанавливаем выравнивание текста по центру
        currentTurnText.alignment = TextAnchor.MiddleCenter;

        // Устанавливаем начальный цвет текста (золотистый для красоты)
        currentTurnText.color = new Color(1f, 0.84f, 0f); // Золотой цвет (#FFD700)

        // Добавляем обводку для лучшей читаемости
        var outline = currentTurnText.gameObject.GetComponent<Outline>();
        if (outline == null)
        {
            outline = currentTurnText.gameObject.AddComponent<Outline>();
        }
        outline.effectColor = Color.black; // Чёрная обводка
        outline.effectDistance = new Vector2(1, -1); // Размер обводки

        // Добавляем тень для дополнительного эффекта
        var shadow = currentTurnText.gameObject.GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = currentTurnText.gameObject.AddComponent<Shadow>();
        }
        shadow.effectColor = new Color(0, 0, 0, 0.5f); // Полупрозрачная чёрная тень
        shadow.effectDistance = new Vector2(1, -1); // Смещение тени
    }

    /// <summary>
    /// Обновляет текст текущего хода на основе того, чей ход.
    /// </summary>
    /// <param name="isPlayer1">True, если ход игрока 1, иначе игрока 2.</param>
    private void UpdateTurnText(bool isPlayer1)
    {
        // Обновляем текст в зависимости от текущего игрока
        currentTurnText.text = isPlayer1 ? "Ход игрока 1" : "Ход игрока 2";

        // Меняем цвет текста для наглядности (синий для игрока 1, красный для игрока 2)
        currentTurnText.color = isPlayer1 ? new Color(0.1f, 0.5f, 1f) : new Color(1f, 0.3f, 0.3f);

        Debug.Log($"UIGameManager: Updated turn text to '{currentTurnText.text}'.");
    }

    /// <summary>
    /// Обработчик кнопки "Назад".
    /// Очищает доску, сбрасывает состояние и возвращает в меню расстановки.
    /// </summary>
    private void OnBack()
    {
        // Очищаем все фигуры на доске
        var pieces = boardManager.GetAllPieces();
        foreach (var piece in pieces)
        {
            boardManager.RemovePiece(piece.Key);
            Object.Destroy(piece.Value.gameObject); // Уничтожаем GameObject фигур
        }

        // Переключаем фазу на расстановку
        gameManager.IsInPlacementPhase = true;

        // Скрываем панель игрового процесса
        gamePanel.SetActive(false);

        // Показываем панель расстановки и сбрасываем её состояние
        uiManualPlacement.Initialize(uiManualPlacement.GetSelectedMountains());
        Debug.Log("UIGameManager: Returned to placement menu, board reset.");
    }
}