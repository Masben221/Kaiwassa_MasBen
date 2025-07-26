using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Класс для фигуры "Лучник".
/// Реализует движение на 1 клетку в любом направлении и дальнюю атаку на 3 клетки по прямой или диагонали.
/// Поддерживает два режима атаки: с проверкой прямой видимости и без неё (через препятствия).
/// </summary>
public class ArcherPiece : Piece
{
    [SerializeField]
    private bool requireClearPath = true; // Переключатель в инспекторе: true - требует прямую видимость, false - атакует через препятствия

    /// <summary>
    /// Настраивает стратегии движения и атаки для Лучника.
    /// Передаёт параметр requireClearPath в стратегию атаки.
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new ArcherMoveStrategy();
        attackStrategy = new ArcherAttackStrategy(requireClearPath);
        Debug.Log($"ArcherPiece: Strategies set up (RequireClearPath: {requireClearPath})");
    }
}

/// <summary>
/// Стратегия движения для Лучника.
/// Позволяет двигаться на 1 клетку в любом направлении (прямо или по диагонали), как у Арбалетчика.
/// </summary>
public class ArcherMoveStrategy : IMovable
{
    /// <summary>
    /// Рассчитывает допустимые ходы для Лучника.
    /// </summary>
    /// <param name="board">Интерфейс доски для проверки состояния.</param>
    /// <param name="piece">Фигура, для которой рассчитываются ходы.</param>
    /// <returns>Список клеток, куда Лучник может пойти.</returns>
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

        Debug.Log($"ArcherMoveStrategy: Calculated {moves.Count} moves for {piece.Type} at {pos}");
        return moves;
    }
}

/// <summary>
/// Стратегия атаки для Лучника.
/// Реализует дальний бой: атака на 1–3 клетки по прямой или диагонали.
/// Поддерживает два режима:
/// - С проверкой прямой видимости (для дистанции 2 и 3).
/// - Без проверки видимости (атака через препятствия).
/// Исключает горы из целей атаки.
/// </summary>
public class ArcherAttackStrategy : IAttackable
{
    private readonly bool requireClearPath; // Определяет, требуется ли прямая видимость для атаки

    /// <summary>
    /// Конструктор, принимающий параметр режима атаки.
    /// </summary>
    /// <param name="requireClearPath">true - требует прямую видимость, false - атакует через препятствия.</param>
    public ArcherAttackStrategy(bool requireClearPath)
    {
        this.requireClearPath = requireClearPath;
    }

    /// <summary>
    /// Рассчитывает клетки, которые Лучник может атаковать (только с вражескими фигурами).
    /// Проверяет прямую видимость, если requireClearPath = true.
    /// </summary>
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
                if (dx == 0 && dz == 0) continue;

                // Проверяем клетки на расстоянии 1, 2 и 3
                for (int distance = 1; distance <= 3; distance++)
                {
                    Vector3Int targetPos = pos + new Vector3Int(dx * distance, 0, dz * distance);
                    if (!board.IsWithinBounds(targetPos)) continue;

                    // Проверяем прямую видимость для дистанции 2 и 3, если требуется
                    bool hasClearPath = true;
                    if (requireClearPath && distance > 1)
                    {
                        for (int i = 1; i < distance; i++)
                        {
                            Vector3Int midPos = pos + new Vector3Int(dx * i, 0, dz * i);
                            if (board.IsBlocked(midPos))
                            {
                                hasClearPath = false;
                                break;
                            }
                        }
                    }

                    if (!hasClearPath) continue;

                    // Проверяем, есть ли вражеская фигура на целевой клетке и это не гора
                    if (board.IsOccupied(targetPos) &&
                        board.GetPieceAt(targetPos).IsPlayer1 != piece.IsPlayer1 &&
                        !board.IsMountain(targetPos))
                    {
                        attacks.Add(targetPos);
                    }
                }
            }
        }

        Debug.Log($"ArcherAttackStrategy: Calculated {attacks.Count} attacks for {piece.Type} at {pos}");
        return attacks;
    }

    /// <summary>
    /// Рассчитывает все потенциальные клетки атаки, включая пустые и свои фигуры, исключая горы.
    /// Учитывает прямую видимость, если requireClearPath = true.
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

                // Проверяем клетки на расстоянии 1, 2 и 3
                for (int distance = 1; distance <= 3; distance++)
                {
                    Vector3Int targetPos = pos + new Vector3Int(dx * distance, 0, dz * distance);
                    if (!board.IsWithinBounds(targetPos) || board.IsMountain(targetPos)) continue;

                    // Проверяем прямую видимость для дистанции 2 и 3, если требуется
                    bool hasClearPath = true;
                    if (requireClearPath && distance > 1)
                    {
                        for (int i = 1; i < distance; i++)
                        {
                            Vector3Int midPos = pos + new Vector3Int(dx * i, 0, dz * i);
                            if (board.IsBlocked(midPos))
                            {
                                hasClearPath = false;
                                break;
                            }
                        }
                    }

                    if (hasClearPath)
                    {
                        attacks.Add(targetPos);
                    }
                }
            }
        }

        Debug.Log($"ArcherAttackStrategy: Calculated {attacks.Count} potential attacks for {piece.Type} at {pos}");
        return attacks;
    }

    /// <summary>
    /// Выполняет дальнюю атаку на указанную клетку.
    /// Уничтожает вражескую фигуру, оставаясь на месте.
    /// </summary>
    public void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager, bool isRangedAttack)
    {
        Debug.Log($"ArcherAttackStrategy: Executing ranged attack from {piece.Position} to {target}");
        Piece targetPiece = boardManager.GetPieceAt(target);
        if (targetPiece != null)
        {
            if (targetPiece.Type == PieceType.Mountain)
            {
                Debug.LogWarning($"ArcherAttackStrategy: Cannot attack mountain at {target}!");
                return;
            }
            piece.SelectAttack(target, isRangedAttack);
            Debug.Log($"ArcherAttackStrategy: Removed piece {targetPiece.GetType().Name} at {target}");
        }
        else
        {
            Debug.LogWarning($"ArcherAttackStrategy: No piece at {target} to attack!");
        }
    }
}