using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ����� ��� ������ "������ ���������".
/// ��������� L-�������� �������� � ��������������� ����������� � L-�������� ����� ��� ��������������.
/// </summary>
public class HeavyHorsePiece : Piece
{
    /// <summary>
    /// ����������� ��������� �������� � ����� ��� ������ ���������.
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new HeavyHorseMoveStrategy();
        attackStrategy = new HeavyHorseAttackStrategy();
        Debug.Log("HeavyHorsePiece: Strategies set up.");
    }
}

/// <summary>
/// ��������� �������� ��� ������ ���������.
/// ��������� ��������� L-�������, ������������ ������ � ����, �� ������ �� ������ ������.
/// </summary>
public class HeavyHorseMoveStrategy : IMovable
{
    public List<Vector3Int> CalculateMoves(IBoardManager board, Piece piece)
    {
        List<Vector3Int> moves = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        // L-�������� ����������� (2 ������ � ����� �����������, 1 ���������������, ��� 1 + 2)
        Vector3Int[] directions = {
            new Vector3Int(2, 0, 1),   // ������ 2, ����� 1
            new Vector3Int(2, 0, -1),  // ������ 2, ���� 1
            new Vector3Int(-2, 0, 1),  // ����� 2, ����� 1
            new Vector3Int(-2, 0, -1), // ����� 2, ���� 1
            new Vector3Int(1, 0, 2),   // ������ 1, ����� 2
            new Vector3Int(1, 0, -2),  // ������ 1, ���� 2
            new Vector3Int(-1, 0, 2),  // ����� 1, ����� 2
            new Vector3Int(-1, 0, -2)  // ����� 1, ���� 2
        };

        foreach (var dir in directions)
        {
            Vector3Int newPos = pos + dir;
            if (board.IsWithinBounds(newPos) && !board.IsMountain(newPos) && !board.IsOccupied(newPos))
            {
                moves.Add(newPos); // ��������� ������ ������ ������, ������������ ������ � ����
            }
        }

        return moves;
    }
}

/// <summary>
/// ��������� ����� ��� ������ ���������.
/// ��������� ������� ���: L-�������� �����, ������� ��������� ���� �� ���� (�� ������������� ������ ��� ����).
/// </summary>
public class HeavyHorseAttackStrategy : IAttackable
{
    public List<Vector3Int> CalculateAttacks(IBoardManager board, Piece piece)
    {
        List<Vector3Int> attacks = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        // L-�������� ����������� (2 ������ � ����� �����������, 1 ���������������, ��� 1 + 2)
        Vector3Int[] directions = {
            new Vector3Int(2, 0, 1),   // ������ 2, ����� 1
            new Vector3Int(2, 0, -1),  // ������ 2, ���� 1
            new Vector3Int(-2, 0, 1),  // ����� 2, ����� 1
            new Vector3Int(-2, 0, -1), // ����� 2, ���� 1
            new Vector3Int(1, 0, 2),   // ������ 1, ����� 2
            new Vector3Int(1, 0, -2),  // ������ 1, ���� 2
            new Vector3Int(-1, 0, 2),  // ����� 1, ����� 2
            new Vector3Int(-1, 0, -2)  // ����� 1, ���� 2
        };

        foreach (var dir in directions)
        {
            Vector3Int newPos = pos + dir;
            if (board.IsWithinBounds(newPos) && !board.IsMountain(newPos) &&
                board.IsOccupied(newPos) && board.GetPieceAt(newPos).IsPlayer1 != piece.IsPlayer1)
            {
                // ��������� ��������� ���� �� ����
                bool pathClear = IsPathClear(board, pos, dir);
                if (pathClear)
                {
                    attacks.Add(newPos); // ����� ��������, ���� ���� ��������
                }
            }
        }

        return attacks;
    }

    /// <summary>
    /// ���������, �������� �� ���� �� ���� ��� L-��������� ����.
    /// ��� ���� (dx, dz) ��������� ������������� ������ (��������, ��� (2,1) ��������� (1,0) � (2,0)).
    /// </summary>
    private bool IsPathClear(IBoardManager board, Vector3Int pos, Vector3Int dir)
    {
        // ���������� ������������� ������ ��� L-��������� ����
        List<Vector3Int> intermediatePositions = new List<Vector3Int>();
        int dx = dir.x;
        int dz = dir.z;

        // ��� ���� (2,1) ��� (-2,1) ��������� (1,0) ��� (-1,0)
        if (Mathf.Abs(dx) == 2 && Mathf.Abs(dz) == 1)
        {
            intermediatePositions.Add(pos + new Vector3Int(dx / 2, 0, 0)); // ������������� �� x
        }
        // ��� ���� (1,2) ��� (-1,2) ��������� (0,1) ��� (0,-1)
        else if (Mathf.Abs(dx) == 1 && Mathf.Abs(dz) == 2)
        {
            intermediatePositions.Add(pos + new Vector3Int(0, 0, dz / 2)); // ������������� �� z
        }

        // ���������, ��� ��� ������������� ������ �������� �� ����� � ���
        foreach (var midPos in intermediatePositions)
        {
            if (!board.IsWithinBounds(midPos) || board.IsMountain(midPos) || board.IsOccupied(midPos))
            {
                return false;
            }
        }

        return true;
    }

    public void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager)
    {
        Debug.Log($"HeavyHorseAttackStrategy: Executing melee attack on {target}");
        // ������� ���: ���������� ������ � ������������
        boardManager.RemovePiece(target);
        piece.GetComponent<PieceAnimator>().MoveTo(target, () =>
        {
            boardManager.MovePiece(piece, piece.Position, target);
        });
    }
}