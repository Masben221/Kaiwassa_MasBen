using UnityEngine;

/// <summary>
/// ������� ��� �������� ������� �����. ���������� �������, �������� � ����������.
/// </summary>
public class PieceFactory : MonoBehaviour
{
    // ������� �����, ������� ����� ���������� � Unity Inspector
    [SerializeField] private GameObject dragonPrefab;
    [SerializeField] private GameObject kingPrefab;

    /// <summary>
    /// ������ ������ ��������� ����, �������������� � � ������������� ��������� �������.
    /// </summary>
    /// <param name="type">��� ������ (King, Dragon � �.�.).</param>
    /// <param name="isPlayer1">�������������� ������.</param>
    /// <param name="position">��������� ������� �� �����.</param>
    /// <returns>��������� ������, ����������� IPiece.</returns>
    public IPiece CreatePiece(PieceType type, bool isPlayer1, Vector3Int position)
    {
        // �������� ������ � ����������� �� ���� ������
        GameObject prefab = type switch
        {
            PieceType.Dragon => dragonPrefab,
            PieceType.King => kingPrefab,
            _ => throw new System.NotImplementedException($"Piece type {type} not implemented")
        };

        // ������ ��������� ������ �� �������
        GameObject instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        IPiece piece = instance.GetComponent<IPiece>();

        // �������������� ������ � ������������� � �������
        piece.Initialize(isPlayer1);
        piece.SetPosition(position);

        Debug.Log($"Created {type} for Player {(isPlayer1 ? 1 : 2)} at {position}");
        return piece;
    }
}