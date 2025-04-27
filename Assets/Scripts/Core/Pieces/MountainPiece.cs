/// <summary>
/// Класс для гор, наследуется от Piece.
/// Горы не двигаются и не атакуют, поэтому стратегии пустые.
/// </summary>
public class MountainPiece : Piece
{
    protected override void SetupStrategies()
    {
        movementStrategy = null; // Горы не двигаются
        attackStrategy = null;   // Горы не атакуют
    }
}