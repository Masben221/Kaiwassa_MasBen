using UnityEngine;
using UnityEngine.UI;
using Zenject;

/// <summary>
/// ��������� UI �������� ��������, ������� ������ ����, ����������� �������� ���� � ������ "�����".
/// </summary>
public class UIGameManager : MonoBehaviour
{
    // �����������, ������������� ����� Zenject (�� GameInstaller)
    [Inject] private IGameManager gameManager; // �������� ���� ��� ���������� ������ � ��������� ������� ����� ����
    [Inject] private IBoardManager boardManager; // �������� ����� ��� ������� �����

    // ��������������� ���� ��� UI-�����������, �������� � ����������
    [SerializeField] private GameObject gamePanel; // ������ �������� ��������
    [SerializeField] private Button backButton; // ������ "�����"
    [SerializeField] private UIManualPlacement uiManualPlacement; // ������ ����������� ��� �������� � ����
    [SerializeField] private Text currentTurnText; // ����� ��� ����������� �������� ���� (��������, "��� ������ 1")

    private void Awake()
    {
        // ���������, ��� ��� ����������� ���������� ������ � ����������
        if (gamePanel == null)
        {
            Debug.LogError("UIGameManager: GamePanel is not assigned in the inspector!");
            return;
        }

        if (backButton == null)
        {
            Debug.LogError("UIGameManager: BackButton is not assigned in the inspector!");
            return;
        }

        if (uiManualPlacement == null)
        {
            Debug.LogError("UIGameManager: UIManualPlacement is not assigned in the inspector!");
            return;
        }

        if (currentTurnText == null)
        {
            Debug.LogError("UIGameManager: CurrentTurnText is not assigned in the inspector!");
            return;
        }

        // ��������� ���������� ��� ������ "�����"
        backButton.onClick.AddListener(OnBack);

        // �������������� ����� ������ ��� ����������� �������� ����
        SetupTurnTextStyle();
    }

    private void OnDestroy()
    {
        // ������� ���������� ������ ��� ����������� �������
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBack);
        }

        // ������������ �� ������� ����� ����, ����� �������� ������ ������
        if (gameManager != null)
        {
            gameManager.OnTurnChanged -= UpdateTurnText;
        }
    }

    /// <summary>
    /// �������������� UI �������� ��������, ��������� ������ ���� � ������������ �� ������� ����� ����.
    /// </summary>
    public void Initialize()
    {
        gamePanel.SetActive(true); // ���������� ������ �������� ��������

        // ������������� �� ������� ����� ���� �� GameManager
        gameManager.OnTurnChanged += UpdateTurnText;

        // ������������� ��������� ����� ��� �������� ����
        UpdateTurnText(gameManager.IsPlayer1Turn);

        Debug.Log("UIGameManager: Game UI initialized.");
    }

    /// <summary>
    /// ����������� ����� ������ ��� ����������� �������� ����.
    /// </summary>
    private void SetupTurnTextStyle()
    {
        // ������������� ����� (���� �� �����, ���������� ����������� ����� Unity)
        currentTurnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // ������������� ������ ������
        currentTurnText.fontSize = 28;

        // ������������� ������������ ������ �� ������
        currentTurnText.alignment = TextAnchor.MiddleCenter;

        // ������������� ��������� ���� ������ (���������� ��� �������)
        currentTurnText.color = new Color(1f, 0.84f, 0f); // ������� ���� (#FFD700)

        // ��������� ������� ��� ������ ����������
        var outline = currentTurnText.gameObject.GetComponent<Outline>();
        if (outline == null)
        {
            outline = currentTurnText.gameObject.AddComponent<Outline>();
        }
        outline.effectColor = Color.black; // ׸���� �������
        outline.effectDistance = new Vector2(1, -1); // ������ �������

        // ��������� ���� ��� ��������������� �������
        var shadow = currentTurnText.gameObject.GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = currentTurnText.gameObject.AddComponent<Shadow>();
        }
        shadow.effectColor = new Color(0, 0, 0, 0.5f); // �������������� ������ ����
        shadow.effectDistance = new Vector2(1, -1); // �������� ����
    }

    /// <summary>
    /// ��������� ����� �������� ���� �� ������ ����, ��� ���.
    /// </summary>
    /// <param name="isPlayer1">True, ���� ��� ������ 1, ����� ������ 2.</param>
    private void UpdateTurnText(bool isPlayer1)
    {
        // ��������� ����� � ����������� �� �������� ������
        currentTurnText.text = isPlayer1 ? "��� ������ 1" : "��� ������ 2";

        // ������ ���� ������ ��� ����������� (����� ��� ������ 1, ������� ��� ������ 2)
        currentTurnText.color = isPlayer1 ? new Color(0.1f, 0.5f, 1f) : new Color(1f, 0.3f, 0.3f);

        Debug.Log($"UIGameManager: Updated turn text to '{currentTurnText.text}'.");
    }

    /// <summary>
    /// ���������� ������ "�����".
    /// ������� �����, ���������� ��������� � ���������� � ���� �����������.
    /// </summary>
    private void OnBack()
    {
        // ������� ��� ������ �� �����
        var pieces = boardManager.GetAllPieces();
        foreach (var piece in pieces)
        {
            boardManager.RemovePiece(piece.Key);
            Object.Destroy(piece.Value.gameObject); // ���������� GameObject �����
        }

        // ����������� ���� �� �����������
        gameManager.IsInPlacementPhase = true;

        // �������� ������ �������� ��������
        gamePanel.SetActive(false);

        // ���������� ������ ����������� � ���������� � ���������
        uiManualPlacement.Initialize(uiManualPlacement.GetSelectedMountains());
        Debug.Log("UIGameManager: Returned to placement menu, board reset.");
    }
}