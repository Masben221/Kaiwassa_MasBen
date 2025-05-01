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
            new Vector3Int(2, 0, 1),
            new Vector3Int(2, 0, -1),
            new Vector3Int(-2, 0, 1),
            new Vector3Int(-2, 0, -1),
            new Vector3Int(1, 0, 2),
            new Vector3Int(1, 0, -2),
            new Vector3Int(-1, 0, 2),
            new Vector3Int(-1, 0, -2)
        };

        foreach (var dir in directions)
        {
            Vector3Int newPos = pos + dir;
            if (board.IsWithinBounds(newPos) && !board.IsMountain(newPos) && !board.IsOccupied(newPos))
            {
                moves.Add(newPos);
            }
        }

        return moves;
    }
}

/// <summary>
/// Стратегия атаки для Тяжёлой кавалерии.
/// Реализует ближний бой: L-образная атака, требует свободный путь до цели (не перепрыгивает фигуры или горы).
/// Предоставляет список всех потенциальных клеток атаки для подсказок (включая пустые и свои фигуры, исключая горы).
/// </summary>
public class HeavyHorseAttackStrategy : IAttackable
{
    public List<Vector3Int> CalculateAttacks(IBoardManager board, Piece piece)
    {
        List<Vector3Int> attacks = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        Vector3Int[] directions = {
            new Vector3Int(2, 0, 1),
            new Vector3Int(2, 0, -1),
            new Vector3Int(-2, 0, 1),
            new Vector3Int(-2, 0, -1),
            new Vector3Int(1, 0, 2),
            new Vector3Int(1, 0, -2),
            new Vector3Int(-1, 0, 2),
            new Vector3Int(-1, 0, -2)
        };

        foreach (var dir in directions)
        {
            Vector3Int targetPos = pos + dir;
            if (board.IsWithinBounds(targetPos) &&
                board.IsOccupied(targetPos) &&
                board.GetPieceAt(targetPos).IsPlayer1 != piece.IsPlayer1)
            {
                if (IsAttackPossible(board, pos, dir, targetPos))
                {
                    attacks.Add(targetPos);
                }
            }
        }

        return attacks;
    }

    /// <summary>
    /// Проверяет, возможна ли атака на целевую клетку.
    /// Атака возможна, если хотя бы один из L-образных путей к цели свободен.
    /// </summary>
    private bool IsAttackPossible(IBoardManager board, Vector3Int startPos, Vector3Int dir, Vector3Int targetPos)
    {
        Vector3Int[] intermediatePositions1 = new Vector3Int[2];
        Vector3Int[] intermediatePositions2 = new Vector3Int[2];
        int dx = dir.x;
        int dz = dir.z;

        if (Mathf.Abs(dx) == 2 && Mathf.Abs(dz) == 1)
        {
            intermediatePositions1[0] = startPos + new Vector3Int(dx / 2, 0, 0);
            intermediatePositions1[1] = startPos + new Vector3Int(dx, 0, 0);
            intermediatePositions2[0] = startPos + new Vector3Int(0, 0, dz);
            intermediatePositions2[1] = startPos + new Vector3Int(dx / 2, 0, dz);
        }
        else if (Mathf.Abs(dx) == 1 && Mathf.Abs(dz) == 2)
        {
            intermediatePositions1[0] = startPos + new Vector3Int(dx, 0, 0);
            intermediatePositions1[1] = startPos + new Vector3Int(dx, 0, dz / 2);
            intermediatePositions2[0] = startPos + new Vector3Int(0, 0, dz / 2);
            intermediatePositions2[1] = startPos + new Vector3Int(0, 0, dz);
        }

        // Проверяем первый путь
        bool path1Clear = true;
        foreach (var pos in intermediatePositions1)
        {
            if (board.IsBlocked(pos))
            {
                path1Clear = false;
                break;
            }
        }

        // Проверяем второй путь
        bool path2Clear = true;
        foreach (var pos in intermediatePositions2)
        {
            if (board.IsBlocked(pos))
            {
                path2Clear = false;
                break;
            }
        }

        return path1Clear || path2Clear; // Атака возможна, если хотя бы один путь свободен
    }

    /// <summary>
    /// Рассчитывает все потенциальные клетки, которые тяжёлая кавалерия может атаковать, включая пустые и свои фигуры, исключая горы.
    /// Учитывает L-образную атаку с проверкой свободного пути.
    /// </summary>
    public List<Vector3Int> CalculateAllAttacks(IBoardManager board, Piece piece)
    {
        List<Vector3Int> attacks = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        Vector3Int[] directions = {
            new Vector3Int(2, 0, 1),
            new Vector3Int(2, 0, -1),
            new Vector3Int(-2, 0, 1),
            new Vector3Int(-2, 0, -1),
            new Vector3Int(1, 0, 2),
            new Vector3Int(1, 0, -2),
            new Vector3Int(-1, 0, 2),
            new Vector3Int(-1, 0, -2)
        };

        foreach (var dir in directions)
        {
            Vector3Int targetPos = pos + dir;
            if (board.IsWithinBounds(targetPos) && !board.IsMountain(targetPos))
            {
                if (IsAttackPossible(board, pos, dir, targetPos))
                {
                    attacks.Add(targetPos);
                }
            }
        }

        return attacks;
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