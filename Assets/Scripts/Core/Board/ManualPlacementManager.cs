using UnityEngine;
using System.Collections.Generic;
using Zenject;

/// <summary>
/// ��������� ������ ������������ ����� � ��� �� �����.
/// �������� �� �������� ������������ ����������, ���������� ���������� ����� � �����������.
/// </summary>
public class ManualPlacementManager : MonoBehaviour, IPiecePlacementManager
{
    // �����������, ������������� ����� Zenject
    [Inject] private IBoardManager boardManager; // �������� ����� ��� �������� ������ � ���������� �����
    [Inject] private IPieceFactory pieceFactory; // ������� ��� �������� �����

    // �������� ���������� ����� ��� ������� ������
    private Dictionary<PieceType, int> player1Pieces = new Dictionary<PieceType, int>(); // ������ ������ 1
    private Dictionary<PieceType, int> player2Pieces = new Dictionary<PieceType, int>(); // ������ ������ 2

    // ���������� ��� �� ������� (������������� ����� UI)
    private int mountainsPerSide;

    // �������� ��� ��������� ���������� ��� �� �������
    public int GetMountainsPerSide => mountainsPerSide;

    /// <summary>
    /// �������������� �������� ����� � ��� ��� ����� �������.
    /// </summary>
    /// <param name="mountainsPerSide">���������� ��� �� �������.</param>
    public void Initialize(int mountainsPerSide)
    {
        this.mountainsPerSide = mountainsPerSide;
        // �������������� �������� ����� ��� ������ 1
        player1Pieces = new Dictionary<PieceType, int>
        {
            { PieceType.King, 1 },
            { PieceType.Dragon, 1 },
            { PieceType.Elephant, 2 },
            { PieceType.HeavyCavalry, 2 },
            { PieceType.LightHorse, 2 }, // ��������� �� 2
            { PieceType.Spearman, 2 },   // ��������� �� 2
            { PieceType.Crossbowman, 2 }, // ��������� �� 2
            { PieceType.Rabble, 2 },     // ��������� �� 2
            { PieceType.Catapult, 1 },
            { PieceType.Trebuchet, 1 },
            { PieceType.Swordsman, 2 },  // ����� ������: ������
            { PieceType.Archer, 2 },     // ����� ������: ������
            { PieceType.Mountain, mountainsPerSide }
        };
        // �������� �������� ��� ������ 2
        player2Pieces = new Dictionary<PieceType, int>(player1Pieces);
        Debug.Log($"ManualPlacementManager: Initialized with {mountainsPerSide} mountains per side.");
    }

