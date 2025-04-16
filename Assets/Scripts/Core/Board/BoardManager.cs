using UnityEngine;

/// <summary>
/// Интерфейс, определяющий методы для управления игровой доской.
/// Используется для соблюдения принципа Dependency Inversion (D из SOLID).
/// </summary>
public interface IBoardManager
{
    // Инициализирует доску заданного размера в 3D-пространстве
    void InitializeBoard(int size);

    // Проверяет, находится ли позиция в пределах доски
    bool IsWithinBounds(Vector3Int position);

    // Проверяет, занята ли клетка фигурой
    bool IsOccupied(Vector3Int position);

    // Размещает фигуру на заданной 3D-позиции
    void PlacePiece(IPiece piece, Vector3Int position);

    // Удаляет фигуру с заданной 3D-позиции
    void RemovePiece(Vector3Int position);

    // Возвращает фигуру, находящуюся на заданной позиции
    IPiece GetPieceAt(Vector3Int position);
}

/// <summary>
/// Управляет игровой доской в 3D-пространстве (плоскость XZ).
/// Хранит состояние клеток и визуализирует доску с плитками разных цветов.
/// </summary>
public class BoardManager : MonoBehaviour, IBoardManager
{
    // Размер доски (например, 10x10)
    private int boardSize;

    // Массив для хранения фигур в 3D (Y=0 для плоскости XZ)
    private IPiece[,,] board;

    // Префаб плитки для визуализации доски
    [SerializeField] private GameObject tilePrefab;

    // Массив материалов для плиток (нефрит, сердолик, ляпис-лазурь)
    [SerializeField] private Material[] tileMaterials;

    // Случайный генератор для выбора материалов
    private System.Random random = new System.Random();

    /// <summary>
    /// Инициализирует доску заданного размера, создавая плитки с разными материалами.
    /// </summary>
    public void InitializeBoard(int size)
    {
        boardSize = size;
        board = new IPiece[size, 1, size]; // Y=1, так как используем только плоскость XZ

        // Проверяем, что материалы назначены
        if (tileMaterials == null || tileMaterials.Length < 3)
        {
            Debug.LogError("BoardManager: Необходимо назначить минимум 3 материала (нефрит, сердолик, ляпис-лазурь)!");
            return;
        }

        // Создаём визуальную доску
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                Vector3 position = new Vector3(x, 0, z);
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                tile.name = $"Tile_{x}_{z}";

                // Шахматный порядок: выбираем материал на основе суммы x+z
                Renderer renderer = tile.GetComponent<Renderer>();
                if (renderer != null)
                {
                    int materialIndex = (x + z) % 3; // Цикл между 0, 1, 2
                    renderer.material = tileMaterials[materialIndex];
                }
            }
        }
        Debug.Log($"Board initialized: {size}x{size} with colored tiles.");
    }

    public bool IsWithinBounds(Vector3Int position)
    {
        // Проверяем, находится ли позиция в пределах доски
        return position.x >= 0 && position.x < boardSize &&
        position.y == 0 && // Фиксируем Y=0
        position.z >= 0 && position.z < boardSize;
    }

    public bool IsOccupied(Vector3Int position)
    {
        return IsWithinBounds(position) && board[position.x, position.y, position.z] != null;
    }

    public void PlacePiece(IPiece piece, Vector3Int position)
    {
        if (IsWithinBounds(position))
        {
            board[position.x, position.y, position.z] = piece;
            piece.SetPosition(position);
        }
    }

    public void RemovePiece(Vector3Int position)
    {
        if (IsWithinBounds(position))
        {
            board[position.x, position.y, position.z] = null;
        }
    }

    public IPiece GetPieceAt(Vector3Int position)
    {
        return IsWithinBounds(position) ? board[position.x, position.y, position.z] : null;
    }
}