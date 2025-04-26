using UnityEngine;
using Zenject;
using System;

/// <summary>
/// Интерфейс для управления игрой.
/// </summary>
public interface IGameManager
{
    void StartGame(int mountainsPerSide, bool isRandomPlacement); // Инициализация игры с числом гор и режимом
    void MakeMove(Piece piece, Vector3Int target); // Выполнение хода или атаки
    bool IsPlayer1Turn { get; } // Чей ход
    event Action<bool> OnTurnChanged; // Событие смены хода
}

/// <summary>
/// Управляет логикой игры: инициализация доски, размещение фигур, обработка ходов и атак, смена игроков.
/// </summary>
public class GameManager : MonoBehaviour, IGameManager
{
    [Inject(Id = "Random")] private IPiecePlacementManager randomPlacementManager; // Менеджер случайной расстановки
    [Inject(Id = "Manual")] private IPiecePlacementManager manualPlacementManager; // Менеджер ручной расстановки
    [Inject] private IBoardManager boardManager; // Интерфейс доски

    private bool isPlayer1Turn = true; // Текущий ход (true = Игрок 1)
    public bool IsPlayer1Turn => isPlayer1Turn; // Геттер для текущего хода
    public event Action<bool> OnTurnChanged; // Событие смены хода

    private void Start()
    {
        Debug.Log("GameManager: Waiting for UI to start game.");
    }

    /// <summary>
    /// Инициализирует игру: создаёт доску 10x10, размещает горы и фигуры.
    /// </summary>
    /// <param name="mountainsPerSide">Количество гор на сторону.</param>
    /// <param name="isRandomPlacement">true, если используется случайная расстановка.</param>
    public void StartGame(int mountainsPerSide, bool isRandomPlacement)
    {
        Debug.Log($"GameManager: StartGame called with {mountainsPerSide} mountains per side, RandomPlacement: {isRandomPlacement}");
        boardManager.InitializeBoard(10);

        var placementManager = isRandomPlacement ? randomPlacementManager : manualPlacementManager;

        Debug.Log("GameManager: Placing mountains...");
        placementManager.PlaceMountains(mountainsPerSide);

        Debug.Log("GameManager: Placing pieces...");
        placementManager.PlacePiecesForPlayer(true); // Игрок 1
        placementManager.PlacePiecesForPlayer(false); // Игрок 2

        Debug.Log("GameManager: Game started successfully!");
        OnTurnChanged?.Invoke(isPlayer1Turn);
    }

    /// <summary>
    /// Обрабатывает ход или атаку фигуры.
    /// Сначала проверяет возможность атаки, затем ход.
    /// </summary>
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

    /// <summary>
    /// Переключает ход между игроками и уведомляет подписчиков.
    /// </summary>
    private void SwitchTurn()
    {
        isPlayer1Turn = !isPlayer1Turn;
        OnTurnChanged?.Invoke(isPlayer1Turn);
        Debug.Log($"GameManager: Turn switched to Player {(isPlayer1Turn ? 1 : 2)}");
    }
}