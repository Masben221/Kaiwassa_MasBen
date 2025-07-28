using UnityEngine;
using Zenject;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

/// <summary>
/// Обрабатывает ввод игрока (мышь или тачскрин) для выбора фигур, ходов и подсказок.
/// Поддерживает перетаскивание фигур в фазе расстановки и подсветку клеток атаки противника.
/// Включает механику подсветки всех потенциальных клеток атаки противника (включая пустые и свои фигуры, исключая горы).
/// Подсвечивает клетки с вражескими фигурами (GetAttackMoves) пульсирующими маркерами.
/// </summary>
public class InputHandler : MonoBehaviour
{
    [Inject] private IGameManager gameManager;
    [Inject] private IBoardManager boardManager;
    [Inject] private DiContainer container; // Для создания объектов через Zenject

    [SerializeField] private GameObject moveMarkerPrefab;
    [SerializeField] private GameObject attackMarkerPrefab;
    [SerializeField] private GameObject pulsatingAttackMarkerPrefab; // Пульсирующий маркер для атак

    private Piece selectedPiece;
    private List<GameObject> currentMarkers = new List<GameObject>();
    [SerializeField] private Material highlightMaterial;
    private Material originalMaterial;
    private bool isInputBlocked;
    private BoardPieceDragHandler activeDragHandler;
    private bool isShowingHints;

    // Пулы объектов для маркеров
    private IObjectPool<GameObject> moveMarkerPool;
    private IObjectPool<GameObject> attackMarkerPool;
    private IObjectPool<GameObject> pulsatingAttackMarkerPool;

    private void Awake()
    {
        // Подписываемся на события PieceAnimator с новой сигнатурой
        PieceAnimator.OnAnimationStarted += (piece, target, isMove, isRangedAttack) => BlockInput(piece);
        PieceAnimator.OnAnimationCompleted += UnblockInput;

        // Инициализация пулов объектов
        moveMarkerPool = new ObjectPool<GameObject>(
            () => container.InstantiatePrefab(moveMarkerPrefab),
            marker => marker.SetActive(true),
            marker => marker.SetActive(false),
            marker => Object.Destroy(marker),
            false, 10, 50);

        attackMarkerPool = new ObjectPool<GameObject>(
            () => container.InstantiatePrefab(attackMarkerPrefab),
            marker => marker.SetActive(true),
            marker => marker.SetActive(false),
            marker => Object.Destroy(marker),
            false, 10, 50);

        pulsatingAttackMarkerPool = new ObjectPool<GameObject>(
            () => container.InstantiatePrefab(pulsatingAttackMarkerPrefab),
            marker => marker.SetActive(true),
            marker => marker.SetActive(false),
            marker => Object.Destroy(marker),
            false, 10, 50);
    }

    private void OnDestroy()
    {
        // Отписываемся от событий PieceAnimator
        PieceAnimator.OnAnimationStarted -= (piece, target, isMove, isRangedAttack) => BlockInput(piece);
        PieceAnimator.OnAnimationCompleted -= UnblockInput;
        ClearSelection();
        ClearHintMarkers();
        // Очистка пулов
        moveMarkerPool.Clear();
        attackMarkerPool.Clear();
        pulsatingAttackMarkerPool.Clear();
    }

