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
/// ������������� ������ ���� ������������� ������ ����� ��� ��������� (������� ������ � ���� ������, �������� ����).
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
            int enemiesFound = 0;
            for (int i = 1; i <= 3; i++)
            {
                Vector3Int newPos = pos + dir * i;

                // ���� newPos ��� �����, ��������� �����������
                if (!board.IsWithinBounds(newPos))
                {
                    break;
                }

                // ���� �� ���� ����, ��������� �����������
                if (board.IsMountain(newPos))
                {
                    break;
                }

                // ���� �� ���� ���� ������, ��������� �����������
                if (board.IsOccupied(newPos) && board.GetPieceAt(newPos).IsPlayer1 == piece.IsPlayer1)
                {
                    break;
                }

                // ���� �� ���� ������ ����������, ��������� � � ������ ����
                if (board.IsOccupied(newPos) && board.GetPieceAt(newPos).IsPlayer1 != piece.IsPlayer1)
                {
                    attacks.Add(newPos); // ��������� ��������� ������ � ������ ����
                    enemiesFound++;
                    if (enemiesFound >= 2)
                    {
                        break; // �������� ��� ������
                    }
                }
                // ���� ���� ������ � ������ � ������� ������� ������, ��������� �����������
                else if (enemiesFound == 1 && !board.IsBlocked(newPos))
                {
                    break;
                }
            }
        }

        return attacks;
    }

    /// <summary>
    /// ������������ ��� ������������� ������, ������� ���� ����� ���������, ������� ������ � ���� ������, �������� ����.
    /// ��������� ��������� 1-3 ������ �� ������, ����������� ��� ������� � �����.
    /// </summary>
    public List<Vector3Int> CalculateAllAttacks(IBoardManager board, Piece piece)
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
                // ��������� �����������, ���� ��������� ����
                if (board.IsMountain(newPos))
                {
                    break;
                }
                attacks.Add(newPos); // ��������� ������ (������� ������ � ���� ������)
            }
        }

        return attacks;
    }

    public void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager, bool isRangedAttack)
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
                    piece.SelectAttack(currentPos, isRangedAttack);
                    Debug.Log($"ElephantAttackStrategy: Removed piece {targetPiece.GetType().Name} at {currentPos}");
                }
            }
        }        
    }
}