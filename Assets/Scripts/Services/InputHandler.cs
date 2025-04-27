using UnityEngine;
using Zenject;
using System.Collections.Generic;

public class InputHandler : MonoBehaviour
{
    [Inject] private IGameManager gameManager;
    [Inject] private IBoardManager boardManager;

    [SerializeField] private GameObject moveMarkerPrefab;
    [SerializeField] private GameObject attackMarkerPrefab;

    private Piece selectedPiece;
    private List<GameObject> currentMarkers = new List<GameObject>();
    [SerializeField] private Material highlightMaterial;
    private Material originalMaterial;
    private bool isInputBlocked;

    private void Awake()
    {
        PieceAnimator.OnAnimationStarted += BlockInput;
        PieceAnimator.OnAnimationFinished += UnblockInput;
    }

    private void OnDestroy()
    {
        PieceAnimator.OnAnimationStarted -= BlockInput;
        PieceAnimator.OnAnimationFinished -= UnblockInput;
        ClearSelection();
    }

    private void Update()
    {
        if (!isInputBlocked && Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    private void HandleClick()
    {
        // Блокируем ввод во время фазы расстановки
        if (gameManager.IsInPlacementPhase)
        {
            Debug.Log("InputHandler: Input blocked during placement phase.");
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

            if (selectedPiece != null)
            {
                var validMoves = selectedPiece.GetValidMoves(boardManager);
                var attackMoves = selectedPiece.GetAttackMoves(boardManager);

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

    private void SelectPiece(Piece piece)
    {
        selectedPiece = piece;

        Renderer renderer = piece.GetComponentInChildren<Renderer>();
        if (renderer != null && highlightMaterial != null)
        {
            originalMaterial = renderer.material;
            renderer.material = highlightMaterial;
        }

        var validMoves = piece.GetValidMoves(boardManager);
        foreach (var move in validMoves)
        {
            GameObject marker = Instantiate(
                moveMarkerPrefab,
                new Vector3(move.x, 0.1f, move.z),
                Quaternion.Euler(90, 0, 0)
            );
            currentMarkers.Add(marker);
        }

        var attackMoves = piece.GetAttackMoves(boardManager);
        foreach (var attack in attackMoves)
        {
            GameObject marker = Instantiate(
                attackMarkerPrefab,
                new Vector3(attack.x, 0.1f, attack.z),
                Quaternion.Euler(90, 0, 0)
            );
            currentMarkers.Add(marker);
        }

        Debug.Log($"Selected piece at {piece.Position}");
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

        foreach (var marker in currentMarkers)
        {
            Destroy(marker);
        }
        currentMarkers.Clear();

        selectedPiece = null;
        Debug.Log("Selection cleared.");
    }

    private void BlockInput()
    {
        isInputBlocked = true;
        Debug.Log("InputHandler: Input blocked during animation.");
    }

    private void UnblockInput()
    {
        isInputBlocked = false;
        Debug.Log("InputHandler: Input unblocked after animation.");
    }
}