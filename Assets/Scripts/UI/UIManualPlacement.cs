using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Zenject;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// Управляет UI для расстановки фигур и гор.
/// </summary>
public class UIManualPlacement : MonoBehaviour
{
    [Inject] private IGameManager gameManager;
    [Inject] private IBoardManager boardManager;
    [Inject(Id = "Manual")] private IPiecePlacementManager placementManager;
    [Inject] private IPieceFactory pieceFactory;
    [Inject] private DiContainer container;

    [SerializeField] private GameObject placementPanel;
    [SerializeField] private RectTransform player1Panel;
    [SerializeField] private RectTransform player2Panel;
    [SerializeField] private Button player1FinishButton;
    [SerializeField] private Button player2FinishButton;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Font buttonFont;

    private bool isPlayer1Turn = true;
    private Vector3Int? highlightedTile;
    private Dictionary<Vector3Int, Material> originalTileMaterials = new Dictionary<Vector3Int, Material>();

    private void Awake()
    {
        if (!placementPanel || !player1Panel || !player2Panel || !player1FinishButton || !player2FinishButton || !highlightMaterial)
        {
            Debug.LogError("UIManualPlacement: Missing UI elements!");
            return;
        }

        player1FinishButton.onClick.AddListener(OnPlayer1Finish);
        player2FinishButton.onClick.AddListener(OnPlayer2Finish);
        player2FinishButton.interactable = false;
    }

    private void OnDestroy()
    {
        player1FinishButton.onClick.RemoveListener(OnPlayer1Finish);
        player2FinishButton.onClick.RemoveListener(OnPlayer2Finish);
        ClearHighlight();
    }

    /// <summary>
    /// Инициализирует UI для расстановки, создавая панели для игроков.
    /// </summary>
    /// <param name="mountainsPerSide">Количество гор на сторону.</param>
    public void Initialize(int mountainsPerSide)
    {
        // TODO: В будущем добавить возможность динамического изменения mountainsPerSide через UI
        placementManager.Initialize(mountainsPerSide);
        SetupPlayerPanels();
        placementPanel.SetActive(true);
        UpdateFinishButtons();
    }

    /// <summary>
    /// Создаёт панели с кнопками для каждого игрока.
    /// </summary>
    private void SetupPlayerPanels()
    {
        foreach (Transform child in player1Panel) Destroy(child.gameObject);
        foreach (Transform child in player2Panel) Destroy(child.gameObject);

        CreatePieceButtons(player1Panel, true);
        CreatePieceButtons(player2Panel, false);
        UpdateFinishButtons(); // Обновляем кнопки после создания панелей
    }

    /// <summary>
    /// Создаёт кнопки для фигур и гор на указанной панели, с горами вверху списка.
    /// </summary>
    /// <param name="panel">Панель игрока.</param>
    /// <param name="isPlayer1">true, если для игрока 1.</param>
    private void CreatePieceButtons(RectTransform panel, bool isPlayer1)
    {
        float yOffset = -20f;

        // Сначала создаём кнопку для гор
        CreatePieceButton(panel, isPlayer1, PieceType.Mountain, ref yOffset);

        // Затем создаём кнопки для остальных типов фигур
        foreach (PieceType type in System.Enum.GetValues(typeof(PieceType)))
        {
            if (type != PieceType.Mountain) // Пропускаем горы, так как они уже добавлены
            {
                CreatePieceButton(panel, isPlayer1, type, ref yOffset);
            }
        }
    }

    /// <summary>
    /// Создаёт кнопку для конкретного типа фигуры или горы.
    /// </summary>
    /// <param name="panel">Панель игрока.</param>
    /// <param name="isPlayer1">true, если для игрока 1.</param>
    /// <param name="type">Тип фигуры (King, Mountain и т.д.).</param>
    /// <param name="yOffset">Смещение по Y для позиционирования.</param>
    private void CreatePieceButton(RectTransform panel, bool isPlayer1, PieceType type, ref float yOffset)
    {
        int count = placementManager.GetRemainingCount(isPlayer1, type);
        if (count <= 0) return;

        GameObject buttonObj = new GameObject(type.ToString());
        buttonObj.transform.SetParent(panel, false);
        RectTransform rt = buttonObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 50);
        rt.anchoredPosition = new Vector2(0, yOffset);
        yOffset -= 60f;

