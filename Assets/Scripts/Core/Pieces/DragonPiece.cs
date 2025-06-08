using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ����� ��� ������ "������".
/// ��������� �������� (�� 3 ������, ������������ �����������) � ����� � ������������� ����� ������� � ������� ����.
/// ������� ���: ����� �� 1-3 ������ � ������ ����������, ��� �����������.
/// ������� ���: ������ �� ������ � ��������� �������, ����������� � �����������.
/// </summary>
public class DragonPiece : Piece
{
    [SerializeField]
    private bool useRangedAttack = true; // ������������� � ����������: true - ������� ���, false - ������� ���

    /// <summary>
    /// ����������� ��������� �������� � ����� ��� �������.
    /// ������� �������� useRangedAttack � ��������� �����.
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new DragonMoveStrategy();
        attackStrategy = new DragonAttackStrategy(useRangedAttack);
        Debug.Log($"DragonPiece: Strategies set up (attackStrategy: {attackStrategy.GetType().Name}, UseRangedAttack: {useRangedAttack})");
    }
}

/// <summary>
/// ��������� �������� ��� �������.
/// ��������� ��������� �� 1-3 ������ �� ������ ��� ���������, ������������ �����������, ������ �� ������ ������.
/// </summary>
public class DragonMoveStrategy : IMovable
{
    public List<Vector3Int> CalculateMoves(IBoardManager board, Piece piece)
    {
        List<Vector3Int> moves = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        int[] directions = { -1, 0, 1 };

        foreach (int dx in directions)
        {
            foreach (int dz in directions)
            {
                if (dx == 0 && dz == 0) continue;
                for (int i = 1; i <= 3; i++)
                {
                    Vector3Int newPos = pos + new Vector3Int(dx * i, 0, dz * i);
                    if (!board.IsWithinBounds(newPos))
                    {
                        break;
                    }
                    // ��������� ������ ������ ������
                    if (!board.IsBlocked(newPos))
                    {
                        moves.Add(newPos);
                    }
                }
            }
        }
        return moves;
    }
}

/// <summary>
/// ��������� ����� ��� �������.
/// ������������ ��� ������:
/// - ������� ���: ����� �� 1-3 ������ �� ������ ��� ���������, ������� ������ ���������, ������� �� �����.
/// - ������� ���: ������ �� ������ � ��������� ������� (1-3 ������), ���������� �����������, ������������ �� ����.
/// ��������� ���� �� ��������� ����� ����� � ����� �������.
/// </summary>
public class DragonAttackStrategy : IAttackable, IRangedAttackable
{
    private readonly bool useRangedAttack; // ����� �����

    public DragonAttackStrategy(bool useRangedAttack)
    {
        this.useRangedAttack = useRangedAttack;
    }

    // ����������, �������� �� ����� �������
    public bool IsRangedAttack()
    {
        return useRangedAttack;
    }

    // ������ ����� �����
    public List<Vector3Int> CalculateAttacks(IBoardManager board, Piece piece)
    {
        List<Vector3Int> attacks = new List<Vector3Int>();
        Vector3Int pos = piece.Position;
        int maxRange = 3; // ��������� �����

        Vector3Int[] directions = {
            new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 0, 1), new Vector3Int(0, 0, -1),
            new Vector3Int(1, 0, 1), new Vector3Int(1, 0, -1),
            new Vector3Int(-1, 0, 1), new Vector3Int(-1, 0, -1)
        };

        foreach (var dir in directions)
        {
            for (int i = 1; i <= maxRange; i++)
            {
                Vector3Int targetPos = pos + dir * i;
                if (!board.IsWithinBounds(targetPos))
                    break;

                if (board.IsMountain(targetPos))
                    break;

                Piece targetPiece = board.GetPieceAt(targetPos);
                if (targetPiece != null)
                {
                    if (targetPiece.IsPlayer1 != piece.IsPlayer1)
                    {
                        attacks.Add(targetPos);
                    }
                    break;
                }

                if (useRangedAttack && board.IsOccupied(targetPos))
                    break;
            }
        }

        return attacks;
    }

    // ������ ���� ��������� ����� �����
    public List<Vector3Int> CalculateAllAttacks(IBoardManager board, Piece piece)
    {
        List<Vector3Int> allAttacks = new List<Vector3Int>();
        Vector3Int pos = piece.Position;
        int maxRange = 3;

        Vector3Int[] directions = {
            new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 0, 1), new Vector3Int(0, 0, -1),
            new Vector3Int(1, 0, 1), new Vector3Int(1, 0, -1),
            new Vector3Int(-1, 0, 1), new Vector3Int(-1, 0, -1)
        };

        foreach (var dir in directions)
        {
            for (int i = 1; i <= maxRange; i++)
            {
                Vector3Int targetPos = pos + dir * i;
                if (!board.IsWithinBounds(targetPos))
                    break;

                if (board.IsMountain(targetPos))
                    break;

                allAttacks.Add(targetPos);

                if (useRangedAttack && board.IsOccupied(targetPos))
                    break;
            }
        }

        return allAttacks;
    }

    // ���������� �����
    public void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager)
    {
        if (boardManager.IsMountain(target))
        {
            Debug.LogWarning($"DragonAttackStrategy: Cannot attack mountain at {target}!");
            return;
        }

        Piece targetPiece = boardManager.GetPieceAt(target);
        if (targetPiece == null)
        {
            Debug.LogWarning($"DragonAttackStrategy: No piece at {target}!");
            return;
        }

        Debug.Log($"DragonAttackStrategy: Executing {(useRangedAttack ? "ranged" : "melee")} attack to {target}");
        boardManager.RemovePiece(target);
    }
}