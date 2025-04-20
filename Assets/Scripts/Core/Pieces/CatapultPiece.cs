using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Класс для фигуры "Катапульта".
/// Реализует движение на 1 клетку по прямой и дальнюю атаку до 4 клеток.
/// </summary>
public class CatapultPiece : Piece
{
    /// <summary>
    /// Настраивает стратегии движения и атаки для Катапульты.
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new CatapultMoveStrategy();
        attackStrategy = new CatapultAttackStrategy();
        Debug.Log("CatapultPiece: Strategies set up.");
    }
}

/// <summary>
/// Стратегия движения для Катапульты.
/// Позволяет двигаться на 1 клетку по прямой (горизонталь/вертикаль).
/// </summary>
public class CatapultMoveStrategy : IMovable
{
    public List<Vector3Int> CalculateMoves(IBoardManager board, Piece piece)
    {
        List<Vector3Int> moves = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        // Возможные направления: вверх, вниз, влево, вправо
        Vector3Int[] directions = {
            new Vector3Int(1, 0, 0),  // Вправо
            new Vector3Int(-1, 0, 0), // Влево
            new Vector3Int(0, 0, 1),  // Вверх
            new Vector3Int(0, 0, -1)  // Вниз
        };

        foreach (var dir in directions)
        {
            Vector3Int newPos = pos + dir;
            if (board.IsWithinBounds(newPos) && !board.IsBlocked(newPos))
            {
                moves.Add(newPos);
            }
        }

        return moves;
    }
}

/// <summary>
/// Стратегия атаки для Катапульты.
/// Реализует дальний бой: атака на 1-4 клетки по прямой с проверкой прямой видимости.
/// </summary>
public class CatapultAttackStrategy : IAttackable
{
    public List<Vector3Int> CalculateAttacks(IBoardManager board, Piece piece)
    {
        List<Vector3Int> attacks = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        // Возможные направления: вверх, вниз, влево, вправо
        Vector3Int[] directions = {
            new Vector3Int(1, 0, 0),  // Вправо
            new Vector3Int(-1, 0, 0), // Влево
            new Vector3Int(0, 0, 1),  // Вверх
            new Vector3Int(0, 0, -1)  // Вниз
        };

        foreach (var dir in directions)
        {
            for (int i = 1; i <= 4; i++)
            {
                Vector3Int newPos = pos + dir * i;
                if (!board.IsWithinBounds(newPos))
                {
                    break;
                }

                // Проверяем, что путь свободен от фигур и гор
                bool pathBlocked = false;
                for (int j = 1; j < i; j++)
                {
                    Vector3Int midPos = pos + dir * j;
                    if (board.IsBlocked(midPos))
                    {
                        pathBlocked = true;
                        break;
                    }
                }
                if (pathBlocked)
                {
                    continue;
                }

                // Если клетка занята противником, добавляем её как цель для атаки
                if (board.IsOccupied(newPos) && board.GetPieceAt(newPos).IsPlayer1 != piece.IsPlayer1)
                {
                    attacks.Add(newPos);
                }
            }
        }

        return attacks;
    }

    public void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager)
    {
        Debug.Log($"CatapultAttackStrategy: Executing ranged attack from {piece.Position} to {target}");
        Piece targetPiece = boardManager.GetPieceAt(target);
        if (targetPiece != null)
        {
            boardManager.RemovePiece(target);
            Debug.Log($"CatapultAttackStrategy: Removed piece {targetPiece.GetType().Name} at {target}");
        }
        else
        {
            Debug.LogWarning($"CatapultAttackStrategy: No piece at {target} to attack!");
        }
        // Катапульта остаётся на месте, не двигается
    }
}