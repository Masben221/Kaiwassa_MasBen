using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Zenject;

/// <summary>
/// ��������� ������� ������ ����������� ����� � ���.
/// ��������� ��������� IPiecePlacementManager ��� ������������� � ��������.
/// </summary>
public class ManualPlacementManager : MonoBehaviour, IPiecePlacementManager
{
    [Inject] private IBoardManager boardManager; // ��������� ��� ���������� ������
    [Inject] private IPieceFactory pieceFactory; // ������� ��� �������� ����� � ���

    private Dictionary<PieceType, int> player1Pieces = new Dictionary<PieceType, int>(); // ������ ������ 1
    private Dictionary<PieceType, int> player2Pieces = new Dictionary<PieceType, int>(); // ������ ������ 2
    private int mountainsPerSide; // ���������� ��� �� �������
    private int player1MountainsRemaining; // ���������� ���� ��� ������ 1
    private int player2MountainsRemaining; // ���������� ���� ��� ������ 2

    /// <summary>
    /// ������ ��� ���������� ��� �� �������.
    /// </summary>
    public int GetMountainsPerSide => mountainsPerSide;

    /// <summary>
    /// �������������� ���������� ����� � ��� ��� ������ �����������.
    /// </summary>
    /// <param name="mountainsPerSide">���������� ��� �� �������, ��������� �� ������ ��������.</param>
    public void Initialize(int mountainsPerSide)
    {
        this.mountainsPerSide = mountainsPerSide;
        player1MountainsRemaining = mountainsPerSide;
        player2MountainsRemaining = mountainsPerSide;

        // ������������� ����� ��� ����� ������� (������������� PiecePlacementManager)
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
            { PieceType.Trebuchet, 1 }
        };
        player2Pieces = new Dictionary<PieceType, int>(player1Pieces); // ����� ��� ������ 2
        Debug.Log($"ManualPlacementManager: Initialized with {mountainsPerSide} mountains per side.");
    }

    /// <summary>
    /// ���������, ����� �� ���������� ������ ��� ���� �� ��������� �������.
    /// </summary>
    /// <param name="isPlayer1">true, ���� ��������� ����� 1.</param>
    /// <param name="position">������� �� �����.</param>
    /// <param name="isMountain">true, ���� ����������� ����.</param>
    /// <returns>true, ���� ���������� ��������.</returns>
    public bool CanPlace(bool isPlayer1, Vector3Int position, bool isMountain)
    {
        // �������� ������ ����� � ��������� ������
        if (!boardManager.IsWithinBounds(position) || boardManager.IsBlocked(position))
            return false;

        // ����������� �� z: ����� 1 � z=0-3, ����� 2 � z=6-9
        if (isPlayer1 && (position.z < 0 || position.z > 3))
            return false;
        if (!isPlayer1 && (position.z < 6 || position.z > 9))
            return false;

        // �������� ����������� ���
        if (isMountain)
        {
            return isPlayer1 ? player1MountainsRemaining > 0 : player2MountainsRemaining > 0;
        }

        return true;
    }

    /// <summary>
    /// ��������� ������ ��� ���� �� �����.
    /// </summary>
    /// <param name="isPlayer1">true, ���� ��������� ����� 1.</param>
    /// <param name="position">������� �� �����.</param>
    /// <param name="type">��� ������ (������������ ��� ���).</param>
    /// <param name="isMountain">true, ���� ����������� ����.</param>
    /// <returns>true, ���� ���������� �������.</returns>
    public bool PlacePieceOrMountain(bool isPlayer1, Vector3Int position, PieceType type, bool isMountain)
    {
        if (!CanPlace(isPlayer1, position, isMountain))
        {
            Debug.LogWarning($"ManualPlacementManager: Cannot place {(isMountain ? "mountain" : type.ToString())} at {position} for Player {(isPlayer1 ? 1 : 2)}");
            return false;
        }

        if (isMountain)
        {
            var mountain = pieceFactory.CreateMountain(position);
            if (mountain != null)
            {
                boardManager.PlaceMountain(position, mountain);
                if (isPlayer1)
                    player1MountainsRemaining--;
                else
                    player2MountainsRemaining--;
                Debug.Log($"ManualPlacementManager: Placed mountain at {position} for Player {(isPlayer1 ? 1 : 2)}");
                return true;
            }
        }
        else
        {
            var pieces = isPlayer1 ? player1Pieces : player2Pieces;
            if (pieces.ContainsKey(type) && pieces[type] > 0)
            {
                var piece = pieceFactory.CreatePiece(type, isPlayer1, position);
                if (piece != null)
                {
                    boardManager.PlacePiece(piece, position);
                    pieces[type]--;
                    Debug.Log($"ManualPlacementManager: Placed {type} at {position} for Player {(isPlayer1 ? 1 : 2)}");
                    return true;
                }
            }
        }
        Debug.LogWarning($"ManualPlacementManager: Failed to place {(isMountain ? "mountain" : type.ToString())} at {position}");
        return false;
    }

    /// <summary>
    /// ���������� ���������� ���������� ����� ��� ��� ��� ������.
    /// </summary>
    /// <param name="isPlayer1">true, ���� ����������� ����� 1.</param>
    /// <param name="type">��� ������ (������������ ��� ���).</param>
    /// <param name="isMountain">true, ���� ����������� ����.</param>
    /// <returns>���������� ���������� ���������.</returns>
    public int GetRemainingCount(bool isPlayer1, PieceType type, bool isMountain)
    {
        if (isMountain)
            return isPlayer1 ? player1MountainsRemaining : player2MountainsRemaining;
        var pieces = isPlayer1 ? player1Pieces : player2Pieces;
        return pieces.ContainsKey(type) ? pieces[type] : 0;
    }

    /// <summary>
    /// ���������, �������� �� ����� ����������� (��� ������ � ���� ���������).
    /// </summary>
    /// <param name="isPlayer1">true, ���� ����������� ����� 1.</param>
    /// <returns>true, ���� ����������� ���������.</returns>
    public bool HasCompletedPlacement(bool isPlayer1)
    {
        var pieces = isPlayer1 ? player1Pieces : player2Pieces;
        // ���������, ��� ��� ������ ��������� (count == 0) � ���� ����
        return pieces.Values.All(count => count == 0) && (isPlayer1 ? player1MountainsRemaining : player2MountainsRemaining) == 0;
    }

    /// <summary>
    /// ���������, �������� �� ������ �������.
    /// </summary>
    /// <param name="isPlayer1">true, ���� ����������� ����� 1.</param>
    /// <returns>true, ���� ������ ��� �� ��������.</returns>
    public bool IsKingNotPlaced(bool isPlayer1)
    {
        var pieces = isPlayer1 ? player1Pieces : player2Pieces;
        return pieces.ContainsKey(PieceType.King) && pieces[PieceType.King] > 0;
    }

    // ���������� ���������� IPiecePlacementManager (�������� ��� ������������� � �������������� ������������)
    public void PlaceMountains(int mountainsPerSide) { /* ��������: ���� ��� ��������� ������� */ }
    public void PlacePiecesForPlayer(bool isPlayer1) { /* ��������: ������ ��� ��������� ������� */ }
}