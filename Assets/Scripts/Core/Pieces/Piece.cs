using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Перечисление типов фигур.
/// </summary>
public enum PieceType
{
    King,
    Dragon,
    Elephant,
    HeavyCavalry,
    LightHorse,
    Spearman,
    Crossbowman,
    Rabble,
    Catapult,
    Trebuchet,
    Mountain // Добавлено для гор
}

/// <summary>
/// Абстрактный базовый класс для всех игровых фигур и гор.
/// Отвечает за позиционирование, инициализацию и делегирование движения/атаки через стратегии.
/// </summary>
public abstract class Piece : MonoBehaviour
{
    protected IMovable movementStrategy; // Стратегия движения фигуры
    protected IAttackable attackStrategy; // Стратегия атаки фигуры
    private Vector3Int position; // Позиция фигуры на доске (в клетках)
    private bool isPlayer1; // Принадлежность игроку (true = Игрок 1, false = Игрок 2)
    [SerializeField] private PieceType type; // Тип фигуры

    public Vector3Int Position => position; // Геттер для позиции
    public bool IsPlayer1 => isPlayer1; // Геттер для принадлежности игроку
    public PieceType Type => type; // Геттер для типа фигуры

    private void Awake()
    {
        SetupStrategies();
    }

    /// <summary>
    /// Абстрактный метод для настройки стратегий движения и атаки.
    /// Реализуется в дочерних классах (KingPiece, DragonPiece, MountainPiece и т.д.).
    /// </summary>
    protected abstract void SetupStrategies();

    /// <summary>
    /// Инициализирует фигуру: задаёт принадлежность игроку и материал.
    /// </summary>
    /// <param name="isPlayer1">true, если фигура принадлежит Игроку 1.</param>
    /// <param name="material">Материал для рендеринга (зелёный/красный).</param>
    public void Initialize(bool isPlayer1, Material material)
    {
        this.isPlayer1 = isPlayer1;
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material = material;
        }
        else
        {
            Debug.LogWarning($"Piece {GetType().Name}: No Renderer found");
        }
        Debug.Log($"Piece {GetType().Name} initialized for Player {(isPlayer1 ? 1 : 2)} with material {material?.name}");
    }

    /// <summary>
    /// Устанавливает позицию фигуры на доске.
    /// Преобразует клеточные координаты в мировые (y=0.5f для высоты над доской).
    /// </summary>
    /// <param name="newPosition">Новая позиция в клеточных координатах.</param>
    public void SetPosition(Vector3Int newPosition)
    {
        position = newPosition;
        transform.position = new Vector3(newPosition.x, 0.5f, newPosition.z); // y=0.5f для корректной высоты
        Debug.Log($"Piece {GetType().Name} set to position {newPosition} (world: {transform.position})");
    }

    /// <summary>
    /// Возвращает список допустимых ходов для фигуры.
    /// Делегирует вычисление стратегии движения.
    /// </summary>
    /// <param name="board">Интерфейс доски для проверки состояния.</param>
    /// <returns>Список клеток, куда фигура может пойти.</returns>
    public List<Vector3Int> GetValidMoves(IBoardManager board)
    {
        return movementStrategy?.CalculateMoves(board, this) ?? new List<Vector3Int>();
    }

    /// <summary>
    /// Возвращает список допустимых клеток для атаки.
    /// Делегирует вычисление стратегии атаки.
    /// </summary>
    /// <param name="board">Интерфейс доски для проверки состояния.</param>
    /// <returns>Список клеток, которые фигура может атаковать.</returns>
    public List<Vector3Int> GetAttackMoves(IBoardManager board)
    {
        return attackStrategy?.CalculateAttacks(board, this) ?? new List<Vector3Int>();
    }

    /// <summary>
    /// Выполняет атаку на указанную клетку.
    /// Делегирует выполнение стратегии атаки.
    /// </summary>
    /// <param name="target">Клетка для атаки.</param>
    /// <param name="boardManager">Интерфейс доски для изменения состояния.</param>
    public void Attack(Vector3Int target, IBoardManager boardManager)
    {
        if (attackStrategy != null)
        {
            Debug.Log($"Piece {GetType().Name} initiating attack on {target}");
            attackStrategy.ExecuteAttack(this, target, boardManager);
        }
        else
        {
            Debug.LogWarning($"Piece {GetType().Name}: No attack strategy assigned");
        }
    }
}
