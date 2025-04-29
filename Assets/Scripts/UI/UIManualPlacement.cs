using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Zenject;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// ��������� UI ��� ����������� ����� � ���.
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

    public void Initialize(int mountainsPerSide)
    {
        placementManager.Initialize(mountainsPerSide);
        SetupPlayerPanels();
        placementPanel.SetActive(true);
        UpdateFinishButtons();
    }

    private void SetupPlayerPanels()
    {
        foreach (Transform child in player1Panel) Destroy(child.gameObject);
        foreach (Transform child in player2Panel) Destroy(child.gameObject);

        CreatePieceButtons(player1Panel, true);
        CreatePieceButtons(player2Panel, false);
    }

    private void CreatePieceButtons(RectTransform panel, bool isPlayer1)
    {
        float yOffset = -20f;
        CreatePieceButton(panel, isPlayer1, null, true, ref yOffset);
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
    }

    public void HighlightTile(Vector3Int position, bool isPlayer1, bool isMountain, PieceType type)
    {
        var piece = boardManager.GetPieceAt(position);
        if (piece != null && piece.IsPlayer1 == isPlayer1 && piece.Type == (isMountain ? PieceType.Mountain : type))
        {
            ClearHighlight();
            return;
        }

        if (!(placementManager as ManualPlacementManager).CanMove(isPlayer1, isMountain ? PieceType.Mountain : type, position))
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

    public bool PlacePieceOrMountain(bool isPlayer1, Vector3Int position, PieceType type, bool isMountain, bool isMove = false)
    {
        bool success = (placementManager as ManualPlacementManager).PlacePieceOrMountain(isPlayer1, position, type, isMountain, isMove);
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
            Debug.LogWarning("UIManualPlacement: Player 1 must place all pieces!");
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
            Debug.LogWarning("UIManualPlacement: Player 2 must place all pieces!");
            return;
        }

        placementPanel.SetActive(false);
        gameManager.StartGame(placementManager.GetMountainsPerSide, false);
    }
}