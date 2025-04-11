using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ������� ����� ��� ���� �����. ��������� ����� ������ ������� � ���������.
/// ��� ���������� ������ (KingPiece, DragonPiece � �.�.) ����������� �� ����� ������.
/// </summary>
public abstract class Piece : MonoBehaviour, IPiece
{
    // ������� ������� ������ � 3D-������������
    public Vector3Int Position { get; private set; }

    // �������������� ������ (true ��� ������ 1, false ��� ������ 2)
    public bool IsPlayer1 { get; private set; }

    // ��������� �������� � �����, ������� ����� ���������� � ����������
    protected IMovable movementStrategy;
    protected IAttackable attackStrategy;

    /// <summary>
    /// ������������� ������. ������������� �������������� ������ � ����������� ���������.
    /// </summary>
    /// <param name="isPlayer1">True, ���� ������ ����������� ������� ������.</param>
    public void Initialize(bool isPlayer1)
    {
        IsPlayer1 = isPlayer1;
        SetupStrategies(); // �������� ��������� ���������
    }

    /// <summary>
    /// ����������� ����� ��� ��������� ��������� �������� � �����.
    /// ������ ���� ���������� � ������ ��������� (��������, KingPiece, DragonPiece).
    /// </summary>
    protected abstract void SetupStrategies();

    /// <summary>
    /// ������������� ������� ������ �� ����� � ��������� � ��������� � 3D-������������.
    /// </summary>
    /// <param name="position">����� ������� � ����������� (X, Y, Z).</param>
    public void SetPosition(Vector3Int position)
    {
        Position = position;
        transform.position = new Vector3(position.x, 0.5f, position.z); // Y=0.5 ��� ��������� ��� ������
    }

    /// <summary>
    /// ���������� ������ ��������� �����, ��������� ������ ��������� ��������.
    /// </summary>
    public List<Vector3Int> GetValidMoves(IBoardManager board)
    {
        return movementStrategy.CalculateMoves(board);
    }

    /// <summary>
    /// ���������� ������ ������� ��� �����, ��������� ������ ��������� �����.
    /// </summary>
    public List<Vector3Int> GetAttackMoves(IBoardManager board)
    {
        return attackStrategy.CalculateAttacks(board);
    }
}