using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Класс для фигуры "Дракон".
/// Реализует движение (до 3 клеток, перепрыгивая препятствия) и дальнюю атаку.
/// </summary>
public class DragonPiece : Piece
{
    /// <summary>
    /// Настраивает стратегии движения и атаки для дракона.
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new DragonMoveStrategy();
        attackStrategy = new DragonAttackStrategy();
        Debug.Log($"DragonPiece: Strategies set up (attackStrategy: {attackStrategy.GetType().Name})");
    }
}

/// <summary>
/// Стратегия движения для дракона.
/// Позволяет двигаться на 1-3 клетки по прямой или диагонали, перепрыгивая препятствия, только на пустые клетки.
/// </summary>
public class DragonMoveStrategy : IMovable
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
                for (int i = 1; i <= 3; i++)
                {
                    Vector3Int newPos = pos + new Vector3Int(dx * i, 0, dz * i);
                    if (!board.IsWithinBounds(newPos))
                    {
                        break;
                    }
                    // Добавляем только пустые клетки
                    if (!board.IsBlocked(newPos))
                    {
                        moves.Add(newPos);
                    }
                }
            }
        }
        return moves;
    }
}

/// <summary>
/// Стратегия атаки для дракона.
/// Реализует дальний бой: атака на 1-3 клетки по прямой или диагонали, только по прямой видимости (без гор или фигур на пути).
/// </summary>
public class DragonAttackStrategy : IAttackable
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
                for (int i = 1; i <= 3; i++)
                {
                    Vector3Int newPos = pos + new Vector3Int(dx * i, 0, dz * i);
                    if (!board.IsWithinBounds(newPos))
                    {
                        break;
                    }

                    // Проверяем прямую видимость: нет гор или фигур на пути
                    bool isPathClear = true;
                    for (int j = 1; j < i; j++)
                    {
                        Vector3Int intermediatePos = pos + new Vector3Int(dx * j, 0, dz * j);
                        if (board.IsBlocked(intermediatePos) || board.IsOccupied(intermediatePos))
                        {
                            isPathClear = false;
                            break;
                        }
                    }

                    if (!isPathClear)
                    {
                        break; // Прерываем, если путь заблокирован
                    }

                    // Проверяем, есть ли вражеская фигура в конечной точке
                    if (board.IsOccupied(newPos) && board.GetPieceAt(newPos).IsPlayer1 != piece.IsPlayer1)
                    {
                        attacks.Add(newPos);
                    }
                }
            }
        }
        return attacks;
    }

    public void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager)
    {
        Debug.Log($"DragonAttackStrategy: Executing ranged attack from {piece.Position} to {target} (piece: {piece.GetType().Name})");
        Piece targetPiece = boardManager.GetPieceAt(target);
        if (targetPiece != null)
        {
            boardManager.RemovePiece(target);
            Debug.Log($"DragonAttackStrategy: Removed piece {targetPiece.GetType().Name} at {target}");
        }
        else
        {
            Debug.LogWarning($"DragonAttackStrategy: No piece at {target} to attack!");
        }
        // НЕ вызываем MoveTo, чтобы дракон оставался на месте
    }
}