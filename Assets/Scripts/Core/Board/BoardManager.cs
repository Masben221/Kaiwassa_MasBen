using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Интерфейс для управления игровой доской.
/// </summary>
public interface IBoardManager
{
    void InitializeBoard(int size);
    void PlacePiece(Piece piece, Vector3Int position);
    Piece GetPieceAt(Vector3Int position);
    void RemovePiece(Vector3Int position);
    void MovePiece(Piece piece, Vector3Int from, Vector3Int to);
    bool IsWithinBounds(Vector3Int position);
    bool IsOccupied(Vector3Int position);
    bool IsMountain(Vector3Int position);
    bool IsBlocked(Vector3Int position);
    Dictionary<Vector3Int, Piece> GetAllPieces();
    GameObject GetTileAt(Vector3Int position);
}

/// <summary>
/// Управляет игровой доской: размещение фигур и гор, состояние клеток.
/// </summary>
public class BoardManager : MonoBehaviour, IBoardManager
{
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Material jadeMaterial;
    [SerializeField] private Material carnelianMaterial;
    [SerializeField] private Material lapisMaterial;

    private readonly Dictionary<Vector3Int, Piece> pieces = new Dictionary<Vector3Int, Piece>();
    private readonly Dictionary<Vector3Int, GameObject> tiles = new Dictionary<Vector3Int, GameObject>();
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

    public void PlacePiece(Piece piece, Vector3Int position)
    {
        if (IsOccupied(position))
        {
            Debug.LogWarning($"Position {position} is already occupied!");
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

        if (IsOccupied(to))
        {
            Debug.LogWarning($"Position {to} is already occupied!");
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
                // ИСПРАВЛЕНИЕ: Вызываем только анимацию смерти
                PieceAnimator animator = piece.GetComponent<PieceAnimator>();
                if (animator != null)
                {
                    animator.AnimateDeath(() => // НОВЫЙ метод
                    {
                        Destroy(piece.gameObject);
                        Debug.Log($"BoardManager: Removed piece {piece.GetType().Name} at {position} after death animation");
                    });
                }
                else
                {
                    Destroy(piece.gameObject);
                    Debug.LogWarning($"BoardManager: No PieceAnimator on piece at {position}, removed immediately");
                }
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
        return pieces.TryGetValue(position, out Piece piece) && piece.Type == PieceType.Mountain;
    }

    public bool IsBlocked(Vector3Int position)
    {
        return IsOccupied(position);
    }

    public Dictionary<Vector3Int, Piece> GetAllPieces()
    {
        return new Dictionary<Vector3Int, Piece>(pieces);
    }

    public GameObject GetTileAt(Vector3Int position)
    {
        tiles.TryGetValue(position, out GameObject tile);
        return tile;
    }
}