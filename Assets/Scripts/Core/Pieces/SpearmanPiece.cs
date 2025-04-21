using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ����� ��� ������ "���������".
/// ��������� �������� � ����� �� 2 ������ �� ������.
/// </summary>
public class SpearmanPiece : Piece
{
    /// <summary>
    /// ����������� ��������� �������� � ����� ��� ����������.
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new SpearmanMoveStrategy();
        attackStrategy = new SpearmanAttackStrategy();
        Debug.Log("SpearmanPiece: Strategies set up.");
    }
}

/// <summary>
/// ��������� �������� ��� ����������.
/// ��������� ��������� �� 1-2 ������ �� ������ (�����������/���������).
/// </summary>
public class SpearmanMoveStrategy : IMovable
{
    public List<Vector3Int> CalculateMoves(IBoardManager board, Piece piece)
    {
        List<Vector3Int> moves = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        // ��������� �����������: �����, ����, �����, ������
        Vector3Int[] directions = {
            new Vector3Int(1, 0, 0),  // ������
            new Vector3Int(-1, 0, 0), // �����
            new Vector3Int(0, 0, 1),  // �����
            new Vector3Int(0, 0, -1)  // ����
        };

        foreach (var dir in directions)
        {
            for (int i = 1; i <= 2; i++)
            {
                Vector3Int newPos = pos + dir * i;
                if (!board.IsWithinBounds(newPos))
                {
                    break;
                }
                if (board.IsBlocked(newPos))
                {
                    break; // ��������� �����������, ���� ��������� ������ ��� ����
                }
                moves.Add(newPos);
            }
        }

        return moves;
    }
}

/// <summary>
/// ��������� ����� ��� ����������.
/// ��������� ������� ���: ����� �� 1-2 ������ �� ������, �������� ������ ����������.
/// ��������� ���� �� ������� �����������.
/// </summary>
public class SpearmanAttackStrategy : IAttackable
{
    public List<Vector3Int> CalculateAttacks(IBoardManager board, Piece piece)
    {
        List<Vector3Int> attacks = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        // ��������� �����������: �����, ����, �����, ������
        Vector3Int[] directions = {
            new Vector3Int(1, 0, 0),  // ������
            new Vector3Int(-1, 0, 0), // �����
            new Vector3Int(0, 0, 1),  // �����
            new Vector3Int(0, 0, -1)  // ����
        };

        foreach (var dir in directions)
        {
            for (int i = 1; i <= 2; i++)
            {
                Vector3Int newPos = pos + dir * i;
                if (!board.IsWithinBounds(newPos))
                {
                    break;
                }
                // ��������� ���� �� ���� (��� ������ ����� newPos)
                bool pathBlocked = false;
                for (int j = 1; j < i; j++)
                {
                    Vector3Int midPos = pos + dir * j;
                    if (board.IsBlocked(midPos) || board.IsOccupied(midPos))
                    {
                        pathBlocked = true;
                        break;
                    }
                }
                if (pathBlocked)
                {
                    break;
                }
                // ��������� ������� ������
                if (board.IsOccupied(newPos) && board.GetPieceAt(newPos).IsPlayer1 != piece.IsPlayer1 && !board.IsMountain(newPos))
                {
                    attacks.Add(newPos);
                    break; // ��������� ����������� ����� ������ ��������� ������
                }
            }
        }

        return attacks;
    }

    public void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager)
    {
        Debug.Log($"SpearmanAttackStrategy: Executing melee attack on {target}");
        // ������� ���: ���������� ������ � ������������
        boardManager.RemovePiece(target);
        piece.GetComponent<PieceAnimator>().MoveTo(target, () =>
        {
            boardManager.MovePiece(piece, piece.Position, target);
        });
    }
}