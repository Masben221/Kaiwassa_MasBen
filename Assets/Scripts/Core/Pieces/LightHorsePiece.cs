using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Класс для фигуры "Лёгкая кавалерия".
/// Реализует движение и атаку до 4 клеток по прямой.
/// </summary>
public class LightHorsePiece : Piece
{
    /// <summary>
    /// Настраивает стратегии движения и атаки для Лёгкой кавалерии.
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new LightHorseMoveStrategy();
        attackStrategy = new LightHorseAttackStrategy();
        Debug.Log("LightHorsePiece: Strategies set up.");
    }
}

/// <summary>
/// Стратегия движения для Лёгкой кавалерии.
/// Позволяет двигаться на 1-4 клетки по прямой (горизонталь/вертикаль).
/// </summary>
public class LightHorseMoveStrategy : IMovable
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
            for (int i = 1; i <= 4; i++)
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
/// Стратегия атаки для Лёгкой кавалерии.
/// Реализует ближний бой: атака на 1-4 клетки по прямой, занимает клетку противника.
/// Проверяет путь на наличие препятствий.
/// Предоставляет список всех потенциальных клеток атаки для подсказок (включая пустые и свои фигуры, исключая горы).
/// </summary>
public class LightHorseAttackStrategy : IAttackable
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

    /// <summary>
    /// Рассчитывает все потенциальные клетки, которые лёгкая кавалерия может атаковать, включая пустые и свои фигуры, исключая горы.
    /// Учитывает дальность 1-4 клетки по прямой, с проверкой свободного пути.
    /// </summary>
    public List<Vector3Int> CalculateAllAttacks(IBoardManager board, Piece piece)
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
                // Добавляем клетку, если она не содержит гору
                if (!board.IsMountain(newPos))
                {
                    attacks.Add(newPos);
                }
            }
        }

        return attacks;
    }

    public void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager)
    {
        Debug.Log($"LightHorseAttackStrategy: Executing melee attack on {target}");
        // Ближний бой: уничтожаем фигуру 
        boardManager.RemovePiece(target);       
    }
}