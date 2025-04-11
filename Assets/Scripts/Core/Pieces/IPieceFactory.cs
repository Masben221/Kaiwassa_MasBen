using UnityEngine;

// <summary>
/// ��������� ������� ��� �������� �����.
/// ������� �������� Factory ��� ����������������� �������� ��������.
/// </summary>
public interface IPieceFactory
{
    IPiece CreatePiece(PieceType type, bool isPlayer1, Vector3Int position); // ����� �������� ������
}

/// <summary>
/// ������������ ���� ����� ����� � ����.
/// ������������ ��� �����������, ����� ������ ���������.
/// </summary>
public enum PieceType { King, Dragon, Elephant, HeavyHorse, LightHorse, Spearman, Crossbowman, Rabble, Catapult, Trebuchet }
