using UnityEngine;

/// <summary>
/// Реализация интерфейса IBoardManager. Отвечает за управление состоянием игровой доски.
/// Следует принципу Single Responsibility (S из SOLID): только управление доской.
/// </summary>
public class BoardManager : MonoBehaviour, IBoardManager
{
    // Размер доски (например, 10x10 в плоскости XZ)
    private int boardSize;

    // 3D-массив для хранения фигур (вместо 2D используется Vector3Int для работы в 3D)
    private IPiece[,,] board;

    // Префаб плитки для визуализации доски в 3D
    [SerializeField] private GameObject tilePrefab;

    public void InitializeBoard(int size)
    {
        // Устанавливаем размер доски
        boardSize = size;

        // Инициализируем массив фигур размером size x size x 1 (высота пока не используется)
        board = new IPiece[size, 1, size];

        // Создаём визуальную доску в 3D-пространстве (плоскость XZ)
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                // Позиция плитки в 3D: Y = 0 (земля), X и Z определяют координаты
                Vector3 position = new Vector3(x, 0, z);

                // Создаём плитку на сцене
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                tile.name = $"Tile_{x}_{z}";
            }
        }

        Debug.Log($"3D Board initialized with size {size}x{size} on XZ plane.");
    }

    public bool IsWithinBounds(Vector3Int position)
    {
        // Проверяем, находится ли позиция в пределах доски (Y фиксировано на 0)
        return position.x >= 0 && position.x < boardSize &&
               position.y == 0 && // Пока используем только Y=0
               position.z >= 0 && position.z < boardSize;
    }

    public bool IsOccupied(Vector3Int position)
    {
        // Проверяем, есть ли фигура на заданной позиции
        return IsWithinBounds(position) && board[position.x, position.y, position.z] != null;
    }

    public void PlacePiece(IPiece piece, Vector3Int position)
    {
        // Размещаем фигуру на доске в 3D-пространстве
        if (IsWithinBounds(position))
        {
            board[position.x, position.y, position.z] = piece;
            piece.SetPosition(position); // Устанавливаем позицию фигуры
            Debug.Log($"Piece placed at {position}");
        }
    }

    public void RemovePiece(Vector3Int position)
    {
        // Удаляем фигуру с заданной позиции
        if (IsWithinBounds(position))
        {
            board[position.x, position.y, position.z] = null;
            Debug.Log($"Piece removed from {position}");
        }
    }

    public IPiece GetPieceAt(Vector3Int position)
    {
        // Возвращаем фигуру с заданной позиции или null, если клетка пуста
        return IsWithinBounds(position) ? board[position.x, position.y, position.z] : null;
    }
}