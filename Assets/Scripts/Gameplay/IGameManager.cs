using UnityEngine;

/// <summary>
/// ��������� ��� ���������� ������� ����.
/// </summary>
public interface IGameManager
{
    void StartGame(); // ������ ����
    void MakeMove(IPiece piece, Vector3Int target); // ���������� ����
    bool IsPlayer1Turn { get; } // ��� ������ ���

    event System.Action<bool> OnTurnChanged; // ������� ����� ���� (������� Observer)
}