    /// <summary>
    /// ���������, ����� �� ���������� ������ ��� ���� �� ��������� �������.
    /// </summary>
    /// <param name="isPlayer1">True, ���� ��������� ����� 1.</param>
    /// <param name="position">���������� ������ �� �����.</param>
    /// <param name="type">��� ������ (��������, King, Mountain, Swordsman, Archer).</param>
    /// <param name="isMove">True, ���� ��� ����������� (�� ������ �� �������).</param>
    /// <returns>True, ���� ���������� ��������.</returns>
    public bool CanPlace(bool isPlayer1, Vector3Int position, PieceType type, bool isMove = false)
    {
        // ���������, ��� ������� ��������� � �������� ����� � �� ������
        if (!boardManager.IsWithinBounds(position) || boardManager.IsOccupied(position))
        {
            Debug.Log($"ManualPlacementManager: Cannot place {type} at {position} - out of bounds or occupied.");
            return false;
        }

        // ������������ ����: z 0�3 ��� ������ 1, z 6�9 ��� ������ 2
        if (isPlayer1 && (position.z < 0 || position.z > 3))
        {
            Debug.Log($"ManualPlacementManager: Cannot place {type} at {position} - out of zone for Player 1.");
            return false;
        }
        if (!isPlayer1 && (position.z < 6 || position.z > 9))
        {
            Debug.Log($"ManualPlacementManager: Cannot place {type} at {position} - out of zone for Player 2.");
            return false;
        }

        // ���������, ���� �� ��������� ������ ���������� ���� (���� ��� �� �����������)
        var pieces = isPlayer1 ? player1Pieces : player2Pieces;
        if (!isMove && (!pieces.ContainsKey(type) || pieces[type] <= 0))
        {
            Debug.Log($"ManualPlacementManager: Cannot place {type} - none remaining for Player {(isPlayer1 ? 1 : 2)}.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// ���������, ����� �� ����������� ������ �� ����� �������.
    /// </summary>
    /// <param name="isPlayer1">True, ���� ���������� ����� 1.</param>
    /// <param name="type">��� ������.</param>
    /// <param name="newPosition">����� �������.</param>
    /// <returns>True, ���� ����������� ��������.</returns>
    public bool CanMove(bool isPlayer1, PieceType type, Vector3Int newPosition)
    {
        // ���������, ��� ������� � �������� ����� � �� ������
        if (!boardManager.IsWithinBounds(newPosition) || boardManager.IsOccupied(newPosition))
        {
            Debug.Log($"ManualPlacementManager: Cannot move {type} to {newPosition} - out of bounds or occupied.");
            return false;
        }

        // ������������ ����: z 0�3 ��� ������ 1, z 6�9 ��� ������ 2
        if (isPlayer1 && (newPosition.z < 0 || newPosition.z > 3))
        {
            Debug.Log($"ManualPlacementManager: Cannot move {type} to {newPosition} - out of zone for Player 1.");
            return false;
        }
        if (!isPlayer1 && (newPosition.z < 6 || newPosition.z > 9))
        {
            Debug.Log($"ManualPlacementManager: Cannot move {type} to {newPosition} - out of zone for Player 2.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// ��������� ������ ��� ���� �� �����.
    /// ������������ ������ ��� ������ ����������� ����� UI.
    /// </summary>
    /// <param name="isPlayer1">True, ���� ��������� ����� 1.</param>
    /// <param name="position">���������� ������ �� �����.</param>
    /// <param name="type">��� ������ (��������, King, Mountain, Swordsman, Archer).</param>
    /// <param name="isMove">True, ���� ��� ����������� (�� ��������� �������).</param>
    /// <returns>True, ���� ���������� �������.</returns>
    public bool PlacePieceOrMountain(bool isPlayer1, Vector3Int position, PieceType type, bool isMove = false)
    {
        // ���������, ����� �� ���������� ������
        if (!CanPlace(isPlayer1, position, type, isMove))
        {
            Debug.LogWarning($"ManualPlacementManager: Cannot place {type} at {position} for Player {(isPlayer1 ? 1 : 2)}.");
            return false;
        }

        // ������ ������ � ��������� � �� �����
        Piece piece = pieceFactory.CreatePiece(type, isPlayer1, position);
        if (piece != null)
        {
            boardManager.PlacePiece(piece, position);
            // ��������� ������� ������ ���� ��� �� �����������
            if (!isMove)
            {
                var pieces = isPlayer1 ? player1Pieces : player2Pieces;
                pieces[type]--;
                Debug.Log($"ManualPlacementManager: Placed {type} at {position} for Player {(isPlayer1 ? 1 : 2)}. Remaining: {pieces[type]}.");
            }
            return true;
        }

        Debug.LogWarning($"ManualPlacementManager: Failed to create {type} at {position} for Player {(isPlayer1 ? 1 : 2)}.");
        return false;
    }

    /// <summary>
    /// ��������� ������� ���������� ����� ���������� ����.
    /// ������������ ��� ������������� ��������� ����� ��������� ���������.
    /// </summary>
    /// <param name="isPlayer1">True, ���� ��� ������ 1.</param>
    /// <param name="type">��� ������.</param>
    /// <returns>True, ���� ������� ������� ��������.</returns>
    public bool DecreasePieceCount(bool isPlayer1, PieceType type)
    {
        var pieces = isPlayer1 ? player1Pieces : player2Pieces;
        if (!pieces.ContainsKey(type) || pieces[type] <= 0)
        {
            Debug.LogWarning($"ManualPlacementManager: Cannot decrease count for {type} - none remaining for Player {(isPlayer1 ? 1 : 2)}.");
            return false;
        }

        pieces[type]--;
        Debug.Log($"ManualPlacementManager: Decreased count for {type} for Player {(isPlayer1 ? 1 : 2)}. Remaining: {pieces[type]}.");
        return true;
    }

    /// <summary>
    /// ������� ������ ��� ���� � ����� � ����������� �������.
    /// </summary>
    /// <param name="isPlayer1">True, ���� ������� ����� 1.</param>
    /// <param name="position">���������� ������ �� �����.</param>
    /// <param name="type">��� ������.</param>
    /// <returns>True, ���� �������� �������.</returns>
    public bool RemovePiece(bool isPlayer1, Vector3Int position, PieceType type)
    {
        if (!boardManager.IsWithinBounds(position))
        {
            Debug.LogWarning($"ManualPlacementManager: Cannot remove piece at {position} - out of bounds.");
            return false;
        }

        var piece = boardManager.GetPieceAt(position);
        if (piece != null && piece.IsPlayer1 == isPlayer1 && piece.Type == type)
        {
            boardManager.RemovePiece(position);
            var pieces = isPlayer1 ? player1Pieces : player2Pieces;
            pieces[type]++;
            Destroy(piece.gameObject);
            Debug.Log($"ManualPlacementManager: Removed {type} at {position} for Player {(isPlayer1 ? 1 : 2)}. Remaining: {pieces[type]}.");
            return true;
        }

        Debug.LogWarning($"ManualPlacementManager: No piece {type} at {position} for Player {(isPlayer1 ? 1 : 2)}.");
        return false;
    }

    /// <summary>
    /// ������� ��������� ������ � ����� � ����������� �������.
    /// </summary>
    /// <param name="piece">������ ��� ��������.</param>
    /// <returns>True, ���� �������� �������.</returns>
    public bool RemovePiece(Piece piece)
    {
        if (!boardManager.IsWithinBounds(piece.Position))
        {
            Debug.LogWarning($"ManualPlacementManager: Cannot remove piece at {piece.Position} - out of bounds.");
            return false;
        }

        var existingPiece = boardManager.GetPieceAt(piece.Position);
        if (existingPiece == piece)
        {
            boardManager.RemovePiece(piece.Position);
            var pieces = piece.IsPlayer1 ? player1Pieces : player2Pieces;
            pieces[piece.Type]++;
            Destroy(piece.gameObject);
            Debug.Log($"ManualPlacementManager: Removed {piece.Type} at {piece.Position} for Player {(piece.IsPlayer1 ? 1 : 2)}. Remaining: {pieces[piece.Type]}.");
            return true;
        }

        Debug.LogWarning($"ManualPlacementManager: No piece {piece.Type} at {piece.Position}.");
        return false;
    }

    /// <summary>
    /// ���������� ������ � ����� ������� �� ������.
    /// </summary>
    /// <param name="piece">������ ��� �����������.</param>
    /// <param name="from">�������� �������.</param>
    /// <param name="to">����� �������.</param>
    /// <returns>True, ���� ����������� �������.</returns>
    public bool MovePiece(Piece piece, Vector3Int from, Vector3Int to)
    {
        if (!boardManager.IsWithinBounds(from) || !boardManager.IsWithinBounds(to))
        {
            Debug.LogWarning($"ManualPlacementManager: Cannot move {piece.Type} from {from} to {to} - out of bounds.");
            return false;
        }

        var existingPiece = boardManager.GetPieceAt(from);
        if (existingPiece != piece || boardManager.IsOccupied(to))
        {
            Debug.LogWarning($"ManualPlacementManager: Cannot move {piece.Type} from {from} to {to} - piece mismatch or target occupied.");
            return false;
        }

        bool isPlayer1 = piece.IsPlayer1;
        if (isPlayer1 && (to.z < 0 || to.z > 3))
        {
            Debug.LogWarning($"ManualPlacementManager: Cannot move {piece.Type} to {to} - out of zone for Player 1.");
            return false;
        }
        if (!isPlayer1 && (to.z < 6 || to.z > 9))
        {
            Debug.LogWarning($"ManualPlacementManager: Cannot move {piece.Type} to {to} - out of zone for Player 2.");
            return false;
        }

        boardManager.MovePiece(piece, from, to);
        Debug.Log($"ManualPlacementManager: Moved {piece.Type} from {from} to {to} for Player {(isPlayer1 ? 1 : 2)}.");
        return true;
    }

    /// <summary>
    /// ���������� ���������� ���������� ����� ���������� ����.
    /// </summary>
    /// <param name="isPlayer1">True, ���� ��� ������ 1.</param>
    /// <param name="type">��� ������.</param>
    /// <returns>���������� ���������� �����.</returns>
    public int GetRemainingCount(bool isPlayer1, PieceType type)
    {
        var pieces = isPlayer1 ? player1Pieces : player2Pieces;
        return pieces.ContainsKey(type) ? pieces[type] : 0;
    }

    /// <summary>
    /// ���������, ��������� �� ����������� ��� ������ (��� �������� ����� 0).
    /// </summary>
    /// <param name="isPlayer1">True, ���� ��� ������ 1.</param>
    /// <returns>True, ���� ��� ������ ���������.</returns>
    public bool HasCompletedPlacement(bool isPlayer1)
    {
        var pieces = isPlayer1 ? player1Pieces : player2Pieces;
        foreach (var count in pieces.Values)
        {
            if (count > 0) return false;
        }
        return true;
    }

    /// <summary>
    /// ���������, �������� �� ������.
    /// </summary>
    /// <param name="isPlayer1">True, ���� ��� ������ 1.</param>
    /// <returns>True, ���� ������ ��� �� ��������.</returns>
    public bool IsKingNotPlaced(bool isPlayer1)
    {
        var pieces = isPlayer1 ? player1Pieces : player2Pieces;
        return pieces.ContainsKey(PieceType.King) && pieces[PieceType.King] > 0;
    }

    /// <summary>
    /// �� �������������� � ������ �����������.
    /// ���������� ������ ��� �������������� ����������� ����� PiecePlacementManager.
    /// </summary>
    public void PlacePiecesForPlayer(bool isPlayer1, int selectedMountains)
    {
        Debug.LogWarning("ManualPlacementManager: PlacePiecesForPlayer not supported in manual placement.");
    }
}