using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ����� ��� ������ "�����" (���������).
/// ��������� �������� � ����� �� 1 ������ �� ����������� (X) � ��������� (Z).
/// </summary>
public class RabblePiece : Piece
{
    /// <summary>
    /// ����������� ��������� �������� � ����� ��� �����.
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new RabbleMoveStrategy();
        attackStrategy = new RabbleAttackStrategy();
        Debug.Log("RabblePiece: Strategies set up.");
    }
}

/// <summary>
/// ��������� �������� ��� �����.
/// ��������� ��������� �� 1 ������ �� ����������� (x � 1) ��� ��������� (z � 1).
/// </summary>
public class RabbleMoveStrategy : IMovable
{
    public List<Vector3Int> CalculateMoves(IBoardManager board, Piece piece)
    {
        List<Vector3Int> moves = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        // ��������� �����������: �����, ������, �����, ����
        Vector3Int[] directions = new[]
        {
            new Vector3Int(1, 0, 0),  // ������ (x + 1)
            new Vector3Int(-1, 0, 0), // ����� (x - 1)
            new Vector3Int(0, 0, 1),  // ����� (z + 1)
            new Vector3Int(0, 0, -1)  // ���� (z - 1)
        };

        foreach (var dir in directions)
        {
            Vector3Int newPos = pos + dir;
            if (board.IsWithinBounds(newPos) && !board.IsBlocked(newPos))
            {
                moves.Add(newPos); // ��������� ������ ��������� ������
            }
        }

        return moves;
    }
}

/// <summary>
/// ��������� ����� ��� �����.
/// ��������� ������� ���: ����� �� 1 ������ �� ����������� (x � 1) ��� ��������� (z � 1), �������� ������ ����������.
/// ������������� ������ ���� ������������� ������ ����� ��� ��������� (������� ������ � ���� ������, �������� ����).
/// </summary>
public class RabbleAttackStrategy : IAttackable
{
    public List<Vector3Int> CalculateAttacks(IBoardManager board, Piece piece)
    {
        List<Vector3Int> attacks = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        // ��������� �����������: �����, ������, �����, ����
        Vector3Int[] directions = new[]
        {
            new Vector3Int(1, 0, 0),  // ������ (x + 1)
            new Vector3Int(-1, 0, 0), // ����� (x - 1)
            new Vector3Int(0, 0, 1),  // ����� (z + 1)
            new Vector3Int(0, 0, -1)  // ���� (z - 1)
        };

        foreach (var dir in directions)
        {
            Vector3Int targetPos = pos + dir;
            if (board.IsWithinBounds(targetPos) && board.IsOccupied(targetPos) &&
                board.GetPieceAt(targetPos).IsPlayer1 != piece.IsPlayer1 && !board.IsMountain(targetPos))
            {
                attacks.Add(targetPos);
            }
        }

        return attacks;
    }

    /// <summary>
    /// ������������ ��� ������������� ������, ������� ��bble ����� ���������, ������� ������ � ���� ������, �������� ����.
    /// ��������� ��������� 1 ������ �� ����������� ��� ���������.
    /// </summary>
    public List<Vector3Int> CalculateAllAttacks(IBoardManager board, Piece piece)
    {
        List<Vector3Int> attacks = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        // ��������� �����������: �����, ������, �����, ����
        Vector3Int[] directions = new[]
        {
            new Vector3Int(1, 0, 0),  // ������ (x + 1)
            new Vector3Int(-1, 0, 0), // ����� (x - 1)
            new Vector3Int(0, 0, 1),  // ����� (z + 1)
            new Vector3Int(0, 0, -1)  // ���� (z - 1)
        };

        foreach (var dir in directions)
        {
            Vector3Int targetPos = pos + dir;
            if (board.IsWithinBounds(targetPos) && !board.IsMountain(targetPos))
            {
                attacks.Add(targetPos);
            }
        }

        return attacks;
    }

    public void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager)
    {
        Debug.Log($"RabbleAttackStrategy: Executing melee attack on {target}");
        boardManager.RemovePiece(target);
        piece.GetComponent<PieceAnimator>().MoveTo(target, null, () =>
        {
            boardManager.MovePiece(piece, piece.Position, target);
        });
    }
}