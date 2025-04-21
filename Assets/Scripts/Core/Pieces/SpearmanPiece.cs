using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Класс для фигуры "Копейщики".
/// Реализует движение и атаку до 2 клеток по прямой.
/// </summary>
public class SpearmanPiece : Piece
{
    /// <summary>
    /// Настраивает стратегии движения и атаки для Копейщиков.
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new SpearmanMoveStrategy();
        attackStrategy = new SpearmanAttackStrategy();
        Debug.Log("SpearmanPiece: Strategies set up.");
    }
}

/// <summary>
/// Стратегия движения для Копейщиков.
/// Позволяет двигаться на 1-2 клетки по прямой (горизонталь/вертикаль).
/// </summary>
public class SpearmanMoveStrategy : IMovable
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
            for (int i = 1; i <= 2; i++)
            {
                Vector3Int newPos = pos + dir * i;
                if (!board.IsWithinBounds(newPos))
                {
                    break;
                }
                if (board.IsBlocked(newPos))
                {
                    break; // Прерываем направление, если встретили фигуру или гору
                }
                moves.Add(newPos);
            }
        }

        return moves;
    }
}

/// <summary>
/// Стратегия атаки для Копейщиков.
/// Реализует ближний бой: атака на 1-2 клетки по прямой, занимает клетку противника.
/// Проверяет путь на наличие препятствий.
/// </summary>
public class SpearmanAttackStrategy : IAttackable
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
            for (int i = 1; i <= 2; i++)
            {
                Vector3Int newPos = pos + dir * i;
                if (!board.IsWithinBounds(newPos))
                {
                    break;
                }
                // Проверяем путь до цели (все клетки перед newPos)
                bool pathBlocked = false;
                for (int j = 1; j < i; j++)
                {
                    Vector3Int midPos = pos + dir * j;
                    if (board.IsBlocked(midPos) || board.IsOccupied(midPos))
                    {
                        pathBlocked = true;
                        break;
                    }
                }
                if (pathBlocked)
                {
                    break;
                }
                // Проверяем целевую клетку
                if (board.IsOccupied(newPos) && board.GetPieceAt(newPos).IsPlayer1 != piece.IsPlayer1 && !board.IsMountain(newPos))
                {
                    attacks.Add(newPos);
                    break; // Прерываем направление после первой вражеской фигуры
                }
            }
        }

        return attacks;
    }

    public void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager)
    {
        Debug.Log($"SpearmanAttackStrategy: Executing melee attack on {target}");
        // Ближний бой: уничтожаем фигуру и перемещаемся
        boardManager.RemovePiece(target);
        piece.GetComponent<PieceAnimator>().MoveTo(target, () =>
        {
            boardManager.MovePiece(piece, piece.Position, target);
        });
    }
}