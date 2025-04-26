using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Zenject;
using System.Collections.Generic;

/// <summary>
/// Управляет UI для ручной расстановки фигур и гор.
/// Реализует drag-and-drop для размещения на доске.
/// </summary>
public class UIManualPlacement : MonoBehaviour
{
    [Inject] private IGameManager gameManager; // Интерфейс для управления игрой
    [Inject] private IBoardManager boardManager; // Интерфейс для управления доской
    [Inject(Id = "Manual")] private IPiecePlacementManager placementManager; // Менеджер ручной расстановки

    [SerializeField] private GameObject placementPanel; // Основная панель UI
    [SerializeField] private RectTransform player1Panel; // Панель игрока 1 (слева)
    [SerializeField] private RectTransform player2Panel; // Панель игрока 2 (справа)
    [SerializeField] private Button player1FinishButton; // Кнопка завершения для игрока 1
    [SerializeField] private Button player2FinishButton; // Кнопка завершения для игрока 2
    [SerializeField] private Material highlightMaterial; // Материал для подсветки клеток (зелёный)
    [SerializeField] private Font buttonFont; // Шрифт для текста кнопок (опционально)

    private bool isPlayer1Turn = true; // Текущий игрок (начинает игрок 1)
    private Vector3Int? highlightedTile; // Текущая подсвеченная клетка
    private Dictionary<Vector3Int, Material> originalTileMaterials = new Dictionary<Vector3Int, Material>(); // Исходные материалы плиток

    /// <summary>
    /// Инициализация: настройка UI и обработчиков.
    /// </summary>
    private void Awake()
    {
        // Проверка, что все UI-элементы привязаны
        if (!placementPanel || !player1Panel || !player2Panel || !player1FinishButton || !player2FinishButton || !highlightMaterial)
        {
            Debug.LogError("UIManualPlacement: One or more UI elements are not assigned in the Inspector!");
            return;
        }

        // Настройка обработчиков кнопок
        player1FinishButton.onClick.AddListener(OnPlayer1Finish);
        player2FinishButton.onClick.AddListener(OnPlayer2Finish);
        player2FinishButton.interactable = false; // Игрок 2 ждёт своей очереди

        Debug.Log("UIManualPlacement: Awake completed, UI elements initialized.");
    }

    /// <summary>
    /// Очистка: удаление обработчиков и подсветки.
    /// </summary>
    private void OnDestroy()
    {
        player1FinishButton.onClick.RemoveListener(OnPlayer1Finish);
        player2FinishButton.onClick.RemoveListener(OnPlayer2Finish);
        ClearHighlight();
    }

    /// <summary>
    /// Инициализирует UI с количеством гор из предыдущего экрана.
    /// </summary>
    /// <param name="mountainsPerSide">Количество гор на сторону.</param>
    public void Initialize(int mountainsPerSide)
    {
        placementManager.Initialize(mountainsPerSide);
        SetupPlayerPanels();
        placementPanel.SetActive(true);
        UpdateFinishButtons();
        Debug.Log($"UIManualPlacement: Initialized with {mountainsPerSide} mountains per side.");
    }

    /// <summary>
    /// Настраивает панели игроков с кнопками для фигур и гор.
    /// </summary>
    private void SetupPlayerPanels()
    {
        // Очищаем старые элементы
        foreach (Transform child in player1Panel) Destroy(child.gameObject);
        foreach (Transform child in player2Panel) Destroy(child.gameObject);

        // Создаём новые кнопки
        CreatePieceButtons(player1Panel, true);
        CreatePieceButtons(player2Panel, false);
        Debug.Log("UIManualPlacement: Player panels set up with buttons.");
    }

    /// <summary>
    /// Создаёт кнопки для фигур и гор на панели игрока.
    /// </summary>
    /// <param name="panel">Панель игрока (RectTransform).</param>
    /// <param name="isPlayer1">true, если для игрока 1.</param>
    private void CreatePieceButtons(RectTransform panel, bool isPlayer1)
    {
        float yOffset = -20f;

        // Добавляем кнопку для гор
        CreatePieceButton(panel, isPlayer1, null, true, ref yOffset);

        // Добавляем кнопки для всех типов фигур
        foreach (PieceType type in System.Enum.GetValues(typeof(PieceType)))
        {
            CreatePieceButton(panel, isPlayer1, type, false, ref yOffset);
        }
    }

