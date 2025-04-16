using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ����� ��� ������ "������". ����������� �� �������� ������ Piece.
/// ��������� ����������� ��������� �������: �������� � ����� �� ���������� �� 3 ������.
/// </summary>
public class DragonPiece : Piece
{    
    /// <summary>
    /// ��������� ��������� �������� � ����� ��� �������.
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new DragonMoveStrategy();
        attackStrategy = new DragonAttackStrategy();
        Debug.Log("DragonPiece strategies set up.");
    }
}

/// <summary>
/// ��������� �������� ��� �������.
/// ��������� ��������� �� ���������� �� 3 ������ �� ������ ��� ���������, ������������ �����������.
/// </summary>
public class DragonMoveStrategy : IMovable
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
                for (int i = 1; i <= 3; i++)
                {
                    Vector3Int newPos = pos + new Vector3Int(dx * i, 0, dz * i);
                    if (board.IsWithinBounds(newPos))
                    {
                        moves.Add(newPos); // ������ ������������� ��
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        return moves;
    }
}

/// <summary>
/// ��������� ����� ��� �������.
/// ����� ��������� � ��������� (�� 3 ������ �� ������ ��� ���������).
/// </summary>
public class DragonAttackStrategy : IAttackable
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
                for (int i = 1; i <= 3; i++)
                {
                    Vector3Int newPos = pos + new Vector3Int(dx * i, 0, dz * i);
                    if (board.IsWithinBounds(newPos))
                    {
                        if (board.IsOccupied(newPos) && board.GetPieceAt(newPos).IsPlayer1 != piece.IsPlayer1)
                        {
                            attacks.Add(newPos);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        return attacks;
    }
}