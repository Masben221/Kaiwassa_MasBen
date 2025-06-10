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
    public List<Vector3Int> CalculateMoves(IBoardManager board, Piece piece)
    {
        List<Vector3Int> moves = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        int[] directions = { -1, 0, 1 };

        foreach (int dx in directions)
        {
            foreach (int dz in directions)
            {
                if (dx == 0 && dz == 0) continue;
                Vector3Int newPos = pos + new Vector3Int(dx, 0, dz);
                if (board.IsWithinBounds(newPos) && !board.IsBlocked(newPos))
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
/// Предоставляет список всех потенциальных клеток атаки для подсказок (включая пустые и свои фигуры, исключая горы).
/// </summary>
public class KingAttackStrategy : IAttackable
{
    public List<Vector3Int> CalculateAttacks(IBoardManager board, Piece piece)
    {
        List<Vector3Int> attacks = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        int[] directions = { -1, 0, 1 };

        foreach (int dx in directions)
        {
            foreach (int dz in directions)
            {
                if (dx == 0 && dz == 0) continue;
                Vector3Int newPos = pos + new Vector3Int(dx, 0, dz);
                if (board.IsWithinBounds(newPos) && board.IsOccupied(newPos) &&
                    board.GetPieceAt(newPos).IsPlayer1 != piece.IsPlayer1 && !board.IsMountain(newPos))
                {
                    attacks.Add(newPos);
                }
            }
        }
        return attacks;
    }

    /// <summary>
    /// Рассчитывает все потенциальные клетки, которые король может атаковать, включая пустые и свои фигуры, исключая горы.
    /// Учитывает дальность 1 клетку в любом направлении.
    /// </summary>
    public List<Vector3Int> CalculateAllAttacks(IBoardManager board, Piece piece)
    {
        List<Vector3Int> attacks = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        int[] directions = { -1, 0, 1 };

        foreach (int dx in directions)
        {
            foreach (int dz in directions)
            {
                if (dx == 0 && dz == 0) continue;
                Vector3Int newPos = pos + new Vector3Int(dx, 0, dz);
                if (board.IsWithinBounds(newPos) && !board.IsMountain(newPos))
                {
                    attacks.Add(newPos);
                }
            }
        }
        return attacks;
    }

    public void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager)
    {
        Debug.Log($"KingAttackStrategy: Executing melee attack on {target}");
        // Ближний бой: уничтожаем фигуру 
        boardManager.RemovePiece(target);       
    }
}