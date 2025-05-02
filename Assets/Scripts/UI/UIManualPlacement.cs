using UnityEngine;
using UnityEngine.UI;
using Zenject;
using System.Collections.Generic;

public class UIManualPlacement : MonoBehaviour
{
    [Inject] private IGameManager gameManager;
    [Inject] private IBoardManager boardManager;
    [Inject(Id = "Manual")] private IPiecePlacementManager placementManager;
    [Inject(Id = "Random")] private IPiecePlacementManager randomPlacementManager;
    [Inject] private IPieceFactory pieceFactory;
    [Inject] private DiContainer container;

    [SerializeField] private GameObject placementPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private RectTransform player1Panel;
    [SerializeField] private RectTransform player2Panel;
    [SerializeField] private Button player1FinishButton;
    [SerializeField] private Button player2FinishButton;
    [SerializeField] private Button player1RandomButton;
    [SerializeField] private Button player2RandomButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Slider mountainsSlider;
    [SerializeField] private Text mountainsValueText;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Font buttonFont;
    [SerializeField] private UIGameManager uiGameManager;

    private bool isPlayer1Turn = true;
    private bool player1Finished = false;
    private bool player2Finished = false;
    private int selectedMountains = 4;
    private Vector3Int? highlightedTile;
    private Dictionary<Vector3Int, Material> originalTileMaterials = new Dictionary<Vector3Int, Material>();

    private void Awake()
    {
        if (placementPanel == null || mainMenuPanel == null || player1Panel == null || player2Panel == null ||
            player1FinishButton == null || player2FinishButton == null || player1RandomButton == null ||
            player2RandomButton == null || startGameButton == null || backButton == null ||
            mountainsSlider == null || mountainsValueText == null || highlightMaterial == null)
        {
            Debug.LogError("UIManualPlacement: Missing UI elements!");
            return;
        }

        player1FinishButton.onClick.AddListener(OnPlayer1Finish);
        player2FinishButton.onClick.AddListener(OnPlayer2Finish);
        player1RandomButton.onClick.AddListener(OnPlayer1Random);
        player2RandomButton.onClick.AddListener(OnPlayer2Random);
        startGameButton.onClick.AddListener(OnStartGame);
        backButton.onClick.AddListener(OnBack);

        mountainsSlider.minValue = 0;
        mountainsSlider.maxValue = 8;
        mountainsSlider.wholeNumbers = true;
        mountainsSlider.onValueChanged.AddListener(OnMountainsSliderChanged);
        mountainsSlider.value = selectedMountains;
        mountainsValueText.text = selectedMountains.ToString();

        startGameButton.interactable = false;
        UpdatePlayerPanelsAndButtons();
    }

    private void OnDestroy()
    {
        player1FinishButton.onClick.RemoveListener(OnPlayer1Finish);
        player2FinishButton.onClick.RemoveListener(OnPlayer2Finish);
        player1RandomButton.onClick.RemoveListener(OnPlayer1Random);
        player2RandomButton.onClick.RemoveListener(OnPlayer2Random);
        startGameButton.onClick.RemoveListener(OnStartGame);
        backButton.onClick.RemoveListener(OnBack);
        mountainsSlider.onValueChanged.RemoveListener(OnMountainsSliderChanged);
        ClearHighlight();
    }

    public void Initialize(int mountainsPerSide)
    {
        selectedMountains = mountainsPerSide;
        mountainsSlider.value = selectedMountains;
        mountainsValueText.text = selectedMountains.ToString();

        placementManager.Initialize(selectedMountains);
        boardManager.InitializeBoard(10);
        gameManager.IsInPlacementPhase = true;

        SetupPlayerPanels();
        placementPanel.SetActive(true);

        isPlayer1Turn = true;
        player1Finished = false;
        player2Finished = false;
        UpdatePlayerPanelsAndButtons();
    }

    public int GetSelectedMountains()
    {
        return selectedMountains;
    }

    private void SetupPlayerPanels()
    {
        foreach (Transform child in player1Panel) Destroy(child.gameObject);
        foreach (Transform child in player2Panel) Destroy(child.gameObject);

        CreatePieceButtons(player1Panel, true);
        CreatePieceButtons(player2Panel, false);
        UpdatePlayerPanelsAndButtons();
    }

    private void CreatePieceButtons(RectTransform panel, bool isPlayer1)
    {
        float yOffset = -20f;

        // Создаём кнопку для гор
        CreatePieceButton(panel, isPlayer1, PieceType.Mountain, ref yOffset);

        // Создаём кнопки для всех фигур
        PieceType[] pieceTypes = new[]
        {
            PieceType.King,
            PieceType.Dragon,
            PieceType.Elephant,
            PieceType.HeavyCavalry,
            PieceType.LightHorse,
            PieceType.Spearman,
            PieceType.Crossbowman,
            PieceType.Rabble,
            PieceType.Catapult,
            PieceType.Trebuchet,
            PieceType.Swordsman,
            PieceType.Archer
        };

        foreach (PieceType type in pieceTypes)
        {
            CreatePieceButton(panel, isPlayer1, type, ref yOffset);
        }
    }

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

