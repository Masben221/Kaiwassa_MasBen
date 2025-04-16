using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ������� ����� ��� ���� �����. ��������� ����� ������ �������, ��������� � ����������� ����������.
/// </summary>
public abstract class Piece : MonoBehaviour, IPiece
{
    // ������� ������� � 3D
    public Vector3Int Position { get; private set; }

    // �������������� ������ (true - ����� 1, false - ����� 2)
    public bool IsPlayer1 { get; private set; }

    // ��������� �������� � ����� �������� � ����������
    protected IMovable movementStrategy;
    protected IAttackable attackStrategy;

    /// <summary>
    /// �������������� ������: ����� �������������� ������ � ��������� ��������.
    /// </summary>
    public void Initialize(bool isPlayer1, Material material)
    {
        IsPlayer1 = isPlayer1;
        SetupStrategies(); // ����������� ��������� �������� � �����

        // ��������� �������� � ������
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null && material != null)
        {
            renderer.material = material;
        }

        // ������������ ������ ������� ������ �� 180 ��������
        if (!isPlayer1)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    /// <summary>
    /// ����������� ����� ��� ��������� ��������� � ����������.
    /// </summary>
    protected abstract void SetupStrategies();

    /// <summary>
    /// ������������� ������� ������ � 3D-������������.
    /// </summary>
    public void SetPosition(Vector3Int position)
    {
        Position = position;
        transform.position = new Vector3(position.x, 0.5f, position.z); // Y=0.5 ��� ��������� ��� ������
    }

    public List<Vector3Int> GetValidMoves(IBoardManager board)
    {
        return movementStrategy.CalculateMoves(board);
    }

    public List<Vector3Int> GetAttackMoves(IBoardManager board)
    {
        return attackStrategy.CalculateAttacks(board);
    }
}