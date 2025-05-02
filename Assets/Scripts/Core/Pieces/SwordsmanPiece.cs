using UnityEngine;

/// <summary>
/// ����� ��� ������ "������".
/// ��������� �������� � ����� �� 1 ������ � ����� �����������, ��� � ������.
/// </summary>
public class SwordsmanPiece : Piece
{
    /// <summary>
    /// ����������� ��������� �������� � ����� ��� �������.
    /// ���������� �� �� ���������, ��� � ������ (�������� � ����� �� 1 ������ �� ��� �������).
    /// </summary>
    protected override void SetupStrategies()
    {
        movementStrategy = new KingMoveStrategy();
        attackStrategy = new KingAttackStrategy();
        Debug.Log("SwordsmanPiece: Strategies set up.");
    }
}