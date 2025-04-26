using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Zenject;
using System.Collections.Generic;

/// <summary>
/// ��������� UI ��� ������ ����������� ����� � ���.
/// ��������� drag-and-drop ��� ���������� �� �����.
/// </summary>
public class UIManualPlacement : MonoBehaviour
{
    [Inject] private IGameManager gameManager; // ��������� ��� ���������� �����
    [Inject] private IBoardManager boardManager; // ��������� ��� ���������� ������
    [Inject(Id = "Manual")] private IPiecePlacementManager placementManager; // �������� ������ �����������

    [SerializeField] private GameObject placementPanel; // �������� ������ UI
    [SerializeField] private RectTransform player1Panel; // ������ ������ 1 (�����)
    [SerializeField] private RectTransform player2Panel; // ������ ������ 2 (������)
    [SerializeField] private Button player1FinishButton; // ������ ���������� ��� ������ 1
    [SerializeField] private Button player2FinishButton; // ������ ���������� ��� ������ 2
    [SerializeField] private Material highlightMaterial; // �������� ��� ��������� ������ (������)
    [SerializeField] private Font buttonFont; // ����� ��� ������ ������ (�����������)

    private bool isPlayer1Turn = true; // ������� ����� (�������� ����� 1)
    private Vector3Int? highlightedTile; // ������� ������������ ������
    private Dictionary<Vector3Int, Material> originalTileMaterials = new Dictionary<Vector3Int, Material>(); // �������� ��������� ������

    /// <summary>
    /// �������������: ��������� UI � ������������.
    /// </summary>
    private void Awake()
    {
        // ��������, ��� ��� UI-�������� ���������
        if (!placementPanel || !player1Panel || !player2Panel || !player1FinishButton || !player2FinishButton || !highlightMaterial)
        {
            Debug.LogError("UIManualPlacement: One or more UI elements are not assigned in the Inspector!");
            return;
        }

        // ��������� ������������ ������
        player1FinishButton.onClick.AddListener(OnPlayer1Finish);
        player2FinishButton.onClick.AddListener(OnPlayer2Finish);
        player2FinishButton.interactable = false; // ����� 2 ��� ����� �������

        Debug.Log("UIManualPlacement: Awake completed, UI elements initialized.");
    }

    /// <summary>
    /// �������: �������� ������������ � ���������.
    /// </summary>
    private void OnDestroy()
    {
        player1FinishButton.onClick.RemoveListener(OnPlayer1Finish);
        player2FinishButton.onClick.RemoveListener(OnPlayer2Finish);
        ClearHighlight();
    }

    /// <summary>
    /// �������������� UI � ����������� ��� �� ����������� ������.
    /// </summary>
    /// <param name="mountainsPerSide">���������� ��� �� �������.</param>
    public void Initialize(int mountainsPerSide)
    {
        placementManager.Initialize(mountainsPerSide);
        SetupPlayerPanels();
        placementPanel.SetActive(true);
        UpdateFinishButtons();
        Debug.Log($"UIManualPlacement: Initialized with {mountainsPerSide} mountains per side.");
    }

    /// <summary>
    /// ����������� ������ ������� � �������� ��� ����� � ���.
    /// </summary>
    private void SetupPlayerPanels()
    {
        // ������� ������ ��������
        foreach (Transform child in player1Panel) Destroy(child.gameObject);
        foreach (Transform child in player2Panel) Destroy(child.gameObject);

        // ������ ����� ������
        CreatePieceButtons(player1Panel, true);
        CreatePieceButtons(player2Panel, false);
        Debug.Log("UIManualPlacement: Player panels set up with buttons.");
    }

    /// <summary>
    /// ������ ������ ��� ����� � ��� �� ������ ������.
    /// </summary>
    /// <param name="panel">������ ������ (RectTransform).</param>
    /// <param name="isPlayer1">true, ���� ��� ������ 1.</param>
    private void CreatePieceButtons(RectTransform panel, bool isPlayer1)
    {
        float yOffset = -20f;

        // ��������� ������ ��� ���
        CreatePieceButton(panel, isPlayer1, null, true, ref yOffset);

        // ��������� ������ ��� ���� ����� �����
        foreach (PieceType type in System.Enum.GetValues(typeof(PieceType)))
        {
            CreatePieceButton(panel, isPlayer1, type, false, ref yOffset);
        }
    }

    /// <summary>
    /// ������ ������ ��� ������ ��� ����.
    /// </summary>
    /// <param name="panel">������ ��� ���������� ������.</param>
    /// <param name="isPlayer1">true, ���� ��� ������ 1.</param>
    /// <param name="type">��� ������ (null ��� ���).</param>
    /// <param name="isMountain">true, ���� ������ ��� ����.</param>
    /// <param name="yOffset">�������� �� y ��� ����������������.</param>
    private void CreatePieceButton(RectTransform panel, bool isPlayer1, PieceType? type, bool isMountain, ref float yOffset)
    {
        int count = placementManager.GetRemainingCount(isPlayer1, type ?? PieceType.King, isMountain);
        if (count <= 0) return; // ����������, ���� ��� ��������� ���������

        // ������ GameObject ��� ������
        GameObject buttonObj = new GameObject(isMountain ? "Mountain" : type.ToString());
        buttonObj.transform.SetParent(panel, false);
        RectTransform rt = buttonObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 50);
        rt.anchoredPosition = new Vector2(0, yOffset);
        yOffset -= 60f;

