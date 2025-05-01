using UnityEngine;
using UnityEngine.UI;
using Zenject;
using System.Collections.Generic;

public class UIManualPlacement : MonoBehaviour
{
    // Зависимости, инъектируемые через Zenject
    [Inject] private IGameManager gameManager; // Менеджер игры
    [Inject] private IBoardManager boardManager; // Менеджер доски
    [Inject(Id = "Manual")] private IPiecePlacementManager placementManager; // Менеджер ручной расстановки
    [Inject(Id = "Random")] private IPiecePlacementManager randomPlacementManager; // Менеджер автоматической расстановки
    [Inject] private IPieceFactory pieceFactory; // Фабрика для создания фигур
    [Inject] private DiContainer container; // Контейнер Zenject для создания компонентов

    // Ссылки на UI-элементы, задаваемые в инспекторе
    [SerializeField] private GameObject placementPanel; // Панель расстановки фигур
    [SerializeField] private GameObject mainMenuPanel; // Панель главного меню
    [SerializeField] private RectTransform player1Panel; // Панель игрока 1 для списка фигур
    [SerializeField] private RectTransform player2Panel; // Панель игрока 2 для списка фигур
    [SerializeField] private Button player1FinishButton; // Кнопка "Завершить" для игрока 1
    [SerializeField] private Button player2FinishButton; // Кнопка "Завершить" для игрока 2
    [SerializeField] private Button player1RandomButton; // Кнопка случайной генерации для игрока 1
    [SerializeField] private Button player2RandomButton; // Кнопка случайной генерации для игрока 2
    [SerializeField] private Button startGameButton; // Кнопка "Старт игры"
    [SerializeField] private Button backButton; // Кнопка "Назад"
    [SerializeField] private Slider mountainsSlider; // Слайдер для выбора количества гор
    [SerializeField] private Text mountainsValueText; // Текст, отображающий количество гор
    [SerializeField] private Material highlightMaterial; // Материал для подсветки клеток
    [SerializeField] private Font buttonFont; // Шрифт для текста на кнопках

    // Переменные состояния
    private bool isPlayer1Turn = true; // Чей ход сейчас (true — игрок 1, false — игрок 2)
    private bool player1Finished = false; // Завершил ли игрок 1 расстановку
    private bool player2Finished = false; // Завершил ли игрок 2 расстановку
    private int selectedMountains = 4; // Выбранное количество гор
    private Vector3Int? highlightedTile; // Подсвеченная клетка (если есть)
    private Dictionary<Vector3Int, Material> originalTileMaterials = new Dictionary<Vector3Int, Material>(); // Исходные материалы клеток

    private void Awake()
    {
        // Проверяем, что все UI-элементы заданы в инспекторе
        if (!placementPanel || !mainMenuPanel || !player1Panel || !player2Panel || !player1FinishButton ||
            !player2FinishButton || !player1RandomButton || !player2RandomButton || !startGameButton ||
            !backButton || !mountainsSlider || !mountainsValueText || !highlightMaterial)
        {
            Debug.LogError("UIManualPlacement: Missing UI elements!");
            return;
        }

        // Назначаем обработчики событий для кнопок
        player1FinishButton.onClick.AddListener(OnPlayer1Finish);
        player2FinishButton.onClick.AddListener(OnPlayer2Finish);
        player1RandomButton.onClick.AddListener(OnPlayer1Random);
        player2RandomButton.onClick.AddListener(OnPlayer2Random);
        startGameButton.onClick.AddListener(OnStartGame);
        backButton.onClick.AddListener(OnBack);

        // Настраиваем слайдер для выбора количества гор
        mountainsSlider.minValue = 0;
        mountainsSlider.maxValue = 8;
        mountainsSlider.wholeNumbers = true;
        mountainsSlider.onValueChanged.AddListener(OnMountainsSliderChanged);
        mountainsSlider.value = selectedMountains;
        mountainsValueText.text = selectedMountains.ToString();

        // Изначально кнопки "Завершить" и "Старт игры" неактивны
        startGameButton.interactable = false;
        player2FinishButton.interactable = false;
    }

    private void OnDestroy()
    {
        // Очищаем обработчики событий при уничтожении объекта
        player1FinishButton.onClick.RemoveListener(OnPlayer1Finish);
        player2FinishButton.onClick.RemoveListener(OnPlayer2Finish);
        player1RandomButton.onClick.RemoveListener(OnPlayer1Random);
        player2RandomButton.onClick.RemoveListener(OnPlayer2Random);
        startGameButton.onClick.RemoveListener(OnStartGame);
        backButton.onClick.RemoveListener(OnBack);
        mountainsSlider.onValueChanged.RemoveListener(OnMountainsSliderChanged);
        ClearHighlight(); // Очищаем подсветку клеток
    }

