using System.Collections.Generic;
using UnityEngine;
using Zenject;

/// <summary>
/// Реализация фигуры "Король".
/// </summary>
public class KingPiece : Piece
{
    // Инициализация короля
    public void Awake()
    {
        Initialize(true); // По умолчанию для игрока 1, можно изменить при создании
    }

    protected override void SetupStrategies()
    {
        // Устанавливаем стратегии для короля
        movementStrategy = new KingMoveStrategy();
        attackStrategy = new KingAttackStrategy();
        Debug.Log("KingPiece strategies set up.");
    }
}

public class KingMoveStrategy : IMovable
{
    public List<Vector3Int> CalculateMoves(IBoardManager board)
    {
        List<Vector3Int> moves = new List<Vector3Int>();
        Vector3Int pos = board.GetPieceAt(new Vector3Int(0, 0, 0))?.Position ?? Vector3Int.zero;

        int[] directions = { -1, 0, 1 };

        foreach (int dx in directions)
        {
            foreach (int dz in directions)
            {
                if (dx == 0 && dz == 0) continue;
                Vector3Int newPos = pos + new Vector3Int(dx, 0, dz);
                if (board.IsWithinBounds(newPos) && !board.IsOccupied(newPos))
                {
                    moves.Add(newPos);
                }
            }
        }
        return moves;
    }
}

public class KingAttackStrategy : IAttackable
{
    public List<Vector3Int> CalculateAttacks(IBoardManager board)
    {
        List<Vector3Int> attacks = new List<Vector3Int>();
        IPiece piece = board.GetPieceAt(new Vector3Int(0, 0, 0));
        Vector3Int pos = piece?.Position ?? Vector3Int.zero;

        int[] directions = { -1, 0, 1 };

        foreach (int dx in directions)
        {
            foreach (int dz in directions)
            {
                if (dx == 0 && dz == 0) continue;
                Vector3Int newPos = pos + new Vector3Int(dx, 0, dz);
                if (board.IsWithinBounds(newPos) && board.IsOccupied(newPos) &&
                    board.GetPieceAt(newPos).IsPlayer1 != piece.IsPlayer1)
                {
                    attacks.Add(newPos);
                }
            }
        }
        return attacks;
    }
}