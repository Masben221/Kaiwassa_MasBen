using UnityEngine;
using UnityEngine.UI;
using Zenject;
using DG.Tweening; // Импортируем пространство имён для DOTween

/// <summary>
/// Управляет UI игрового процесса, включая панель игры, отображение текущего хода, кнопку "Назад" и кнопки подсказок.
/// Кнопки подсказок показывают все потенциальные клетки атаки противника (включая пустые и свои фигуры, исключая горы).
/// </summary>
public class UIGameManager : MonoBehaviour
{
    // Зависимости, инъектируемые через Zenject (из GameInstaller)
    [Inject] private IGameManager gameManager; // Менеджер игры для управления фазами и получения событий смены хода
    [Inject] private IBoardManager boardManager; // Менеджер доски для очистки фигур
    [Inject] private InputHandler inputHandler; // Обработчик ввода для управления подсветкой подсказок

    // Сериализируемые поля для UI-компонентов, задаются в инспекторе
    [SerializeField] private GameObject gamePanel; // Панель игрового процесса
    [SerializeField] private Button backButton; // Кнопка "Назад"
    [SerializeField] private UIManualPlacement uiManualPlacement; // Панель расстановки для возврата в меню
    [SerializeField] private Text currentTurnText; // Текст для отображения текущего хода (например, "Ход игрока 1")
    [SerializeField] private Button hintButtonPlayer1; // Кнопка подсказки для игрока 1 (показывает атаки игрока 2)
    [SerializeField] private Button hintButtonPlayer2; // Кнопка подсказки для игрока 2 (показывает атаки игрока 1)

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

        if (hintButtonPlayer1 == null)
        {
            Debug.LogError("UIGameManager: HintButtonPlayer1 is not assigned in the inspector!");
            return;
        }

        if (hintButtonPlayer2 == null)
        {
            Debug.LogError("UIGameManager: HintButtonPlayer2 is not assigned in the inspector!");
            return;
        }

        // Назначаем обработчики для кнопок
        backButton.onClick.AddListener(OnBack);
        hintButtonPlayer1.onClick.AddListener(() => OnHintButtonPressed(true));
        hintButtonPlayer2.onClick.AddListener(() => OnHintButtonPressed(false));

        // Настраиваем стиль текста для отображения текущего хода
        SetupTurnTextStyle();
    }

    private void OnDestroy()
    {
        // Очищаем обработчики кнопок при уничтожении объекта
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBack);
        }

        if (hintButtonPlayer1 != null)
        {
            hintButtonPlayer1.onClick.RemoveListener(() => OnHintButtonPressed(true));
        }

        if (hintButtonPlayer2 != null)
        {
            hintButtonPlayer2.onClick.RemoveListener(() => OnHintButtonPressed(false));
        }

        // Отписываемся от события смены хода
        if (gameManager != null)
        {
            gameManager.OnTurnChanged -= UpdateTurnText;
        }

        // Убиваем все активные анимации DOTween
        DOTween.Kill(currentTurnText);
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

        // Устанавливаем начальное состояние кнопок подсказок
        hintButtonPlayer1.interactable = !gameManager.IsInPlacementPhase;
        hintButtonPlayer2.interactable = !gameManager.IsInPlacementPhase;

        Debug.Log("UIGameManager: Game UI initialized.");
    }

    /// <summary>
    /// Настраивает стиль текста для отображения текущего хода.
    /// Шрифт и размер шрифта задаются через инспектор.
    /// </summary>
    private void SetupTurnTextStyle()
    {
        // Устанавливаем выравнивание текста по центру
        currentTurnText.alignment = TextAnchor.MiddleCenter;

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
    /// Обновляет текст текущего хода с анимацией, используя DOTween.
    /// </summary>
    /// <param name="isPlayer1">True, если ход игрока 1, иначе игрока 2.</param>
    private void UpdateTurnText(bool isPlayer1)
    {
        // Убиваем предыдущие анимации, чтобы не было наложений
        DOTween.Kill(currentTurnText);

        // Анимация исчезновения текста
        currentTurnText.DOFade(0f, 0.3f).OnComplete(() =>
        {
            // Обновляем текст и цвет
            currentTurnText.text = isPlayer1 ? "Ход игрока 1" : "Ход игрока 2";
            currentTurnText.color = isPlayer1 ? new Color(1f, 0.84f, 0f) : new Color(1f, 0.3f, 0.3f);

            // Сбрасываем масштаб текста
            currentTurnText.transform.localScale = Vector3.one;

            // Анимация появления текста
            currentTurnText.DOFade(1f, 0.3f);
            currentTurnText.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                currentTurnText.transform.DOScale(1f, 0.15f).SetEase(Ease.InOutQuad);
            });
        });

        Debug.Log($"UIGameManager: Updated turn text to '{currentTurnText.text}' with animation.");
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
            Object.Destroy(piece.Value.gameObject);
        }

        // Переключаем фазу на расстановку
        gameManager.IsInPlacementPhase = true;

        // Скрываем панель игрового процесса
        gamePanel.SetActive(false);

        // Показываем панель расстановки и сбрасываем её состояние
        uiManualPlacement.Initialize(uiManualPlacement.GetSelectedMountains());
        Debug.Log("UIGameManager: Returned to placement menu, board reset.");
    }

    /// <summary>
    /// Обработчик нажатия кнопки подсказки.
    /// Показывает все потенциальные клетки атаки указанного игрока (включая пустые и свои фигуры, исключая горы).
    /// </summary>
    /// <param name="isPlayer1">True, если показываем атаки игрока 1, иначе игрока 2.</param>
    private void OnHintButtonPressed(bool isPlayer1)
    {
        if (gameManager.IsInPlacementPhase)
        {
            Debug.LogWarning("UIGameManager: Hint buttons are disabled during placement phase.");
            return;
        }

        inputHandler.ShowAllPotentialAttackTiles(isPlayer1);
        Debug.Log($"UIGameManager: Hint button pressed for Player {(isPlayer1 ? 1 : 2)} to show all potential attack tiles.");
    }
}