        // ��������� ��������: ������� �������
        Image image = buttonObj.AddComponent<Image>();
        image.color = isMountain ? Color.gray : Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);

        // ��������� ����� � ��������� � �����������
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.text = $"{(isMountain ? "Mountain" : type.ToString())} x{count}";
        text.font = buttonFont != null ? buttonFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.color = Color.black;
        text.alignment = TextAnchor.MiddleCenter;
        RectTransform textRt = text.GetComponent<RectTransform>();
        textRt.sizeDelta = new Vector2(90, 40);

        // ��������� ��������� drag-and-drop
        PieceDragHandler dragHandler = buttonObj.AddComponent<PieceDragHandler>();
        dragHandler.Initialize(isPlayer1, type, isMountain, this);
        Debug.Log($"UIManualPlacement: Created button for {(isMountain ? "Mountain" : type.ToString())} x{count} for Player {(isPlayer1 ? 1 : 2)}");
    }

    /// <summary>
    /// ������������ ������ ��� �������� ��� ��������������.
    /// </summary>
    /// <param name="position">������� ������.</param>
    /// <param name="isPlayer1">true, ���� ����� 1.</param>
    /// <param name="isMountain">true, ���� ��������������� ����.</param>
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
        var tile = boardManager.GetTileAt(position); // ��������� ����� � BoardManager
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
    /// ������� ��������� ������.
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
    /// ��������� ������ ��� ���� �� �����.
    /// </summary>
    /// <param name="isPlayer1">true, ���� ����� 1.</param>
    /// <param name="position">������� �� �����.</param>
    /// <param name="type">��� ������ (null ��� ���).</param>
    /// <param name="isMountain">true, ���� ����������� ����.</param>
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
    /// ��������� ������ ������� ����� ����������.
    /// </summary>
    private void UpdatePlayerPanels()
    {
        SetupPlayerPanels();
    }

    /// <summary>
    /// ��������� ��������� ������ "���������".
    /// </summary>
    private void UpdateFinishButtons()
    {
        // ������ �������, ������ ���� ��� ������ � ���� ���������
        player1FinishButton.interactable = isPlayer1Turn && placementManager.HasCompletedPlacement(true);
        player2FinishButton.interactable = !isPlayer1Turn && placementManager.HasCompletedPlacement(false);
        Debug.Log($"UIManualPlacement: Finish buttons updated. Player1: {player1FinishButton.interactable}, Player2: {player2FinishButton.interactable}");
    }

    /// <summary>
    /// ���������� ����������� ������� 1.
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
    /// ���������� ����������� ������� 2 � ������ ����.
    /// </summary>
    private void OnPlayer2Finish()
    {
        if (!placementManager.HasCompletedPlacement(false))
        {
            Debug.LogWarning("UIManualPlacement: Player 2 must place all pieces and mountains before finishing!");
            return;
        }

        placementPanel.SetActive(false);
        gameManager.StartGame(placementManager.GetMountainsPerSide, false); // ������ �����������
        Debug.Log("UIManualPlacement: Player 2 finished placement, starting game.");
    }
}

/// <summary>
/// ���������� drag-and-drop ��� ����� � ���.
/// </summary>
public class PieceDragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private bool isPlayer1; // �������������� ������
    private PieceType? type; // ��� ������ (null ��� ���)
    private bool isMountain; // true, ���� ��� ����
    private UIManualPlacement uiManager; // ������ �� UI-��������
    private Vector3Int? lastHighlighted; // ��������� ������������ ������

    /// <summary>
    /// �������������� ���������� drag-and-drop.
    /// </summary>
    /// <param name="isPlayer1">true, ���� ��� ������ 1.</param>
    /// <param name="type">��� ������ (null ��� ���).</param>
    /// <param name="isMountain">true, ���� ��� ����.</param>
    /// <param name="uiManager">������ �� UI-��������.</param>
    public void Initialize(bool isPlayer1, PieceType? type, bool isMountain, UIManualPlacement uiManager)
    {
        this.isPlayer1 = isPlayer1;
        this.type = type;
        this.isMountain = isMountain;
        this.uiManager = uiManager;
        Debug.Log($"PieceDragHandler: Initialized for {(isMountain ? "mountain" : type.ToString())} for Player {(isPlayer1 ? 1 : 2)}");
    }

    /// <summary>
    /// ��������� ������ ��������������.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"PieceDragHandler: Started dragging {(isMountain ? "mountain" : type.ToString())} for Player {(isPlayer1 ? 1 : 2)}");
    }

    /// <summary>
    /// ��������� ��������������.
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
    /// ��������� ���������� ��������������.
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