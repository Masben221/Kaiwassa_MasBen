/// <summary>
/// ����� ��� ���, ����������� �� Piece.
/// ���� �� ��������� � �� �������, ������� ��������� ������.
/// </summary>
public class MountainPiece : Piece
{
    protected override void SetupStrategies()
    {
        movementStrategy = null; // ���� �� ���������
        attackStrategy = null;   // ���� �� �������
    }
}