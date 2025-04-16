using System.Collections.Generic;
using UnityEngine;
using Zenject;

/// <summary>
/// ����� ��� ������ "������". ����������� �� �������� ������ Piece.
/// ��������� ��������� ������: �������� � ����� �� 1 ������ � ����� �����������.
/// </summary>
public class KingPiece : Piece
{   
    /// <summary>
    /// ��������� ��������� �������� � ����� ��� ������.
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new KingMoveStrategy();
        attackStrategy = new KingAttackStrategy();
        Debug.Log("KingPiece strategies set up.");
    }
}

/// <summary>
/// ��������� �������� ��� ������.
/// ��������� ��������� �� 1 ������ � ����� ����������� (������ � ���������).
/// </summary>
public class KingMoveStrategy : IMovable
{
    public List<Vector3Int> CalculateMoves(IBoardManager board)
    {
        List<Vector3Int> moves = new List<Vector3Int>();
        IPiece piece = board.GetPieceAt(new Vector3Int(0, 0, 0));
        Vector3Int pos = piece != null ? piece.Position : Vector3Int.zero;

        int[] directions = { -1, 0, 1 };

        foreach (int dx in directions)
        {
            foreach (int dz in directions)
            {
                if (dx == 0 && dz == 0) continue;
                Vector3Int newPos = pos + new Vector3Int(dx, 0, dz);
                if (board.IsWithinBounds(newPos) && !board.IsOccupied(newPos))
                {
                    moves.Add(newPos);
                }
            }
        }
        return moves;
    }
}

/// <summary>
/// ��������� ����� ��� ������.
/// ����� �� 1 ������ � ����� ����������� (������ �� ������ ����������).
/// </summary>
public class KingAttackStrategy : IAttackable
{
    public List<Vector3Int> CalculateAttacks(IBoardManager board)
    {
        List<Vector3Int> attacks = new List<Vector3Int>();
        IPiece piece = board.GetPieceAt(new Vector3Int(0, 0, 0));
        Vector3Int pos = piece != null ? piece.Position : Vector3Int.zero;

        int[] directions = { -1, 0, 1 };

        foreach (int dx in directions)
        {
            foreach (int dz in directions)
            {
                if (dx == 0 && dz == 0) continue;
                Vector3Int newPos = pos + new Vector3Int(dx, 0, dz);
                if (board.IsWithinBounds(newPos) && board.IsOccupied(newPos) &&
                    board.GetPieceAt(newPos).IsPlayer1 != piece.IsPlayer1)
                {
                    attacks.Add(newPos);
                }
            }
        }
        return attacks;
    }
}