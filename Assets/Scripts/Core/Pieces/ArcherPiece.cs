using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ����� ��� ������ "������".
/// ��������� �������� �� 1 ������ � ����� ����������� � ������� ����� �� 3 ������ �� ������ ��� ���������.
/// ������������ ��� ������ �����: � ��������� ������ ��������� � ��� �� (����� �����������).
/// </summary>
public class ArcherPiece : Piece
{
    [SerializeField]
    private bool requireClearPath = true; // ������������� � ����������: true - ������� ������ ���������, false - ������� ����� �����������

    /// <summary>
    /// ����������� ��������� �������� � ����� ��� �������.
    /// ������� �������� requireClearPath � ��������� �����.
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new ArcherMoveStrategy();
        attackStrategy = new ArcherAttackStrategy(requireClearPath);
        Debug.Log($"ArcherPiece: Strategies set up (RequireClearPath: {requireClearPath})");
    }
}

/// <summary>
/// ��������� �������� ��� �������.
/// ��������� ��������� �� 1 ������ � ����� ����������� (����� ��� �� ���������), ��� � �����������.
/// </summary>
public class ArcherMoveStrategy : IMovable
{
    /// <summary>
    /// ������������ ���������� ���� ��� �������.
    /// </summary>
    /// <param name="board">��������� ����� ��� �������� ���������.</param>
    /// <param name="piece">������, ��� ������� �������������� ����.</param>
    /// <returns>������ ������, ���� ������ ����� �����.</returns>
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

        Debug.Log($"ArcherMoveStrategy: Calculated {moves.Count} moves for {piece.Type} at {pos}");
        return moves;
    }
}

/// <summary>
/// ��������� ����� ��� �������.
/// ��������� ������� ���: ����� �� 1�3 ������ �� ������ ��� ���������.
/// ������������ ��� ������:
/// - � ��������� ������ ��������� (��� ��������� 2 � 3).
/// - ��� �������� ��������� (����� ����� �����������).
/// ��������� ���� �� ����� �����.
/// </summary>
public class ArcherAttackStrategy : IAttackable
{
    private readonly bool requireClearPath; // ����������, ��������� �� ������ ��������� ��� �����

    /// <summary>
    /// �����������, ����������� �������� ������ �����.
    /// </summary>
    /// <param name="requireClearPath">true - ������� ������ ���������, false - ������� ����� �����������.</param>
    public ArcherAttackStrategy(bool requireClearPath)
    {
        this.requireClearPath = requireClearPath;
    }

    /// <summary>
    /// ������������ ������, ������� ������ ����� ��������� (������ � ���������� ��������).
    /// ��������� ������ ���������, ���� requireClearPath = true.
    /// </summary>
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

                // ��������� ������ �� ���������� 1, 2 � 3
                for (int distance = 1; distance <= 3; distance++)
                {
                    Vector3Int targetPos = pos + new Vector3Int(dx * distance, 0, dz * distance);
                    if (!board.IsWithinBounds(targetPos)) continue;

                    // ��������� ������ ��������� ��� ��������� 2 � 3, ���� ���������
                    bool hasClearPath = true;
                    if (requireClearPath && distance > 1)
                    {
                        for (int i = 1; i < distance; i++)
                        {
                            Vector3Int midPos = pos + new Vector3Int(dx * i, 0, dz * i);
                            if (board.IsBlocked(midPos))
                            {
                                hasClearPath = false;
                                break;
                            }
                        }
                    }

                    if (!hasClearPath) continue;

                    // ���������, ���� �� ��������� ������ �� ������� ������ � ��� �� ����
                    if (board.IsOccupied(targetPos) &&
                        board.GetPieceAt(targetPos).IsPlayer1 != piece.IsPlayer1 &&
                        !board.IsMountain(targetPos))
                    {
                        attacks.Add(targetPos);
                    }
                }
            }
        }

        Debug.Log($"ArcherAttackStrategy: Calculated {attacks.Count} attacks for {piece.Type} at {pos}");
        return attacks;
    }

    /// <summary>
    /// ������������ ��� ������������� ������ �����, ������� ������ � ���� ������, �������� ����.
    /// ��������� ������ ���������, ���� requireClearPath = true.
    /// </summary>
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

                // ��������� ������ �� ���������� 1, 2 � 3
                for (int distance = 1; distance <= 3; distance++)
                {
                    Vector3Int targetPos = pos + new Vector3Int(dx * distance, 0, dz * distance);
                    if (!board.IsWithinBounds(targetPos) || board.IsMountain(targetPos)) continue;

                    // ��������� ������ ��������� ��� ��������� 2 � 3, ���� ���������
                    bool hasClearPath = true;
                    if (requireClearPath && distance > 1)
                    {
                        for (int i = 1; i < distance; i++)
                        {
                            Vector3Int midPos = pos + new Vector3Int(dx * i, 0, dz * i);
                            if (board.IsBlocked(midPos))
                            {
                                hasClearPath = false;
                                break;
                            }
                        }
                    }

                    if (hasClearPath)
                    {
                        attacks.Add(targetPos);
                    }
                }
            }
        }

        Debug.Log($"ArcherAttackStrategy: Calculated {attacks.Count} potential attacks for {piece.Type} at {pos}");
        return attacks;
    }

    /// <summary>
    /// ��������� ������� ����� �� ��������� ������.
    /// ���������� ��������� ������, ��������� �� �����.
    /// </summary>
    public void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager, bool isRangedAttack)
    {
        Debug.Log($"ArcherAttackStrategy: Executing ranged attack from {piece.Position} to {target}");
        Piece targetPiece = boardManager.GetPieceAt(target);
        if (targetPiece != null)
        {
            if (targetPiece.Type == PieceType.Mountain)
            {
                Debug.LogWarning($"ArcherAttackStrategy: Cannot attack mountain at {target}!");
                return;
            }
            piece.SelectAttack(target, isRangedAttack);
            Debug.Log($"ArcherAttackStrategy: Removed piece {targetPiece.GetType().Name} at {target}");
        }
        else
        {
            Debug.LogWarning($"ArcherAttackStrategy: No piece at {target} to attack!");
        }
    }
}