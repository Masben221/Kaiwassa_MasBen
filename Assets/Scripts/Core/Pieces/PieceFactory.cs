using UnityEngine;

/// <summary>
/// Фабрика для создания игровых фигур. Использует префабы, заданные в инспекторе.
/// </summary>
public class PieceFactory : MonoBehaviour
{
    // Префабы фигур, которые будут задаваться в Unity Inspector
    [SerializeField] private GameObject dragonPrefab;
    [SerializeField] private GameObject kingPrefab;

    /// <summary>
    /// Создаёт фигуру заданного типа, инициализирует её и устанавливает начальную позицию.
    /// </summary>
    /// <param name="type">Тип фигуры (King, Dragon и т.д.).</param>
    /// <param name="isPlayer1">Принадлежность игроку.</param>
    /// <param name="position">Начальная позиция на доске.</param>
    /// <returns>Созданная фигура, реализующая IPiece.</returns>
    public IPiece CreatePiece(PieceType type, bool isPlayer1, Vector3Int position)
    {
        // Выбираем префаб в зависимости от типа фигуры
        GameObject prefab = type switch
        {
            PieceType.Dragon => dragonPrefab,
            PieceType.King => kingPrefab,
            _ => throw new System.NotImplementedException($"Piece type {type} not implemented")
        };

        // Создаём экземпляр фигуры из префаба
        GameObject instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        IPiece piece = instance.GetComponent<IPiece>();

        // Инициализируем фигуру и устанавливаем её позицию
        piece.Initialize(isPlayer1);
        piece.SetPosition(position);

        Debug.Log($"Created {type} for Player {(isPlayer1 ? 1 : 2)} at {position}");
        return piece;
    }
}