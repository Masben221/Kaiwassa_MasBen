using UnityEngine;
using System.Collections.Generic;

///   <summary>
///   ����� ��� ������ "������ ���������".
///   ��������� L-�������� �������� � ��������������� ����������� � L-�������� ����� ��� ��������������.
///   </summary>
public class HeavyHorsePiece : Piece
{
    ///   <summary>
    ///   ����������� ��������� �������� � ����� ��� ������ ���������.
    ///   </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new HeavyHorseMoveStrategy();
        attackStrategy = new HeavyHorseAttackStrategy();
        Debug.Log("HeavyHorsePiece: Strategies set up.");
    }
}

///   <summary>
///   ��������� �������� ��� ������ ���������.
///   ��������� ��������� L-�������, ������������ ������ � ����, �� ������ �� ������ ������.
///   </summary>
public class HeavyHorseMoveStrategy : IMovable
{
    public List<Vector3Int> CalculateMoves(IBoardManager board, Piece piece)
    {
        List<Vector3Int> moves = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        //   L-�������� ����������� (2 ������ � ����� �����������, 1 ���������������, ��� 1 + 2)
        Vector3Int[] directions = {
            new Vector3Int(2, 0, 1),
            new Vector3Int(2, 0, -1),
            new Vector3Int(-2, 0, 1),
            new Vector3Int(-2, 0, -1),
            new Vector3Int(1, 0, 2),
            new Vector3Int(1, 0, -2),
            new Vector3Int(-1, 0, 2),
            new Vector3Int(-1, 0, -2)
        };

        foreach (var dir in directions)
        {
            Vector3Int newPos = pos + dir;
            if (board.IsWithinBounds(newPos) && !board.IsMountain(newPos) && !board.IsOccupied(newPos))
            {
                moves.Add(newPos);
            }
        }

        return moves;
    }
}

///   <summary>
///   ��������� ����� ��� ������ ���������.
///   ��������� ������� ���: L-�������� �����, ������� ��������� ���� �� ���� (�� ������������� ������ ��� ����).
///   </summary>
public class HeavyHorseAttackStrategy : IAttackable
{
    public List<Vector3Int> CalculateAttacks(IBoardManager board, Piece piece)
    {
        List<Vector3Int> attacks = new List<Vector3Int>();
        Vector3Int pos = piece.Position;

        Vector3Int[] directions = {
            new Vector3Int(2, 0, 1),
            new Vector3Int(2, 0, -1),
            new Vector3Int(-2, 0, 1),
            new Vector3Int(-2, 0, -1),
            new Vector3Int(1, 0, 2),
            new Vector3Int(1, 0, -2),
            new Vector3Int(-1, 0, 2),
            new Vector3Int(-1, 0, -2)
        };

        foreach (var dir in directions)
        {
            Vector3Int targetPos = pos + dir;
            if (board.IsWithinBounds(targetPos) &&
                board.IsOccupied(targetPos) &&
                board.GetPieceAt(targetPos).IsPlayer1 != piece.IsPlayer1)
            {
                if (IsAttackPossible(board, pos, dir, targetPos)) //   ������� targetPos
                {
                    attacks.Add(targetPos);
                }
            }
        }

        return attacks;
    }

    ///   <summary>
    ///   ���������, �������� �� ����� �� ������� ������.
    ///   ����� ��������, ���� ���� �� ���� �� L-�������� ����� � ���� ��������.
    ///   </summary>
    private bool IsAttackPossible(IBoardManager board, Vector3Int startPos, Vector3Int dir, Vector3Int targetPos) //   ��������� targetPos
    {
        Vector3Int[] intermediatePositions1 = new Vector3Int[2];
        Vector3Int[] intermediatePositions2 = new Vector3Int[2];
        int dx = dir.x;
        int dz = dir.z;

        Debug.Log($"IsAttackPossible: StartPos = {startPos}, Dir = {dir}, TargetPos = {targetPos}");

        if (Mathf.Abs(dx) == 2 && Mathf.Abs(dz) == 1)
        {
            intermediatePositions1[0] = startPos + new Vector3Int(dx / 2, 0, 0);
            intermediatePositions1[1] = startPos + new Vector3Int(dx, 0, 0);
            intermediatePositions2[0] = startPos + new Vector3Int(0, 0, dz);
            intermediatePositions2[1] = startPos + new Vector3Int(dx / 2, 0, dz);

            Debug.Log($"  Path 1: Pos1 = {intermediatePositions1[0]}, Pos2 = {intermediatePositions1[1]}");
            Debug.Log($"  Path 2: Pos1 = {intermediatePositions2[0]}, Pos2 = {intermediatePositions2[1]}");
        }

        else if (Mathf.Abs(dx) == 1 && Mathf.Abs(dz) == 2)
        {
            intermediatePositions1[0] = startPos + new Vector3Int(dx, 0, 0);
            intermediatePositions1[1] = startPos + new Vector3Int(dx, 0, dz / 2);
            intermediatePositions2[0] = startPos + new Vector3Int(0, 0, dz / 2);
            intermediatePositions2[1] = startPos + new Vector3Int(0, 0, dz);

            Debug.Log($"  Path 1: Pos1 = {intermediatePositions1[0]}, Pos2 = {intermediatePositions1[1]}");
            Debug.Log($"  Path 2: Pos1 = {intermediatePositions2[0]}, Pos2 = {intermediatePositions2[1]}");
        }

        //   ��������� ������ ����
        bool path1Clear = true;
        foreach (var pos in intermediatePositions1)
        {
            if (board.IsBlocked(pos))//���������, ������������� �� ������ ������� ��� �����
            {
                Debug.Log($"  Path 1 blocked at {pos}");
                path1Clear = false;
                break;//   ���� ���� ���� ������ ������, ���� �� ��������
            }
            else
            {
                Debug.Log($"  Path 1 clear at {pos}");
            }
        }

        //   ��������� ������ ����
        bool path2Clear = true;
        foreach (var pos in intermediatePositions2)
        {
            if (board.IsBlocked(pos))//���������, ������������� �� ������ ������� ��� �����
            {
                Debug.Log($"  Path 2 blocked at {pos}");
                path2Clear = false;
                break;//   ���� ���� ���� ������ ������, ���� �� ��������
            }
            else
            {
                Debug.Log($"  Path 2 clear at {pos}");
            }
        }

        bool result = path1Clear || path2Clear;
        Debug.Log($"  IsAttackPossible result: {result}");
        return result; //   ����� ��������, ���� ���� �� ���� ���� ��������
    }

    public void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager)
    {
        Debug.Log($"HeavyHorseAttackStrategy: Executing melee attack on {target}");
        //   ������� ���: ���������� ������ � ������������
        boardManager.RemovePiece(target);
        piece.GetComponent<PieceAnimator>().MoveTo(target, () =>
        {
            boardManager.MovePiece(piece, piece.Position, target);
        });
    }
}