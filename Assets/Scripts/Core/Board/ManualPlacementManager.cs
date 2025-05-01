using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Zenject;

/// <summary>
/// ��������� ������ ������������ ����� � ���.
/// </summary>
public class ManualPlacementManager : MonoBehaviour, IPiecePlacementManager
{
    [Inject] private IBoardManager boardManager;
    [Inject] private IPieceFactory pieceFactory;

    private Dictionary<PieceType, int> player1Pieces = new Dictionary<PieceType, int>();
    private Dictionary<PieceType, int> player2Pieces = new Dictionary<PieceType, int>();

    public int GetMountainsPerSide { get; private set; }

    /// <summary>
    /// �������������� �������� ����� � ��� ��� ����� �������.
    /// </summary>
    /// <param name="mountainsPerSide">���������� ��� �� �������.</param>
    public void Initialize(int mountainsPerSide)
    {
        GetMountainsPerSide = mountainsPerSide;
        player1Pieces = new Dictionary<PieceType, int>
        {
            { PieceType.King, 1 }, { PieceType.Dragon, 1 }, { PieceType.Elephant, 2 },
            { PieceType.HeavyCavalry, 2 }, { PieceType.LightHorse, 3 }, { PieceType.Spearman, 3 },
            { PieceType.Crossbowman, 3 }, { PieceType.Rabble, 3 }, { PieceType.Catapult, 1 },
            { PieceType.Trebuchet, 1 }, { PieceType.Mountain, mountainsPerSide }
        };
        player2Pieces = new Dictionary<PieceType, int>(player1Pieces);
    }

    /// <summary>
    /// ���������, ����� �� ���������� ������ ��� ���� �� ��������� �������.
    /// </summary>
    /// <param name="isPlayer1">true, ���� ��� ������ 1.</param>
    /// <param name="position">���������� ������.</param>
    /// <param name="type">��� ������ (King, Mountain � �.�.).</param>
    /// <param name="isMove">true, ���� ��� �����������.</param>
    /// <returns>true, ���� ���������� ��������.</returns>
    public bool CanPlace(bool isPlayer1, Vector3Int position, PieceType type, bool isMove = false)
    {
        if (!boardManager.IsWithinBounds(position) || boardManager.IsOccupied(position))
            return false;

        // ����������� ���: z 0�3 ��� ������ 1, z 6�9 ��� ������ 2
        if (isPlayer1 && (position.z < 0 || position.z > 3)) return false;
        if (!isPlayer1 && (position.z < 6 || position.z > 9)) return false;

        var pieces = isPlayer1 ? player1Pieces : player2Pieces;
        return isMove || (pieces.ContainsKey(type) && pieces[type] > 0);
    }

    // ���������� ����� ��� �������������
    public bool CanPlace(bool isPlayer1, Vector3Int position, bool isMountain)
    {
        return CanPlace(isPlayer1, position, isMountain ? PieceType.Mountain : PieceType.King, false);
    }

    /// <summary>
    /// ���������, ����� �� ����������� ������ �� ����� �������.
    /// </summary>
    /// <param name="isPlayer1">true, ���� ��� ������ 1.</param>
    /// <param name="type">��� ������.</param>
    /// <param name="newPosition">����� �������.</param>
    /// <returns>true, ���� ����������� ��������.</returns>
    public bool CanMove(bool isPlayer1, PieceType type, Vector3Int newPosition)
    {
        if (!boardManager.IsWithinBounds(newPosition) || boardManager.IsOccupied(newPosition))
            return false;

        if (isPlayer1 && (newPosition.z < 0 || newPosition.z > 3)) return false;
        if (!isPlayer1 && (newPosition.z < 6 || newPosition.z > 9)) return false;

        return true;
    }

    /// <summary>
    /// ��������� ������ ��� ���� �� �����.
    /// </summary>
    /// <param name="isPlayer1">true, ���� ��� ������ 1.</param>
    /// <param name="position">���������� ������.</param>
    /// <param name="type">��� ������ (King, Mountain � �.�.).</param>
    /// <param name="isMove">true, ���� ��� �����������.</param>
    /// <returns>true, ���� ���������� �������.</returns>
    public bool PlacePieceOrMountain(bool isPlayer1, Vector3Int position, PieceType type, bool isMove = false)
    {
        if (!CanPlace(isPlayer1, position, type, isMove))
        {
            Debug.LogWarning($"Cannot place {type} at {position}");
            return false;
        }

        Piece piece = pieceFactory.CreatePiece(type, isPlayer1, position);
        if (piece != null)
        {
            boardManager.PlacePiece(piece, position);
            if (!isMove)
            {
                var pieces = isPlayer1 ? player1Pieces : player2Pieces;
                pieces[type]--;
            }
            return true;
        }

        Debug.LogWarning($"Failed to place {type} at {position}");
        return false;
    }