    /// <summary>
    /// Инициализирует панель расстановки.
    /// </summary>
    /// <param name="mountainsPerSide">Начальное количество гор на сторону.</param>
    public void Initialize(int mountainsPerSide)
    {
        selectedMountains = mountainsPerSide; // Устанавливаем количество гор
        mountainsSlider.value = selectedMountains; // Обновляем слайдер
        mountainsValueText.text = selectedMountains.ToString(); // Обновляем текст

        placementManager.Initialize(selectedMountains); // Инициализируем менеджер расстановки
        boardManager.InitializeBoard(10); // Создаём доску 10x10
        gameManager.IsInPlacementPhase = true; // Устанавливаем фазу расстановки

        SetupPlayerPanels(); // Создаём списки фигур для игроков
        placementPanel.SetActive(true); // Показываем панель расстановки

        // Сбрасываем состояние
        isPlayer1Turn = true;
        player1Finished = false;
        player2Finished = false;
        player1FinishButton.interactable = false;
        player2FinishButton.interactable = false;
        startGameButton.interactable = false;
    }

    /// <summary>
    /// Создаёт списки фигур для обоих игроков в UI.
    /// </summary>
    private void SetupPlayerPanels()
    {
        // Удаляем существующие кнопки фигур
        foreach (Transform child in player1Panel) Destroy(child.gameObject);
        foreach (Transform child in player2Panel) Destroy(child.gameObject);

        // Создаём новые кнопки
        CreatePieceButtons(player1Panel, true);
        CreatePieceButtons(player2Panel, false);
        UpdateFinishButtons(); // Обновляем состояние кнопок завершения
    }

    /// <summary>
    /// Создаёт кнопки для фигур и гор на указанной панели.
    /// </summary>
    /// <param name="panel">Панель для размещения кнопок.</param>
    /// <param name="isPlayer1">True, если для игрока 1.</param>
    private void CreatePieceButtons(RectTransform panel, bool isPlayer1)
    {
        float yOffset = -20f; // Начальное смещение по Y для кнопок

        // Создаём кнопку для гор
        CreatePieceButton(panel, isPlayer1, PieceType.Mountain, ref yOffset);

        // Создаём кнопки для остальных типов фигур
        foreach (PieceType type in System.Enum.GetValues(typeof(PieceType)))
        {
            if (type != PieceType.Mountain)
            {
                CreatePieceButton(panel, isPlayer1, type, ref yOffset);
            }
        }
    }

    /// <summary>
    /// Создаёт кнопку для конкретного типа фигуры или горы.
    /// </summary>
    /// <param name="panel">Панель для размещения кнопки.</param>
    /// <param name="isPlayer1">True, если для игрока 1.</param>
    /// <param name="type">Тип фигуры.</param>
    /// <param name="yOffset">Смещение по Y (обновляется).</param>
    private void CreatePieceButton(RectTransform panel, bool isPlayer1, PieceType type, ref float yOffset)
    {
        int count = placementManager.GetRemainingCount(isPlayer1, type); // Получаем количество оставшихся фигур
        if (count <= 0) return; // Пропускаем, если фигур нет

        // Создаём объект кнопки
        GameObject buttonObj = new GameObject(type.ToString());
        buttonObj.transform.SetParent(panel, false);
        RectTransform rt = buttonObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 50); // Размер кнопки
        rt.anchoredPosition = new Vector2(0, yOffset); // Позиция кнопки
        yOffset -= 60f; // Смещение для следующей кнопки

