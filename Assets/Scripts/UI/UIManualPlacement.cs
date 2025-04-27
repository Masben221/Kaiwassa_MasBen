using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Zenject;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// Управляет UI для ручной расстановки фигур и гор.
/// Реализует drag-and-drop для размещения на доске.
/// </summary>
public class UIManualPlacement : MonoBehaviour
{
    [Inject] private IGameManager gameManager; // Интерфейс для управления игрой
    [Inject] private IBoardManager boardManager; // Интерфейс для управления доской
    [Inject(Id = "Manual")] private IPiecePlacementManager placementManager; // Менеджер ручной расстановки
    [Inject] private IPieceFactory pieceFactory; // Фабрика для создания фигур и гор

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

    // Публичное свойство для доступа к текущему игроку
    public bool IsPlayer1Turn => isPlayer1Turn;

    private void Awake()
    {
        if (!placementPanel || !player1Panel || !player2Panel || !player1FinishButton || !player2FinishButton || !highlightMaterial)
        {
            Debug.LogError("UIManualPlacement: One or more UI elements are not assigned in the Inspector!");
            return;
        }

        player1FinishButton.onClick.AddListener(OnPlayer1Finish);
        player2FinishButton.onClick.AddListener(OnPlayer2Finish);
        player2FinishButton.interactable = false;

        Debug.Log("UIManualPlacement: Awake completed, UI elements initialized.");
    }

    private void OnDestroy()
    {
        player1FinishButton.onClick.RemoveListener(OnPlayer1Finish);
        player2FinishButton.onClick.RemoveListener(OnPlayer2Finish);
        ClearHighlight();
    }

    public void Initialize(int mountainsPerSide)
    {
        placementManager.Initialize(mountainsPerSide);
        SetupPlayerPanels();
        placementPanel.SetActive(true);
        UpdateFinishButtons();
        Debug.Log($"UIManualPlacement: Initialized with {mountainsPerSide} mountains per side.");
    }

    private void SetupPlayerPanels()
    {
        foreach (Transform child in player1Panel) Destroy(child.gameObject);
        foreach (Transform child in player2Panel) Destroy(child.gameObject);

        CreatePieceButtons(player1Panel, true);
        CreatePieceButtons(player2Panel, false);
        Debug.Log("UIManualPlacement: Player panels set up with buttons.");
    }

    private void CreatePieceButtons(RectTransform panel, bool isPlayer1)
    {
        float yOffset = -20f;
        CreatePieceButton(panel, isPlayer1, null, true, ref yOffset);
        foreach (PieceType type in System.Enum.GetValues(typeof(PieceType)))
        {
            CreatePieceButton(panel, isPlayer1, type, false, ref yOffset);
        }
    }

    private void CreatePieceButton(RectTransform panel, bool isPlayer1, PieceType? type, bool isMountain, ref float yOffset)
    {
        int count = placementManager.GetRemainingCount(isPlayer1, type ?? PieceType.King, isMountain);
        if (count <= 0) return;

        GameObject buttonObj = new GameObject(isMountain ? "Mountain" : type.ToString());
        buttonObj.transform.SetParent(panel, false);
        RectTransform rt = buttonObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 50);
        rt.anchoredPosition = new Vector2(0, yOffset);
        yOffset -= 60f;

        Image image = buttonObj.AddComponent<Image>();
        image.color = isMountain ? Color.gray : Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.text = $"{(isMountain ? "Mountain" : type.ToString())} x{count}";
        text.font = buttonFont != null ? buttonFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.color = Color.black;
        text.alignment = TextAnchor.MiddleCenter;
        RectTransform textRt = text.GetComponent<RectTransform>();
        textRt.sizeDelta = new Vector2(90, 40);

        PieceDragHandler dragHandler = buttonObj.AddComponent<PieceDragHandler>();
        dragHandler.Initialize(isPlayer1, type, isMountain, this, pieceFactory);
        Debug.Log($"UIManualPlacement: Created button for {(isMountain ? "Mountain" : type.ToString())} x{count} for Player {(isPlayer1 ? 1 : 2)}");
    }

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
        var tile = boardManager.GetTileAt(position);
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

    public bool PlacePieceOrMountain(bool isPlayer1, Vector3Int position, PieceType? type, bool isMountain)
    {
        bool success = placementManager.PlacePieceOrMountain(isPlayer1, position, type ?? PieceType.King, isMountain);
        if (success)
        {
            UpdatePlayerPanels();
            UpdateFinishButtons();
            Debug.Log($"UIManualPlacement: Placed {(isMountain ? "mountain" : type.ToString())} at {position} for Player {(isPlayer1 ? 1 : 2)}");
        }
        else
        {
            Debug.LogWarning($"UIManualPlacement: Failed to place {(isMountain ? "mountain" : type.ToString())} at {position}");
        }
        return success;
    }

    private void UpdatePlayerPanels()
    {
        SetupPlayerPanels();
    }

    private void UpdateFinishButtons()
    {
        player1FinishButton.interactable = isPlayer1Turn && placementManager.HasCompletedPlacement(true);
        player2FinishButton.interactable = !isPlayer1Turn && placementManager.HasCompletedPlacement(false);
        Debug.Log($"UIManualPlacement: Finish buttons updated. Player1: {player1FinishButton.interactable}, Player2: {player2FinishButton.interactable}");
    }

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

    private void OnPlayer2Finish()
    {
        if (!placementManager.HasCompletedPlacement(false))
        {
            Debug.LogWarning("UIManualPlacement: Player 2 must place all pieces and mountains before finishing!");
            return;
        }

        placementPanel.SetActive(false);
        gameManager.StartGame(placementManager.GetMountainsPerSide, false);
        Debug.Log("UIManualPlacement: Player 2 finished placement, starting game.");
    }
}