    /// <summary>
    /// Создаёт кнопку для фигуры или горы.
    /// </summary>
    /// <param name="panel">Панель для размещения кнопки.</param>
    /// <param name="isPlayer1">true, если для игрока 1.</param>
    /// <param name="type">Тип фигуры (null для гор).</param>
    /// <param name="isMountain">true, если кнопка для горы.</param>
    /// <param name="yOffset">Смещение по y для позиционирования.</param>
    private void CreatePieceButton(RectTransform panel, bool isPlayer1, PieceType? type, bool isMountain, ref float yOffset)
    {
        int count = placementManager.GetRemainingCount(isPlayer1, type ?? PieceType.King, isMountain);
        if (count <= 0) return; // Пропускаем, если нет доступных элементов

        // Создаём GameObject для кнопки
        GameObject buttonObj = new GameObject(isMountain ? "Mountain" : type.ToString());
        buttonObj.transform.SetParent(panel, false);
        RectTransform rt = buttonObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 50);
        rt.anchoredPosition = new Vector2(0, yOffset);
        yOffset -= 60f;

        // Добавляем заглушку: цветной квадрат
        Image image = buttonObj.AddComponent<Image>();
        image.color = isMountain ? Color.gray : Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);

        // Добавляем текст с названием и количеством
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.text = $"{(isMountain ? "Mountain" : type.ToString())} x{count}";
        text.font = buttonFont != null ? buttonFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.color = Color.black;
        text.alignment = TextAnchor.MiddleCenter;
        RectTransform textRt = text.GetComponent<RectTransform>();
        textRt.sizeDelta = new Vector2(90, 40);

        // Добавляем компонент drag-and-drop
        PieceDragHandler dragHandler = buttonObj.AddComponent<PieceDragHandler>();
        dragHandler.Initialize(isPlayer1, type, isMountain, this);
        Debug.Log($"UIManualPlacement: Created button for {(isMountain ? "Mountain" : type.ToString())} x{count} for Player {(isPlayer1 ? 1 : 2)}");
    }

    /// <summary>
    /// Подсвечивает клетку под курсором при перетаскивании.
    /// </summary>
    /// <param name="position">Позиция клетки.</param>
    /// <param name="isPlayer1">true, если игрок 1.</param>
    /// <param name="isMountain">true, если перетаскивается гора.</param>
    public void HighlightTile(Vector3Int position, bool isPlayer1, bool isMountain)
    {
        if (!placementManager.CanPlace(isPlayer1, position, isMountain))
        {
            ClearHighlight();
            Debug.Log($"UIManualPlacement: Cannot highlight tile at {position} (invalid placement)");
            return;
        }

        ClearHighlight();
        highlightedTile = position;
        var tile = boardManager.GetTileAt(position); // Требуется метод в BoardManager
        if (tile != null)
        {
            var renderer = tile.GetComponent<Renderer>();
            originalTileMaterials[position] = renderer.material;
            renderer.material = highlightMaterial;
            Debug.Log($"UIManualPlacement: Highlighted tile at {position}");
        }
        else
        {
            Debug.LogWarning($"UIManualPlacement: No tile found at {position}");
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
                tile.GetComponent<Renderer>().material = originalTileMaterials[highlightedTile.Value];
                Debug.Log($"UIManualPlacement: Cleared highlight at {highlightedTile.Value}");
            }
            originalTileMaterials.Remove(highlightedTile.Value);
            highlightedTile = null;
        }
    }

    /// <summary>
    /// Размещает фигуру или гору на доске.
    /// </summary>
    /// <param name="isPlayer1">true, если игрок 1.</param>
    /// <param name="position">Позиция на доске.</param>
    /// <param name="type">Тип фигуры (null для гор).</param>
    /// <param name="isMountain">true, если размещается гора.</param>
    public void PlacePieceOrMountain(bool isPlayer1, Vector3Int position, PieceType? type, bool isMountain)
    {
        if (placementManager.PlacePieceOrMountain(isPlayer1, position, type ?? PieceType.King, isMountain))
        {
            UpdatePlayerPanels();
            UpdateFinishButtons();
            Debug.Log($"UIManualPlacement: Placed {(isMountain ? "mountain" : type.ToString())} at {position} for Player {(isPlayer1 ? 1 : 2)}");
        }
        else
        {
            Debug.LogWarning($"UIManualPlacement: Failed to place {(isMountain ? "mountain" : type.ToString())} at {position}");
        }
    }

    /// <summary>
    /// Обновляет панели игроков после размещения.
    /// </summary>
    private void UpdatePlayerPanels()
    {
        SetupPlayerPanels();
    }

    /// <summary>
    /// Обновляет состояние кнопок "Завершить".
    /// </summary>
    private void UpdateFinishButtons()
    {
        // Кнопка активна, только если все фигуры и горы размещены
        player1FinishButton.interactable = isPlayer1Turn && placementManager.HasCompletedPlacement(true);
        player2FinishButton.interactable = !isPlayer1Turn && placementManager.HasCompletedPlacement(false);
        Debug.Log($"UIManualPlacement: Finish buttons updated. Player1: {player1FinishButton.interactable}, Player2: {player2FinishButton.interactable}");
    }

    /// <summary>
    /// Завершение расстановки игроком 1.
    /// </summary>
    private void OnPlayer1Finish()
    {
        if (!placementManager.HasCompletedPlacement(true))
        {
            Debug.LogWarning("UIManualPlacement: Player 1 must place all pieces and mountains before finishing!");
            return;
        }

        isPlayer1Turn = false;
        player1FinishButton.interactable = false;
        player2FinishButton.interactable = placementManager.HasCompletedPlacement(false);
        Debug.Log("UIManualPlacement: Player 1 finished placement.");
    }

    /// <summary>
    /// Завершение расстановки игроком 2 и запуск игры.
    /// </summary>
    private void OnPlayer2Finish()
    {
        if (!placementManager.HasCompletedPlacement(false))
        {
            Debug.LogWarning("UIManualPlacement: Player 2 must place all pieces and mountains before finishing!");
            return;
        }

        placementPanel.SetActive(false);
        gameManager.StartGame(placementManager.GetMountainsPerSide, false); // Ручная расстановка
        Debug.Log("UIManualPlacement: Player 2 finished placement, starting game.");
    }
}

