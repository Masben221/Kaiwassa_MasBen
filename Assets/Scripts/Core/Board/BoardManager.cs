using UnityEngine;
using Zenject;
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
}

/// <summary>
/// Управляет игровой доской: размещение фигур, гор, состояние клеток.
/// </summary>
public class BoardManager : MonoBehaviour, IBoardManager
{
    [SerializeField] private GameObject tilePrefab; // Префаб плитки
    [SerializeField] private GameObject mountainPrefab; // Префаб горы
    [SerializeField] private Material jadeMaterial; // Материал для нефритовых клеток
    [SerializeField] private Material carnelianMaterial; // Материал для сердоликовых клеток
    [SerializeField] private Material lapisMaterial; // Материал для клеток с ляпис-лазурью

    private readonly Dictionary<Vector3Int, Piece> pieces = new Dictionary<Vector3Int, Piece>();
    private readonly Dictionary<Vector3Int, GameObject> tiles = new Dictionary<Vector3Int, GameObject>();
    private readonly Dictionary<Vector3Int, GameObject> mountains = new Dictionary<Vector3Int, GameObject>();
    private int boardSize;

    /// <summary>
    /// Инициализирует доску заданного размера с клетками в шахматном порядке.
    /// </summary>
    public void InitializeBoard(int size)
    {
        if (tilePrefab == null || mountainPrefab == null || jadeMaterial == null || carnelianMaterial == null || lapisMaterial == null)
        {
            Debug.LogError("Tile prefab, mountain prefab, or materials not assigned in BoardManager!");
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

                // Шахматный порядок с тремя материалами
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

    /// <summary>
    /// Размещает указанное количество гор на каждой половине доски.
    /// </summary>
    public void PlaceMountains(int mountainsPerSide)
    {
        // Половинная доска для игрока 1 (z = 0–4)
        List<Vector3Int> player1Positions = new List<Vector3Int>();
        for (int x = 0; x < boardSize; x++)
        {
            for (int z = 0; z < boardSize / 2; z++)
            {
                Vector3Int pos = new Vector3Int(x, 0, z);
                if (!pieces.ContainsKey(pos) && !mountains.ContainsKey(pos))
                {
                    player1Positions.Add(pos);
                }
            }
        }

        // Половинная доска для игрока 2 (z = 5–9)
        List<Vector3Int> player2Positions = new List<Vector3Int>();
        for (int x = 0; x < boardSize; x++)
        {
            for (int z = boardSize / 2; z < boardSize; z++)
            {
                Vector3Int pos = new Vector3Int(x, 0, z);
                if (!pieces.ContainsKey(pos) && !mountains.ContainsKey(pos))
                {
                    player2Positions.Add(pos);
                }
            }
        }

        // Размещаем горы для игрока 1
        mountainsPerSide = Mathf.Min(mountainsPerSide, player1Positions.Count);
        for (int i = 0; i < mountainsPerSide; i++)
        {
            int index = UnityEngine.Random.Range(0, player1Positions.Count);
            Vector3Int pos = player1Positions[index];
            GameObject mountain = Instantiate(mountainPrefab, new Vector3(pos.x, 0.5f, pos.z), Quaternion.identity);
            mountains[pos] = mountain;
            player1Positions.RemoveAt(index);
            Debug.Log($"Placed mountain for Player 1 at {pos}");
        }

        // Размещаем горы для игрока 2
        mountainsPerSide = Mathf.Min(mountainsPerSide, player2Positions.Count);
        for (int i = 0; i < mountainsPerSide; i++)
        {
            int index = UnityEngine.Random.Range(0, player2Positions.Count);
            Vector3Int pos = player2Positions[index];
            GameObject mountain = Instantiate(mountainPrefab, new Vector3(pos.x, 0.5f, pos.z), Quaternion.identity);
            mountains[pos] = mountain;
            player2Positions.RemoveAt(index);
            Debug.Log($"Placed mountain for Player 2 at {pos}");
        }
    }

    /// <summary>
    /// Размещает фигуру на доске в указанной позиции.
    /// </summary>
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

    /// <summary>
    /// Перемещает фигуру из одной позиции в другую.
    /// </summary>
    public void MovePiece(Piece piece, Vector3Int from, Vector3Int to)
    {
        //Debug.Log($"BoardManager: Attempting to move piece {piece.GetType().Name} from {from} to {to}");

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

    /// <summary>
    /// Возвращает фигуру на указанной позиции или null, если клетка пуста.
    /// </summary>
    public Piece GetPieceAt(Vector3Int position)
    {
        pieces.TryGetValue(position, out Piece piece);
        return piece;
    }

    /// <summary>
    /// Удаляет фигуру с указанной позиции.
    /// </summary>
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

    /// <summary>
    /// Проверяет, находится ли позиция в пределах доски.
    /// </summary>
    public bool IsWithinBounds(Vector3Int position)
    {
        return position.x >= 0 && position.x < boardSize &&
               position.z >= 0 && position.z < boardSize &&
               position.y == 0;
    }

    /// <summary>
    /// Проверяет, занята ли клетка фигурой.
    /// </summary>
    public bool IsOccupied(Vector3Int position)
    {
        return pieces.ContainsKey(position);
    }

    /// <summary>
    /// Проверяет, является ли клетка горой.
    /// </summary>
    public bool IsMountain(Vector3Int position)
    {
        return mountains.ContainsKey(position);
    }

    /// <summary>
    /// Проверяет, заблокирована ли клетка фигурой или горой.
    /// </summary>
    public bool IsBlocked(Vector3Int position)
    {
        return IsOccupied(position) || IsMountain(position);
    }
}