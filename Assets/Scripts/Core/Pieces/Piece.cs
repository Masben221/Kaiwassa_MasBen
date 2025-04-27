using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ������������ ����� �����.
/// </summary>
public enum PieceType
{
    King,
    Dragon,
    Elephant,
    HeavyCavalry,
    LightHorse,
    Spearman,
    Crossbowman,
    Rabble,
    Catapult,
    Trebuchet,
    Mountain // ��������� ��� ���
}

/// <summary>
/// ����������� ������� ����� ��� ���� ������� ����� � ���.
/// �������� �� ����������������, ������������� � ������������� ��������/����� ����� ���������.
/// </summary>
public abstract class Piece : MonoBehaviour
{
    protected IMovable movementStrategy; // ��������� �������� ������
    protected IAttackable attackStrategy; // ��������� ����� ������
    private Vector3Int position; // ������� ������ �� ����� (� �������)
    private bool isPlayer1; // �������������� ������ (true = ����� 1, false = ����� 2)
    [SerializeField] private PieceType type; // ��� ������

    public Vector3Int Position => position; // ������ ��� �������
    public bool IsPlayer1 => isPlayer1; // ������ ��� �������������� ������
    public PieceType Type => type; // ������ ��� ���� ������

    private void Awake()
    {
        SetupStrategies();
    }

    /// <summary>
    /// ����������� ����� ��� ��������� ��������� �������� � �����.
    /// ����������� � �������� ������� (KingPiece, DragonPiece, MountainPiece � �.�.).
    /// </summary>
    protected abstract void SetupStrategies();

    /// <summary>
    /// �������������� ������: ����� �������������� ������ � ��������.
    /// </summary>
    /// <param name="isPlayer1">true, ���� ������ ����������� ������ 1.</param>
    /// <param name="material">�������� ��� ���������� (������/�������).</param>
    public void Initialize(bool isPlayer1, Material material)
    {
        this.isPlayer1 = isPlayer1;
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material = material;
        }
        else
        {
            Debug.LogWarning($"Piece {GetType().Name}: No Renderer found");
        }
        Debug.Log($"Piece {GetType().Name} initialized for Player {(isPlayer1 ? 1 : 2)} with material {material?.name}");
    }

    /// <summary>
    /// ������������� ������� ������ �� �����.
    /// ����������� ��������� ���������� � ������� (y=0.5f ��� ������ ��� ������).
    /// </summary>
    /// <param name="newPosition">����� ������� � ��������� �����������.</param>
    public void SetPosition(Vector3Int newPosition)
    {
        position = newPosition;
        transform.position = new Vector3(newPosition.x, 0.5f, newPosition.z); // y=0.5f ��� ���������� ������
        Debug.Log($"Piece {GetType().Name} set to position {newPosition} (world: {transform.position})");
    }

    /// <summary>
    /// ���������� ������ ���������� ����� ��� ������.
    /// ���������� ���������� ��������� ��������.
    /// </summary>
    /// <param name="board">��������� ����� ��� �������� ���������.</param>
    /// <returns>������ ������, ���� ������ ����� �����.</returns>
    public List<Vector3Int> GetValidMoves(IBoardManager board)
    {
        return movementStrategy?.CalculateMoves(board, this) ?? new List<Vector3Int>();
    }

    /// <summary>
    /// ���������� ������ ���������� ������ ��� �����.
    /// ���������� ���������� ��������� �����.
    /// </summary>
    /// <param name="board">��������� ����� ��� �������� ���������.</param>
    /// <returns>������ ������, ������� ������ ����� ���������.</returns>
    public List<Vector3Int> GetAttackMoves(IBoardManager board)
    {
        return attackStrategy?.CalculateAttacks(board, this) ?? new List<Vector3Int>();
    }

    /// <summary>
    /// ��������� ����� �� ��������� ������.
    /// ���������� ���������� ��������� �����.
    /// </summary>
    /// <param name="target">������ ��� �����.</param>
    /// <param name="boardManager">��������� ����� ��� ��������� ���������.</param>
    public void Attack(Vector3Int target, IBoardManager boardManager)
    {
        if (attackStrategy != null)
        {
            Debug.Log($"Piece {GetType().Name} initiating attack on {target}");
            attackStrategy.ExecuteAttack(this, target, boardManager);
        }
        else
        {
            Debug.LogWarning($"Piece {GetType().Name}: No attack strategy assigned");
        }
    }
}
