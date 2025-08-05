using UnityEngine;
using System;
using System.Collections.Generic;
using Zenject;

public interface IRangedAttackable
{
    bool IsRangedAttack();
}

public interface IGameManager
{
    void StartGame(int mountainsPerSide, bool isRandomPlacement);
    void MakeMove(Piece piece, Vector3Int target);
    void CheckWinCondition();
    bool IsPlayer1Turn { get; }
    bool IsInPlacementPhase { get; set; }
    event Action<bool> OnTurnChanged;
    event Action<bool> OnGameEnded;
    event Action<Piece, Vector3Int, bool, bool> OnMoveInitiated;
}

public class GameManager : MonoBehaviour, IGameManager
{
    [Inject(Id = "Random")] private IPiecePlacementManager randomPlacementManager;
    [Inject(Id = "Manual")] private IPiecePlacementManager manualPlacementManager;
    [Inject] private IBoardManager boardManager;
    private CameraController cameraController;

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
    public event Action<Piece, Vector3Int, bool, bool> OnMoveInitiated;

    private void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        if (cameraController == null)
        {
            Debug.LogError("GameManager: CameraController not found on Main Camera!");
        }
    }

    private void Start()
    {
        Debug.Log("GameManager: Waiting for UI to start game.");
    }

    public void StartGame(int mountainsPerSide, bool isRandomPlacement)
    {
        Debug.Log($"GameManager: Starting game with {mountainsPerSide} mountains, Random: {isRandomPlacement}");
        boardManager.InitializeBoard(10);

        var placementManager = isRandomPlacement ? randomPlacementManager : manualPlacementManager;
        if (isRandomPlacement)
        {
            placementManager.PlacePiecesForPlayer(true, mountainsPerSide);
            placementManager.PlacePiecesForPlayer(false, mountainsPerSide);
        }

        isInPlacementPhase = false;
        isGameOver = false;
        Debug.Log("GameManager: Game started!");
        OnTurnChanged?.Invoke(isPlayer1Turn);
    }

    public void MakeMove(Piece piece, Vector3Int target)
    {
        if (isGameOver)
        {
            Debug.LogWarning("GameManager: Game is over!");
            return;
        }

        if (piece.IsPlayer1 != isPlayer1Turn)
        {
            Debug.LogWarning("GameManager: Not your turn!");
            return;
        }

        var validMoves = piece.GetValidMoves();
        var attackMoves = piece.GetAttackMoves();

        bool isMove = validMoves.Contains(target) && !attackMoves.Contains(target);
        bool isAttack = attackMoves.Contains(target);
        bool isRangedAttack = isAttack && IsRangedAttack(piece);

        if (!isMove && !isAttack)
        {
            Debug.LogError($"GameManager: {target} is invalid for {piece.Type}");
            return;
        }

        if (isAttack)
        {
            Piece targetPiece = boardManager.GetPieceAt(target);
            if (targetPiece == null || targetPiece.IsPlayer1 == piece.IsPlayer1)
            {
                Debug.LogError($"GameManager: Cannot attack own piece or empty cell at {target}!");
                return;
            }

            PerformMove(piece, target, isMove, isRangedAttack);
        }
        else
        {
            PerformMove(piece, target, isMove, isRangedAttack);
        }
    }

    private void PerformMove(Piece piece, Vector3Int target, bool isMove, bool isRangedAttack)
    {
        Debug.Log($"GameManager: Processing {(isMove ? "move" : isRangedAttack ? "ranged attack" : "melee attack")} to {target} by {piece.Type}");
        OnMoveInitiated?.Invoke(piece, target, isMove, isRangedAttack);
        piece.PerformAction(target, isMove, isRangedAttack, () =>
        {
            SwitchTurn();
            CheckWinCondition();
        });
    }

    private bool IsRangedAttack(Piece piece)
    {
        switch (piece.Type)
        {
            case PieceType.Archer:
            case PieceType.Crossbowman:
            case PieceType.Catapult:
            case PieceType.Trebuchet:
                return true;
            case PieceType.Dragon:
                if (piece.AttackStrategy is IRangedAttackable ranged)
                {
                    return ranged.IsRangedAttack();
                }
                Debug.LogWarning("GameManager: Dragon has no ranged attack strategy!");
                return false;
            default:
                return false;
        }
    }

    private void SwitchTurn()
    {
        if (isGameOver)
            return;

        isPlayer1Turn = !isPlayer1Turn;
        Debug.Log($"GameManager: Turn switched to Player {(isPlayer1Turn ? 1 : 2)}");
        OnTurnChanged?.Invoke(isPlayer1Turn);
    }

    public void CheckWinCondition()
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
            Debug.Log("GameManager: Player 2 wins!");
            OnGameEnded?.Invoke(false);
        }
        else if (!player2KingAlive)
        {
            isGameOver = true;
            Debug.Log("GameManager: Player 1 wins!");
            OnGameEnded?.Invoke(true);
        }
    }
}