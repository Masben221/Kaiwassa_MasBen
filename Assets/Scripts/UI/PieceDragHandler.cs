using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// Обработчик drag-and-drop для фигур и гор из UI-панели.
/// Отвечает за создание визуального предпросмотра, подсветку клеток и размещение объектов на доске.
/// </summary>
public class PieceDragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private bool isPlayer1; // true, если фигура принадлежит игроку 1, иначе игроку 2
    private PieceType? type; // Тип фигуры (King, Dragon, Mountain и т.д.)
    private UIManualPlacement uiManager; // UI-менеджер для взаимодействия с панелями и подсветкой
    private IPieceFactory pieceFactory; // Фабрика для создания фигур и гор
    private Vector3Int? lastHighlighted; // Последняя подсвеченная клетка
    private GameObject previewObject; // Объект предпросмотра при перетаскивании
    private Material originalMaterial; // Исходный материал объекта предпросмотра

    /// <summary>
    /// Инициализирует обработчик drag-and-drop.
    /// </summary>
    /// <param name="isPlayer1">true, если для игрока 1, иначе для игрока 2.</param>
    /// <param name="type">Тип фигуры (King, Dragon, Mountain и т.д.).</param>
    /// <param name="uiManager">UI-менеджер для взаимодействия.</param>
    /// <param name="pieceFactory">Фабрика для создания объектов.</param>
    public void Initialize(bool isPlayer1, PieceType? type, UIManualPlacement uiManager, IPieceFactory pieceFactory)
    {
        this.isPlayer1 = isPlayer1;
        this.type = type;
        this.uiManager = uiManager;
        this.pieceFactory = pieceFactory;
    }

    /// <summary>
    /// Вызывается при нажатии на кнопку мыши, создавая объект предпросмотра.
    /// </summary>
    /// <param name="eventData">Данные события указателя.</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!type.HasValue) return;

        previewObject = pieceFactory.CreatePiece(type.Value, isPlayer1, Vector3Int.zero).gameObject;

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

    /// <summary>
    /// Вызывается при перетаскивании, обновляя позицию предпросмотра и подсветку клеток.
    /// </summary>
    /// <param name="eventData">Данные события указателя.</param>
    public void OnDrag(PointerEventData eventData)
    {
        if (previewObject == null || !type.HasValue) return;

        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3Int position = new Vector3Int(
                Mathf.FloorToInt(hit.point.x + 0.5f),
                0,
                Mathf.FloorToInt(hit.point.z + 0.5f)
            );

            // Подсвечиваем клетку с учётом типа фигуры
            uiManager.HighlightTile(position, isPlayer1, type.Value);
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

    /// <summary>
    /// Вызывается при отпускании кнопки мыши, размещая фигуру или гору на доске.
    /// </summary>
    /// <param name="eventData">Данные события указателя.</param>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (previewObject != null)
        {
            previewObject.transform.DOKill();
            Destroy(previewObject);
        }

        if (lastHighlighted.HasValue && type.HasValue)
        {
            // Размещаем новую фигуру (не перемещение)
            uiManager.PlacePieceOrMountain(isPlayer1, lastHighlighted.Value, type.Value);
        }

        uiManager.ClearHighlight();
        lastHighlighted = null;
    }
}