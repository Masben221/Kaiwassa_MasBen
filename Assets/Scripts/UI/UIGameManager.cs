using UnityEngine;
using UnityEngine.UI;
using Zenject;
using DG.Tweening;

public class UIGameManager : MonoBehaviour
{
    [Inject] private IGameManager gameManager;
    [Inject] private IBoardManager boardManager;
    [Inject] private InputHandler inputHandler;

    [SerializeField] private GameObject gamePanel;
    [SerializeField] private Button backButton;
    [SerializeField] private UIManualPlacement uiManualPlacement;
    [SerializeField] private Text currentTurnText;
    [SerializeField] private Button hintButtonPlayer1;
    [SerializeField] private Button hintButtonPlayer2;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private Text victoryText;
    [SerializeField] private Button returnToMenuButton;

    private void Awake()
    {
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
        if (hintButtonPlayer1 == null)
        {
            Debug.LogError("UIGameManager: HintButtonPlayer1 is not assigned in the inspector!");
            return;
        }
        if (hintButtonPlayer2 == null)
        {
            Debug.LogError("UIGameManager: HintButtonPlayer2 is not assigned in the inspector!");
            return;
        }
        if (victoryPanel == null)
        {
            Debug.LogError("UIGameManager: VictoryPanel is not assigned in the inspector!");
            return;
        }
        if (victoryText == null)
        {
            Debug.LogError("UIGameManager: VictoryText is not assigned in the inspector!");
            return;
        }
        if (returnToMenuButton == null)
        {
            Debug.LogError("UIGameManager: ReturnToMenuButton is not assigned in the inspector!");
            return;
        }

        backButton.onClick.AddListener(OnBack);
        hintButtonPlayer1.onClick.AddListener(() => OnHintButtonPressed(true));
        hintButtonPlayer2.onClick.AddListener(() => OnHintButtonPressed(false));
        returnToMenuButton.onClick.AddListener(OnReturnToMenu);

        victoryPanel.SetActive(false);
        returnToMenuButton.gameObject.SetActive(true);

        SetupTurnTextStyle();
    }

    private void OnDestroy()
    {
        if (backButton != null)
            backButton.onClick.RemoveListener(OnBack);
        if (hintButtonPlayer1 != null)
            hintButtonPlayer1.onClick.RemoveListener(() => OnHintButtonPressed(true));
        if (hintButtonPlayer2 != null)
            hintButtonPlayer2.onClick.RemoveListener(() => OnHintButtonPressed(false));
        if (returnToMenuButton != null)
            returnToMenuButton.onClick.RemoveListener(OnReturnToMenu);

        if (gameManager != null)
        {
            gameManager.OnTurnChanged -= UpdateTurnText;
            gameManager.OnGameEnded -= DisplayGameResult;
        }

        DOTween.Kill(currentTurnText);
        DOTween.Kill(victoryText);
        DOTween.Kill(victoryPanel);
    }

    public void Initialize()
    {
        gamePanel.SetActive(true);
        victoryPanel.SetActive(false);
        gameManager.OnTurnChanged += UpdateTurnText;
        gameManager.OnGameEnded += DisplayGameResult;
        UpdateTurnText(gameManager.IsPlayer1Turn);
        hintButtonPlayer1.interactable = !gameManager.IsInPlacementPhase;
        hintButtonPlayer2.interactable = !gameManager.IsInPlacementPhase;
        Debug.Log("UIGameManager: Game UI initialized, subscribed to OnGameEnded.");
    }

    private void SetupTurnTextStyle()
    {
        currentTurnText.alignment = TextAnchor.MiddleCenter;
        var outline = currentTurnText.gameObject.GetComponent<Outline>();
        if (outline == null)
            outline = currentTurnText.gameObject.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);