        Image image = buttonObj.AddComponent<Image>();
        image.color = type == PieceType.Mountain ? Color.gray : Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.text = $"{type} x{count}";
        text.font = buttonFont != null ? buttonFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.color = Color.black;
        text.alignment = TextAnchor.MiddleCenter;
        RectTransform textRt = text.GetComponent<RectTransform>();
        textRt.sizeDelta = new Vector2(90, 40);

        PieceDragHandler dragHandler = buttonObj.AddComponent<PieceDragHandler>();
        dragHandler.Initialize(isPlayer1, type, this, pieceFactory);
    }

    /// <summary>
    /// Подсвечивает клетку, если она доступна для размещения фигуры.
    /// </summary>
    /// <param name="position">Координаты клетки.</param>
    /// <param name="isPlayer1">true, если для игрока 1.</param>
    /// <param name="type">Тип фигуры (King, Mountain и т.д.).</param>
    public void HighlightTile(Vector3Int position, bool isPlayer1, PieceType type)
    {
        var piece = boardManager.GetPieceAt(position);
        if (piece != null)
        {
            // Если клетка занята, проверяем, соответствует ли фигура игроку и типу
            if (piece.IsPlayer1 == isPlayer1 && piece.Type == type)
            {
                ClearHighlight();
                return;
            }
            // Используем принадлежность фигуры на доске для проверки перемещения
            isPlayer1 = piece.IsPlayer1;
        }

        if (!(placementManager as ManualPlacementManager).CanMove(isPlayer1, type, position))
        {
            ClearHighlight();
            return;
        }

        ClearHighlight();
        highlightedTile = position;
        var tile = boardManager.GetTileAt(position);
        if (tile != null)
        {
            var renderer = tile.GetComponent<Renderer>();
            originalTileMaterials[position] = renderer.material;
            renderer.material = highlightMaterial;
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
            }
            originalTileMaterials.Remove(highlightedTile.Value);
            highlightedTile = null;
        }
    }

    /// <summary>
    /// Размещает фигуру или гору на доске.
    /// </summary>
    /// <param name="isPlayer1">true, если для игрока 1.</param>
    /// <param name="position">Координаты клетки.</param>
    /// <param name="type">Тип фигуры (King, Mountain и т.д.).</param>
    /// <returns>true, если размещение успешно.</returns>
    public bool PlacePieceOrMountain(bool isPlayer1, Vector3Int position, PieceType type)
    {
        bool success = (placementManager as ManualPlacementManager).PlacePieceOrMountain(isPlayer1, position, type);
        if (success)
        {
            var piece = boardManager.GetPieceAt(position);
            if (piece != null)
            {
                if (!piece.gameObject.GetComponent<BoardPieceDragHandler>())
                {
                    var dragHandler = container.InstantiateComponent<BoardPieceDragHandler>(piece.gameObject);
                    dragHandler.Initialize(this);
                }
            }
            UpdatePlayerPanels();
            UpdateFinishButtons(); // Обновляем кнопки после размещения
        }
        return success;
    }

    /// <summary>
    /// Обновляет панели игроков, перестраивая кнопки.
    /// </summary>
    public void UpdatePlayerPanels()
    {
        SetupPlayerPanels();
    }

    /// <summary>
    /// Обновляет состояние кнопок завершения расстановки.
    /// </summary>
    private void UpdateFinishButtons()
    {
        bool player1Completed = placementManager.HasCompletedPlacement(true);
        bool player2Completed = placementManager.HasCompletedPlacement(false);
        player1FinishButton.interactable = isPlayer1Turn && player1Completed;
        player2FinishButton.interactable = !isPlayer1Turn && player2Completed;
    }

    /// <summary>
    /// Завершает расстановку для игрока 1, переключая ход.
    /// </summary>
    private void OnPlayer1Finish()
    {
        if (!placementManager.HasCompletedPlacement(true))
        {
            Debug.LogWarning("UIManualPlacement: Player 1 must place all pieces!");
            return;
        }

        isPlayer1Turn = false;
        player1FinishButton.interactable = false;
        player2FinishButton.interactable = placementManager.HasCompletedPlacement(false);
        UpdateFinishButtons(); // Обновляем кнопки после переключения хода
    }

    /// <summary>
    /// Завершает расстановку для игрока 2, начиная игру.
    /// </summary>
    private void OnPlayer2Finish()
    {
        if (!placementManager.HasCompletedPlacement(false))
        {
            Debug.LogWarning("UIManualPlacement: Player 2 must place all pieces!");
            return;
        }

        placementPanel.SetActive(false);
        gameManager.StartGame(placementManager.GetMountainsPerSide, false);
    }
}