using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ����� ��� ������ "��������".
/// ��������� �������� �� 1 ������ �� ������ � ������� ����� �� 5 ������.
/// </summary>
public class TrebuchetPiece : Piece
{
    /// <summary>
    /// ����������� ��������� �������� � ����� ��� ���������.
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new TrebuchetMoveStrategy();
        attackStrategy = new TrebuchetAttackStrategy();
        Debug.Log("TrebuchetPiece: Strategies set up.");
    }
}

/// <summary>
/// ��������� �������� ��� ���������.
/// ��������� ��������� �� 1 ������ �� ������ (�����������/���������).
/// </summary>
public class TrebuchetMoveStrategy : IMovable
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
            Vector3Int newPos = pos + dir;
            if (board.IsWithinBounds(newPos) && !board.IsBlocked(newPos))
            {
                moves.Add(newPos);
            }
        }

        return moves;
    }
}

/// <summary>
/// ��������� ����� ��� ���������.
/// ��������� ������� ���: ����� �� 1-5 ������ �� ������ � ��������� ������ ���������.
/// ������������� ������ ���� ������������� ������ ����� ��� ��������� (������� ������ � ���� ������, �������� ����).
/// </summary>
public class TrebuchetAttackStrategy : IAttackable
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
            for (int i = 1; i <= 5; i++)
            {
                Vector3Int newPos = pos + dir * i;
                if (!board.IsWithinBounds(newPos))
                {
                    break;
                }

                // ���������, ��� ���� �������� �� ����� � ���
                bool pathBlocked = false;
                for (int j = 1; j < i; j++)
                {
                    Vector3Int midPos = pos + dir * j;
                    if (board.IsBlocked(midPos))
                    {
                        pathBlocked = true;
                        break;
                    }
                }
                if (pathBlocked)
                {
                    continue;
                }

                // ���� ������ ������ ����������� � ��� �� ����, ��������� � ��� ���� ��� �����
                if (board.IsOccupied(newPos) &&
                    board.GetPieceAt(newPos).IsPlayer1 != piece.IsPlayer1 &&
                    board.GetPieceAt(newPos).Type != PieceType.Mountain)
                {
                    attacks.Add(newPos);
                }
            }
        }

        return attacks;
    }

    // CalculateAllAttacks ������� ��� ���������, ��� ��� ��� ��������� ����
    public List<Vector3Int> CalculateAllAttacks(IBoardManager board, Piece piece)
    {
        List<Vector3Int> attacks = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        Vector3Int[] directions = {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 0, 1),
            new Vector3Int(0, 0, -1)
        };

        foreach (var dir in directions)
        {
            for (int i = 1; i <= 5; i++)
            {
                Vector3Int newPos = pos + dir * i;
                if (!board.IsWithinBounds(newPos))
                {
                    break;
                }

                bool pathBlocked = false;
                for (int j = 1; j < i; j++)
                {
                    Vector3Int midPos = pos + dir * j;
                    if (board.IsBlocked(midPos))
                    {
                        pathBlocked = true;
                        break;
                    }
                }
                if (pathBlocked)
                {
                    break;
                }

                if (!board.IsMountain(newPos))
                {
                    attacks.Add(newPos);
                }
            }
        }

        return attacks;
    }

    public void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager)
    {
        Debug.Log($"TrebuchetAttackStrategy: Executing ranged attack from {piece.Position} to {target}");
        Piece targetPiece = boardManager.GetPieceAt(target);
        if (targetPiece != null)
        {
            if (targetPiece.Type == PieceType.Mountain)
            {
                Debug.LogWarning($"TrebuchetAttackStrategy: Cannot attack mountain at {target}!");
                return;
            }
            boardManager.RemovePiece(target);
            Debug.Log($"TrebuchetAttackStrategy: Removed piece {targetPiece.GetType().Name} at {target}");
        }
        else
        {
            Debug.LogWarning($"TrebuchetAttackStrategy: No piece at {target} to attack!");
        }
    }
}