using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Zenject;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class UIManualPlacement : MonoBehaviour
{
    // �����������, ������������� ����� Zenject
    [Inject] private IGameManager gameManager; // �������� ����
    [Inject] private IBoardManager boardManager; // �������� �����
    [Inject(Id = "Manual")] private IPiecePlacementManager placementManager; // �������� ������ �����������
    [Inject(Id = "Random")] private IPiecePlacementManager randomPlacementManager; // �������� �������������� �����������
    [Inject] private IPieceFactory pieceFactory; // ������� ��� �������� �����
    [Inject] private DiContainer container; // ��������� Zenject ��� �������� �����������

    // ������ �� UI-��������, ������� �������� � ����������
    [SerializeField] private GameObject placementPanel; // ������ �����������
    [SerializeField] private GameObject mainMenuPanel; // ������ �������� ����
    [SerializeField] private RectTransform player1Panel; // ������ ������ 1 ��� ������ �����
    [SerializeField] private RectTransform player2Panel; // ������ ������ 2 ��� ������ �����
    [SerializeField] private Button player1FinishButton; // ������ "��������� �����������" ��� ������ 1
    [SerializeField] private Button player2FinishButton; // ������ "��������� �����������" ��� ������ 2
    [SerializeField] private Button player1RandomButton; // ������ ��������� ��������� ��� ������ 1
    [SerializeField] private Button player2RandomButton; // ������ ��������� ��������� ��� ������ 2
    [SerializeField] private Button startGameButton; // ������ "����� ����"
    [SerializeField] private Button backButton; // ������ "�����"
    [SerializeField] private Slider mountainsSlider; // ������� ��� ������ ���������� ���
    [SerializeField] private Text mountainsValueText; // �����, ������������ ���������� ���
    [SerializeField] private Material highlightMaterial; // �������� ��� ��������� ������
    [SerializeField] private Font buttonFont; // ����� ��� ������ �� �������

    // ���������� ���������
    private bool isPlayer1Turn = true; // ��� ��� ������ (true � ����� 1, false � ����� 2)
    private bool player1Finished = false; // �������� �� ����� 1 �����������
    private bool player2Finished = false; // �������� �� ����� 2 �����������
    private int selectedMountains = 4; // ��������� ���������� ���
    private Vector3Int? highlightedTile; // ������������ ������ (���� ����)
    private Dictionary<Vector3Int, Material> originalTileMaterials = new Dictionary<Vector3Int, Material>(); // �������� ��������� ������

    // ���������� ��� ���������� ��������� (������ reservedPassages �� PiecePlacementManager)
    private List<int> reservedPassagesPlayer1 = new List<int>(); // ����������������� ������� ��� ������ 1
    private List<int> reservedPassagesPlayer2 = new List<int>(); // ����������������� ������� ��� ������ 2

    private void Awake()
    {
        // ���������, ��� ��� UI-�������� ������ � ����������
        if (!placementPanel || !mainMenuPanel || !player1Panel || !player2Panel || !player1FinishButton ||
            !player2FinishButton || !player1RandomButton || !player2RandomButton || !startGameButton ||
            !backButton || !mountainsSlider || !mountainsValueText || !highlightMaterial)
        {
            Debug.LogError("UIManualPlacement: Missing UI elements!");
            return;
        }

        // ��������� ����������� ������� ��� ������
        player1FinishButton.onClick.AddListener(OnPlayer1Finish);
        player2FinishButton.onClick.AddListener(OnPlayer2Finish);
        player1RandomButton.onClick.AddListener(OnPlayer1Random);
        player2RandomButton.onClick.AddListener(OnPlayer2Random);
        startGameButton.onClick.AddListener(OnStartGame);
        backButton.onClick.AddListener(OnBack);

        // ����������� ������� ��� ������ ���������� ���
        mountainsSlider.minValue = 0;
        mountainsSlider.maxValue = 8;
        mountainsSlider.value = selectedMountains;
        mountainsSlider.wholeNumbers = true;
        mountainsSlider.onValueChanged.AddListener(OnMountainsSliderChanged);
        mountainsValueText.text = selectedMountains.ToString();

        // ���������� ������ "��������� �����������" � "����� ����" ���������
        startGameButton.interactable = false;
        player2FinishButton.interactable = false;
    }

    private void OnDestroy()
    {
        // ������� ����������� ������� ��� ����������� �������
        player1FinishButton.onClick.RemoveListener(OnPlayer1Finish);
        player2FinishButton.onClick.RemoveListener(OnPlayer2Finish);
        player1RandomButton.onClick.RemoveListener(OnPlayer1Random);
        player2RandomButton.onClick.RemoveListener(OnPlayer2Random);
        startGameButton.onClick.RemoveListener(OnStartGame);
        backButton.onClick.RemoveListener(OnBack);
        mountainsSlider.onValueChanged.RemoveListener(OnMountainsSliderChanged);
        ClearHighlight(); // ������� ��������� ������
    }

    // ������������� ������ �����������
    public void Initialize(int mountainsPerSide)
    {
        selectedMountains = mountainsPerSide; // ������������� ��������� ���������� ���
        mountainsSlider.value = selectedMountains; // ��������� �������
        mountainsValueText.text = selectedMountains.ToString(); // ��������� �����

        placementManager.Initialize(selectedMountains); // �������������� �������� ������ �����������
        boardManager.InitializeBoard(10); // ������ ����� �������� 10x10
        gameManager.IsInPlacementPhase = true; // ������������� ���� �����������

        SetupPlayerPanels(); // ������ ������ ����� ��� �������
        placementPanel.SetActive(true); // ���������� ������ �����������

        // ���������� ���������
        isPlayer1Turn = true;
        player1Finished = false;
        player2Finished = false;
        player1FinishButton.interactable = false;
        player2FinishButton.interactable = false;
        startGameButton.interactable = false;

        // ������� �������
        reservedPassagesPlayer1.Clear();
        reservedPassagesPlayer2.Clear();
    }

    // ������ ������ ����� ��� ����� �������
    private void SetupPlayerPanels()
    {
        // ������� ��� ������������ ������ �����
        foreach (Transform child in player1Panel) Destroy(child.gameObject);
        foreach (Transform child in player2Panel) Destroy(child.gameObject);

        // ������ ����� ������
        CreatePieceButtons(player1Panel, true);
        CreatePieceButtons(player2Panel, false);
        UpdateFinishButtons(); // ��������� ��������� ������ ����������
    }

    // ������ ������ ��� ����� � ��� �� ��������� ������
    private void CreatePieceButtons(RectTransform panel, bool isPlayer1)
    {
        float yOffset = -20f; // ��������� �������� �� Y ��� ���������������� ������

        // ������� ������ ������ ��� ���
        CreatePieceButton(panel, isPlayer1, PieceType.Mountain, ref yOffset);

        // ����� ������ ������ ��� ��������� ����� �����
        foreach (PieceType type in System.Enum.GetValues(typeof(PieceType)))
        {
            if (type != PieceType.Mountain)
            {
                CreatePieceButton(panel, isPlayer1, type, ref yOffset);
            }
        }
    }

    // ������ ������ ��� ����������� ���� ������ ��� ����
    private void CreatePieceButton(RectTransform panel, bool isPlayer1, PieceType type, ref float yOffset)
    {
        int count = placementManager.GetRemainingCount(isPlayer1, type); // �������� ���������� ���������� �����
        if (count <= 0) return; // ���� ����� ���, ����������

        // ������ ������ ������
        GameObject buttonObj = new GameObject(type.ToString());
        buttonObj.transform.SetParent(panel, false);
        RectTransform rt = buttonObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 50); // ������ ������
        rt.anchoredPosition = new Vector2(0, yOffset); // ������� ������
        yOffset -= 60f; // �������� ��� ��������� ������

        // ��������� ����������� ������
        Image image = buttonObj.AddComponent<Image>();
        image.color = type == PieceType.Mountain ? Color.gray : Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);

        // ��������� ����� �� ������
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.text = $"{type} x{count}"; // ������: "King x1"
        text.font = buttonFont != null ? buttonFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.color = Color.black;
        text.alignment = TextAnchor.MiddleCenter;
        RectTransform textRt = text.GetComponent<RectTransform>();
        textRt.sizeDelta = new Vector2(90, 40);

        // ��������� ��������� ��� ��������������
        PieceDragHandler dragHandler = buttonObj.AddComponent<PieceDragHandler>();
        dragHandler.Initialize(isPlayer1, type, this, pieceFactory);
    }

    // ������������ ������, ���� ��� �������� ��� ���������� ������
    public void HighlightTile(Vector3Int position, bool isPlayer1, PieceType type)
    {
        var piece = boardManager.GetPieceAt(position); // ���������, ���� �� ������ �� ������
        if (piece != null)
        {
            // ���� ������ ����������� ������ � ���� �� ����, ������� ���������
            if (piece.IsPlayer1 == isPlayer1 && piece.Type == type)
            {
                ClearHighlight();
                return;
            }
            isPlayer1 = piece.IsPlayer1; // ���������� ��������� ������ ��� �������� �����������
        }

        // ���������, ����� �� ����������� ������ �� ��� ������
        if (!(placementManager as ManualPlacementManager).CanMove(isPlayer1, type, position))
        {
            ClearHighlight();
            return;
        }

        // ������������ ������
        ClearHighlight();
        highlightedTile = position;
        var tile = boardManager.GetTileAt(position);
        if (tile != null)
        {
            var renderer = tile.GetComponent<Renderer>();
            originalTileMaterials[position] = renderer.material; // ��������� �������� ��������
            renderer.material = highlightMaterial; // ��������� �������� ���������
        }
    }

    // ������� ��������� ������
    public void ClearHighlight()
    {
        if (highlightedTile.HasValue)
        {
            var tile = boardManager.GetTileAt(highlightedTile.Value);
            if (tile != null && originalTileMaterials.ContainsKey(highlightedTile.Value))
            {
                tile.GetComponent<Renderer>().material = originalTileMaterials[highlightedTile.Value]; // ��������������� ��������
            }
            originalTileMaterials.Remove(highlightedTile.Value);
            highlightedTile = null;
        }
    }

    // ��������� ������ ��� ���� �� �����
    public bool PlacePieceOrMountain(bool isPlayer1, Vector3Int position, PieceType type)
    {
        // �������� ���������� ������ ����� placementManager
        bool success = (placementManager as ManualPlacementManager).PlacePieceOrMountain(isPlayer1, position, type);
        if (success)
        {
            var piece = boardManager.GetPieceAt(position); // �������� ����������� ������
            if (piece != null)
            {
                // ��������� ��������� ��� ��������������, ���� ��� ���
                if (!piece.gameObject.GetComponent<BoardPieceDragHandler>())
                {
                    var dragHandler = container.InstantiateComponent<BoardPieceDragHandler>(piece.gameObject);
                    dragHandler.Initialize(this);
                }
            }
            UpdatePlayerPanels(); // ��������� ������ �����
            UpdateFinishButtons(); // ��������� ��������� ������ ����������
        }
        return success;
    }

    // ��������� ������ ����� �������
    public void UpdatePlayerPanels()
    {
        SetupPlayerPanels();
    }

    // ��������� ��������� ������ ���������� �����������
    private void UpdateFinishButtons()
    {
        bool player1Completed = placementManager.HasCompletedPlacement(true); // ���������, �������� �� ����� 1
        bool player2Completed = placementManager.HasCompletedPlacement(false); // ���������, �������� �� ����� 2
        player1FinishButton.interactable = isPlayer1Turn && player1Completed && !player1Finished;
        player2FinishButton.interactable = !isPlayer1Turn && player2Completed && !player2Finished;
        startGameButton.interactable = player1Finished && player2Finished; // ���������� "����� ����" ����� ����� �������
    }

    // ���������� ������� ������ "��������� �����������" ��� ������ 1
    private void OnPlayer1Finish()
    {
        if (!placementManager.HasCompletedPlacement(true))
        {
            Debug.LogWarning("UIManualPlacement: Player 1 must place all pieces!");
            return;
        }

        player1Finished = true; // ��������, ��� ����� 1 ��������
        isPlayer1Turn = false; // ������� ��� ������ 2
        player1FinishButton.interactable = false;
        UpdateFinishButtons();
    }

    // ���������� ������� ������ "��������� �����������" ��� ������ 2
    private void OnPlayer2Finish()
    {
        if (!placementManager.HasCompletedPlacement(false))
        {
            Debug.LogWarning("UIManualPlacement: Player 2 must place all pieces!");
            return;
        }

        player2Finished = true; // ��������, ��� ����� 2 ��������
        player2FinishButton.interactable = false;
        UpdateFinishButtons();
    }

    // ���������� ������� ������ ��������� ��������� ��� ������ 1
    private void OnPlayer1Random()
    {
        // ������� ������� ������ � ���� ������ 1
        var pieces = boardManager.GetAllPieces();
        foreach (var piece in pieces)
        {
            if (piece.Value.IsPlayer1)
            {
                boardManager.RemovePiece(piece.Key);
            }
        }

        // ���������� ������� ��� ������ 1
        reservedPassagesPlayer1.Clear();
        
        // ����������� ���� � ������ ��� ������ 1
        randomPlacementManager.PlacePiecesForPlayer(true, selectedMountains);

        // ��������� drag handlers � �������� �������� � placementManager
        foreach (var piece in boardManager.GetAllPieces())
        {
            if (piece.Value.IsPlayer1)
            {
                // ��������� ������� � placementManager, ��� ���� �� ������ ���� ��������� �������
                (placementManager as ManualPlacementManager).PlacePieceOrMountain(true, piece.Key, piece.Value.Type, true);
                if (!piece.Value.gameObject.GetComponent<BoardPieceDragHandler>())
                {
                    var dragHandler = container.InstantiateComponent<BoardPieceDragHandler>(piece.Value.gameObject);
                    dragHandler.Initialize(this);
                }
            }
        }

        // ������� ������ ����� ������ 1
        foreach (Transform child in player1Panel) Destroy(child.gameObject);
        UpdateFinishButtons();
    }

    // ���������� ������� ������ ��������� ��������� ��� ������ 2
    private void OnPlayer2Random()
    {
        // ������� ������� ������ � ���� ������ 2
        var pieces = boardManager.GetAllPieces();
        foreach (var piece in pieces)
        {
            if (!piece.Value.IsPlayer1)
            {
                boardManager.RemovePiece(piece.Key);
            }
        }

        // ���������� ������� ��� ������ 2
        reservedPassagesPlayer2.Clear();        

        // ����������� ���� � ������ ��� ������ 2
        randomPlacementManager.PlacePiecesForPlayer(false, selectedMountains);

        // ��������� drag handlers � �������� �������� � placementManager
        foreach (var piece in boardManager.GetAllPieces())
        {
            if (!piece.Value.IsPlayer1)
            {
                // ��������� ������� � placementManager, ��� ���� �� ������ ���� ��������� �������
                (placementManager as ManualPlacementManager).PlacePieceOrMountain(false, piece.Key, piece.Value.Type, true);
                if (!piece.Value.gameObject.GetComponent<BoardPieceDragHandler>())
                {
                    var dragHandler = container.InstantiateComponent<BoardPieceDragHandler>(piece.Value.gameObject);
                    dragHandler.Initialize(this);
                }
            }
        }

        // ������� ������ ����� ������ 2
        foreach (Transform child in player2Panel) Destroy(child.gameObject);
        UpdateFinishButtons();
    }  

    // ���������� ������� ������ "����� ����"
    private void OnStartGame()
    {
        if (!player1Finished || !player2Finished)
        {
            Debug.LogWarning("UIManualPlacement: Both players must finish placement!");
            return;
        }

        placementPanel.SetActive(false); // �������� ������ �����������
        gameManager.StartGame(selectedMountains, false); // ��������� ����
    }

    // ���������� ������� ������ "�����"
    private void OnBack()
    {
        // ������� ����� � ��� ������
        var pieces = boardManager.GetAllPieces();
        foreach (var piece in pieces)
        {
            boardManager.RemovePiece(piece.Key);
        }
        placementManager.Initialize(selectedMountains); // ���������� placementManager
        boardManager.InitializeBoard(10); // ���������� �����
        ClearHighlight(); // ������� ��������� ������

        // ���������� ���������
        player1Finished = false;
        player2Finished = false;
        isPlayer1Turn = true;
        reservedPassagesPlayer1.Clear();
        reservedPassagesPlayer2.Clear();

        placementPanel.SetActive(false); // �������� ������ �����������
        mainMenuPanel.SetActive(true); // ���������� ������� ����
    }

    // ���������� ��������� �������� �������� ���������� ���
    private void OnMountainsSliderChanged(float value)
    {
        selectedMountains = Mathf.FloorToInt(value); // ��������� ���������� ���
        mountainsValueText.text = selectedMountains.ToString(); // ��������� �����
        placementManager.Initialize(selectedMountains); // ������������������ placementManager
        // ���������� ����� � ������
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