using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ����� ��� ������ "������".
/// ��������� �������� (�� 3 ������, ������������ �����������) � ������� �����.
/// </summary>
public class DragonPiece : Piece
{
    /// <summary>
    /// ����������� ��������� �������� � ����� ��� �������.
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new DragonMoveStrategy();
        attackStrategy = new DragonAttackStrategy();
        Debug.Log($"DragonPiece: Strategies set up (attackStrategy: {attackStrategy.GetType().Name})");
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
/// ��������� ������� ���: ����� �� 1-3 ������ �� ������ ��� ���������, ������ �� ������ ��������� (��� ��� ��� ����� �� ����).
/// ��������� ���� �� ��������� ����� �����, ��� ��� ��� �������� ������������ �������������.
/// ������������� ������ ���� ������������� ������ ����� ��� ��������� (������� ������ � ���� ������, �������� ����).
/// </summary>
public class DragonAttackStrategy : IAttackable
{
    /// <summary>
    /// ������������ ������, ������� ������ ����� ��������� � ������� ������ (������ � ���������� ��������).
    /// ��������� ������ ��������� � ��������� ����.
    /// </summary>
    /// <param name="board">��������� ����� ��� �������� ���������.</param>
    /// <param name="piece">������ �������.</param>
    /// <returns>������ ������ � ���������� ��������, ������� ����� ���������.</returns>
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

                    // ��������� ������ ���������: ��� ��� ��� ����� �� ����
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

                    if (!isPathClear)
                    {
                        break; // ���������, ���� ���� ������������
                    }

                    // ���������, ���� �� ��������� ������ � �������� �����, � ��� ��� �� ����
                    if (board.IsOccupied(newPos) &&
                        board.GetPieceAt(newPos).IsPlayer1 != piece.IsPlayer1 &&
                        board.GetPieceAt(newPos).Type != PieceType.Mountain)
                    {
                        attacks.Add(newPos);
                    }
                }
            }
        }
        return attacks;
    }

    /// <summary>
    /// ������������ ��� ������������� ������, ������� ������ ����� ���������, ������� ������ � ���� ������, �������� ����.
    /// ��������� ������ ��������� (��� ��� ��� ����� �� ����) � ��������� 1-3 ������.
    /// </summary>
    /// <param name="board">��������� ����� ��� �������� ���������.</param>
    /// <param name="piece">������ �������.</param>
    /// <returns>������ ���� ������������� ������ �����.</returns>
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

                    // ��������� ������ ���������: ��� ��� ��� ����� �� ����
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

                    if (!isPathClear)
                    {
                        break; // ���������, ���� ���� ������������
                    }

                    // ��������� ������, ���� ��� �� �������� ����
                    if (!board.IsMountain(newPos))
                    {
                        attacks.Add(newPos);
                    }
                }
            }
        }
        return attacks;
    }

    /// <summary>
    /// ��������� ����� ������� �� ��������� ������.
    /// ������� ��������� ������, ���� ��� ���� � �� �������� �����.
    /// </summary>
    /// <param name="piece">������ �������.</param>
    /// <param name="target">������� ������ ��� �����.</param>
    /// <param name="boardManager">��������� ����� ��� ��������� ���������.</param>
    public void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager)
    {
        Debug.Log($"DragonAttackStrategy: Executing ranged attack from {piece.Position} to {target} (piece: {piece.GetType().Name})");
        Piece targetPiece = boardManager.GetPieceAt(target);
        if (targetPiece != null && targetPiece.Type != PieceType.Mountain)
        {
            boardManager.RemovePiece(target);
            Debug.Log($"DragonAttackStrategy: Removed piece {targetPiece.GetType().Name} at {target}");
        }
        else
        {
            Debug.LogWarning($"DragonAttackStrategy: No valid piece at {target} to attack or target is a mountain!");
        }
        // �� �������� MoveTo, ����� ������ ��������� �� �����
    }
}