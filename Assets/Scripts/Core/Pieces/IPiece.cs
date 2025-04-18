using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Интерфейс для стратегий движения фигур.
/// </summary>
public interface IMovable
{
    List<Vector3Int> CalculateMoves(IBoardManager board, Piece piece);
}

/// <summary>
/// Интерфейс для стратегий атаки фигур.
/// </summary>
public interface IAttackable
{
    List<Vector3Int> CalculateAttacks(IBoardManager board, Piece piece);
    void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager);
}

/// <summary>
/// Интерфейс, определяющий общие свойства и методы для всех игровых фигур в 3D-пространстве.
/// Используется для обеспечения единообразного взаимодействия с фигурами в игре.
/// </summary>
public interface IPiece// отказались от интерфейса так как от него было больше вреда чем пользы
{
    /// <summary>
    /// Свойство, возвращающее текущую позицию фигуры в 3D-пространстве.
    /// Использует Vector3Int для целочисленных координат (X, Y, Z).
    /// В игре Y обычно равно 0, так как доска расположена в плоскости XZ.
    /// </summary>
    Vector3Int Position { get; }

    /// <summary>
    /// Свойство, указывающее, принадлежит ли фигура первому игроку (true) или второму (false).
    /// Используется для определения стороны, которой принадлежит фигура.
    /// </summary>
    bool IsPlayer1 { get; }

    /// <summary>
    /// Метод для инициализации фигуры после её создания.
    /// Задаёт принадлежность игроку (isPlayer1) и выполняет начальную настройку.
    /// </summary>
    /// <param name="isPlayer1">True, если фигура принадлежит первому игроку, иначе false.</param>
    void Initialize(bool isPlayer1, Material material);

    /// <summary>
    /// Метод для установки позиции фигуры на доске.
    /// Обновляет как логическую позицию (Vector3Int), так и физическое положение в 3D-пространстве.
    /// </summary>
    /// <param name="position">Новая позиция фигуры в координатах (X, Y, Z).</param>
    void SetPosition(Vector3Int position);

    /// <summary>
    /// Метод, возвращающий список доступных ходов для фигуры.
    /// Рассчитывает позиции, на которые фигура может переместиться, с учётом правил игры.
    /// </summary>
    /// <param name="board">Ссылка на IBoardManager для проверки состояния доски.</param>
    /// <returns>Список позиций (Vector3Int), куда фигура может пойти.</returns>
    List<Vector3Int> GetValidMoves(IBoardManager board);

    /// <summary>
    /// Метод, возвращающий список позиций, которые фигура может атаковать.
    /// Учитывает правила атаки конкретной фигуры (ближний или дальний бой).
    /// </summary>
    /// <param name="board">Ссылка на IBoardManager для проверки состояния доски.</param>
    /// <returns>Список позиций (Vector3Int), которые фигура может атаковать.</returns>
    List<Vector3Int> GetAttackMoves(IBoardManager board);
}

