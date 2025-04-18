using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ����� ��� ������ "������ ���������". ��������� L-�������� �������� � ������� ���.
/// </summary>
public class HeavyCavalryPiece : Piece
{
    /// <summary>
    /// ��������� ��������� ��� ������ ���������.
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new HeavyCavalryMoveStrategy();
        attackStrategy = new HeavyCavalryAttackStrategy();
        Debug.Log("HeavyCavalryPiece strategies set up.");
    }
}

/// <summary>
/// ��������� �������� ��� ������ ���������: L-�������� ��� (2+1 ��� 1+2).
/// </summary>
public class HeavyCavalryMoveStrategy : IMovable
{
    public List<Vector3Int> CalculateMoves(IBoardManager board, Piece piece)
    {
        List<Vector3Int> moves = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        // ��� ��������� L-�������� ����
        Vector3Int[] directions = {
            new Vector3Int(2, 0, 1), new Vector3Int(2, 0, -1), new Vector3Int(-2, 0, 1), new Vector3Int(-2, 0, -1),
            new Vector3Int(1, 0, 2), new Vector3Int(1, 0, -2), new Vector3Int(-1, 0, 2), new Vector3Int(-1, 0, -2)
        };

        foreach (var dir in directions)
        {
            Vector3Int newPos = pos + dir;
            if (board.IsWithinBounds(newPos))
            {
                // ���������, ��� ������ �� ���� � ���� �����, ���� ������ �����������
                if (!board.IsMountain(newPos) &&
                    (!board.IsOccupied(newPos) || (board.IsOccupied(newPos) && board.GetPieceAt(newPos).IsPlayer1 != piece.IsPlayer1)))
                {
                    moves.Add(newPos);
                }
            }
        }

        return moves;
    }
}

/// <summary>
/// ��������� ����� ��� ������ ���������: ������� ��� �� �������� ������.
/// </summary>
public class HeavyCavalryAttackStrategy : IAttackable
{
    public List<Vector3Int> CalculateAttacks(IBoardManager board, Piece piece)
    {
        List<Vector3Int> attacks = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        // �� �� L-�������� �����������
        Vector3Int[] directions = {
            new Vector3Int(2, 0, 1), new Vector3Int(2, 0, -1), new Vector3Int(-2, 0, 1), new Vector3Int(-2, 0, -1),
            new Vector3Int(1, 0, 2), new Vector3Int(1, 0, -2), new Vector3Int(-1, 0, 2), new Vector3Int(-1, 0, -2)
        };

        foreach (var dir in directions)
        {
            Vector3Int newPos = pos + dir;
            if (board.IsWithinBounds(newPos) && board.IsOccupied(newPos) &&
                board.GetPieceAt(newPos).IsPlayer1 != piece.IsPlayer1)
            {
                // ���������, ��� ���� �������� �� ����� � ���
                if (IsPathClear(pos, newPos, board))
                {
                    attacks.Add(newPos);
                }
            }
        }

        return attacks;
    }

    public void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager)
    {
        Debug.Log($"HeavyCavalryAttackStrategy: Executing melee attack on {target}");
        // ������� ���: ���������� ������ � ������������
        boardManager.RemovePiece(target);
        piece.GetComponent<PieceAnimator>().MoveTo(target, () =>
        {
            boardManager.MovePiece(piece, piece.Position, target);
        });
    }

    private bool IsPathClear(Vector3Int start, Vector3Int end, IBoardManager board)
    {
        Vector3Int delta = end - start;
        if (Mathf.Abs(delta.x) == 2 && Mathf.Abs(delta.z) == 1)
        {
            Vector3Int mid = start + new Vector3Int(delta.x / 2, 0, 0);
            return !board.IsBlocked(mid);
        }
        else if (Mathf.Abs(delta.x) == 1 && Mathf.Abs(delta.z) == 2)
        {
            Vector3Int mid = start + new Vector3Int(0, 0, delta.z / 2);
            return !board.IsBlocked(mid);
        }
        return true;
    }
}