/// <summary>
/// Обработчик drag-and-drop для новых фигур и гор.
/// </summary>
public class PieceDragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private bool isPlayer1;
    private PieceType? type;
    private bool isMountain;
    private UIManualPlacement uiManager;
    private IPieceFactory pieceFactory;
    private Vector3Int? lastHighlighted;
    private GameObject previewObject;
    private Material originalMaterial;

    public void Initialize(bool isPlayer1, PieceType? type, bool isMountain, UIManualPlacement uiManager, IPieceFactory pieceFactory)
    {
        this.isPlayer1 = isPlayer1;
        this.type = type;
        this.isMountain = isMountain;
        this.uiManager = uiManager;
        this.pieceFactory = pieceFactory;
        Debug.Log($"PieceDragHandler: Initialized for {(isMountain ? "mountain" : type.ToString())} for Player {(isPlayer1 ? 1 : 2)}");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isPlayer1 != uiManager.IsPlayer1Turn)
        {
            Debug.LogWarning($"PieceDragHandler: Cannot drag, it's {(isPlayer1 ? "Player 1" : "Player 2")}'s turn!");
            return;
        }

        if (isMountain)
        {
            var mountainPiece = pieceFactory.CreateMountain(Vector3Int.zero);
            previewObject = mountainPiece != null ? mountainPiece.gameObject : null;
        }
        else
        {
            var piece = pieceFactory.CreatePiece(type ?? PieceType.King, isPlayer1, Vector3Int.zero);
            previewObject = piece != null ? piece.gameObject : null;
        }

        if (previewObject != null)
        {
            var collider = previewObject.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;

            var renderer = previewObject.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                originalMaterial = renderer.material;
                Material transparentMat = new Material(originalMaterial);
                transparentMat.SetFloat("_Mode", 3);
                transparentMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                transparentMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                transparentMat.SetInt("_ZWrite", 0);
                transparentMat.DisableKeyword("_ALPHATEST_ON");
                transparentMat.EnableKeyword("_ALPHABLEND_ON");
                transparentMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                transparentMat.renderQueue = 3000;
                Color color = transparentMat.color;
                color.a = 0.5f;
                transparentMat.color = color;
                renderer.material = transparentMat;
            }
            else
            {
                Debug.LogWarning($"PieceDragHandler: No Renderer found on preview for {(isMountain ? "mountain" : type.ToString())}");
            }

            previewObject.transform.DORotate(new Vector3(0, 360, 0), 2f, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Incremental)
                .SetEase(Ease.Linear);

            Debug.Log($"PieceDragHandler: Created preview for {(isMountain ? "mountain" : type.ToString())}");
        }
        else
        {
            Debug.LogWarning($"PieceDragHandler: Failed to create preview for {(isMountain ? "mountain" : type.ToString())}");
        }

        Debug.Log($"PieceDragHandler: Started dragging {(isMountain ? "mountain" : type.ToString())} for Player {(isPlayer1 ? 1 : 2)}");
    }

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

            if (previewObject != null)
            {
                previewObject.transform.position = new Vector3(position.x, 0.5f, position.z);
            }
        }
        else
        {
            uiManager.ClearHighlight();
            lastHighlighted = null;

            if (previewObject != null && Camera.main != null)
            {
                Ray cursorRay = Camera.main.ScreenPointToRay(eventData.position);
                Plane boardPlane = new Plane(Vector3.up, new Vector3(0, 0.5f, 0));
                if (boardPlane.Raycast(cursorRay, out float distance))
                {
                    Vector3 worldPoint = cursorRay.GetPoint(distance);
                    previewObject.transform.position = new Vector3(worldPoint.x, 0.5f, worldPoint.z);
                }
                else
                {
                    Vector3 screenPoint = new Vector3(eventData.position.x, eventData.position.y, 10f);
                    Vector3 fallbackPoint = Camera.main.ScreenToWorldPoint(screenPoint);
                    previewObject.transform.position = new Vector3(fallbackPoint.x, 0.5f, fallbackPoint.z);
                }
            }
            Debug.Log("PieceDragHandler: Raycast missed, no tile hit.");
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        bool placed = false;
        if (lastHighlighted.HasValue)
        {
            placed = uiManager.PlacePieceOrMountain(isPlayer1, lastHighlighted.Value, type, isMountain);
            if (placed)
            {
                Debug.Log($"PieceDragHandler: Dropped {(isMountain ? "mountain" : type.ToString())} at {lastHighlighted.Value}");
            }
        }

        if (previewObject != null)
        {
            previewObject.transform.DOKill();
            var renderer = previewObject.GetComponentInChildren<Renderer>();
            if (renderer != null && originalMaterial != null)
            {
                renderer.material = originalMaterial;
            }
            Destroy(previewObject);
            previewObject = null;
            originalMaterial = null;
        }

        if (!placed)
        {
            Debug.Log("PieceDragHandler: Drop failed, no tile highlighted or placement invalid.");
        }

        uiManager.ClearHighlight();
        lastHighlighted = null;
    }
}