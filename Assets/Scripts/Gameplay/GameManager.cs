using UnityEngine;
using Zenject;
using System;

public interface IGameManager
{
    void StartGame(int mountainsPerSide, bool isRandomPlacement);
    void MakeMove(Piece piece, Vector3Int target);
    bool IsPlayer1Turn { get; }
    bool IsInPlacementPhase { get; set; }
    event Action<bool> OnTurnChanged;
    event Action<bool> OnGameEnded; // Новое событие: true, если победил игрок 1; false, если игрок 2
}

public class GameManager : MonoBehaviour, IGameManager
{
    [Inject(Id = "Random")] private IPiecePlacementManager randomPlacementManager;
    [Inject(Id = "Manual")] private IPiecePlacementManager manualPlacementManager;
    [Inject] private IBoardManager boardManager;

    private bool isPlayer1Turn = true;
    private bool isInPlacementPhase = false;
    private bool isGameOver = false;

    public bool IsPlayer1Turn => isPlayer1Turn;
    public bool IsInPlacementPhase
    {
        get => isInPlacementPhase;
        set => isInPlacementPhase = value;
    }
    public event Action<bool> OnTurnChanged;
    public event Action<bool> OnGameEnded;

    private void Start()
    {
        Debug.Log("GameManager: Waiting for UI to start game.");
    }

    public void StartGame(int mountainsPerSide, bool isRandomPlacement)
    {
        Debug.Log($"GameManager: StartGame called with {mountainsPerSide} mountains per side, RandomPlacement: {isRandomPlacement}");
        boardManager.InitializeBoard(10);

        var placementManager = isRandomPlacement ? randomPlacementManager : manualPlacementManager;
        if (isRandomPlacement)
        {
            Debug.Log("GameManager: Placing pieces...");
            placementManager.PlacePiecesForPlayer(true, mountainsPerSide);
            placementManager.PlacePiecesForPlayer(false, mountainsPerSide);
        }

        isInPlacementPhase = false;
        isGameOver = false;
        Debug.Log("GameManager: Game started successfully!");
        OnTurnChanged?.Invoke(isPlayer1Turn);
    }

    public void MakeMove(Piece piece, Vector3Int target)
    {
        if (isGameOver)
        {
            Debug.LogWarning("GameManager: Game is over, no moves allowed!");
            return;
        }

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
                Debug.Log($"GameManager: Valid attack on piece {targetPiece.Type} at {target}");
                piece.Attack(target, boardManager);
                SwitchTurn();
                CheckWinCondition();
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
        if (isGameOver) return;
        isPlayer1Turn = !isPlayer1Turn;
        OnTurnChanged?.Invoke(isPlayer1Turn);
        Debug.Log($"GameManager: Turn switched to Player {(isPlayer1Turn ? 1 : 2)}");
    }

    private void CheckWinCondition()
    {
        bool player1KingAlive = false;
        bool player2KingAlive = false;

        var pieces = boardManager.GetAllPieces();
        foreach (var piece in pieces.Values)
        {
            if (piece.Type == PieceType.King)
            {
                if (piece.IsPlayer1)
                    player1KingAlive = true;
                else
                    player2KingAlive = true;
            }
        }

        if (!player1KingAlive)
        {
            isGameOver = true;
            Debug.Log("GameManager: Player 2 wins! Player 1's King is destroyed.");
            OnGameEnded?.Invoke(false); // Игрок 2 победил
        }
        else if (!player2KingAlive)
        {
            isGameOver = true;
            Debug.Log("GameManager: Player 1 wins! Player 2's King is destroyed.");
            OnGameEnded?.Invoke(true); // Игрок 1 победил
        }
    }
}