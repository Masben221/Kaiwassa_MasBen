using UnityEngine;
using Zenject;

/// <summary>
/// Интерфейс фабрики для создания игровых фигур, включая горы.
/// </summary>
public interface IPieceFactory
{
    Piece CreatePiece(PieceType type, bool isPlayer1, Vector3Int position);
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
    [SerializeField] private GameObject swordsmanPrefab; // Префаб для Мечника
    [SerializeField] private GameObject archerPrefab;   // Префаб для Лучника
    [SerializeField] private Material player1Material;  // Слоновая кость (игрок 1)
    [SerializeField] private Material player2Material;  // Чёрный обсидиан (игрок 2)

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
    }

    /// <summary>
    /// Создаёт фигуру или гору указанного типа на заданной позиции.
    /// Поворачивает фигуры второго игрока на 180 градусов (кроме гор).
    /// </summary>
    /// <param name="type">Тип фигуры (King, Dragon, Mountain, Swordsman, Archer и т.д.).</param>
    /// <param name="isPlayer1">true, если фигура для игрока 1.</param>
    /// <param name="position">Позиция в клеточных координатах.</param>
    /// <returns>Созданная фигура или null, если создание не удалось.</returns>
    public Piece CreatePiece(PieceType type, bool isPlayer1, Vector3Int position)
    {
        GameObject prefab = null;
        switch (type)
        {
            case PieceType.King:
                prefab = kingPrefab;
                break;
            case PieceType.Dragon:
                prefab = dragonPrefab;
                break;
            case PieceType.Elephant:
                prefab = elephantPrefab;
                break;
            case PieceType.HeavyCavalry:
                prefab = heavyCavalryPrefab;
                break;
            case PieceType.LightHorse:
                prefab = lightHorsePrefab;
                break;
            case PieceType.Spearman:
                prefab = spearmanPrefab;
                break;
            case PieceType.Crossbowman:
                prefab = crossbowmanPrefab;
                break;
            case PieceType.Rabble:
                prefab = rabblePrefab;
                break;
            case PieceType.Catapult:
                prefab = catapultPrefab;
                break;
            case PieceType.Trebuchet:
                prefab = trebuchetPrefab;
                break;
            case PieceType.Mountain:
                prefab = mountainPrefab;
                break;
            case PieceType.Swordsman:
                prefab = swordsmanPrefab;
                break;
            case PieceType.Archer:
                prefab = archerPrefab;
                break;
            default:
                Debug.LogError($"PieceFactory: Unknown piece type {type}");
                return null;
        }

        if (prefab == null)
        {
            Debug.LogWarning($"PieceFactory: Prefab for {type} is not assigned!");
            return null;
        }

        // Поворот для второго игрока (180 градусов по Y), для гор поворот не нужен
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
}