/// <summary>
/// Обработчик drag-and-drop для фигур и гор.
/// </summary>
public class PieceDragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private bool isPlayer1; // Принадлежность игроку
    private PieceType? type; // Тип фигуры (null для гор)
    private bool isMountain; // true, если это гора
    private UIManualPlacement uiManager; // Ссылка на UI-менеджер
    private Vector3Int? lastHighlighted; // Последняя подсвеченная клетка

    /// <summary>
    /// Инициализирует обработчик drag-and-drop.
    /// </summary>
    /// <param name="isPlayer1">true, если для игрока 1.</param>
    /// <param name="type">Тип фигуры (null для гор).</param>
    /// <param name="isMountain">true, если это гора.</param>
    /// <param name="uiManager">Ссылка на UI-менеджер.</param>
    public void Initialize(bool isPlayer1, PieceType? type, bool isMountain, UIManualPlacement uiManager)
    {
        this.isPlayer1 = isPlayer1;
        this.type = type;
        this.isMountain = isMountain;
        this.uiManager = uiManager;
        Debug.Log($"PieceDragHandler: Initialized for {(isMountain ? "mountain" : type.ToString())} for Player {(isPlayer1 ? 1 : 2)}");
    }

    /// <summary>
    /// Обработка начала перетаскивания.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"PieceDragHandler: Started dragging {(isMountain ? "mountain" : type.ToString())} for Player {(isPlayer1 ? 1 : 2)}");
    }

    /// <summary>
    /// Обработка перетаскивания.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3Int position = new Vector3Int(
                Mathf.FloorToInt(hit.point.x + 0.5f),
                0,
                Mathf.FloorToInt(hit.point.z + 0.5f)
            );
            uiManager.HighlightTile(position, isPlayer1, isMountain);
            lastHighlighted = position;
        }
        else
        {
            uiManager.ClearHighlight();
            lastHighlighted = null;
            Debug.Log("PieceDragHandler: Raycast missed, no tile hit.");
        }
    }

    /// <summary>
    /// Обработка завершения перетаскивания.
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (lastHighlighted.HasValue)
        {
            uiManager.PlacePieceOrMountain(isPlayer1, lastHighlighted.Value, type, isMountain);
            Debug.Log($"PieceDragHandler: Dropped {(isMountain ? "mountain" : type.ToString())} at {lastHighlighted.Value}");
        }
        else
        {
            Debug.Log("PieceDragHandler: Drop failed, no tile highlighted.");
        }
        uiManager.ClearHighlight();
        lastHighlighted = null;
    }
}