using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Zenject;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// Управляет UI для ручной расстановки фигур и гор.
/// Реализует drag-and-drop для размещения из панели и поддержку перетаскивания размещённых фигур/гор.
/// </summary>
public class UIManualPlacement : MonoBehaviour
{
    [Inject] private IGameManager gameManager;
    [Inject] private IBoardManager boardManager;
    [Inject(Id = "Manual")] private IPiecePlacementManager placementManager;
    [Inject] private IPieceFactory pieceFactory;

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
        CreatePieceButton(panel, isPlayer1, null, true, ref yOffset); // Горы
        foreach (PieceType type in System.Enum.GetValues(typeof(PieceType)))
        {
            if (type != PieceType.Mountain)
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

    public bool PlacePieceOrMountain(bool isPlayer1, Vector3Int position, PieceType type, bool isMountain)
    {
        bool success = placementManager.PlacePieceOrMountain(isPlayer1, position, type, isMountain);
        if (success)
        {
            var piece = boardManager.GetPieceAt(position);
            if (piece != null)
            {
                var dragHandler = piece.gameObject.AddComponent<BoardPieceDragHandler>();
                dragHandler.Initialize();
            }
            UpdatePlayerPanels();
            UpdateFinishButtons();
            Debug.Log($"UIManualPlacement: Placed {(isMountain ? "mountain" : type.ToString())} at {position}");
        }
        return success;
    }

    public void UpdatePlayerPanels()
    {
        SetupPlayerPanels();
    }

    private void UpdateFinishButtons()
    {
        player1FinishButton.interactable = isPlayer1Turn && placementManager.HasCompletedPlacement(true);
        player2FinishButton.interactable = !isPlayer1Turn && placementManager.HasCompletedPlacement(false);
    }

    private void OnPlayer1Finish()
    {
        if (!placementManager.HasCompletedPlacement(true))
        {
            Debug.LogWarning("UIManualPlacement: Player 1 must place all pieces and mountains!");
            return;
        }

        isPlayer1Turn = false;
        player1FinishButton.interactable = false;
        player2FinishButton.interactable = placementManager.HasCompletedPlacement(false);
    }

    private void OnPlayer2Finish()
    {
        if (!placementManager.HasCompletedPlacement(false))
        {
            Debug.LogWarning("UIManualPlacement: Player 2 must place all pieces and mountains!");
            return;
        }

        placementPanel.SetActive(false);
        gameManager.StartGame(placementManager.GetMountainsPerSide, false);
    }
}

/// <summary>
/// Обработчик drag-and-drop для фигур и гор из UI панели.
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
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isMountain)
        {
            previewObject = pieceFactory.CreateMountain(Vector3Int.zero).gameObject;
        }
        else if (type.HasValue)
        {
            previewObject = pieceFactory.CreatePiece(type.Value, isPlayer1, Vector3Int.zero).gameObject;
        }

        if (previewObject != null)
        {
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

            previewObject.transform.DORotate(new Vector3(0, 360, 0), 2f, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Incremental)
                .SetEase(Ease.Linear);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (previewObject == null)
            return;

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

            previewObject.transform.position = new Vector3(position.x, 0.5f, position.z);
        }
        else
        {
            uiManager.ClearHighlight();
            lastHighlighted = null;

            Ray cursorRay = Camera.main.ScreenPointToRay(eventData.position);
            Plane boardPlane = new Plane(Vector3.up, new Vector3(0, 0.5f, 0));
            if (boardPlane.Raycast(cursorRay, out float distance))
            {
                Vector3 worldPoint = cursorRay.GetPoint(distance);
                previewObject.transform.position = new Vector3(worldPoint.x, 0.5f, worldPoint.z);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (previewObject != null)
        {
            previewObject.transform.DOKill();
            Destroy(previewObject);
        }

        if (lastHighlighted.HasValue)
        {
            PieceType targetType = isMountain ? PieceType.Mountain : type.Value;
            uiManager.PlacePieceOrMountain(isPlayer1, lastHighlighted.Value, targetType, isMountain);
        }

        uiManager.ClearHighlight();
        lastHighlighted = null;
    }
}