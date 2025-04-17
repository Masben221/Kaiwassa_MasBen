using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Базовый класс для всех фигур. Реализует общую логику позиции, стратегий и визуального оформления.
/// </summary>
public abstract class Piece : MonoBehaviour, IPiece
{
    // Текущая позиция в 3D
    public Vector3Int Position { get; private set; }

    // Принадлежность игроку
    public bool IsPlayer1 { get; private set; }

    // Ссылка на GameObject фигуры
    public GameObject GameObject => gameObject;

    // Стратегии движения и атаки задаются в подклассах
    protected IMovable movementStrategy;
    protected IAttackable attackStrategy;

    /// <summary>
    /// Инициализируется автоматически при создании объекта.
    /// Вызывает настройку стратегий.
    /// </summary>
    protected virtual void Awake()
    {
        SetupStrategies();
    }

    /// <summary>
    /// Инициализирует фигуру: задаёт принадлежность игроку и применяет материал.
    /// </summary>
    public void Initialize(bool isPlayer1, Material material)
    {
        IsPlayer1 = isPlayer1;

        // Применяем материал к дочернему Renderer
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null && material != null)
        {
            renderer.material = material;
        }
        else
        {
            Debug.LogWarning($"No Renderer found in children of {gameObject.name} or material is null.");
        }

        // Поворачиваем фигуру второго игрока на 180 градусов
        if (!isPlayer1)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        Debug.Log($"Initialized piece for Player {(isPlayer1 ? 1 : 2)}");
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
        transform.position = new Vector3(position.x, 0.5f, position.z); // Угол клетки, Y=0.5 для видимости
    }

    public List<Vector3Int> GetValidMoves(IBoardManager board)
    {
        return movementStrategy.CalculateMoves(board, this);
    }

    public List<Vector3Int> GetAttackMoves(IBoardManager board)
    {
        return attackStrategy.CalculateAttacks(board, this);
    }
}