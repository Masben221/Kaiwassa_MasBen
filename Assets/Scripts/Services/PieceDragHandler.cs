using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Zenject;
using UnityEngine.UI;

/// <summary>
/// Перетаскивание фигур из панели расстановки на доску.
/// </summary>
public class PieceDragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private bool isPlayer1;
    private PieceType type;
    private UIManualPlacement uiManager;
    private IPieceFactory pieceFactory;
    private GameObject previewObject;
    private Piece previewPiece;
    private Vector3Int? lastHighlighted;
    private bool isDragging;
    private Image iconImage; // НОВОЕ: Ссылка на иконку для затемнения

    public void Initialize(bool isPlayer1, PieceType type, UIManualPlacement uiManager, IPieceFactory pieceFactory)
    {
        this.isPlayer1 = isPlayer1;
        this.type = type;
        this.uiManager = uiManager;
        this.pieceFactory = pieceFactory;
        // НОВОЕ: Получаем Image для иконки
        iconImage = transform.Find("Icon")?.GetComponent<Image>();
        if (iconImage == null)
        {
            Debug.LogError($"PieceDragHandler: Icon Image not found in {type} button!");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isDragging || uiManager == null) return;

        isDragging = true;

        // НОВОЕ: Затемняем иконку
        if (iconImage != null)
        {
            Color color = iconImage.color;
            color.a = 0.5f;
            iconImage.color = color;
        }

        previewObject = new GameObject("PiecePreview");
        previewPiece = pieceFactory.CreatePiece(type, isPlayer1, Vector3Int.zero);
        if (previewPiece == null)
        {
            Debug.LogError($"PieceDragHandler: Failed to create preview for {type}");
            Destroy(previewObject);
            isDragging = false;
            // НОВОЕ: Восстанавливаем иконку
            if (iconImage != null)
            {
                Color color = iconImage.color;
                color.a = 1f;
                iconImage.color = color;
            }
            return;
        }

        previewObject.transform.position = new Vector3(0, 0.5f, 0);
        previewPiece.transform.SetParent(previewObject.transform, false);
        previewPiece.transform.localPosition = Vector3.zero;

        var renderer = previewPiece.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            Material transparentMat = new Material(Shader.Find("Standard"));
            transparentMat.SetFloat("_Mode", 3);
            transparentMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            transparentMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            transparentMat.SetInt("_ZWrite", 0);
            transparentMat.DisableKeyword("_ALPHATEST_ON");
            transparentMat.EnableKeyword("_ALPHABLEND_ON");
            transparentMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            transparentMat.renderQueue = 3000;
            Color color = renderer.material.color;
            color.a = 0.5f;
            transparentMat.color = color;
            renderer.material = transparentMat;
        }

        previewObject.transform.DORotate(new Vector3(0, 360, 0), 2f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || previewObject == null) return;

        int boardLayerMask = LayerMask.GetMask("Board");
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, boardLayerMask))
        {
            Vector3Int position = new Vector3Int(
                Mathf.FloorToInt(hit.point.x + 0.5f),
                0,
                Mathf.FloorToInt(hit.point.z + 0.5f)
            );

            uiManager.HighlightTile(position, isPlayer1, type);
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
        if (!isDragging || previewObject == null) return;

        isDragging = false;
        previewObject.transform.DOKill();
        Destroy(previewObject);
        previewPiece = null;

        // НОВОЕ: Восстанавливаем иконку
        if (iconImage != null)
        {
            Color color = iconImage.color;
            color.a = 1f;
            iconImage.color = color;
        }

        if (lastHighlighted.HasValue)
        {
            bool success = uiManager.PlacePieceOrMountain(isPlayer1, lastHighlighted.Value, type);
            if (success)
            {
                uiManager.UpdatePlayerPanels();
            }
        }

        uiManager.ClearHighlight();
        lastHighlighted = null;
    }
}