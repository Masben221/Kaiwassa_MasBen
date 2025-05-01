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
/// Исключает горы из возможных целей атаки, так как они являются статическими препятствиями.
/// Предоставляет список всех потенциальных клеток атаки для подсказок (включая пустые и свои фигуры, исключая горы).
/// </summary>
public class DragonAttackStrategy : IAttackable
{
    /// <summary>
    /// Рассчитывает клетки, которые дракон может атаковать в текущий момент (только с вражескими фигурами).
    /// Проверяет прямую видимость и исключает горы.
    /// </summary>
    /// <param name="board">Интерфейс доски для проверки состояния.</param>
    /// <param name="piece">Фигура дракона.</param>
    /// <returns>Список клеток с вражескими фигурами, которые можно атаковать.</returns>
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

                    // Проверяем, есть ли вражеская фигура в конечной точке, и что это не гора
                    if (board.IsOccupied(newPos) &&
                        board.GetPieceAt(newPos).IsPlayer1 != piece.IsPlayer1 &&
                        board.GetPieceAt(newPos).Type != PieceType.Mountain)
                    {
                        attacks.Add(newPos);
                    }
                }
            }
        }
        return attacks;
    }

    /// <summary>
    /// Рассчитывает все потенциальные клетки, которые дракон может атаковать, включая пустые и свои фигуры, исключая горы.
    /// Учитывает прямую видимость (без гор или фигур на пути) и дальность 1-3 клетки.
    /// </summary>
    /// <param name="board">Интерфейс доски для проверки состояния.</param>
    /// <param name="piece">Фигура дракона.</param>
    /// <returns>Список всех потенциальных клеток атаки.</returns>
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

                    // Добавляем клетку, если она не содержит гору
                    if (!board.IsMountain(newPos))
                    {
                        attacks.Add(newPos);
                    }
                }
            }
        }
        return attacks;
    }

    /// <summary>
    /// Выполняет атаку дракона на указанную клетку.
    /// Удаляет вражескую фигуру, если она есть и не является горой.
    /// </summary>
    /// <param name="piece">Фигура дракона.</param>
    /// <param name="target">Целевая клетка для атаки.</param>
    /// <param name="boardManager">Интерфейс доски для изменения состояния.</param>
    public void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager)
    {
        Debug.Log($"DragonAttackStrategy: Executing ranged attack from {piece.Position} to {target} (piece: {piece.GetType().Name})");
        Piece targetPiece = boardManager.GetPieceAt(target);
        if (targetPiece != null && targetPiece.Type != PieceType.Mountain)
        {
            boardManager.RemovePiece(target);
            Debug.Log($"DragonAttackStrategy: Removed piece {targetPiece.GetType().Name} at {target}");
        }
        else
        {
            Debug.LogWarning($"DragonAttackStrategy: No valid piece at {target} to attack or target is a mountain!");
        }
        // НЕ вызываем MoveTo, чтобы дракон оставался на месте
    }
}