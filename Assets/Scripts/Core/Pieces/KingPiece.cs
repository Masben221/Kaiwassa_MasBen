using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Реализация фигуры "Король".
/// </summary>
public class KingPiece : Piece
{
    /// <summary>
    /// Настройка стратегий для короля.
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new KingMoveStrategy();
        attackStrategy = new KingAttackStrategy();
        Debug.Log("KingPiece strategies set up.");
    }
}

/// <summary>
/// Стратегия движения для короля: на 1 клетку в любом направлении.
/// </summary>
public class KingMoveStrategy : IMovable
{
    public List<Vector3Int> CalculateMoves(IBoardManager board, IPiece piece)
    {
        List<Vector3Int> moves = new List<Vector3Int>();
        Vector3Int pos = piece.Position; // Используем актуальную позицию

        int[] directions = { -1, 0, 1 };

        foreach (int dx in directions)
        {
            foreach (int dz in directions)
            {
                if (dx == 0 && dz == 0) continue;
                Vector3Int newPos = pos + new Vector3Int(dx, 0, dz);
                if (board.IsWithinBounds(newPos) && !board.IsOccupied(newPos))
                {
                    moves.Add(newPos);
                }
            }
        }
        return moves;
    }
}

/// <summary>
/// Стратегия атаки для короля: на 1 клетку в любом направлении (только противники).
/// </summary>
public class KingAttackStrategy : IAttackable
{
    public List<Vector3Int> CalculateAttacks(IBoardManager board, IPiece piece)
    {
        List<Vector3Int> attacks = new List<Vector3Int>();
        Vector3Int pos = piece.Position; // Используем актуальную позицию

        int[] directions = { -1, 0, 1 };

        foreach (int dx in directions)
        {
            foreach (int dz in directions)
            {
                if (dx == 0 && dz == 0) continue;
                Vector3Int newPos = pos + new Vector3Int(dx, 0, dz);
                if (board.IsWithinBounds(newPos) && board.IsOccupied(newPos) &&
                    board.GetPieceAt(newPos).IsPlayer1 != piece.IsPlayer1)
                {
                    attacks.Add(newPos);
                }
            }
        }
        return attacks;
    }
}