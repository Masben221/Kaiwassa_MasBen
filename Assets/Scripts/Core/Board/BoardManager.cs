using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Интерфейс для управления игровой доской.
/// </summary>
public interface IBoardManager
{
    void InitializeBoard(int size); // Инициализация доски
    void PlacePiece(Piece piece, Vector3Int position); // Размещение фигуры
    Piece GetPieceAt(Vector3Int position); // Получение фигуры по позиции
    void RemovePiece(Vector3Int position); // Удаление фигуры
    void MovePiece(Piece piece, Vector3Int from, Vector3Int to); // Перемещение фигуры
    bool IsWithinBounds(Vector3Int position); // Проверка границ
    bool IsOccupied(Vector3Int position); // Проверка занятости клетки фигурами
    bool IsMountain(Vector3Int position); // Проверка, является ли клетка горой
    void PlaceMountains(int mountainsPerSide); // Размещение гор
    bool IsBlocked(Vector3Int position); // Проверка, заблокирована ли клетка
    void PlaceMountain(Vector3Int position, GameObject mountain); // Размещение горы
    Dictionary<Vector3Int, Piece> GetAllPieces(); // Получение всех фигур
    GameObject GetTileAt(Vector3Int position);
}

/// <summary>
/// Управляет игровой доской: размещение фигур, гор, состояние клеток.
/// </summary>
public class BoardManager : MonoBehaviour, IBoardManager
{
    [SerializeField] private GameObject tilePrefab; // Префаб плитки
    [SerializeField] private Material jadeMaterial; // Материал для нефритовых клеток
    [SerializeField] private Material carnelianMaterial; // Материал для сердоликовых клеток
    [SerializeField] private Material lapisMaterial; // Материал для клеток с ляпис-лазурью

    private readonly Dictionary<Vector3Int, Piece> pieces = new Dictionary<Vector3Int, Piece>();
    private readonly Dictionary<Vector3Int, GameObject> tiles = new Dictionary<Vector3Int, GameObject>();
    private readonly Dictionary<Vector3Int, GameObject> mountains = new Dictionary<Vector3Int, GameObject>();
    private int boardSize;

    public void InitializeBoard(int size)
    {
        if (tilePrefab == null || jadeMaterial == null || carnelianMaterial == null || lapisMaterial == null)
        {
            Debug.LogError("Tile prefab or materials not assigned in BoardManager!");
            return;
        }

        boardSize = size;

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                Vector3Int position = new Vector3Int(x, 0, z);
                GameObject tile = Instantiate(tilePrefab, new Vector3(x, 0, z), Quaternion.identity);
                tiles[position] = tile;

                Material material;
                if ((x + z) % 3 == 0)
                    material = jadeMaterial;
                else if ((x + z) % 3 == 1)
                    material = carnelianMaterial;
                else
                    material = lapisMaterial;

                tile.GetComponent<Renderer>().material = material;
            }
        }

        Debug.Log($"Board initialized with size {size}x{size}");
    }

    public void PlaceMountains(int mountainsPerSide)
    {
        Debug.LogWarning("BoardManager.PlaceMountains is deprecated. Use PiecePlacementManager.PlaceMountains instead.");
    }

    /// <summary>
    /// Размещает гору на указанной позиции.
    /// </summary>
    public void PlaceMountain(Vector3Int position, GameObject mountain)
    {
        if (IsBlocked(position))
        {
            Debug.LogWarning($"BoardManager: Position {position} is already blocked!");
            return;
        }
        mountains[position] = mountain;
        Debug.Log($"BoardManager: Placed mountain at {position}");
    }

    public void PlacePiece(Piece piece, Vector3Int position)
    {
        if (IsBlocked(position))
        {
            Debug.LogWarning($"Position {position} is blocked by a piece or mountain!");
            return;
        }

        if (pieces.ContainsKey(position))
        {
            Debug.LogWarning($"Position {position} was occupied, removing existing piece.");
            RemovePiece(position);
        }

        pieces[position] = piece;
        piece.SetPosition(position);
        Debug.Log($"BoardManager: Placed piece {piece.GetType().Name} at {position}");
    }

    public void MovePiece(Piece piece, Vector3Int from, Vector3Int to)
    {
        if (!pieces.TryGetValue(from, out Piece existingPiece) || existingPiece != piece)
        {
            Debug.LogWarning($"No piece found at {from} or piece mismatch.");
            return;
        }

        pieces.Remove(from);

        if (IsMountain(to))
        {
            Debug.LogWarning($"Position {to} is blocked by a mountain!");
            pieces[from] = piece;
            piece.SetPosition(from);
            return;
        }

        if (pieces.ContainsKey(to))
        {
            Debug.LogWarning($"Position {to} was occupied, removing existing piece.");
            RemovePiece(to);
        }

        pieces[to] = piece;
        piece.SetPosition(to);
        Debug.Log($"BoardManager: Moved piece {piece.GetType().Name} from {from} to {to}");
    }

    public Piece GetPieceAt(Vector3Int position)
    {
        pieces.TryGetValue(position, out Piece piece);
        return piece;
    }

    public void RemovePiece(Vector3Int position)
    {
        if (pieces.TryGetValue(position, out Piece piece))
        {
            pieces.Remove(position);
            if (piece != null && piece.gameObject != null)
            {
                Destroy(piece.gameObject);
                Debug.Log($"BoardManager: Removed piece {piece.GetType().Name} at {position}");
            }
            else
            {
                Debug.LogWarning($"Piece at {position} was null or already destroyed.");
            }
        }
        else
        {
            Debug.LogWarning($"No piece found at {position} to remove.");
        }
    }

    public bool IsWithinBounds(Vector3Int position)
    {
        return position.x >= 0 && position.x < boardSize &&
               position.z >= 0 && position.z < boardSize &&
               position.y == 0;
    }

    public bool IsOccupied(Vector3Int position)
    {
        return pieces.ContainsKey(position);
    }

    public bool IsMountain(Vector3Int position)
    {
        return mountains.ContainsKey(position);
    }

    public bool IsBlocked(Vector3Int position)
    {
        return IsOccupied(position) || IsMountain(position);
    }

    /// <summary>
    /// Возвращает копию словаря всех фигур на доске.
    /// </summary>
    public Dictionary<Vector3Int, Piece> GetAllPieces()
    {
        return new Dictionary<Vector3Int, Piece>(pieces);
    }

    /// <summary>
    /// Возвращает объект плитки по указанной позиции.
    /// </summary>
    /// <param name="position">Позиция на доске.</param>
    /// <returns>GameObject плитки или null, если плитка не найдена.</returns>
    public GameObject GetTileAt(Vector3Int position)
    {
        tiles.TryGetValue(position, out GameObject tile);
        return tile;
    }
}