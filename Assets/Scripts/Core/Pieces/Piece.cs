using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Базовый класс для всех фигур. Реализует общую логику позиции, стратегий и визуального оформления.
/// </summary>
public abstract class Piece : MonoBehaviour, IPiece
{
    // Текущая позиция в 3D
    public Vector3Int Position { get; private set; }

    // Принадлежность игроку (true - игрок 1, false - игрок 2)
    public bool IsPlayer1 { get; private set; }

    // Стратегии движения и атаки задаются в подклассах
    protected IMovable movementStrategy;
    protected IAttackable attackStrategy;

    /// <summary>
    /// Инициализирует фигуру: задаёт принадлежность игроку и применяет материал.
    /// </summary>
    public void Initialize(bool isPlayer1, Material material)
    {
        IsPlayer1 = isPlayer1;
        SetupStrategies(); // Настраиваем стратегии движения и атаки

        // Применяем материал к фигуре
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null && material != null)
        {
            renderer.material = material;
        }

        // Поворачиваем фигуру второго игрока на 180 градусов
        if (!isPlayer1)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    /// <summary>
    /// Абстрактный метод для настройки стратегий в подклассах.
    /// </summary>
    protected abstract void SetupStrategies();

    /// <summary>
    /// Устанавливает позицию фигуры в 3D-пространстве.
    /// </summary>
    public void SetPosition(Vector3Int position)
    {
        Position = position;
        transform.position = new Vector3(position.x, 0.5f, position.z); // Y=0.5 для видимости над доской
    }

    public List<Vector3Int> GetValidMoves(IBoardManager board)
    {
        return movementStrategy.CalculateMoves(board);
    }

    public List<Vector3Int> GetAttackMoves(IBoardManager board)
    {
        return attackStrategy.CalculateAttacks(board);
    }
}