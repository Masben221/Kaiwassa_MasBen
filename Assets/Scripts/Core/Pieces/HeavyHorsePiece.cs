using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Класс для фигуры "Тяжёлая кавалерия".
/// Реализует L-образное движение с перепрыгиванием препятствий и L-образную атаку без перепрыгивания.
/// </summary>
public class HeavyHorsePiece : Piece
{
    /// <summary>
    /// Настраивает стратегии движения и атаки для Тяжёлой кавалерии.
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new HeavyHorseMoveStrategy();
        attackStrategy = new HeavyHorseAttackStrategy();
        Debug.Log("HeavyHorsePiece: Strategies set up.");
    }
}

/// <summary>
/// Стратегия движения для Тяжёлой кавалерии.
/// Позволяет двигаться L-образно, перепрыгивая фигуры и горы, но только на пустые клетки.
/// </summary>
public class HeavyHorseMoveStrategy : IMovable
{
    public List<Vector3Int> CalculateMoves(IBoardManager board, Piece piece)
    {
        List<Vector3Int> moves = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        // L-образные направления (2 клетки в одном направлении, 1 перпендикулярно, или 1 + 2)
        Vector3Int[] directions = {
            new Vector3Int(2, 0, 1),   // Вправо 2, вверх 1
            new Vector3Int(2, 0, -1),  // Вправо 2, вниз 1
            new Vector3Int(-2, 0, 1),  // Влево 2, вверх 1
            new Vector3Int(-2, 0, -1), // Влево 2, вниз 1
            new Vector3Int(1, 0, 2),   // Вправо 1, вверх 2
            new Vector3Int(1, 0, -2),  // Вправо 1, вниз 2
            new Vector3Int(-1, 0, 2),  // Влево 1, вверх 2
            new Vector3Int(-1, 0, -2)  // Влево 1, вниз 2
        };

        foreach (var dir in directions)
        {
            Vector3Int newPos = pos + dir;
            if (board.IsWithinBounds(newPos) && !board.IsMountain(newPos) && !board.IsOccupied(newPos))
            {
                moves.Add(newPos); // Добавляем только пустые клетки, перепрыгивая фигуры и горы
            }
        }

        return moves;
    }
}

/// <summary>
/// Стратегия атаки для Тяжёлой кавалерии.
/// Реализует ближний бой: L-образная атака, требует свободный путь до цели (не перепрыгивает фигуры или горы).
/// </summary>
public class HeavyHorseAttackStrategy : IAttackable
{
    public List<Vector3Int> CalculateAttacks(IBoardManager board, Piece piece)
    {
        List<Vector3Int> attacks = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        // L-образные направления (2 клетки в одном направлении, 1 перпендикулярно, или 1 + 2)
        Vector3Int[] directions = {
            new Vector3Int(2, 0, 1),   // Вправо 2, вверх 1
            new Vector3Int(2, 0, -1),  // Вправо 2, вниз 1
            new Vector3Int(-2, 0, 1),  // Влево 2, вверх 1
            new Vector3Int(-2, 0, -1), // Влево 2, вниз 1
            new Vector3Int(1, 0, 2),   // Вправо 1, вверх 2
            new Vector3Int(1, 0, -2),  // Вправо 1, вниз 2
            new Vector3Int(-1, 0, 2),  // Влево 1, вверх 2
            new Vector3Int(-1, 0, -2)  // Влево 1, вниз 2
        };

        foreach (var dir in directions)
        {
            Vector3Int newPos = pos + dir;
            if (board.IsWithinBounds(newPos) && !board.IsMountain(newPos) &&
                board.IsOccupied(newPos) && board.GetPieceAt(newPos).IsPlayer1 != piece.IsPlayer1)
            {
                // Проверяем свободный путь до цели
                bool pathClear = IsPathClear(board, pos, dir);
                if (pathClear)
                {
                    attacks.Add(newPos); // Атака возможна, если путь свободен
                }
            }
        }

        return attacks;
    }

    /// <summary>
    /// Проверяет, свободен ли путь до цели для L-образного хода.
    /// Для хода (dx, dz) проверяет промежуточные клетки (например, для (2,1) проверяет (1,0) и (2,0)).
    /// </summary>
    private bool IsPathClear(IBoardManager board, Vector3Int pos, Vector3Int dir)
    {
        // Определяем промежуточные клетки для L-образного хода
        List<Vector3Int> intermediatePositions = new List<Vector3Int>();
        int dx = dir.x;
        int dz = dir.z;

        // Для хода (2,1) или (-2,1) проверяем (1,0) или (-1,0)
        if (Mathf.Abs(dx) == 2 && Mathf.Abs(dz) == 1)
        {
            intermediatePositions.Add(pos + new Vector3Int(dx / 2, 0, 0)); // Промежуточная по x
        }
        // Для хода (1,2) или (-1,2) проверяем (0,1) или (0,-1)
        else if (Mathf.Abs(dx) == 1 && Mathf.Abs(dz) == 2)
        {
            intermediatePositions.Add(pos + new Vector3Int(0, 0, dz / 2)); // Промежуточная по z
        }

        // Проверяем, что все промежуточные клетки свободны от фигур и гор
        foreach (var midPos in intermediatePositions)
        {
            if (!board.IsWithinBounds(midPos) || board.IsMountain(midPos) || board.IsOccupied(midPos))
            {
                return false;
            }
        }

        return true;
    }

    public void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager)
    {
        Debug.Log($"HeavyHorseAttackStrategy: Executing melee attack on {target}");
        // Ближний бой: уничтожаем фигуру и перемещаемся
        boardManager.RemovePiece(target);
        piece.GetComponent<PieceAnimator>().MoveTo(target, () =>
        {
            boardManager.MovePiece(piece, piece.Position, target);
        });
    }
}