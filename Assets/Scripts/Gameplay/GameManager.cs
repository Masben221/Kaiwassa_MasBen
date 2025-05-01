using UnityEngine;
using Zenject;
using System;

public interface IGameManager
{
    void StartGame(int mountainsPerSide, bool isRandomPlacement);
    void MakeMove(Piece piece, Vector3Int target);
    bool IsPlayer1Turn { get; }
    bool IsInPlacementPhase { get; set; } // Добавляем сеттер
    event Action<bool> OnTurnChanged;
}

public class GameManager : MonoBehaviour, IGameManager
{
    [Inject(Id = "Random")] private IPiecePlacementManager randomPlacementManager;
    [Inject(Id = "Manual")] private IPiecePlacementManager manualPlacementManager;
    [Inject] private IBoardManager boardManager;

    private bool isPlayer1Turn = true;
    private bool isInPlacementPhase = false;
    public bool IsPlayer1Turn => isPlayer1Turn;
    public bool IsInPlacementPhase
    {
        get => isInPlacementPhase;
        set => isInPlacementPhase = value;
    }
    public event Action<bool> OnTurnChanged;

    private void Start()
    {
        Debug.Log("GameManager: Waiting for UI to start game.");
    }

    public void StartGame(int mountainsPerSide, bool isRandomPlacement)
    {
        Debug.Log($"GameManager: StartGame called with {mountainsPerSide} mountains per side, RandomPlacement: {isRandomPlacement}");
        boardManager.InitializeBoard(10);

        var placementManager = isRandomPlacement ? randomPlacementManager : manualPlacementManager;

        Debug.Log("GameManager: Placing pieces...");
        placementManager.PlacePiecesForPlayer(true, mountainsPerSide);
        placementManager.PlacePiecesForPlayer(false, mountainsPerSide);

        isInPlacementPhase = false;
        Debug.Log("GameManager: Game started successfully!");
        OnTurnChanged?.Invoke(isPlayer1Turn);
    }

    public void MakeMove(Piece piece, Vector3Int target)
    {
        Debug.Log($"GameManager: Attempting move for piece {piece.GetType().Name} at {piece.Position} to {target}");
        if (piece.IsPlayer1 != isPlayer1Turn)
        {
            Debug.LogWarning("GameManager: Not your turn!");
            return;
        }

        var validMoves = piece.GetValidMoves(boardManager);
        var attackMoves = piece.GetAttackMoves(boardManager);

        if (attackMoves.Contains(target))
        {
            Piece targetPiece = boardManager.GetPieceAt(target);
            if (targetPiece != null && targetPiece.IsPlayer1 != piece.IsPlayer1)
            {
                Debug.Log($"GameManager: Valid attack on piece {targetPiece.GetType().Name} at {target}");
                piece.Attack(target, boardManager);
                SwitchTurn();
            }
            else
            {
                Debug.LogWarning($"GameManager: No valid enemy piece at {target} to attack!");
            }
        }
        else if (validMoves.Contains(target))
        {
            Debug.Log($"GameManager: Valid move to {target}");
            piece.GetComponent<PieceAnimator>().MoveTo(target, () =>
            {
                boardManager.MovePiece(piece, piece.Position, target);
                SwitchTurn();
            });
        }
        else
        {
            Debug.LogWarning($"GameManager: Invalid move or attack to {target}");
        }
    }

    private void SwitchTurn()
    {
        isPlayer1Turn = !isPlayer1Turn;
        OnTurnChanged?.Invoke(isPlayer1Turn);
        Debug.Log($"GameManager: Turn switched to Player {(isPlayer1Turn ? 1 : 2)}");
    }
}