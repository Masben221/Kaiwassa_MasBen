using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Zenject;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class UIManualPlacement : MonoBehaviour
{
    // Зависимости, инъектируемые через Zenject
    [Inject] private IGameManager gameManager; // Менеджер игры
    [Inject] private IBoardManager boardManager; // Менеджер доски
    [Inject(Id = "Manual")] private IPiecePlacementManager placementManager; // Менеджер ручной расстановки
    [Inject(Id = "Random")] private IPiecePlacementManager randomPlacementManager; // Менеджер автоматической расстановки
    [Inject] private IPieceFactory pieceFactory; // Фабрика для создания фигур
    [Inject] private DiContainer container; // Контейнер Zenject для создания компонентов

    // Ссылки на UI-элементы, которые задаются в инспекторе
    [SerializeField] private GameObject placementPanel; // Панель расстановки
    [SerializeField] private GameObject mainMenuPanel; // Панель главного меню
    [SerializeField] private RectTransform player1Panel; // Панель игрока 1 для кнопок фигур
    [SerializeField] private RectTransform player2Panel; // Панель игрока 2 для кнопок фигур
    [SerializeField] private Button player1FinishButton; // Кнопка "Завершить расстановку" для игрока 1
    [SerializeField] private Button player2FinishButton; // Кнопка "Завершить расстановку" для игрока 2
    [SerializeField] private Button player1RandomButton; // Кнопка случайной генерации для игрока 1
    [SerializeField] private Button player2RandomButton; // Кнопка случайной генерации для игрока 2
    [SerializeField] private Button startGameButton; // Кнопка "Старт игры"
    [SerializeField] private Button backButton; // Кнопка "Назад"
    [SerializeField] private Slider mountainsSlider; // Слайдер для выбора количества гор
    [SerializeField] private Text mountainsValueText; // Текст, показывающий количество гор
    [SerializeField] private Material highlightMaterial; // Материал для подсветки клеток
    [SerializeField] private Font buttonFont; // Шрифт для текста на кнопках

    // Переменные состояния
    private bool isPlayer1Turn = true; // Чей ход сейчас (true — игрок 1, false — игрок 2)
    private bool player1Finished = false; // Завершил ли игрок 1 расстановку
    private bool player2Finished = false; // Завершил ли игрок 2 расстановку
    private int selectedMountains = 4; // Выбранное количество гор
    private Vector3Int? highlightedTile; // Подсвеченная клетка (если есть)
    private Dictionary<Vector3Int, Material> originalTileMaterials = new Dictionary<Vector3Int, Material>(); // Исходные материалы клеток

    // Переменные для управления проходами (аналог reservedPassages из PiecePlacementManager)
    private List<int> reservedPassagesPlayer1 = new List<int>(); // Зарезервированные проходы для игрока 1
    private List<int> reservedPassagesPlayer2 = new List<int>(); // Зарезервированные проходы для игрока 2

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
        mountainsSlider.value = selectedMountains;
        mountainsSlider.wholeNumbers = true;
        mountainsSlider.onValueChanged.AddListener(OnMountainsSliderChanged);
        mountainsValueText.text = selectedMountains.ToString();

        // Изначально кнопки "Завершить расстановку" и "Старт игры" неактивны
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

    // Инициализация панели расстановки
    public void Initialize(int mountainsPerSide)
    {
        selectedMountains = mountainsPerSide; // Устанавливаем начальное количество гор
        mountainsSlider.value = selectedMountains; // Обновляем слайдер
        mountainsValueText.text = selectedMountains.ToString(); // Обновляем текст

        placementManager.Initialize(selectedMountains); // Инициализируем менеджер ручной расстановки
        boardManager.InitializeBoard(10); // Создаём доску размером 10x10
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

        // Очищаем проходы
        reservedPassagesPlayer1.Clear();
        reservedPassagesPlayer2.Clear();
    }

    // Создаём списки фигур для обоих игроков
    private void SetupPlayerPanels()
    {
        // Удаляем все существующие кнопки фигур
        foreach (Transform child in player1Panel) Destroy(child.gameObject);
        foreach (Transform child in player2Panel) Destroy(child.gameObject);

        // Создаём новые кнопки
        CreatePieceButtons(player1Panel, true);
        CreatePieceButtons(player2Panel, false);
        UpdateFinishButtons(); // Обновляем состояние кнопок завершения
    }

    // Создаём кнопки для фигур и гор на указанной панели
    private void CreatePieceButtons(RectTransform panel, bool isPlayer1)
    {
        float yOffset = -20f; // Начальное смещение по Y для позиционирования кнопок

        // Сначала создаём кнопку для гор
        CreatePieceButton(panel, isPlayer1, PieceType.Mountain, ref yOffset);

        // Затем создаём кнопки для остальных типов фигур
        foreach (PieceType type in System.Enum.GetValues(typeof(PieceType)))
        {
            if (type != PieceType.Mountain)
            {
                CreatePieceButton(panel, isPlayer1, type, ref yOffset);
            }
        }
    }

    // Создаём кнопку для конкретного типа фигуры или горы
    private void CreatePieceButton(RectTransform panel, bool isPlayer1, PieceType type, ref float yOffset)
    {
        int count = placementManager.GetRemainingCount(isPlayer1, type); // Получаем количество оставшихся фигур
        if (count <= 0) return; // Если фигур нет, пропускаем

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

    // Подсвечиваем клетку, если она доступна для размещения фигуры
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

    // Очищаем подсветку клетки
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

    // Размещаем фигуру или гору на доске
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

    // Обновляем списки фигур игроков
    public void UpdatePlayerPanels()
    {
        SetupPlayerPanels();
    }

    // Обновляем состояние кнопок завершения расстановки
    private void UpdateFinishButtons()
    {
        bool player1Completed = placementManager.HasCompletedPlacement(true); // Проверяем, завершил ли игрок 1
        bool player2Completed = placementManager.HasCompletedPlacement(false); // Проверяем, завершил ли игрок 2
        player1FinishButton.interactable = isPlayer1Turn && player1Completed && !player1Finished;
        player2FinishButton.interactable = !isPlayer1Turn && player2Completed && !player2Finished;
        startGameButton.interactable = player1Finished && player2Finished; // Активируем "Старт игры" после обоих игроков
    }

    // Обработчик нажатия кнопки "Завершить расстановку" для игрока 1
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

    // Обработчик нажатия кнопки "Завершить расстановку" для игрока 2
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

    // Обработчик нажатия кнопки случайной генерации для игрока 1
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

        // Сбрасываем проходы для игрока 1
        reservedPassagesPlayer1.Clear();
        
        // Расставляем горы и фигуры для игрока 1
        randomPlacementManager.PlacePiecesForPlayer(true, selectedMountains);

        // Добавляем drag handlers и обнуляем счётчики в placementManager
        foreach (var piece in boardManager.GetAllPieces())
        {
            if (piece.Value.IsPlayer1)
            {
                // Уменьшаем счётчик в placementManager, как если бы фигура была размещена вручную
                (placementManager as ManualPlacementManager).PlacePieceOrMountain(true, piece.Key, piece.Value.Type, true);
                if (!piece.Value.gameObject.GetComponent<BoardPieceDragHandler>())
                {
                    var dragHandler = container.InstantiateComponent<BoardPieceDragHandler>(piece.Value.gameObject);
                    dragHandler.Initialize(this);
                }
            }
        }

        // Очищаем список фигур игрока 1
        foreach (Transform child in player1Panel) Destroy(child.gameObject);
        UpdateFinishButtons();
    }

    // Обработчик нажатия кнопки случайной генерации для игрока 2
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

        // Сбрасываем проходы для игрока 2
        reservedPassagesPlayer2.Clear();        

        // Расставляем горы и фигуры для игрока 2
        randomPlacementManager.PlacePiecesForPlayer(false, selectedMountains);

        // Добавляем drag handlers и обнуляем счётчики в placementManager
        foreach (var piece in boardManager.GetAllPieces())
        {
            if (!piece.Value.IsPlayer1)
            {
                // Уменьшаем счётчик в placementManager, как если бы фигура была размещена вручную
                (placementManager as ManualPlacementManager).PlacePieceOrMountain(false, piece.Key, piece.Value.Type, true);
                if (!piece.Value.gameObject.GetComponent<BoardPieceDragHandler>())
                {
                    var dragHandler = container.InstantiateComponent<BoardPieceDragHandler>(piece.Value.gameObject);
                    dragHandler.Initialize(this);
                }
            }
        }

        // Очищаем список фигур игрока 2
        foreach (Transform child in player2Panel) Destroy(child.gameObject);
        UpdateFinishButtons();
    }  

    // Обработчик нажатия кнопки "Старт игры"
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

    // Обработчик нажатия кнопки "Назад"
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
        reservedPassagesPlayer1.Clear();
        reservedPassagesPlayer2.Clear();

        placementPanel.SetActive(false); // Скрываем панель расстановки
        mainMenuPanel.SetActive(true); // Показываем главное меню
    }

    // Обработчик изменения значения слайдера количества гор
    private void OnMountainsSliderChanged(float value)
    {
        selectedMountains = Mathf.FloorToInt(value); // Обновляем количество гор
        mountainsValueText.text = selectedMountains.ToString(); // Обновляем текст
        placementManager.Initialize(selectedMountains); // Переинициализируем placementManager
        // Сбрасываем доску и списки
        var pieces = boardManager.GetAllPieces();
        foreach (var piece in pieces)
        {
            boardManager.RemovePiece(piece.Key);
        }
        player1Finished = false;
        player2Finished = false;
        isPlayer1Turn = true;
        reservedPassagesPlayer1.Clear();
        reservedPassagesPlayer2.Clear();
        SetupPlayerPanels();
        UpdateFinishButtons();
    }
}