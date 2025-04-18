using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Интерфейс для стратегий движения фигур.
/// </summary>
public interface IMovable
{
    List<Vector3Int> CalculateMoves(IBoardManager board, Piece piece);
}

/// <summary>
/// Интерфейс для стратегий атаки фигур.
/// </summary>
public interface IAttackable
{
    List<Vector3Int> CalculateAttacks(IBoardManager board, Piece piece);
    void ExecuteAttack(Piece piece, Vector3Int target, IBoardManager boardManager);
}
