using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ����� ��� ������ "�����������".
/// ��������� �������� �� 1 ������ � ����� ����������� � ������� ����� �� 2 ������ �� ������ ��� ���������.
/// </summary>
public class CrossbowmanPiece : Piece
{
    /// <summary>
    /// ����������� ��������� �������� � ����� ��� ������������.
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new CrossbowmanMoveStrategy();
        attackStrategy = new CrossbowmanAttackStrategy();
        Debug.Log("CrossbowmanPiece: Strategies set up.");
    }
}

/// <summary>
/// ��������� �������� ��� ������������.
/// ��������� ��������� �� 1 ������ � ����� ����������� (����� ��� �� ���������).
/// </summary>
public class CrossbowmanMoveStrategy : IMovable
{
    public List<Vector3Int> CalculateMoves(IBoardManager board, Piece piece)
    {
        List<Vector3Int> moves = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        // ��� �����������: �����, ����, �����, ������, ���������
        int[] directions = { -1, 0, 1 };

        foreach (int dx in directions)
        {
            foreach (int dz in directions)
            {
                if (dx == 0 && dz == 0) continue; // ���������� ������� �������
                Vector3Int newPos = pos + new Vector3Int(dx, 0, dz);
                if (board.IsWithinBounds(newPos) && !board.IsBlocked(newPos))
                {
                    moves.Add(newPos); // ��������� ������ ��������� ������
                }
            }
        }

        return moves;
    }
}

/// <summary>
/// ��������� ����� ��� ������������.
/// ��������� ������� ���: ����� �� 1-2 ������ �� ������ ��� ���������, ������� ������ ��������� ��� ��������� 2.
/// ������������� ������ ���� ������������� ������ ����� ��� ��������� (������� ������ � ���� ������, �������� ����).
/// </summary>
public class CrossbowmanAttackStrategy : IAttackable
{
    public List<Vector3Int> CalculateAttacks(IBoardManager board, Piece piece)
    {
        List<Vector3Int> attacks = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        // �����������: �����, ����, �����, ������, ���������
        int[] directions = { -1, 0, 1 };

        foreach (int dx in directions)
        {
            foreach (int dz in directions)
            {
                if (dx == 0 && dz == 0) continue;

                // ��������� ������ �� ���������� 1
                Vector3Int targetPos1 = pos + new Vector3Int(dx, 0, dz);
                if (board.IsWithinBounds(targetPos1) &&
                    board.IsOccupied(targetPos1) &&
                    board.GetPieceAt(targetPos1).IsPlayer1 != piece.IsPlayer1 &&
                    board.GetPieceAt(targetPos1).Type != PieceType.Mountain)
                {
                    attacks.Add(targetPos1); // ����� �� 1 ������
                }

                // ��������� ������ �� ���������� 2
                Vector3Int targetPos2 = pos + new Vector3Int(dx * 2, 0, dz * 2);
                if (!board.IsWithinBounds(targetPos2)) continue;

                // ��������� ������ ��������� ��� ����� �� 2 ������
                Vector3Int midPos = pos + new Vector3Int(dx, 0, dz);
                if (board.IsBlocked(midPos)) continue;

                // ���������, ���� �� ��������� ������ �� ������� ������ � ��� �� ����
                if (board.IsOccupied(targetPos2) &&
                    board.GetPieceAt(targetPos2).IsPlayer1 != piece.IsPlayer1 &&
                    board.GetPieceAt(targetPos2).Type != PieceType.Mountain)
                {
                    attacks.Add(targetPos2);
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

        int[] directions = { -1, 0, 1 };

        foreach (int dx in directions)
        {
            foreach (int dz in directions)
            {
                if (dx == 0 && dz == 0) continue;

                Vector3Int targetPos1 = pos + new Vector3Int(dx, 0, dz);
                if (board.IsWithinBounds(targetPos1) && !board.IsMountain(targetPos1))
                {
                    attacks.Add(targetPos1);
                }

                Vector3Int targetPos2 = pos + new Vector3Int(dx * 2, 0, dz * 2);
                if (board.IsWithinBounds(targetPos2) && !board.IsMountain(targetPos2))
                {
                    Vector3Int midPos = pos + new Vector3Int(dx, 0, dz);
                    if (!board.IsBlocked(midPos))
                    {
                        attacks.Add(targetPos2);
                    }
                }
            }
        }

        return attacks;
    }

    public void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager)
    {
        Debug.Log($"CrossbowmanAttackStrategy: Executing ranged attack from {piece.Position} to {target}");
        Piece targetPiece = boardManager.GetPieceAt(target);
        if (targetPiece != null)
        {
            if (targetPiece.Type == PieceType.Mountain)
            {
                Debug.LogWarning($"CrossbowmanAttackStrategy: Cannot attack mountain at {target}!");
                return;
            }
            boardManager.RemovePiece(target);
            Debug.Log($"CrossbowmanAttackStrategy: Removed piece {targetPiece.GetType().Name} at {target}");
        }
        else
        {
            Debug.LogWarning($"CrossbowmanAttackStrategy: No piece at {target} to attack!");
        }
    }
}