        // Добавляем изображение кнопки
        Image image = buttonObj.AddComponent<Image>();
        image.color = type == PieceType.Mountain ? Color.gray : Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);

        // Добавляем текст на кнопку
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.text = $"{type} x{count}"; // Формат: "King x1"
        text.font = buttonFont != null ? buttonFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.color = Color.black;
        text.alignment = TextAnchor.MiddleCenter;
        RectTransform textRt = text.GetComponent<RectTransform>();
        textRt.sizeDelta = new Vector2(90, 40);

        // Добавляем компонент для перетаскивания
        PieceDragHandler dragHandler = buttonObj.AddComponent<PieceDragHandler>();
        dragHandler.Initialize(isPlayer1, type, this, pieceFactory);
    }

    /// <summary>
    /// Подсвечивает клетку, если она доступна для размещения фигуры.
    /// </summary>
    /// <param name="position">Координаты клетки.</param>
    /// <param name="isPlayer1">True, если для игрока 1.</param>
    /// <param name="type">Тип фигуры.</param>
    public void HighlightTile(Vector3Int position, bool isPlayer1, PieceType type)
    {
        var piece = boardManager.GetPieceAt(position); // Проверяем, есть ли фигура на клетке
        if (piece != null)
        {
            // Если фигура принадлежит игроку и того же типа, очищаем подсветку
            if (piece.IsPlayer1 == isPlayer1 && piece.Type == type)
            {
                ClearHighlight();
                return;
            }
            isPlayer1 = piece.IsPlayer1; // Используем владельца фигуры для проверки перемещения
        }

        // Проверяем, можно ли переместить фигуру на эту клетку
        if (!(placementManager as ManualPlacementManager).CanMove(isPlayer1, type, position))
        {
            ClearHighlight();
            return;
        }

        // Подсвечиваем клетку
        ClearHighlight();
        highlightedTile = position;
        var tile = boardManager.GetTileAt(position);
        if (tile != null)
        {
            var renderer = tile.GetComponent<Renderer>();
            originalTileMaterials[position] = renderer.material; // Сохраняем исходный материал
            renderer.material = highlightMaterial; // Применяем материал подсветки
        }
    }

    /// <summary>
    /// Очищает подсветку клетки.
    /// </summary>
    public void ClearHighlight()
    {
        if (highlightedTile.HasValue)
        {
            var tile = boardManager.GetTileAt(highlightedTile.Value);
            if (tile != null && originalTileMaterials.ContainsKey(highlightedTile.Value))
            {
                tile.GetComponent<Renderer>().material = originalTileMaterials[highlightedTile.Value]; // Восстанавливаем материал
            }
            originalTileMaterials.Remove(highlightedTile.Value);
            highlightedTile = null;
        }
    }

    /// <summary>
    /// Размещает фигуру или гору на доске через UI.
    /// </summary>
    /// <param name="isPlayer1">True, если для игрока 1.</param>
    /// <param name="position">Координаты клетки.</param>
    /// <param name="type">Тип фигуры.</param>
    /// <returns>True, если размещение успешно.</returns>
    public bool PlacePieceOrMountain(bool isPlayer1, Vector3Int position, PieceType type)
    {
        // Пытаемся разместить фигуру через placementManager
        bool success = (placementManager as ManualPlacementManager).PlacePieceOrMountain(isPlayer1, position, type);
        if (success)
        {
            var piece = boardManager.GetPieceAt(position); // Получаем размещённую фигуру
            if (piece != null)
            {
                // Добавляем компонент для перетаскивания, если его нет
                if (!piece.gameObject.GetComponent<BoardPieceDragHandler>())
                {
                    var dragHandler = container.InstantiateComponent<BoardPieceDragHandler>(piece.gameObject);
                    dragHandler.Initialize(this);
                }
            }
            UpdatePlayerPanels(); // Обновляем списки фигур
            UpdateFinishButtons(); // Обновляем состояние кнопок завершения
        }
        return success;
    }

    /// <summary>
    /// Обновляет списки фигур игроков в UI.
    /// </summary>
    public void UpdatePlayerPanels()
    {
        SetupPlayerPanels();
    }

    /// <summary>
    /// Обновляет состояние кнопок завершения расстановки.
    /// Кнопка "Завершить" активируется для каждого игрока независимо, как только он разместит все свои фигуры.
    /// </summary>
    private void UpdateFinishButtons()
    {
        bool player1Completed = placementManager.HasCompletedPlacement(true); // Проверяем, завершил ли игрок 1
        bool player2Completed = placementManager.HasCompletedPlacement(false); // Проверяем, завершил ли игрок 2
        player1FinishButton.interactable = player1Completed && !player1Finished; // Условие для игрока 1
        player2FinishButton.interactable = player2Completed && !player2Finished; // Условие для игрока 2
        startGameButton.interactable = player1Finished && player2Finished; // Активируем "Старт игры" после обоих игроков
    }

    /// <summary>
    /// Обработчик кнопки "Завершить расстановку" для игрока 1.
    /// </summary>
    private void OnPlayer1Finish()
    {
        if (!placementManager.HasCompletedPlacement(true))
        {
            Debug.LogWarning("UIManualPlacement: Player 1 must place all pieces!");
            return;
        }

        player1Finished = true; // Отмечаем, что игрок 1 завершил
        isPlayer1Turn = false; // Передаём ход игроку 2
        player1FinishButton.interactable = false;
        UpdateFinishButtons();
    }

    /// <summary>
    /// Обработчик кнопки "Завершить расстановку" для игрока 2.
    /// </summary>
    private void OnPlayer2Finish()
    {
        if (!placementManager.HasCompletedPlacement(false))
        {
            Debug.LogWarning("UIManualPlacement: Player 2 must place all pieces!");
            return;
        }

        player2Finished = true; // Отмечаем, что игрок 2 завершил
        player2FinishButton.interactable = false;
        UpdateFinishButtons();
    }

    /// <summary>
    /// Обработчик кнопки случайной генерации для игрока 1.
    /// </summary>
    private void OnPlayer1Random()
    {
        // Очищаем текущие фигуры и горы игрока 1
        var pieces = boardManager.GetAllPieces();
        foreach (var piece in pieces)
        {
            if (piece.Value.IsPlayer1)
            {
                boardManager.RemovePiece(piece.Key);
            }
        }

        // Выполняем случайную генерацию фигур и гор для игрока 1
        randomPlacementManager.PlacePiecesForPlayer(true, selectedMountains);

        // Обновляем счётчики и добавляем drag handlers
        foreach (var piece in boardManager.GetAllPieces())
        {
            if (piece.Value.IsPlayer1)
            {
                // Уменьшаем счётчик в placementManager
                (placementManager as ManualPlacementManager).DecreasePieceCount(true, piece.Value.Type);
                // Добавляем drag handler, если его нет (нужно для перемещения фигур после генерации)
                if (!piece.Value.gameObject.GetComponent<BoardPieceDragHandler>())
                {
                    var dragHandler = container.InstantiateComponent<BoardPieceDragHandler>(piece.Value.gameObject);
                    dragHandler.Initialize(this);
                }
            }
        }

        // Обновляем список фигур игрока 1 в UI (должен стать пустым)
        foreach (Transform child in player1Panel) Destroy(child.gameObject);
        UpdateFinishButtons();
    }

    /// <summary>
    /// Обработчик кнопки случайной генерации для игрока 2.
    /// </summary>
    private void OnPlayer2Random()
    {
        // Очищаем текущие фигуры и горы игрока 2
        var pieces = boardManager.GetAllPieces();
        foreach (var piece in pieces)
        {
            if (!piece.Value.IsPlayer1)
            {
                boardManager.RemovePiece(piece.Key);
            }
        }

        // Выполняем случайную генерацию фигур и гор для игрока 2
        randomPlacementManager.PlacePiecesForPlayer(false, selectedMountains);

        // Обновляем счётчики и добавляем drag handlers
        foreach (var piece in boardManager.GetAllPieces())
        {
            if (!piece.Value.IsPlayer1)
            {
                // Уменьшаем счётчик в placementManager
                (placementManager as ManualPlacementManager).DecreasePieceCount(false, piece.Value.Type);
                // Добавляем drag handler, если его нет (нужно для перемещения фигур после генерации)
                if (!piece.Value.gameObject.GetComponent<BoardPieceDragHandler>())
                {
                    var dragHandler = container.InstantiateComponent<BoardPieceDragHandler>(piece.Value.gameObject);
                    dragHandler.Initialize(this);
                }
            }
        }

        // Обновляем список фигур игрока 2 в UI (должен стать пустым)
        foreach (Transform child in player2Panel) Destroy(child.gameObject);
        UpdateFinishButtons();
    }

    /// <summary>
    /// Обработчик кнопки "Старт игры".
    /// </summary>
    private void OnStartGame()
    {
        if (!player1Finished || !player2Finished)
        {
            Debug.LogWarning("UIManualPlacement: Both players must finish placement!");
            return;
        }

        placementPanel.SetActive(false); // Скрываем панель расстановки
        gameManager.StartGame(selectedMountains, false); // Запускаем игру
    }

    /// <summary>
    /// Обработчик кнопки "Назад".
    /// </summary>
    private void OnBack()
    {
        // Очищаем доску и все фигуры
        var pieces = boardManager.GetAllPieces();
        foreach (var piece in pieces)
        {
            boardManager.RemovePiece(piece.Key);
        }
        placementManager.Initialize(selectedMountains); // Сбрасываем placementManager
        boardManager.InitializeBoard(10); // Пересоздаём доску
        ClearHighlight(); // Очищаем подсветку клеток

        // Сбрасываем состояние
        player1Finished = false;
        player2Finished = false;
        isPlayer1Turn = true;

        placementPanel.SetActive(false); // Скрываем панель расстановки
        mainMenuPanel.SetActive(true); // Показываем главное меню
    }

    /// <summary>
    /// Обработчик изменения значения слайдера количества гор.
    /// </summary>
    /// <param name="value">Новое значение слайдера.</param>
    private void OnMountainsSliderChanged(float value)
    {
        selectedMountains = Mathf.FloorToInt(value); // Обновляем количество гор
        mountainsValueText.text = selectedMountains.ToString(); // Обновляем текст
        placementManager.Initialize(selectedMountains); // Переинициализируем placementManager

        // Очищаем доску
        var pieces = boardManager.GetAllPieces();
        foreach (var piece in pieces)
        {
            boardManager.RemovePiece(piece.Key);
        }

        // Сбрасываем состояние
        player1Finished = false;
        player2Finished = false;
        isPlayer1Turn = true;
        SetupPlayerPanels();
        UpdateFinishButtons();
    }
}