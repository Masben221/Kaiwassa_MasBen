using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Zenject;

/// <summary>
/// Компонент для перетаскивания уже размещённых фигур или гор во время фазы расстановки.
/// Добавляется на фигуру/гору при её размещении на доске.
/// Реализует перетаскивание на другую клетку или возврат в список при отпускании вне доски.
/// </summary>
public class BoardPieceDragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Inject] private UIManualPlacement uiManager; // UI для подсветки и размещения
    [Inject] private IBoardManager boardManager; // Для проверки позиций и удаления
    [Inject(Id = "Manual")] private IPiecePlacementManager placementManager; // Для проверки и возврата
    [Inject] private IGameManager gameManager; // Для проверки фазы расстановки

    private Piece piece; // Ссылка на компонент Piece (фигура или гора)
    private Vector3Int originalPosition; // Исходная позиция
    private Material originalMaterial; // Исходный материал
    private Vector3Int? lastHighlighted; // Последняя подсвеченная клетка
    private bool isDragging; // Флаг перетаскивания

    /// <summary>
    /// Инициализирует компонент с ссылкой на фигуру/гору.
    /// </summary>
    public void Initialize()
    {
        piece = GetComponent<Piece>();
        if (piece == null)
        {
            Debug.LogError($"BoardPieceDragHandler: No Piece component found on {gameObject.name}");
            Destroy(this);
        }
        originalPosition = piece.Position;
        Debug.Log($"BoardPieceDragHandler: Initialized for {piece.Type} at {originalPosition}");
    }

    /// <summary>
    /// Начинает перетаскивание при клике.
    /// </summary>
    public void StartDragging()
    {
        if (!gameManager.IsInPlacementPhase)
        {
            Debug.LogWarning($"BoardPieceDragHandler: Cannot drag {piece.Type} at {piece.Position}, not in placement phase");
            return;
        }

        isDragging = true;

        // Отключаем коллайдер
        var collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // Делаем полупрозрачным
        var renderer = GetComponentInChildren<Renderer>();
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

        // Запускаем вращение
        transform.DORotate(new Vector3(0, 360, 0), 2f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear);

        Debug.Log($"BoardPieceDragHandler: Started dragging {piece.Type} from {originalPosition}");
    }

    public void OnPointerDown(PointerEventData eventData) { }

    /// <summary>
    /// Обрабатывает перетаскивание: подсвечивает клетки и перемещает объект.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3Int position = new Vector3Int(
                Mathf.FloorToInt(hit.point.x + 0.5f),
                0,
                Mathf.FloorToInt(hit.point.z + 0.5f)
            );

            bool isMountain = piece.Type == PieceType.Mountain;
            if (position != originalPosition)
            {
                uiManager.HighlightTile(position, piece.IsPlayer1, isMountain);
                lastHighlighted = position;
            }
            else
            {
                uiManager.ClearHighlight();
                lastHighlighted = null;
            }

            transform.position = new Vector3(position.x, 0.5f, position.z);
        }
        else
        {
            uiManager.ClearHighlight();
            lastHighlighted = null;

            // Следуем за курсором вне доски
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
    /// Обрабатывает отпускание кнопки: перемещает или возвращает в список.
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        isDragging = false;

        // Останавливаем анимацию
        transform.DOKill();

        // Восстанавливаем материал и коллайдер
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null && originalMaterial != null)
        {
            renderer.material = originalMaterial;
        }
        var collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
        }

        bool isMountain = piece.Type == PieceType.Mountain;

        if (lastHighlighted.HasValue && lastHighlighted.Value != originalPosition)
        {
            // Перемещаем на новую клетку
            bool success = uiManager.PlacePieceOrMountain(piece.IsPlayer1, lastHighlighted.Value, piece.Type, isMountain);
            if (success)
            {
                placementManager.RemovePiece(piece.IsPlayer1, originalPosition, piece.Type);
                Debug.Log($"BoardPieceDragHandler: Moved {piece.Type} from {originalPosition} to {lastHighlighted.Value}");
            }
            else
            {
                piece.SetPosition(originalPosition);
                transform.position = new Vector3(originalPosition.x, 0.5f, originalPosition.z);
                Debug.Log($"BoardPieceDragHandler: Failed to move {piece.Type}, returned to {originalPosition}");
            }
        }
        else if (!lastHighlighted.HasValue)
        {
            // Возвращаем в список
            bool success = placementManager.RemovePiece(piece.IsPlayer1, originalPosition, piece.Type);
            if (success)
            {
                uiManager.UpdatePlayerPanels();
                Debug.Log($"BoardPieceDragHandler: Returned {piece.Type} to available list");
            }
            else
            {
                piece.SetPosition(originalPosition);
                transform.position = new Vector3(originalPosition.x, 0.5f, originalPosition.z);
                Debug.Log($"BoardPieceDragHandler: Failed to return {piece.Type}, returned to {originalPosition}");
            }
        }
        else
        {
            // Возвращаем на исходную позицию
            piece.SetPosition(originalPosition);
            transform.position = new Vector3(originalPosition.x, 0.5f, originalPosition.z);
            Debug.Log($"BoardPieceDragHandler: {piece.Type} stayed at {originalPosition}");
        }

        uiManager.ClearHighlight();
        lastHighlighted = null;
    }
}