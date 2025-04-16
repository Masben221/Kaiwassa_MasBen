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

/// <summary>
/// ������� ��� �������� �����. ����� ������� � ��������� ��� ������� ������.
/// </summary>
public class PieceFactory : MonoBehaviour
{
    // ������� �����, �������� � ����������
    [SerializeField] private GameObject dragonPrefab;
    [SerializeField] private GameObject kingPrefab;

    // ��������� ��� ����� �������
    [SerializeField] private Material player1Material; // ��� ������ 1
    [SerializeField] private Material player2Material; // ��� ������ 2

    /// <summary>
    /// ������ ������ ��������� ���� � ������ �������������� ������.
    /// </summary>
    public IPiece CreatePiece(PieceType type, bool isPlayer1, Vector3Int position)
    {
        // �������� ������ � ����������� �� ���� ������
        GameObject prefab = type switch
        {
            PieceType.Dragon => dragonPrefab,
            PieceType.King => kingPrefab,
            _ => throw new System.NotImplementedException($"Piece type {type} not implemented")
        };

        // ������ ��������� �� �������
        GameObject instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        IPiece piece = instance.GetComponent<IPiece>();

        // �������� �������� � ����������� �� ������
        Material material = isPlayer1 ? player1Material : player2Material;

        // �������������� ������ � ������ ����������
        piece.Initialize(isPlayer1, material);
        piece.SetPosition(position);

        Debug.Log($"Created {type} for Player {(isPlayer1 ? 1 : 2)} at {position}");
        return piece;
    }
}