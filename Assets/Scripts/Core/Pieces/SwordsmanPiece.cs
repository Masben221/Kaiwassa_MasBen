using UnityEngine;

/// <summary>
/// Класс для фигуры "Мечник".
/// Реализует движение и атаку на 1 клетку в любом направлении, как у Короля.
/// </summary>
public class SwordsmanPiece : Piece
{
    /// <summary>
    /// Настраивает стратегии движения и атаки для Мечника.
    /// Использует те же стратегии, что и Король (движение и атака на 1 клетку во все стороны).
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new KingMoveStrategy();
        attackStrategy = new KingAttackStrategy();
        Debug.Log("SwordsmanPiece: Strategies set up.");
    }
}