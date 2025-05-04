using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Zenject;

/// <summary>
/// Перетаскивание фигур и гор на доске во время фазы расстановки.
/// </summary>
public class BoardPieceDragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Inject] private IBoardManager boardManager;
    [Inject(Id = "Manual")] private IPiecePlacementManager placementManager;
    [Inject] private IGameManager gameManager;

    private Piece piece;
    private UIManualPlacement uiManager;
    private Vector3Int originalPosition;
    private Material originalMaterial;
    private Vector3Int? lastHighlighted;
    private bool isDragging;

    [Inject]
    private void InjectDependencies(IBoardManager boardManager, [Inject(Id = "Manual")] IPiecePlacementManager placementManager, IGameManager gameManager)
    {
        this.boardManager = boardManager;
        this.placementManager = placementManager;
        this.gameManager = gameManager;
    }

    /// <summary>
    /// Инициализирует обработчик перетаскивания.
    /// </summary>
    /// <param name="uiManager">UI-менеджер для взаимодействия.</param>
    public void Initialize(UIManualPlacement uiManager = null)
    {
        this.uiManager = uiManager;
        piece = GetComponent<Piece>();
        if (piece == null)
        {
            Debug.LogError($"BoardPieceDragHandler: No Piece on {gameObject.name}.");
            Destroy(this);
            return;
        }
        originalPosition = piece.Position;
    }

    /// <summary>
    /// Запускает процесс перетаскивания фигуры или горы.
    /// </summary>
    public void StartDragging()
    {
        // Проверяем, находится ли игра в фазе расстановки
        if (gameManager == null || !gameManager.IsInPlacementPhase)
        {
            Debug.LogWarning($"BoardPieceDragHandler: Cannot drag {piece?.Type}, not in placement phase");
            return;
        }

        // Проверяем, завершил ли игрок расстановку
        if (uiManager != null)
        {
            bool isPlayer1 = piece.IsPlayer1;
            bool isFinished = isPlayer1 ? uiManager.IsPlayer1Finished : uiManager.IsPlayer2Finished;
            if (isFinished)
            {
                Debug.LogWarning($"BoardPieceDragHandler: Cannot drag {piece.Type} for Player {(isPlayer1 ? 1 : 2)} - placement finished.");
                return;
            }
        }

        isDragging = true;

        var collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;

        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            originalMaterial = renderer.material;
            Material transparentMat = new Material(Shader.Find("Standard"));
            transparentMat.SetFloat("_Mode", 3);
            transparentMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            transparentMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            transparentMat.SetInt("_ZWrite", 0);
            transparentMat.DisableKeyword("_ALPHATEST_ON");
            transparentMat.EnableKeyword("_ALPHABLEND_ON");
            transparentMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            transparentMat.renderQueue = 3000;
            Color color = originalMaterial.color;
            color.a = 0.5f;
            transparentMat.color = color;
            renderer.material = transparentMat;
        }

        transform.DORotate(new Vector3(0, 360, 0), 2f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear);
    }

    public void OnPointerDown(PointerEventData eventData) { }

    /// <summary>
    /// Обрабатывает перетаскивание, обновляя позицию и подсветку клеток.
    /// </summary>
    /// <param name="eventData">Данные события указателя.</param>
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        int boardLayerMask = LayerMask.GetMask("Board");
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, boardLayerMask))
        {
            Vector3Int position = new Vector3Int(
                Mathf.FloorToInt(hit.point.x + 0.5f),
                0,
                Mathf.FloorToInt(hit.point.z + 0.5f)
            );

            if (position != originalPosition && uiManager != null)
            {
                uiManager.HighlightTile(position, piece.IsPlayer1, piece.Type);
                lastHighlighted = position;
            }
            else
            {
                uiManager?.ClearHighlight();
                lastHighlighted = null;
            }

            transform.position = new Vector3(position.x, 0.5f, position.z);
        }
        else
        {
            uiManager?.ClearHighlight();
            lastHighlighted = null;

            Ray cursorRay = Camera.main.ScreenPointToRay(eventData.position);
            Plane boardPlane = new Plane(Vector3.up, new Vector3(0, 0.5f, 0));
            if (boardPlane.Raycast(cursorRay, out float distance))
            {
                Vector3 worldPoint = cursorRay.GetPoint(distance);
                transform.position = new Vector3(worldPoint.x, 0.5f, worldPoint.z);
            }
        }
    }

    /// <summary>
    /// Обрабатывает отпускание мыши, размещая или возвращая фигуру.
    /// </summary>
    /// <param name="eventData">Данные события указателя.</param>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging) return;

        isDragging = false;
        transform.DOKill();

        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null && originalMaterial != null)
            renderer.material = originalMaterial;

        var collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = true;

        if (lastHighlighted.HasValue && lastHighlighted.Value != originalPosition && uiManager != null)
        {
            bool success = (placementManager as ManualPlacementManager).MovePiece(piece, originalPosition, lastHighlighted.Value);
            if (success)
            {
                originalPosition = lastHighlighted.Value;
                // Устанавливаем поворот лицом к противнику (0° для игрока 1, 180° для игрока 2, горы не поворачиваются)
                if (piece.Type != PieceType.Mountain)
                {
                    Quaternion targetRotation = piece.IsPlayer1 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
                    transform.rotation = targetRotation;
                    Debug.Log($"BoardPieceDragHandler: Rotated {piece.Type} at {lastHighlighted.Value} to {targetRotation.eulerAngles} for Player {(piece.IsPlayer1 ? 1 : 2)}");
                }
            }
            else
            {
                piece.SetPosition(originalPosition);
                transform.position = new Vector3(originalPosition.x, 0.5f, originalPosition.z);
            }
        }
        else if (!lastHighlighted.HasValue && uiManager != null)
        {
            bool success = (placementManager as ManualPlacementManager).RemovePiece(piece);
            if (success)
                uiManager.UpdatePlayerPanels();
            else
            {
                piece.SetPosition(originalPosition);
                transform.position = new Vector3(originalPosition.x, 0.5f, originalPosition.z);
            }
        }
        else
        {
            piece.SetPosition(originalPosition);
            transform.position = new Vector3(originalPosition.x, 0.5f, originalPosition.z);
        }

        uiManager?.ClearHighlight();
        lastHighlighted = null;
    }
}