using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Класс для фигуры "Дракон". Наследуется от базового класса Piece.
/// Реализует специфичное поведение дракона: движение и атака на расстоянии до 3 клеток.
/// </summary>
public class DragonPiece : Piece
{
    /// <summary>
    /// Метод Awake вызывается Unity при создании объекта на сцене.
    /// Здесь мы инициализируем дракона с принадлежностью игроку 1 (можно изменить при создании).
    /// </summary>
    private void Awake()
    {
        Initialize(true); // По умолчанию дракон принадлежит игроку 1
    }

    /// <summary>
    /// Настройка стратегий движения и атаки для дракона.
    /// Переопределяет абстрактный метод из базового класса Piece.
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new DragonMoveStrategy(); // Стратегия движения дракона
        attackStrategy = new DragonAttackStrategy(); // Стратегия атаки дракона
        Debug.Log("DragonPiece strategies set up.");
    }
}

/// <summary>
/// Стратегия движения для дракона.
/// Позволяет двигаться на расстояние до 3 клеток по прямой или диагонали, перепрыгивая препятствия.
/// </summary>
public class DragonMoveStrategy : IMovable
{
    public List<Vector3Int> CalculateMoves(IBoardManager board)
    {
        // Создаём список для хранения всех возможных ходов
        List<Vector3Int> moves = new List<Vector3Int>();

        // Получаем текущую позицию дракона (предполагаем, что он уже размещён на доске)
        // Здесь используется фиктивная позиция (0,0,0) для примера; реальная позиция берётся из объекта
        IPiece piece = board.GetPieceAt(new Vector3Int(0, 0, 0));
        Vector3Int pos = piece != null ? piece.Position : Vector3Int.zero;

        // Возможные направления движения по осям X и Z (в 3D плоскости XZ)
        int[] directions = { -1, 0, 1 };

        // Перебираем все комбинации направлений (прямые и диагонали)
        foreach (int dx in directions)
        {
            foreach (int dz in directions)
            {
                // Пропускаем случай, когда дракон остаётся на месте (dx=0, dz=0)
                if (dx == 0 && dz == 0) continue;

                // Проверяем все расстояния от 1 до 3 клеток в заданном направлении
                for (int i = 1; i <= 3; i++)
                {
                    // Новая позиция в 3D: X и Z изменяются, Y остаётся 0
                    Vector3Int newPos = pos + new Vector3Int(dx * i, 0, dz * i);

                    // Если позиция в пределах доски, добавляем её в список ходов
                    if (board.IsWithinBounds(newPos))
                    {
                        moves.Add(newPos); // Дракон перепрыгивает фигуры и горы
                    }
                    else
                    {
                        // Если вышли за пределы доски, прерываем это направление
                        break;
                    }
                }
            }
        }

        return moves;
    }
}

/// <summary>
/// Стратегия атаки для дракона.
/// Дракон может атаковать на расстоянии до 3 клеток по прямой или диагонали, перепрыгивая препятствия.
/// </summary>
public class DragonAttackStrategy : IAttackable
{
    public List<Vector3Int> CalculateAttacks(IBoardManager board)
    {
        // Создаём список для хранения всех возможных позиций атаки
        List<Vector3Int> attacks = new List<Vector3Int>();

        // Получаем текущую позицию дракона
        IPiece piece = board.GetPieceAt(new Vector3Int(0, 0, 0));
        Vector3Int pos = piece != null ? piece.Position : Vector3Int.zero;

        // Возможные направления атаки по осям X и Z
        int[] directions = { -1, 0, 1 };

        // Перебираем все направления
        foreach (int dx in directions)
        {
            foreach (int dz in directions)
            {
                if (dx == 0 && dz == 0) continue;

                // Проверяем расстояния от 1 до 3 клеток
                for (int i = 1; i <= 3; i++)
                {
                    Vector3Int newPos = pos + new Vector3Int(dx * i, 0, dz * i);

                    if (board.IsWithinBounds(newPos))
                    {
                        // Если клетка занята фигурой противника, добавляем её как цель атаки
                        if (board.IsOccupied(newPos) && board.GetPieceAt(newPos).IsPlayer1 != piece.IsPlayer1)
                        {
                            attacks.Add(newPos);
                        }
                    }
                    else
                    {
                        break; // Выход за пределы доски
                    }
                }
            }
        }

        return attacks;
    }
}