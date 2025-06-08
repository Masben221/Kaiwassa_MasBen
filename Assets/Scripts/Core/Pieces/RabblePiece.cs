using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Класс для фигуры "Раббл" (Ополченцы).
/// Реализует движение и атаку на 1 клетку по горизонтали (X) и вертикали (Z).
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
/// Позволяет двигаться на 1 клетку по горизонтали (x ± 1) или вертикали (z ± 1).
/// </summary>
public class RabbleMoveStrategy : IMovable
{
    public List<Vector3Int> CalculateMoves(IBoardManager board, Piece piece)
    {
        List<Vector3Int> moves = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        // Возможные направления: влево, вправо, вверх, вниз
        Vector3Int[] directions = new[]
        {
            new Vector3Int(1, 0, 0),  // вправо (x + 1)
            new Vector3Int(-1, 0, 0), // влево (x - 1)
            new Vector3Int(0, 0, 1),  // вверх (z + 1)
            new Vector3Int(0, 0, -1)  // вниз (z - 1)
        };

        foreach (var dir in directions)
        {
            Vector3Int newPos = pos + dir;
            if (board.IsWithinBounds(newPos) && !board.IsBlocked(newPos))
            {
                moves.Add(newPos); // Добавляем только свободную клетку
            }
        }

        return moves;
    }
}

/// <summary>
/// Стратегия атаки для Раббл.
/// Реализует ближний бой: атака на 1 клетку по горизонтали (x ± 1) или вертикали (z ± 1), занимает клетку противника.
/// Предоставляет список всех потенциальных клеток атаки для подсказок (включая пустые и свои фигуры, исключая горы).
/// </summary>
public class RabbleAttackStrategy : IAttackable
{
    public List<Vector3Int> CalculateAttacks(IBoardManager board, Piece piece)
    {
        List<Vector3Int> attacks = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        // Возможные направления: влево, вправо, вверх, вниз
        Vector3Int[] directions = new[]
        {
            new Vector3Int(1, 0, 0),  // вправо (x + 1)
            new Vector3Int(-1, 0, 0), // влево (x - 1)
            new Vector3Int(0, 0, 1),  // вверх (z + 1)
            new Vector3Int(0, 0, -1)  // вниз (z - 1)
        };

        foreach (var dir in directions)
        {
            Vector3Int targetPos = pos + dir;
            if (board.IsWithinBounds(targetPos) && board.IsOccupied(targetPos) &&
                board.GetPieceAt(targetPos).IsPlayer1 != piece.IsPlayer1 && !board.IsMountain(targetPos))
            {
                attacks.Add(targetPos);
            }
        }

        return attacks;
    }

    /// <summary>
    /// Рассчитывает все потенциальные клетки, которые раbble может атаковать, включая пустые и свои фигуры, исключая горы.
    /// Учитывает дальность 1 клетку по горизонтали или вертикали.
    /// </summary>
    public List<Vector3Int> CalculateAllAttacks(IBoardManager board, Piece piece)
    {
        List<Vector3Int> attacks = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        // Возможные направления: влево, вправо, вверх, вниз
        Vector3Int[] directions = new[]
        {
            new Vector3Int(1, 0, 0),  // вправо (x + 1)
            new Vector3Int(-1, 0, 0), // влево (x - 1)
            new Vector3Int(0, 0, 1),  // вверх (z + 1)
            new Vector3Int(0, 0, -1)  // вниз (z - 1)
        };

        foreach (var dir in directions)
        {
            Vector3Int targetPos = pos + dir;
            if (board.IsWithinBounds(targetPos) && !board.IsMountain(targetPos))
            {
                attacks.Add(targetPos);
            }
        }

        return attacks;
    }

    public void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager)
    {
        Debug.Log($"RabbleAttackStrategy: Executing melee attack on {target}");
        boardManager.RemovePiece(target);
        piece.GetComponent<PieceAnimator>().MoveTo(target, null, () =>
        {
            boardManager.MovePiece(piece, piece.Position, target);
        });
    }
}