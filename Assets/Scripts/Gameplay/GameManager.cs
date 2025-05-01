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
}

public class GameManager : MonoBehaviour, IGameManager
{
    [Inject(Id = "Random")] private IPiecePlacementManager randomPlacementManager; // Менеджер случайной расстановки
    [Inject(Id = "Manual")] private IPiecePlacementManager manualPlacementManager; // Менеджер ручной расстановки
    [Inject] private IBoardManager boardManager; // Менеджер доски

    private bool isPlayer1Turn = true; // Чей ход: true — игрок 1, false — игрок 2
    private bool isInPlacementPhase = false; // Флаг фазы расстановки
    public bool IsPlayer1Turn => isPlayer1Turn; // Свойство для проверки текущего хода
    public bool IsInPlacementPhase
    {
        get => isInPlacementPhase; // Возвращает, находится ли игра в фазе расстановки
        set => isInPlacementPhase = value; // Устанавливает фазу расстановки
    }
    public event Action<bool> OnTurnChanged; // Событие смены хода

    private void Start()
    {
        Debug.Log("GameManager: Waiting for UI to start game.");
    }

    /// <summary>
    /// Запускает игру, завершая фазу расстановки.
    /// </summary>
    /// <param name="mountainsPerSide">Количество гор на сторону.</param>
    /// <param name="isRandomPlacement">True, если используется случайная расстановка.</param>
    public void StartGame(int mountainsPerSide, bool isRandomPlacement)
    {
        Debug.Log($"GameManager: StartGame called with {mountainsPerSide} mountains per side, RandomPlacement: {isRandomPlacement}");
        boardManager.InitializeBoard(10); // Инициализируем доску размером 10x10

        // Используем соответствующий менеджер расстановки
        var placementManager = isRandomPlacement ? randomPlacementManager : manualPlacementManager;

        // Если это случайная расстановка, размещаем фигуры
        if (isRandomPlacement)
        {
            Debug.Log("GameManager: Placing pieces...");
            placementManager.PlacePiecesForPlayer(true, mountainsPerSide);
            placementManager.PlacePiecesForPlayer(false, mountainsPerSide);
        }

        isInPlacementPhase = false; // Завершаем фазу расстановки
        Debug.Log("GameManager: Game started successfully!");
        OnTurnChanged?.Invoke(isPlayer1Turn); // Уведомляем о текущем ходе
    }

    /// <summary>
    /// Выполняет ход или атаку для указанной фигуры.
    /// </summary>
    /// <param name="piece">Фигура, которая делает ход.</param>
    /// <param name="target">Целевая позиция для хода или атаки.</param>
    public void MakeMove(Piece piece, Vector3Int target)
    {
        Debug.Log($"GameManager: Attempting move for piece {piece.GetType().Name} at {piece.Position} to {target}");
        if (piece.IsPlayer1 != isPlayer1Turn)
        {
            Debug.LogWarning("GameManager: Not your turn!");
            return;
        }

        var validMoves = piece.GetValidMoves(boardManager); // Получаем возможные ходы
        var attackMoves = piece.GetAttackMoves(boardManager); // Получаем возможные атаки

        if (attackMoves.Contains(target))
        {
            Piece targetPiece = boardManager.GetPieceAt(target);
            if (targetPiece != null && targetPiece.IsPlayer1 != piece.IsPlayer1)
            {
                Debug.Log($"GameManager: Valid attack on piece {targetPiece.GetType().Name} at {target}");
                piece.Attack(target, boardManager); // Выполняем атаку
                SwitchTurn(); // Переключаем ход
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
                boardManager.MovePiece(piece, piece.Position, target); // Перемещаем фигуру
                SwitchTurn(); // Переключаем ход
            });
        }
        else
        {
            Debug.LogWarning($"GameManager: Invalid move or attack to {target}");
        }
    }

    /// <summary>
    /// Переключает ход на другого игрока.
    /// </summary>
    private void SwitchTurn()
    {
        isPlayer1Turn = !isPlayer1Turn;
        OnTurnChanged?.Invoke(isPlayer1Turn);
        Debug.Log($"GameManager: Turn switched to Player {(isPlayer1Turn ? 1 : 2)}");
    }
}