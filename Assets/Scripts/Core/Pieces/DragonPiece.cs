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
public class DragonAttackStrategy : IAttackable
{
    private readonly bool useRangedAttack; // ���������� ����� �����: true - ������� ���, false - ������� ���

    /// <summary>
    /// �����������, ����������� �������� ������ �����.
    /// </summary>
    /// <param name="useRangedAttack">true - ������� ���, false - ������� ���.</param>
    public DragonAttackStrategy(bool useRangedAttack)
    {
        this.useRangedAttack = useRangedAttack;
    }

    /// <summary>
    /// ������������ ������, ������� ������ ����� ��������� � ������� ������ (������ � ���������� ��������).
    /// ��� �������� ��� ������� ������ ���������, ��� �������� ��� ���������� �����������.
    /// ��������� ���� � ����� �������.
    /// </summary>
    public List<Vector3Int> CalculateAttacks(IBoardManager board, Piece piece)
    {
        List<Vector3Int> attacks = new List<Vector3Int>();
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

                    // ��������� ������� ��������� ������ � ��������� ����
                    if (board.IsOccupied(newPos) &&
                        board.GetPieceAt(newPos).IsPlayer1 != piece.IsPlayer1 &&
                        !board.IsMountain(newPos))
                    {
                        if (useRangedAttack)
                        {
                            // ������� ���: ��������� ������ ���������
                            bool isPathClear = true;
                            for (int j = 1; j < i; j++)
                            {
                                Vector3Int intermediatePos = pos + new Vector3Int(dx * j, 0, dz * j);
                                if (board.IsBlocked(intermediatePos) || board.IsOccupied(intermediatePos))
                                {
                                    isPathClear = false;
                                    break;
                                }
                            }
                            if (isPathClear)
                            {
                                attacks.Add(newPos);
                            }
                        }
                        else
                        {
                            // ������� ���: ��������� ���� ��� �������� ����
                            attacks.Add(newPos);
                        }
                    }
                }
            }
        }
        return attacks;
    }

    /// <summary>
    /// ������������ ��� ������������� ������, ������� ������ ����� ���������, ������� ������ � ���� ������, �������� ����.
    /// ��� �������� ��� ������� ������ ���������, ��� �������� ��� ���������� �����������.
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
                for (int i = 1; i <= 3; i++)
                {
                    Vector3Int newPos = pos + new Vector3Int(dx * i, 0, dz * i);
                    if (!board.IsWithinBounds(newPos))
                    {
                        break;
                    }

                    if (!board.IsMountain(newPos))
                    {
                        if (useRangedAttack)
                        {
                            // ������� ���: ��������� ������ ���������
                            bool isPathClear = true;
                            for (int j = 1; j < i; j++)
                            {
                                Vector3Int intermediatePos = pos + new Vector3Int(dx * j, 0, dz * j);
                                if (board.IsBlocked(intermediatePos) || board.IsOccupied(intermediatePos))
                                {
                                    isPathClear = false;
                                    break;
                                }
                            }
                            if (isPathClear)
                            {
                                attacks.Add(newPos);
                            }
                        }
                        else
                        {
                            // ������� ���: ��������� ������ ��� �������� ����
                            attacks.Add(newPos);
                        }
                    }
                }
            }
        }
        return attacks;
    }

    /// <summary>
    /// ��������� ����� ������� �� ��������� ������.
    /// - ������� ���: ���������� ������, ������� �� �����.
    /// - ������� ���: ���������� ������ � ������������ �� � �����.
    /// ���� �� ���������.
    /// </summary>
    public void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager)
    {
        // ��������, ��� ���� �� �������� �����
        if (boardManager.IsMountain(target))
        {
            Debug.LogWarning($"DragonAttackStrategy: Cannot attack mountain at {target}!");
            return;
        }

        Piece targetPiece = boardManager.GetPieceAt(target);
        if (targetPiece == null)
        {
            Debug.LogWarning($"DragonAttackStrategy: No piece at {target} to attack!");
            return;
        }

        if (useRangedAttack)
        {
            // ������� ���: ���������� ������, ������� �� �����
            Debug.Log($"DragonAttackStrategy: Executing ranged attack from {piece.Position} to {target} (piece: {piece.GetType().Name})");
            boardManager.RemovePiece(target);
        }
        else
        {
            // ������� ���: ���������� ������ � ������������
            Debug.Log($"DragonAttackStrategy: Executing melee attack from {piece.Position} to {target} (piece: {piece.GetType().Name})");
            boardManager.RemovePiece(target);
            piece.GetComponent<PieceAnimator>().MoveTo(target, () =>
            {
                boardManager.MovePiece(piece, piece.Position, target);
            });
        }
    }
}