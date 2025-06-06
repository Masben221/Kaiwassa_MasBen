using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Класс для фигуры "Дракон".
/// Реализует движение (до 3 клеток, перепрыгивая препятствия) и атаку с переключением между дальним и ближним боем.
/// Дальний бой: атака на 1-3 клетки с прямой видимостью, без перемещения.
/// Ближний бой: прыжок на клетку с вражеской фигурой, уничтожение и перемещение.
/// </summary>
public class DragonPiece : Piece
{
    [SerializeField]
    private bool useRangedAttack = true; // Переключатель в инспекторе: true - дальний бой, false - ближний бой

    /// <summary>
    /// Настраивает стратегии движения и атаки для дракона.
    /// Передаёт параметр useRangedAttack в стратегию атаки.
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new DragonMoveStrategy();
        attackStrategy = new DragonAttackStrategy(useRangedAttack);
        Debug.Log($"DragonPiece: Strategies set up (attackStrategy: {attackStrategy.GetType().Name}, UseRangedAttack: {useRangedAttack})");
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
/// Поддерживает два режима:
/// - Дальний бой: атака на 1-3 клетки по прямой или диагонали, требует прямую видимость, остаётся на месте.
/// - Ближний бой: прыжок на клетку с вражеской фигурой (1-3 клетки), игнорирует препятствия, перемещается на цель.
/// Исключает горы из возможных целей атаки в обоих режимах.
/// </summary>
public class DragonAttackStrategy : IAttackable
{
    private readonly bool useRangedAttack; // Определяет режим атаки: true - дальний бой, false - ближний бой

    /// <summary>
    /// Конструктор, принимающий параметр режима атаки.
    /// </summary>
    /// <param name="useRangedAttack">true - дальний бой, false - ближний бой.</param>
    public DragonAttackStrategy(bool useRangedAttack)
    {
        this.useRangedAttack = useRangedAttack;
    }

    /// <summary>
    /// Рассчитывает клетки, которые дракон может атаковать в текущий момент (только с вражескими фигурами).
    /// Для дальнего боя требует прямую видимость, для ближнего боя игнорирует препятствия.
    /// Исключает горы в обоих режимах.
    /// </summary>
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

                    // Проверяем наличие вражеской фигуры и исключаем горы
                    if (board.IsOccupied(newPos) &&
                        board.GetPieceAt(newPos).IsPlayer1 != piece.IsPlayer1 &&
                        !board.IsMountain(newPos))
                    {
                        if (useRangedAttack)
                        {
                            // Дальний бой: проверяем прямую видимость
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
                            if (isPathClear)
                            {
                                attacks.Add(newPos);
                            }
                        }
                        else
                        {
                            // Ближний бой: добавляем цель без проверки пути
                            attacks.Add(newPos);
                        }
                    }
                }
            }
        }
        return attacks;
    }

    /// <summary>
    /// Рассчитывает все потенциальные клетки, которые дракон может атаковать, включая пустые и свои фигуры, исключая горы.
    /// Для дальнего боя требует прямую видимость, для ближнего боя игнорирует препятствия.
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
                for (int i = 1; i <= 3; i++)
                {
                    Vector3Int newPos = pos + new Vector3Int(dx * i, 0, dz * i);
                    if (!board.IsWithinBounds(newPos))
                    {
                        break;
                    }

                    if (!board.IsMountain(newPos))
                    {
                        if (useRangedAttack)
                        {
                            // Дальний бой: проверяем прямую видимость
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
                            if (isPathClear)
                            {
                                attacks.Add(newPos);
                            }
                        }
                        else
                        {
                            // Ближний бой: добавляем клетку без проверки пути
                            attacks.Add(newPos);
                        }
                    }
                }
            }
        }
        return attacks;
    }

    /// <summary>
    /// Выполняет атаку дракона на указанную клетку.
    /// - Дальний бой: уничтожает фигуру, остаётся на месте.
    /// - Ближний бой: уничтожает фигуру и перемещается на её место.
    /// Горы не атакуются.
    /// </summary>
    public void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager)
    {
        // Проверка, что цель не является горой
        if (boardManager.IsMountain(target))
        {
            Debug.LogWarning($"DragonAttackStrategy: Cannot attack mountain at {target}!");
            return;
        }

        Piece targetPiece = boardManager.GetPieceAt(target);
        if (targetPiece == null)
        {
            Debug.LogWarning($"DragonAttackStrategy: No piece at {target} to attack!");
            return;
        }

        if (useRangedAttack)
        {
            // Дальний бой: уничтожаем фигуру, остаёмся на месте
            Debug.Log($"DragonAttackStrategy: Executing ranged attack from {piece.Position} to {target} (piece: {piece.GetType().Name})");
            boardManager.RemovePiece(target);
        }
        else
        {
            // Ближний бой: уничтожаем фигуру и перемещаемся
            Debug.Log($"DragonAttackStrategy: Executing melee attack from {piece.Position} to {target} (piece: {piece.GetType().Name})");
            boardManager.RemovePiece(target);
            piece.GetComponent<PieceAnimator>().MoveTo(target, () =>
            {
                boardManager.MovePiece(piece, piece.Position, target);
            });
        }
    }
}