    public void HighlightTile(Vector3Int position, bool isPlayer1, PieceType type)
    {
        var piece = boardManager.GetPieceAt(position);
        if (piece != null)
        {
            if (piece.IsPlayer1 == isPlayer1 && piece.Type == type)
            {
                ClearHighlight();
                return;
            }
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
            UpdatePlayerPanelsAndButtons();
        }
        return success;
    }

    public void UpdatePlayerPanels()
    {
        SetupPlayerPanels();
    }

    private void UpdatePlayerPanelsAndButtons()
    {
        bool player1Completed = placementManager.HasCompletedPlacement(true);
        bool player2Completed = placementManager.HasCompletedPlacement(false);

        player1Panel.gameObject.SetActive(isPlayer1Turn && !player1Finished);
        player2Panel.gameObject.SetActive(!isPlayer1Turn && !player2Finished);

        player1FinishButton.interactable = isPlayer1Turn && player1Completed && !player1Finished;
        player2FinishButton.interactable = !isPlayer1Turn && player2Completed && !player2Finished;
        player1RandomButton.interactable = isPlayer1Turn && !player1Finished;
        player2RandomButton.interactable = !isPlayer1Turn && !player2Finished;
        startGameButton.interactable = player1Finished && player2Finished;
    }

    private void OnPlayer1Finish()
    {
        if (!placementManager.HasCompletedPlacement(true))
        {
            Debug.LogWarning("UIManualPlacement: Player 1 must place all pieces!");
            return;
        }

        player1Finished = true;
        isPlayer1Turn = false;
        UpdatePlayerPanelsAndButtons();
    }

    private void OnPlayer2Finish()
    {
        if (!placementManager.HasCompletedPlacement(false))
        {
            Debug.LogWarning("UIManualPlacement: Player 2 must place all pieces!");
            return;
        }

        player2Finished = true;
        isPlayer1Turn = true;
        UpdatePlayerPanelsAndButtons();
    }

    private void OnPlayer1Random()
    {
        var pieces = boardManager.GetAllPieces();
        foreach (var piece in pieces)
        {
            if (piece.Value.IsPlayer1)
            {
                boardManager.RemovePiece(piece.Key);
            }
        }

        randomPlacementManager.PlacePiecesForPlayer(true, selectedMountains);

        foreach (var piece in boardManager.GetAllPieces())
        {
            if (piece.Value.IsPlayer1)
            {
                (placementManager as ManualPlacementManager).DecreasePieceCount(true, piece.Value.Type);
                if (!piece.Value.gameObject.GetComponent<BoardPieceDragHandler>())
                {
                    var dragHandler = container.InstantiateComponent<BoardPieceDragHandler>(piece.Value.gameObject);
                    dragHandler.Initialize(this);
                }
            }
        }

        foreach (Transform child in player1Panel) Destroy(child.gameObject);
        UpdatePlayerPanelsAndButtons();
    }

    private void OnPlayer2Random()
    {
        var pieces = boardManager.GetAllPieces();
        foreach (var piece in pieces)
        {
            if (!piece.Value.IsPlayer1)
            {
                boardManager.RemovePiece(piece.Key);
            }
        }

        randomPlacementManager.PlacePiecesForPlayer(false, selectedMountains);

        foreach (var piece in boardManager.GetAllPieces())
        {
            if (!piece.Value.IsPlayer1)
            {
                (placementManager as ManualPlacementManager).DecreasePieceCount(false, piece.Value.Type);
                if (!piece.Value.gameObject.GetComponent<BoardPieceDragHandler>())
                {
                    var dragHandler = container.InstantiateComponent<BoardPieceDragHandler>(piece.Value.gameObject);
                    dragHandler.Initialize(this);
                }
            }
        }

        foreach (Transform child in player2Panel) Destroy(child.gameObject);
        UpdatePlayerPanelsAndButtons();
    }

    private void OnStartGame()
    {
        if (!player1Finished || !player2Finished)
        {
            Debug.LogWarning("UIManualPlacement: Both players must finish placement!");
            return;
        }

        placementPanel.SetActive(false);
        gameManager.StartGame(selectedMountains, false);
        uiGameManager.Initialize();
    }

    private void OnBack()
    {
        var pieces = boardManager.GetAllPieces();
        foreach (var piece in pieces)
        {
            boardManager.RemovePiece(piece.Key);
        }
        placementManager.Initialize(selectedMountains);
        boardManager.InitializeBoard(10);
        ClearHighlight();

        player1Finished = false;
        player2Finished = false;
        isPlayer1Turn = true;

        placementPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        UpdatePlayerPanelsAndButtons();
    }

    private void OnMountainsSliderChanged(float value)
    {
        selectedMountains = Mathf.FloorToInt(value);
        mountainsValueText.text = selectedMountains.ToString();
        placementManager.Initialize(selectedMountains);

        var pieces = boardManager.GetAllPieces();
        foreach (var piece in pieces)
        {
            boardManager.RemovePiece(piece.Key);
        }

        player1Finished = false;
        player2Finished = false;
        isPlayer1Turn = true;
        SetupPlayerPanels();
        UpdatePlayerPanelsAndButtons();
    }
}