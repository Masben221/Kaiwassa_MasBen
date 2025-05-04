using UnityEngine;
using Zenject;

/// <summary>
/// Интерфейс фабрики для создания игровых фигур, включая горы.
/// </summary>
public interface IPieceFactory
{
    Piece CreatePiece(PieceType type, bool isPlayer1, Vector3Int position);
    Sprite GetIconForPiece(PieceType type);
}

/// <summary>
/// Фабрика для создания игровых фигур и гор.
/// Отвечает за инстанцирование префабов, их инициализацию и поворот для второго игрока.
/// </summary>
public class PieceFactory : MonoBehaviour, IPieceFactory
{
    [SerializeField] private GameObject kingPrefab;
    [SerializeField] private GameObject dragonPrefab;
    [SerializeField] private GameObject elephantPrefab;
    [SerializeField] private GameObject heavyCavalryPrefab;
    [SerializeField] private GameObject lightHorsePrefab;
    [SerializeField] private GameObject spearmanPrefab;
    [SerializeField] private GameObject crossbowmanPrefab;
    [SerializeField] private GameObject rabblePrefab;
    [SerializeField] private GameObject catapultPrefab;
    [SerializeField] private GameObject trebuchetPrefab;
    [SerializeField] private GameObject mountainPrefab;
    [SerializeField] private GameObject swordsmanPrefab;
    [SerializeField] private GameObject archerPrefab;
    [SerializeField] private Material player1Material;
    [SerializeField] private Material player2Material;
    [SerializeField] private Sprite defaultSprite; // НОВОЕ: Дефолтный спрайт для UI

    private DiContainer container;

    [Inject]
    public void Construct(DiContainer diContainer)
    {
        container = diContainer;
    }

    private void Awake()
    {
        // Проверяем наличие всех префабов
        if (kingPrefab == null || dragonPrefab == null || heavyCavalryPrefab == null || mountainPrefab == null ||
            swordsmanPrefab == null || archerPrefab == null)
        {
            Debug.LogError("PieceFactory: Required prefabs (King, Dragon, HeavyCavalry, Mountain, Swordsman, Archer) not assigned!");
        }
        // НОВОЕ: Проверяем дефолтный спрайт
        if (defaultSprite == null)
        {
            Debug.LogWarning("PieceFactory: Default sprite not assigned, using empty sprite.");
        }
    }

    public Piece CreatePiece(PieceType type, bool isPlayer1, Vector3Int position)
    {
        GameObject prefab = null;
        switch (type)
        {
            case PieceType.King: prefab = kingPrefab; break;
            case PieceType.Dragon: prefab = dragonPrefab; break;
            case PieceType.Elephant: prefab = elephantPrefab; break;
            case PieceType.HeavyCavalry: prefab = heavyCavalryPrefab; break;
            case PieceType.LightHorse: prefab = lightHorsePrefab; break;
            case PieceType.Spearman: prefab = spearmanPrefab; break;
            case PieceType.Crossbowman: prefab = crossbowmanPrefab; break;
            case PieceType.Rabble: prefab = rabblePrefab; break;
            case PieceType.Catapult: prefab = catapultPrefab; break;
            case PieceType.Trebuchet: prefab = trebuchetPrefab; break;
            case PieceType.Mountain: prefab = mountainPrefab; break;
            case PieceType.Swordsman: prefab = swordsmanPrefab; break;
            case PieceType.Archer: prefab = archerPrefab; break;
            default:
                Debug.LogError($"PieceFactory: Unknown piece type {type}");
                return null;
        }

        if (prefab == null)
        {
            Debug.LogWarning($"PieceFactory: Prefab for {type} is not assigned!");
            return null;
        }

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

    // НОВОЕ: Возвращает спрайт иконки для указанного типа фигуры
    public Sprite GetIconForPiece(PieceType type)
    {
        GameObject prefab = null;
        switch (type)
        {
            case PieceType.King: prefab = kingPrefab; break;
            case PieceType.Dragon: prefab = dragonPrefab; break;
            case PieceType.Elephant: prefab = elephantPrefab; break;
            case PieceType.HeavyCavalry: prefab = heavyCavalryPrefab; break;
            case PieceType.LightHorse: prefab = lightHorsePrefab; break;
            case PieceType.Spearman: prefab = spearmanPrefab; break;
            case PieceType.Crossbowman: prefab = crossbowmanPrefab; break;
            case PieceType.Rabble: prefab = rabblePrefab; break;
            case PieceType.Catapult: prefab = catapultPrefab; break;
            case PieceType.Trebuchet: prefab = trebuchetPrefab; break;
            case PieceType.Mountain: prefab = mountainPrefab; break;
            case PieceType.Swordsman: prefab = swordsmanPrefab; break;
            case PieceType.Archer: prefab = archerPrefab; break;
            default:
                Debug.LogError($"PieceFactory: Unknown piece type {type}");
                return defaultSprite;
        }

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
}