        var shadow = currentTurnText.gameObject.GetComponent<Shadow>();
        if (shadow == null)
            shadow = currentTurnText.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.5f);
        shadow.effectDistance = new Vector2(1, -1);
    }

    private void UpdateTurnText(bool isPlayer1)
    {
        if (currentTurnText == null)
        {
            Debug.LogError("UIGameManager: currentTurnText is null in UpdateTurnText!");
            return;
        }

        DOTween.Kill(currentTurnText);
        currentTurnText.DOFade(0f, 0.3f).OnComplete(() =>
        {
            currentTurnText.text = isPlayer1 ? "Ход игрока 1" : "Ход игрока 2";
            currentTurnText.color = isPlayer1 ? new Color(1f, 0.84f, 0f) : new Color(1f, 0.3f, 0.3f);
            currentTurnText.transform.localScale = Vector3.one;
            currentTurnText.DOFade(1f, 0.3f);
            currentTurnText.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                currentTurnText.transform.DOScale(1f, 0.15f).SetEase(Ease.InOutQuad);
            });
        });
        Debug.Log($"UIGameManager: Updated turn text to '{currentTurnText.text}' with animation.");
    }

    private void DisplayGameResult(bool isPlayer1Winner)
    {
        if (victoryText == null || victoryPanel == null)
        {
            Debug.LogError("UIGameManager: victoryText or victoryPanel is null in DisplayGameResult!");
            return;
        }

        Debug.Log($"UIGameManager: Displaying game result: Player {(isPlayer1Winner ? 1 : 2)} wins!");

        // Деактивируем кнопки
        hintButtonPlayer1.interactable = false;
        hintButtonPlayer2.interactable = false;
        backButton.interactable = false;

        // Активируем панель победы
        victoryPanel.SetActive(true);

        // Настраиваем текст победы
        victoryText.text = isPlayer1Winner ? "Игрок 1 победил!" : "Игрок 2 победил!";
        Debug.Log($"UIGameManager: Set victoryText.text to '{victoryText.text}'");
        victoryText.color = isPlayer1Winner ? new Color(1f, 0.84f, 0f) : new Color(1f, 0.3f, 0.3f);
        victoryText.fontSize = 40;

        // Настраиваем начальную видимость текста
        victoryText.canvasRenderer.SetAlpha(1f); // Устанавливаем альфа на 1, чтобы текст был виден

        // Настраиваем фон панели
        var panelImage = victoryPanel.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.color = new Color(1, 1, 1, 1); // Прозрачный фон изначально
        }

        // Анимация появления панели
        DOTween.Kill(victoryPanel);
        DOTween.Kill(victoryText);
        victoryPanel.transform.localScale = Vector3.one * 0.5f; // Начальный масштаб панели

        if (panelImage != null)
        {
            panelImage.DOFade(0.7f, 0.5f); // Затемнение фона
        }

        // Анимация масштабирования всей панели
        victoryPanel.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            // Дополнительная анимация текста (пульсация)
            victoryText.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                victoryText.transform.DOScale(1f, 0.3f).SetEase(Ease.InOutQuad);
            });

            // Эффект свечения текста через изменение цвета
            var baseColor = victoryText.color;
            victoryText.DOColor(baseColor * 1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        });
    }

    private void OnBack()
    {
        var pieces = boardManager.GetAllPieces();
        foreach (var piece in pieces)
        {
            boardManager.RemovePiece(piece.Key);
            Object.Destroy(piece.Value.gameObject);
        }

        gameManager.IsInPlacementPhase = true;
        gamePanel.SetActive(false);
        victoryPanel.SetActive(false);
        uiManualPlacement.Initialize(uiManualPlacement.GetSelectedMountains());
        Debug.Log("UIGameManager: Returned to placement menu, board reset.");
    }

    private void OnReturnToMenu()
    {
        var pieces = boardManager.GetAllPieces();
        foreach (var piece in pieces)
        {
            boardManager.RemovePiece(piece.Key);
            Object.Destroy(piece.Value.gameObject);
        }

        gameManager.IsInPlacementPhase = true;
        gamePanel.SetActive(false);
        victoryPanel.SetActive(false);
        uiManualPlacement.Initialize(uiManualPlacement.GetSelectedMountains());
        Debug.Log("UIGameManager: Returned to placement menu after game end.");
    }

    private void OnHintButtonPressed(bool isPlayer1)
    {
        if (gameManager.IsInPlacementPhase)
        {
            Debug.LogWarning("UIGameManager: Hint buttons are disabled during placement phase.");
            return;
        }

        inputHandler.ShowAllPotentialAttackTiles(isPlayer1);
        Debug.Log($"UIGameManager: Hint button pressed for Player {(isPlayer1 ? 1 : 2)} to show all potential attack tiles.");
    }
}