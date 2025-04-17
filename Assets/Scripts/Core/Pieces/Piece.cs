using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ������� ����� ��� ���� �����. ��������� ����� ������ �������, ��������� � ����������� ����������.
/// </summary>
public abstract class Piece : MonoBehaviour, IPiece
{
    // ������� ������� � 3D
    public Vector3Int Position { get; private set; }

    // �������������� ������
    public bool IsPlayer1 { get; private set; }

    // ������ �� GameObject ������
    public GameObject GameObject => gameObject;

    // ��������� �������� � ����� �������� � ����������
    protected IMovable movementStrategy;
    protected IAttackable attackStrategy;

    /// <summary>
    /// ���������������� ������������� ��� �������� �������.
    /// �������� ��������� ���������.
    /// </summary>
    protected virtual void Awake()
    {
        SetupStrategies();
    }

    /// <summary>
    /// �������������� ������: ����� �������������� ������ � ��������� ��������.
    /// </summary>
    public void Initialize(bool isPlayer1, Material material)
    {
        IsPlayer1 = isPlayer1;

        // ��������� �������� � ��������� Renderer
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null && material != null)
        {
            renderer.material = material;
        }
        else
        {
            Debug.LogWarning($"No Renderer found in children of {gameObject.name} or material is null.");
        }

        // ������������ ������ ������� ������ �� 180 ��������
        if (!isPlayer1)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        Debug.Log($"Initialized piece for Player {(isPlayer1 ? 1 : 2)}");
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
        transform.position = new Vector3(position.x, 0.5f, position.z); // ���� ������, Y=0.5 ��� ���������
    }

    public List<Vector3Int> GetValidMoves(IBoardManager board)
    {
        return movementStrategy.CalculateMoves(board, this);
    }

    public List<Vector3Int> GetAttackMoves(IBoardManager board)
    {
        return attackStrategy.CalculateAttacks(board, this);
    }
}