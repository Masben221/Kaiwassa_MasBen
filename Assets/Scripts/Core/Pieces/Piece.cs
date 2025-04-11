using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Базовый класс для всех фигур. Реализует общую логику позиции и стратегий.
/// Все конкретные фигуры (KingPiece, DragonPiece и т.д.) наследуются от этого класса.
/// </summary>
public abstract class Piece : MonoBehaviour, IPiece
{
    // Текущая позиция фигуры в 3D-пространстве
    public Vector3Int Position { get; private set; }

    // Принадлежность игроку (true для игрока 1, false для игрока 2)
    public bool IsPlayer1 { get; private set; }

    // Стратегии движения и атаки, которые будут задаваться в подклассах
    protected IMovable movementStrategy;
    protected IAttackable attackStrategy;

    /// <summary>
    /// Инициализация фигуры. Устанавливает принадлежность игроку и настраивает стратегии.
    /// </summary>
    /// <param name="isPlayer1">True, если фигура принадлежит первому игроку.</param>
    public void Initialize(bool isPlayer1)
    {
        IsPlayer1 = isPlayer1;
        SetupStrategies(); // Вызываем настройку стратегий
    }

    /// <summary>
    /// Абстрактный метод для настройки стратегий движения и атаки.
    /// Должен быть реализован в каждом подклассе (например, KingPiece, DragonPiece).
    /// </summary>
    protected abstract void SetupStrategies();

    /// <summary>
    /// Устанавливает позицию фигуры на доске и обновляет её положение в 3D-пространстве.
    /// </summary>
    /// <param name="position">Новая позиция в координатах (X, Y, Z).</param>
    public void SetPosition(Vector3Int position)
    {
        Position = position;
        transform.position = new Vector3(position.x, 0.5f, position.z); // Y=0.5 для видимости над доской
    }

    /// <summary>
    /// Возвращает список доступных ходов, делегируя расчёт стратегии движения.
    /// </summary>
    public List<Vector3Int> GetValidMoves(IBoardManager board)
    {
        return movementStrategy.CalculateMoves(board);
    }

    /// <summary>
    /// Возвращает список позиций для атаки, делегируя расчёт стратегии атаки.
    /// </summary>
    public List<Vector3Int> GetAttackMoves(IBoardManager board)
    {
        return attackStrategy.CalculateAttacks(board);
    }
}