    public bool PlacePieceOrMountain(bool isPlayer1, Vector3Int position, PieceType type)
    {
        return PlacePieceOrMountain(isPlayer1, position, type, false);
    }

    /// <summary>
    /// ������� ������ ��� ���� � �����.
    /// </summary>
    /// <param name="isPlayer1">true, ���� ��� ������ 1.</param>
    /// <param name="position">���������� ������.</param>
    /// <param name="type">��� ������.</param>
    /// <returns>true, ���� �������� �������.</returns>
    public bool RemovePiece(bool isPlayer1, Vector3Int position, PieceType type)
    {
        if (!boardManager.IsWithinBounds(position))
        {
            Debug.LogWarning($"Cannot remove piece at {position}");
            return false;
        }

        var piece = boardManager.GetPieceAt(position);
        if (piece != null && piece.IsPlayer1 == isPlayer1 && piece.Type == type)
        {
            boardManager.RemovePiece(position);
            var pieces = isPlayer1 ? player1Pieces : player2Pieces;
            pieces[type]++;
            Destroy(piece.gameObject);
            return true;
        }

        Debug.LogWarning($"No piece {type} at {position}");
        return false;
    }

    /// <summary>
    /// ������� ��������� ������ � �����.
    /// </summary>
    /// <param name="piece">������ ��� ��������.</param>
    /// <returns>true, ���� �������� �������.</returns>
    public bool RemovePiece(Piece piece)
    {
        if (!boardManager.IsWithinBounds(piece.Position))
        {
            Debug.LogWarning($"Cannot remove piece at {piece.Position}");
            return false;
        }

        var existingPiece = boardManager.GetPieceAt(piece.Position);
        if (existingPiece == piece)
        {
            boardManager.RemovePiece(piece.Position);
            var pieces = piece.IsPlayer1 ? player1Pieces : player2Pieces;
            pieces[piece.Type]++;
            Destroy(piece.gameObject);
            return true;
        }

        Debug.LogWarning($"No piece {piece.Type} at {piece.Position}");
        return false;
    }

    /// <summary>
    /// ���������� ������ � ����� ������� �� ������.
    /// </summary>
    /// <param name="piece">������ ��� �����������.</param>
    /// <param name="from">�������� �������.</param>
    /// <param name="to">����� �������.</param>
    /// <returns>true, ���� ����������� �������.</returns>
    public bool MovePiece(Piece piece, Vector3Int from, Vector3Int to)
    {
        if (!boardManager.IsWithinBounds(from) || !boardManager.IsWithinBounds(to))
            return false;

        var existingPiece = boardManager.GetPieceAt(from);
        if (existingPiece != piece || boardManager.IsOccupied(to))
        {
            Debug.LogWarning($"Cannot move {piece.Type} from {from} to {to}");
            return false;
        }

        bool isPlayer1 = piece.IsPlayer1;
        if (isPlayer1 && (to.z < 0 || to.z > 3)) return false;
        if (!isPlayer1 && (to.z < 6 || to.z > 9)) return false;

        boardManager.MovePiece(piece, from, to);
        return true;
    }

    /// <summary>
    /// ���������� ���������� ���������� ����� ���������� ����.
    /// </summary>
    /// <param name="isPlayer1">true, ���� ��� ������ 1.</param>
    /// <param name="type">��� ������.</param>
    /// <returns>���������� ���������� �����.</returns>
    public int GetRemainingCount(bool isPlayer1, PieceType type)
    {
        var pieces = isPlayer1 ? player1Pieces : player2Pieces;
        return pieces.ContainsKey(type) ? pieces[type] : 0;
    }

    /// <summary>
    /// ���������, ��������� �� ����������� ��� ������.
    /// </summary>
    /// <param name="isPlayer1">true, ���� ��� ������ 1.</param>
    /// <returns>true, ���� ��� ������ ���������.</returns>
    public bool HasCompletedPlacement(bool isPlayer1)
    {
        var pieces = isPlayer1 ? player1Pieces : player2Pieces;
        return pieces.Values.All(count => count == 0);
    }

    /// <summary>
    /// ���������, �������� �� ������.
    /// </summary>
    /// <param name="isPlayer1">true, ���� ��� ������ 1.</param>
    /// <returns>true, ���� ������ �� ��������.</returns>
    public bool IsKingNotPlaced(bool isPlayer1)
    {
        var pieces = isPlayer1 ? player1Pieces : player2Pieces;
        return pieces.ContainsKey(PieceType.King) && pieces[PieceType.King] > 0;
    }
       
    public void PlacePiecesForPlayer(bool isPlayer1, int selectedMountains) { }
}