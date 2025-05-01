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
/// Определяет методы для расчёта текущих атак, всех потенциальных атак и выполнения атаки.
/// </summary>
public interface IAttackable
{
    /// <summary>
    /// Рассчитывает клетки, которые фигура может атаковать в текущий момент (обычно только с вражескими фигурами).
    /// </summary>
    /// <param name="board">Интерфейс доски для проверки состояния.</param>
    /// <param name="piece">Фигура, для которой рассчитываются атаки.</param>
    /// <returns>Список клеток, которые можно атаковать.</returns>
    List<Vector3Int> CalculateAttacks(IBoardManager board, Piece piece);

    /// <summary>
    /// Рассчитывает все потенциальные клетки атаки фигуры, включая пустые и свои фигуры, исключая горы и клетки вне доски.
    /// Используется для подсказок, показывающих угрозы от фигуры.
    /// </summary>
    /// <param name="board">Интерфейс доски для проверки состояния.</param>
    /// <param name="piece">Фигура, для которой рассчитываются атаки.</param>
    /// <returns>Список всех потенциальных клеток атаки.</returns>
    List<Vector3Int> CalculateAllAttacks(IBoardManager board, Piece piece);

    /// <summary>
    /// Выполняет атаку на указанную клетку.
    /// </summary>
    /// <param name="piece">Фигура, выполняющая атаку.</param>
    /// <param name="target">Целевая клетка для атаки.</param>
    /// <param name="boardManager">Интерфейс доски для изменения состояния.</param>
    void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager);
}