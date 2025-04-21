using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ����� ��� ������ "����".
/// ��������� �������� � ����� �� 3 ������ �� ������ � ������������ ���������� ��� ������ ������.
/// </summary>
public class ElephantPiece : Piece
{
    /// <summary>
    /// ����������� ��������� �������� � ����� ��� �����.
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new ElephantMoveStrategy();
        attackStrategy = new ElephantAttackStrategy();
        Debug.Log("ElephantPiece: Strategies set up.");
    }
}

/// <summary>
/// ��������� �������� ��� �����.
/// ��������� ��������� �� 1-3 ������ �� ������ (�����������/���������).
/// </summary>
public class ElephantMoveStrategy : IMovable
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
            for (int i = 1; i <= 3; i++)
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
/// ��������� ����� ��� �����.
/// ��������� ������� ���: ����� �� 1-3 ������ �� ������, ���������� �� ���� ����� ���������� ������.
/// ��������� ���� �� ������� �����������.
/// </summary>
public class ElephantAttackStrategy : IAttackable
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
            for (int i = 1; i <= 3; i++)
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
                    // ��������� ��������� ������, ���� ������� �� ���������
                    if (i < 3)
                    {
                        Vector3Int nextPos = pos + dir * (i + 1);
                        if (board.IsWithinBounds(nextPos) && board.IsOccupied(nextPos) &&
                            board.GetPieceAt(nextPos).IsPlayer1 != piece.IsPlayer1 && !board.IsMountain(nextPos))
                        {
                            // ��������� ���� �� ��������� ������
                            bool nextPathBlocked = false;
                            for (int j = 1; j <= i; j++)
                            {
                                Vector3Int midPos = pos + dir * j;
                                if (board.IsBlocked(midPos) || (board.IsOccupied(midPos) && midPos != newPos))
                                {
                                    nextPathBlocked = true;
                                    break;
                                }
                            }
                            if (!nextPathBlocked)
                            {
                                attacks.Add(nextPos);
                            }
                        }
                    }
                    break; // ��������� ����������� ����� ������ ��� ������ ������
                }
            }
        }

        return attacks;
    }

    public void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager)
    {
        Debug.Log($"ElephantAttackStrategy: Executing melee attack on {target}");
        Vector3Int pos = piece.Position;
        // ��������� ����������� �����
        Vector3Int delta = target - pos;
        int distance = Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.z));
        Vector3Int dir = new Vector3Int(
            delta.x == 0 ? 0 : (delta.x > 0 ? 1 : -1),
            0,
            delta.z == 0 ? 0 : (delta.z > 0 ? 1 : -1)
        ); // �����������: (+1,0,0), (-1,0,0), (0,0,+1), ��� (0,0,-1)

        // ���������� ������ �� ���� (�� ����)
        for (int i = 1; i <= distance && i <= 3; i++)
        {
            Vector3Int currentPos = pos + dir * i;
            if (boardManager.IsOccupied(currentPos))
            {
                Piece targetPiece = boardManager.GetPieceAt(currentPos);
                if (targetPiece != null && targetPiece.IsPlayer1 != piece.IsPlayer1)
                {
                    boardManager.RemovePiece(currentPos);
                    Debug.Log($"ElephantAttackStrategy: Removed piece {targetPiece.GetType().Name} at {currentPos}");
                }
            }
        }

        // ������������ �� �������� ������
        piece.GetComponent<PieceAnimator>().MoveTo(target, () =>
        {
            boardManager.MovePiece(piece, piece.Position, target);
        });
    }
}