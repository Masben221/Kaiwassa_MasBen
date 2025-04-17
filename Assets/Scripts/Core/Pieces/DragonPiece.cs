using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Класс для фигуры "Дракон". Реализует специфичное поведение дракона.
/// </summary>
public class DragonPiece : Piece
{
    /// <summary>
    /// Настройка стратегий для дракона.
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new DragonMoveStrategy();
        attackStrategy = new DragonAttackStrategy();
        Debug.Log("DragonPiece strategies set up.");
    }
}

/// <summary>
/// Стратегия движения для дракона: до 3 клеток по прямой или диагонали, перепрыгивая препятствия.
/// </summary>
public class DragonMoveStrategy : IMovable
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
                for (int i = 1; i <= 3; i++)
                {
                    Vector3Int newPos = pos + new Vector3Int(dx * i, 0, dz * i);
                    if (board.IsWithinBounds(newPos))
                    {
                        moves.Add(newPos); // Дракон перепрыгивает всё
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        return moves;
    }
}

/// <summary>
/// Стратегия атаки для дракона: до 3 клеток по прямой или диагонали (только противники).
/// </summary>
public class DragonAttackStrategy : IAttackable
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
                for (int i = 1; i <= 3; i++)
                {
                    Vector3Int newPos = pos + new Vector3Int(dx * i, 0, dz * i);
                    if (board.IsWithinBounds(newPos))
                    {
                        if (board.IsOccupied(newPos) && board.GetPieceAt(newPos).IsPlayer1 != piece.IsPlayer1)
                        {
                            attacks.Add(newPos);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        return attacks;
    }
}