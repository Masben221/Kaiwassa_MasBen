using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Класс для фигуры "Арбалетчики".
/// Реализует движение на 1 клетку в любом направлении и дальнюю атаку на 2 клетки по прямой или диагонали.
/// </summary>
public class CrossbowmanPiece : Piece
{
    /// <summary>
    /// Настраивает стратегии движения и атаки для Арбалетчиков.
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new CrossbowmanMoveStrategy();
        attackStrategy = new CrossbowmanAttackStrategy();
        Debug.Log("CrossbowmanPiece: Strategies set up.");
    }
}

/// <summary>
/// Стратегия движения для Арбалетчиков.
/// Позволяет двигаться на 1 клетку в любом направлении (прямо или по диагонали).
/// </summary>
public class CrossbowmanMoveStrategy : IMovable
{
    public List<Vector3Int> CalculateMoves(IBoardManager board, Piece piece)
    {
        List<Vector3Int> moves = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        // Все направления: вверх, вниз, влево, вправо, диагонали
        int[] directions = { -1, 0, 1 };

        foreach (int dx in directions)
        {
            foreach (int dz in directions)
            {
                if (dx == 0 && dz == 0) continue; // Пропускаем текущую позицию
                Vector3Int newPos = pos + new Vector3Int(dx, 0, dz);
                if (board.IsWithinBounds(newPos) && !board.IsBlocked(newPos))
                {
                    moves.Add(newPos); // Добавляем только свободные клетки
                }
            }
        }

        return moves;
    }
}

/// <summary>
/// Стратегия атаки для Арбалетчиков.
/// Реализует дальний бой: атака на 1-2 клетки по прямой или диагонали, требует прямой видимости.
/// </summary>
public class CrossbowmanAttackStrategy : IAttackable
{
    public List<Vector3Int> CalculateAttacks(IBoardManager board, Piece piece)
    {
        List<Vector3Int> attacks = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        // Направления: вверх, вниз, влево, вправо, диагонали
        int[] directions = { -1, 0, 1 };

        foreach (int dx in directions)
        {
            foreach (int dz in directions)
            {
                if (dx == 0 && dz == 0) continue; // Пропускаем текущую позицию

                // Проверяем клетки на расстоянии 1
                Vector3Int targetPos1 = pos + new Vector3Int(dx, 0, dz);
                if (board.IsWithinBounds(targetPos1) && board.IsOccupied(targetPos1) &&
                    board.GetPieceAt(targetPos1).IsPlayer1 != piece.IsPlayer1)
                {
                    attacks.Add(targetPos1); // Атака на 1 клетку не требует проверки видимости
                }

                // Проверяем клетки на расстоянии 2
                Vector3Int targetPos2 = pos + new Vector3Int(dx * 2, 0, dz * 2);
                if (!board.IsWithinBounds(targetPos2)) continue;

                // Проверяем прямую видимость для атаки на 2 клетки
                Vector3Int midPos = pos + new Vector3Int(dx, 0, dz);
                if (board.IsBlocked(midPos)) continue; // Путь заблокирован

                // Проверяем, есть ли вражеская фигура на целевой клетке
                if (board.IsOccupied(targetPos2) && board.GetPieceAt(targetPos2).IsPlayer1 != piece.IsPlayer1)
                {
                    attacks.Add(targetPos2);
                }
            }
        }

        return attacks;
    }

    public void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager)
    {
        Debug.Log($"CrossbowmanAttackStrategy: Executing ranged attack from {piece.Position} to {target}");
        Piece targetPiece = boardManager.GetPieceAt(target);
        if (targetPiece != null)
        {
            boardManager.RemovePiece(target);
            Debug.Log($"CrossbowmanAttackStrategy: Removed piece {targetPiece.GetType().Name} at {target}");
        }
        else
        {
            Debug.LogWarning($"CrossbowmanAttackStrategy: No piece at {target} to attack!");
        }
        // Арбалетчик остаётся на месте
    }
}