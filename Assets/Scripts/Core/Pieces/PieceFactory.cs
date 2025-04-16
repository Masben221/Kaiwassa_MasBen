using UnityEngine;

// <summary>
/// Интерфейс фабрики для создания фигур.
/// Следует паттерну Factory для централизованного создания объектов.
/// </summary>
public interface IPieceFactory
{
    IPiece CreatePiece(PieceType type, bool isPlayer1, Vector3Int position); // Метод создания фигуры
}

/// <summary>
/// Перечисление всех типов фигур в игре.
/// Используется для определения, какую фигуру создавать.
/// </summary>
public enum PieceType { King, Dragon, Elephant, HeavyHorse, LightHorse, Spearman, Crossbowman, Rabble, Catapult, Trebuchet }

/// <summary>
/// Фабрика для создания фигур. Задаёт префабы и материалы для каждого игрока.
/// </summary>
public class PieceFactory : MonoBehaviour
{
    // Префабы фигур, задаются в инспекторе
    [SerializeField] private GameObject dragonPrefab;
    [SerializeField] private GameObject kingPrefab;

    // Материалы для фигур игроков
    [SerializeField] private Material player1Material; // Для игрока 1
    [SerializeField] private Material player2Material; // Для игрока 2

    /// <summary>
    /// Создаёт фигуру заданного типа с учётом принадлежности игрока.
    /// </summary>
    public IPiece CreatePiece(PieceType type, bool isPlayer1, Vector3Int position)
    {
        // Выбираем префаб в зависимости от типа фигуры
        GameObject prefab = type switch
        {
            PieceType.Dragon => dragonPrefab,
            PieceType.King => kingPrefab,
            _ => throw new System.NotImplementedException($"Piece type {type} not implemented")
        };

        // Создаём экземпляр из префаба
        GameObject instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        IPiece piece = instance.GetComponent<IPiece>();

        // Выбираем материал в зависимости от игрока
        Material material = isPlayer1 ? player1Material : player2Material;

        // Инициализируем фигуру с нужным материалом
        piece.Initialize(isPlayer1, material);
        piece.SetPosition(position);

        Debug.Log($"Created {type} for Player {(isPlayer1 ? 1 : 2)} at {position}");
        return piece;
    }
}