    private void Update()
    {
        if (!isInputBlocked)
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleClick();
            }
            else if (Input.GetMouseButton(0) && activeDragHandler != null)
            {
                HandleDrag();
            }
            else if (Input.GetMouseButtonUp(0) && activeDragHandler != null)
            {
                HandlePointerUp();
            }
        }
    }

    private void HandleClick()
    {
        if (isShowingHints)
        {
            ClearHintMarkers();
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3Int clickedPos = new Vector3Int(
                Mathf.FloorToInt(hit.point.x + 0.5f),
                0,
                Mathf.FloorToInt(hit.point.z + 0.5f)
            );

            Piece clickedPiece = boardManager.GetPieceAt(clickedPos);

            if (gameManager.IsInPlacementPhase)
            {
                if (clickedPiece != null)
                {
                    var dragHandler = clickedPiece.GetComponent<BoardPieceDragHandler>();
                    if (dragHandler != null)
                    {
                        activeDragHandler = dragHandler;
                        dragHandler.StartDragging();
                        Debug.Log($"InputHandler: Started dragging piece {clickedPiece.Type} at {clickedPos}");
                    }
                    else
                    {
                        Debug.LogWarning($"InputHandler: No BoardPieceDragHandler found on piece at {clickedPos}");
                    }
                }
                else
                {
                    Debug.Log("InputHandler: Clicked on empty tile during placement phase, ignoring.");
                }
                return;
            }

            if (selectedPiece != null)
            {
                var validMoves = selectedPiece.GetValidMoves();
                var attackMoves = selectedPiece.GetAttackMoves();

                if (validMoves.Contains(clickedPos) || attackMoves.Contains(clickedPos))
                {
                    gameManager.MakeMove(selectedPiece, clickedPos);
                }

                ClearSelection();
            }
            else if (clickedPiece != null && clickedPiece.IsPlayer1 == gameManager.IsPlayer1Turn)
            {
                SelectPiece(clickedPiece);
            }
            else if (!boardManager.IsWithinBounds(clickedPos))
            {
                ClearSelection();
            }
        }
        else
        {
            ClearSelection();
        }
    }

    private void HandleDrag()
    {
        if (activeDragHandler == null)
            return;

        PointerEventData eventData = new PointerEventData(UnityEngine.EventSystems.EventSystem.current)
        {
            position = Input.mousePosition
        };
        activeDragHandler.OnDrag(eventData);
    }

    private void HandlePointerUp()
    {
        if (activeDragHandler == null)
            return;

        PointerEventData eventData = new PointerEventData(UnityEngine.EventSystems.EventSystem.current)
        {
            position = Input.mousePosition
        };
        activeDragHandler.OnPointerUp(eventData);
        activeDragHandler = null;
    }

    private void SelectPiece(Piece piece)
    {
        selectedPiece = piece;

        Renderer renderer = piece.GetComponentInChildren<Renderer>();
        if (renderer != null && highlightMaterial != null)
        {
            originalMaterial = renderer.material;
            renderer.material = highlightMaterial;
        }

        var validMoves = piece.GetValidMoves();
        foreach (var move in validMoves)
        {
            GameObject marker = moveMarkerPool.Get();
            marker.transform.position = new Vector3(move.x, 0.1f, move.z);
            marker.transform.rotation = Quaternion.Euler(90, 0, 0);
            currentMarkers.Add(marker);
        }

        var attackMoves = piece.GetAttackMoves();
        foreach (var attack in attackMoves)
        {
            GameObject marker = pulsatingAttackMarkerPool.Get();
            marker.transform.position = new Vector3(attack.x, 0.1f, attack.z);
            marker.transform.rotation = Quaternion.Euler(90, 0, 0);
            currentMarkers.Add(marker);
        }

        Debug.Log($"Selected piece at {piece.Position}");
    }

    public void ShowOpponentAttackTiles(bool isPlayer1)
    {
        ClearSelection();
        ClearHintMarkers();

        var pieces = boardManager.GetAllPieces();
        HashSet<Vector3Int> attackTiles = new HashSet<Vector3Int>();

        foreach (var pieceEntry in pieces)
        {
            Piece piece = pieceEntry.Value;
            if (piece.IsPlayer1 == isPlayer1 && piece.Type != PieceType.Mountain)
            {
                var attackMoves = piece.GetAttackMoves();
                foreach (var attack in attackMoves)
                {
                    attackTiles.Add(attack);
                }
            }
        }

        foreach (var attack in attackTiles)
        {
            GameObject marker = pulsatingAttackMarkerPool.Get();
            marker.transform.position = new Vector3(attack.x, 0.1f, attack.z);
            marker.transform.rotation = Quaternion.Euler(90, 0, 0);
            currentMarkers.Add(marker);
        }

        isShowingHints = true;
        Debug.Log($"InputHandler: Showing attack tiles for Player {(isPlayer1 ? 1 : 2)}. Total tiles: {attackTiles.Count}");
    }

    public void ShowAllPotentialAttackTiles(bool isPlayer1)
    {
        ClearSelection();
        ClearHintMarkers();

        var pieces = boardManager.GetAllPieces();
        HashSet<Vector3Int> potentialAttackTiles = new HashSet<Vector3Int>();
        HashSet<Vector3Int> enemyAttackTiles = new HashSet<Vector3Int>();

        foreach (var pieceEntry in pieces)
        {
            Piece piece = pieceEntry.Value;
            if (piece.IsPlayer1 == isPlayer1 && piece.Type != PieceType.Mountain)
            {
                var potentialAttacks = piece.GetAllPotentialAttackMoves();
                foreach (var attack in potentialAttacks)
                {
                    potentialAttackTiles.Add(attack);
                }

                var enemyAttacks = piece.GetAttackMoves();
                foreach (var attack in enemyAttacks)
                {
                    enemyAttackTiles.Add(attack);
                }
            }
        }

        foreach (var attack in potentialAttackTiles)
        {
            GameObject marker;
            if (enemyAttackTiles.Contains(attack))
            {
                marker = pulsatingAttackMarkerPool.Get();
            }
            else
            {
                marker = attackMarkerPool.Get();
            }
            marker.transform.position = new Vector3(attack.x, 0.1f, attack.z);
            marker.transform.rotation = Quaternion.Euler(90, 0, 0);
            currentMarkers.Add(marker);
        }

        isShowingHints = true;
        Debug.Log($"InputHandler: Showing all potential attack tiles for Player {(isPlayer1 ? 1 : 2)}. Total tiles: {potentialAttackTiles.Count}, Enemy tiles: {enemyAttackTiles.Count}");
    }

    public void ClearHintMarkers()
    {
        foreach (var marker in currentMarkers)
        {
            if (marker.CompareTag("MoveMarker"))
                moveMarkerPool.Release(marker);
            else if (marker.CompareTag("AttackMarker"))
                attackMarkerPool.Release(marker);
            else if (marker.CompareTag("PulsatingAttackMarker"))
                pulsatingAttackMarkerPool.Release(marker);
        }
        currentMarkers.Clear();
        isShowingHints = false;
        Debug.Log("InputHandler: Hint markers cleared.");
    }

    private void ClearSelection()
    {
        if (selectedPiece != null)
        {
            Renderer renderer = selectedPiece.GetComponentInChildren<Renderer>();
            if (renderer != null && originalMaterial != null)
            {
                renderer.material = originalMaterial;
            }
        }

        if (!isShowingHints)
        {
            foreach (var marker in currentMarkers)
            {
                if (marker.CompareTag("MoveMarker"))
                    moveMarkerPool.Release(marker);
                else if (marker.CompareTag("AttackMarker"))
                    attackMarkerPool.Release(marker);
                else if (marker.CompareTag("PulsatingAttackMarker"))
                    pulsatingAttackMarkerPool.Release(marker);
            }
            currentMarkers.Clear();
        }

        selectedPiece = null;
        Debug.Log("Selection cleared.");
    }

    private void BlockInput(Piece piece)
    {
        isInputBlocked = true;
        Debug.Log($"InputHandler: Input blocked during animation for {piece.Type} at {piece.Position}.");
    }

    private void UnblockInput(Piece piece)
    {
        isInputBlocked = false;
        Debug.Log($"InputHandler: Input unblocked after animation for {piece.Type} at {piece.Position}.");
    }
}