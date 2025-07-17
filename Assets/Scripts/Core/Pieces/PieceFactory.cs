using System.Collections.Generic;
using UnityEngine;
using Zenject;

/// <summary>
/// ��������� ������� ��� �������� ������� �����, ������� ����.
/// </summary>
public interface IPieceFactory
{
    Piece CreatePiece(PieceType type, bool isPlayer1, Vector3Int position);
    Sprite GetIconForPiece(PieceType type);
}

/// <summary>
/// ������� ��� �������� ������� ����� � ���.
/// �������� �� ��������������� ��������, �� ������������� � ������� ��� ������� ������.
/// </summary>
public class PieceFactory : MonoBehaviour, IPieceFactory
{
    [SerializeField, Tooltip("������ ������")] private GameObject kingPrefab;
    [SerializeField, Tooltip("������ �������")] private GameObject dragonPrefab;
    [SerializeField, Tooltip("������ �����")] private GameObject elephantPrefab;
    [SerializeField, Tooltip("������ ������ ���������")] private GameObject heavyCavalryPrefab;
    [SerializeField, Tooltip("������ ˸���� ���������")] private GameObject lightHorsePrefab;
    [SerializeField, Tooltip("������ ���������")] private GameObject spearmanPrefab;
    [SerializeField, Tooltip("������ �����������")] private GameObject crossbowmanPrefab;
    [SerializeField, Tooltip("������ ���������")] private GameObject rabblePrefab;
    [SerializeField, Tooltip("������ ����������")] private GameObject catapultPrefab;
    [SerializeField, Tooltip("������ ���������")] private GameObject trebuchetPrefab;
    [SerializeField, Tooltip("������ ����")] private GameObject mountainPrefab;
    [SerializeField, Tooltip("������ �������")] private GameObject swordsmanPrefab;
    [SerializeField, Tooltip("������ �������")] private GameObject archerPrefab;
    [SerializeField, Tooltip("�������� ��� ����� ������ 1")] private Material player1Material;
    [SerializeField, Tooltip("�������� ��� ����� ������ 2")] private Material player2Material;
    [SerializeField, Tooltip("��������� ������ ��� UI")] private Sprite defaultSprite;
    [SerializeField, Tooltip("������������� ������������ �������� �� ���������")]
    private PieceAnimationConfig defaultAnimationConfig;

    private DiContainer container;

    [Inject]
    public void Construct(DiContainer diContainer)
    {
        container = diContainer;
    }

    private void Awake()
    {
        // �������� ������� ���� ��������
        if (kingPrefab == null || dragonPrefab == null || heavyCavalryPrefab == null || mountainPrefab == null ||
            swordsmanPrefab == null || archerPrefab == null)
        {
            Debug.LogError("PieceFactory: Required prefabs (King, Dragon, HeavyCavalry, Mountain, Swordsman, Archer) not assigned!");
        }
        if (defaultSprite == null)
        {
            Debug.LogWarning("PieceFactory: Default sprite not assigned, using empty sprite.");
        }
        if (defaultAnimationConfig == null)
        {
            Debug.LogWarning("PieceFactory: Default animation config not assigned!");
        }
    }

    public Piece CreatePiece(PieceType type, bool isPlayer1, Vector3Int position)
    {
        GameObject prefab = GetPrefabForType(type);
        if (prefab == null)
        {
            Debug.LogWarning($"PieceFactory: Prefab for {type} is not assigned!");
            return null;
        }

        // ������������� �������: 0� ��� ������ 1, 180� ��� ������ 2 (����� ���)
        Quaternion rotation = (type != PieceType.Mountain && !isPlayer1) ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
        GameObject pieceObject = container.InstantiatePrefab(prefab, new Vector3(position.x, 0.5f, position.z), rotation, null);
        Piece piece = pieceObject.GetComponent<Piece>();
        if (piece == null)
        {
            Debug.LogError($"PieceFactory: No Piece component on {type} prefab!");
            Destroy(pieceObject);
            return null;
        }

        Material material = isPlayer1 ? player1Material : player2Material;
        if (material == null)
        {
            Debug.LogError($"PieceFactory: Material for Player {(isPlayer1 ? 1 : 2)} not assigned!");
            Destroy(pieceObject);
            return null;
        }

        piece.Initialize(isPlayer1, material);
        piece.SetPosition(position);
        Debug.Log($"PieceFactory: Created {type} for Player {(isPlayer1 ? 1 : 2)} at {position} (world: {pieceObject.transform.position}, rotation: {pieceObject.transform.rotation.eulerAngles})");
        return piece;
    }

    public Sprite GetIconForPiece(PieceType type)
    {
        GameObject prefab = GetPrefabForType(type);
        if (prefab == null)
        {
            Debug.LogWarning($"PieceFactory: Prefab for {type} is not assigned!");
            return defaultSprite;
        }

        Piece piece = prefab.GetComponent<Piece>();
        if (piece == null)
        {
            Debug.LogError($"PieceFactory: No Piece component on {type} prefab!");
            return defaultSprite;
        }

        Sprite sprite = piece.IconSprite;
        if (sprite == null)
        {
            Debug.LogWarning($"PieceFactory: Sprite not found for piece type {type}");
            return defaultSprite;
        }

        return sprite;
    }

    /// <summary>
    /// ���������� ��������� ������������ ��������.
    /// </summary>
    public PieceAnimationConfig GetDefaultAnimationConfig()
    {
        if (defaultAnimationConfig == null)
        {
            Debug.LogWarning("PieceFactory: Default animation config not assigned!");
        }
        return defaultAnimationConfig;
    }

    // ��������������� ����� ��� ��������� ������� �� ���� ������
    private GameObject GetPrefabForType(PieceType type)
    {
        switch (type)
        {
            case PieceType.King: return kingPrefab;
            case PieceType.Dragon: return dragonPrefab;
            case PieceType.Elephant: return elephantPrefab;
            case PieceType.HeavyCavalry: return heavyCavalryPrefab;
            case PieceType.LightHorse: return lightHorsePrefab;
            case PieceType.Spearman: return spearmanPrefab;
            case PieceType.Crossbowman: return crossbowmanPrefab;
            case PieceType.Rabble: return rabblePrefab;
            case PieceType.Catapult: return catapultPrefab;
            case PieceType.Trebuchet: return trebuchetPrefab;
            case PieceType.Mountain: return mountainPrefab;
            case PieceType.Swordsman: return swordsmanPrefab;
            case PieceType.Archer: return archerPrefab;
            default:
                Debug.LogError($"PieceFactory: Unknown piece type {type}");
                return null;
        }
    }
}