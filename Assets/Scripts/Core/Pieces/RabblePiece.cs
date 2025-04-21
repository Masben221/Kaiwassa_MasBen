using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Класс для фигуры "Раббл" (Ополченцы).
/// Реализует движение на 1 клетку вперёд и ближнюю атаку на 1 клетку вперёд.
/// </summary>
public class RabblePiece : Piece
{
    /// <summary>
    /// Настраивает стратегии движения и атаки для Раббл.
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new RabbleMoveStrategy();
        attackStrategy = new RabbleAttackStrategy();
        Debug.Log("RabblePiece: Strategies set up.");
    }
}

/// <summary>
/// Стратегия движения для Раббл.
/// Позволяет двигаться на 1 клетку вперёд (вдоль оси Z в сторону противника).
/// </summary>
public class RabbleMoveStrategy : IMovable
{
    public List<Vector3Int> CalculateMoves(IBoardManager board, Piece piece)
    {
        List<Vector3Int> moves = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        // Направление "вперёд" зависит от игрока
        int dz = piece.IsPlayer1 ? 1 : -1; // Игрок 1: +Z, Игрок 2: -Z
        Vector3Int newPos = pos + new Vector3Int(0, 0, dz);

        if (board.IsWithinBounds(newPos) && !board.IsBlocked(newPos))
        {
            moves.Add(newPos); // Добавляем только свободную клетку
        }

        return moves;
    }
}

/// <summary>
/// Стратегия атаки для Раббл.
/// Реализует ближний бой: атака на 1 клетку вперёд, занимает клетку противника.
/// </summary>
public class RabbleAttackStrategy : IAttackable
{
    public List<Vector3Int> CalculateAttacks(IBoardManager board, Piece piece)
    {
        List<Vector3Int> attacks = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        // Направление "вперёд" зависит от игрока
        int dz = piece.IsPlayer1 ? 1 : -1; // Игрок 1: +Z, Игрок 2: -Z
        Vector3Int targetPos = pos + new Vector3Int(0, 0, dz);

        if (board.IsWithinBounds(targetPos) && board.IsOccupied(targetPos) &&
            board.GetPieceAt(targetPos).IsPlayer1 != piece.IsPlayer1 && !board.IsMountain(targetPos))
        {
            attacks.Add(targetPos);
        }

        return attacks;
    }

    public void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager)
    {
        Debug.Log($"RabbleAttackStrategy: Executing melee attack on {target}");
        boardManager.RemovePiece(target);
        piece.GetComponent<PieceAnimator>().MoveTo(target, () =>
        {
            boardManager.MovePiece(piece, piece.Position, target);
        });
    }
}