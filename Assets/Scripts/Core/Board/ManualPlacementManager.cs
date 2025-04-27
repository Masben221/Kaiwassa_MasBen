using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Zenject;

/// <summary>
/// Управляет логикой ручной расстановки фигур и гор.
/// Реализует интерфейс IPiecePlacementManager для совместимости с системой.
/// </summary>
public class ManualPlacementManager : MonoBehaviour, IPiecePlacementManager
{
    [Inject] private IBoardManager boardManager;
    [Inject] private IPieceFactory pieceFactory;

    private Dictionary<PieceType, int> player1Pieces = new Dictionary<PieceType, int>();
    private Dictionary<PieceType, int> player2Pieces = new Dictionary<PieceType, int>();

    public int GetMountainsPerSide { get; private set; }

    public void Initialize(int mountainsPerSide)
    {
        this.GetMountainsPerSide = mountainsPerSide;

        player1Pieces = new Dictionary<PieceType, int>
        {
            { PieceType.King, 1 },
            { PieceType.Dragon, 1 },
            { PieceType.Elephant, 2 },
            { PieceType.HeavyCavalry, 2 },
            { PieceType.LightHorse, 3 },
            { PieceType.Spearman, 3 },
            { PieceType.Crossbowman, 3 },
            { PieceType.Rabble, 3 },
            { PieceType.Catapult, 1 },
            { PieceType.Trebuchet, 1 },
            { PieceType.Mountain, mountainsPerSide }
        };
        player2Pieces = new Dictionary<PieceType, int>(player1Pieces);
        Debug.Log($"ManualPlacementManager: Initialized with {mountainsPerSide} mountains per side.");
    }

    public bool CanPlace(bool isPlayer1, Vector3Int position, bool isMountain)
    {
        if (!boardManager.IsWithinBounds(position) || boardManager.IsOccupied(position))
            return false;

        if (isPlayer1 && (position.z < 0 || position.z > 3))
            return false;
        if (!isPlayer1 && (position.z < 6 || position.z > 9))
            return false;

        var pieces = isPlayer1 ? player1Pieces : player2Pieces;
        PieceType type = isMountain ? PieceType.Mountain : PieceType.King;
        return pieces.ContainsKey(type) && pieces[type] > 0;
    }

    public bool PlacePieceOrMountain(bool isPlayer1, Vector3Int position, PieceType type, bool isMountain)
    {
        if (!CanPlace(isPlayer1, position, isMountain))
        {
            Debug.LogWarning($"ManualPlacementManager: Cannot place {(isMountain ? "mountain" : type.ToString())} at {position} for Player {(isPlayer1 ? 1 : 2)}");
            return false;
        }

        Piece piece = isMountain ? pieceFactory.CreateMountain(position) : pieceFactory.CreatePiece(type, isPlayer1, position);
        if (piece != null)
        {
            boardManager.PlacePiece(piece, position);
            var pieces = isPlayer1 ? player1Pieces : player2Pieces;
            pieces[type]--;
            Debug.Log($"ManualPlacementManager: Placed {(isMountain ? "mountain" : type.ToString())} at {position} for Player {(isPlayer1 ? 1 : 2)}");
            return true;
        }

        Debug.LogWarning($"ManualPlacementManager: Failed to place {(isMountain ? "mountain" : type.ToString())} at {position}");
        return false;
    }

    public bool RemovePiece(bool isPlayer1, Vector3Int position, PieceType type)
    {
        if (!boardManager.IsWithinBounds(position))
        {
            Debug.LogWarning($"ManualPlacementManager: Cannot remove piece at {position} (out of bounds)");
            return false;
        }

        var piece = boardManager.GetPieceAt(position);
        if (piece != null && piece.IsPlayer1 == isPlayer1 && piece.Type == type)
        {
            boardManager.RemovePiece(position);
            var pieces = isPlayer1 ? player1Pieces : player2Pieces;
            if (pieces.ContainsKey(type))
            {
                pieces[type]++;
                Debug.Log($"ManualPlacementManager: Removed {type} at {position} for Player {(isPlayer1 ? 1 : 2)}, remaining: {pieces[type]}");
                return true;
            }
            else
            {
                Debug.LogWarning($"ManualPlacementManager: Type {type} not found in pieces dictionary for Player {(isPlayer1 ? 1 : 2)}");
                return false;
            }
        }
        else
        {
            Debug.LogWarning($"ManualPlacementManager: No piece of type {type} found at {position} for Player {(isPlayer1 ? 1 : 2)}");
            return false;
        }
    }

    public int GetRemainingCount(bool isPlayer1, PieceType type, bool isMountain)
    {
        var pieces = isPlayer1 ? player1Pieces : player2Pieces;
        PieceType targetType = isMountain ? PieceType.Mountain : type;
        return pieces.ContainsKey(targetType) ? pieces[targetType] : 0;
    }

    public bool HasCompletedPlacement(bool isPlayer1)
    {
        var pieces = isPlayer1 ? player1Pieces : player2Pieces;
        return pieces.Values.All(count => count == 0);
    }

    public bool IsKingNotPlaced(bool isPlayer1)
    {
        var pieces = isPlayer1 ? player1Pieces : player2Pieces;
        return pieces.ContainsKey(PieceType.King) && pieces[PieceType.King] > 0;
    }

    public void PlaceMountains(int mountainsPerSide) { /* Заглушка */ }
    public void PlacePiecesForPlayer(bool isPlayer1) { /* Заглушка */ }
}