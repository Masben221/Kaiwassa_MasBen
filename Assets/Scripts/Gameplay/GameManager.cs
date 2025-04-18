using UnityEngine;
using Zenject;
using System;
using System.Collections.Generic;

/// <summary>
/// Интерфейс для управления игрой.
/// </summary>
public interface IGameManager
{
    void StartGame(); // Инициализация игры
    void MakeMove(Piece piece, Vector3Int target); // Выполнение хода или атаки
    bool IsPlayer1Turn { get; } // Чей ход
    event Action<bool> OnTurnChanged; // Событие смены хода
}

/// <summary>
/// Управляет логикой игры: инициализация доски, размещение фигур, обработка ходов и атак, смена игроков.
/// </summary>
public class GameManager : MonoBehaviour, IGameManager
{
    [Inject] private IBoardManager boardManager; // Интерфейс доски
    [Inject] private IPieceFactory pieceFactory; // Фабрика фигур

    [SerializeField] private int mountainsPerSide = 4; // Количество гор на сторону
    [SerializeField] private int kingsPerSide = 1; // Короли на сторону
    [SerializeField] private int dragonsPerSide = 1; // Драконы на сторону
    [SerializeField] private int heavyCavalryPerSide = 2; // Тяжёлая кавалерия на сторону
    [SerializeField] private int elephantsPerSide = 0; // Слоны на сторону
    [SerializeField] private int lightHorsesPerSide = 0; // Лёгкая кавалерия на сторону
    [SerializeField] private int spearmenPerSide = 0; // Копейщики на сторону
    [SerializeField] private int crossbowmenPerSide = 0; // Арбалетчики на сторону
    [SerializeField] private int rabblePerSide = 0; // Толпа на сторону
    [SerializeField] private int catapultsPerSide = 0; // Катапульты на сторону
    [SerializeField] private int trebuchetsPerSide = 0; // Требушеты на сторону

    private bool isPlayer1Turn = true; // Текущий ход (true = Игрок 1)
    public bool IsPlayer1Turn => isPlayer1Turn; // Геттер для текущего хода
    public event Action<bool> OnTurnChanged; // Событие смены хода

    private void Start()
    {
        Debug.Log("GameManager: Start called.");
        StartGame();
    }

    /// <summary>
    /// Инициализирует игру: создаёт доску 10x10, размещает горы и фигуры.
    /// </summary>
    public void StartGame()
    {
        Debug.Log("GameManager: StartGame called.");
        boardManager.InitializeBoard(10);

        Debug.Log("GameManager: Placing mountains...");
        boardManager.PlaceMountains(mountainsPerSide);

        Debug.Log("GameManager: Placing pieces...");
        PlacePiecesForPlayer(true); // Игрок 1
        PlacePiecesForPlayer(false); // Игрок 2

        Debug.Log("GameManager: Game started successfully!");
        OnTurnChanged?.Invoke(isPlayer1Turn);
    }

    /// <summary>
    /// Размещает фигуры для указанного игрока на его половине доски (z=0–4 для Игрока 1, z=5–9 для Игрока 2).
    /// </summary>
    /// <param name="isPlayer1">true, если фигуры для Игрока 1.</param>
    private void PlacePiecesForPlayer(bool isPlayer1)
    {
        List<Vector3Int> availablePositions = new List<Vector3Int>();
        int zStart = isPlayer1 ? 0 : 5;
        int zEnd = isPlayer1 ? 5 : 10;

        // Собираем доступные позиции
        for (int x = 0; x < 10; x++)
        {
            for (int z = zStart; z < zEnd; z++)
            {
                Vector3Int pos = new Vector3Int(x, 0, z);
                if (!boardManager.IsBlocked(pos))
                {
                    availablePositions.Add(pos);
                }
            }
        }

        // Список фигур и их количества
        (PieceType type, int count)[] piecesToPlace = {
            (PieceType.King, kingsPerSide),
            (PieceType.Dragon, dragonsPerSide),
            (PieceType.HeavyCavalry, heavyCavalryPerSide),
            (PieceType.Elephant, elephantsPerSide),
            (PieceType.LightHorse, lightHorsesPerSide),
            (PieceType.Spearman, spearmenPerSide),
            (PieceType.Crossbowman, crossbowmenPerSide),
            (PieceType.Rabble, rabblePerSide),
            (PieceType.Catapult, catapultsPerSide),
            (PieceType.Trebuchet, trebuchetsPerSide)
        };

        // Размещаем фигуры случайным образом
        foreach (var (type, count) in piecesToPlace)
        {
            for (int i = 0; i < count && availablePositions.Count > 0; i++)
            {
                int index = UnityEngine.Random.Range(0, availablePositions.Count);
                Vector3Int pos = availablePositions[index];
                PlacePiece(type, isPlayer1, pos);
                availablePositions.RemoveAt(index);
            }
        }
    }

    /// <summary>
    /// Создаёт и размещает фигуру на доске через фабрику.
    /// </summary>
    /// <param name="type">Тип фигуры.</param>
    /// <param name="isPlayer1">true, если для Игрока 1.</param>
    /// <param name="position">Позиция на доске.</param>
    private void PlacePiece(PieceType type, bool isPlayer1, Vector3Int position)
    {
        Debug.Log($"GameManager: Creating {type} for Player {(isPlayer1 ? 1 : 2)} at {position}");
        Piece piece = pieceFactory.CreatePiece(type, isPlayer1, position);
        if (piece != null)
        {
            boardManager.PlacePiece(piece, position);
        }
        else
        {
            Debug.LogWarning($"GameManager: Failed to create {type} at {position}");
        }
    }

    /// <summary>
    /// Обрабатывает ход или атаку фигуры.
    /// Сначала проверяет возможность атаки, затем ход.
    /// </summary>
    /// <param name="piece">Фигура, которая действует.</param>
    /// <param name="target">Целевая клетка.</param>
    public void MakeMove(Piece piece, Vector3Int target)
    {
        Debug.Log($"GameManager: Attempting move for piece {piece.GetType().Name} at {piece.Position} to {target}");
        if (piece.IsPlayer1 != isPlayer1Turn)
        {
            Debug.LogWarning("GameManager: Not your turn!");
            return;
        }

        var validMoves = piece.GetValidMoves(boardManager);
        var attackMoves = piece.GetAttackMoves(boardManager);

        if (attackMoves.Contains(target))
        {
            Piece targetPiece = boardManager.GetPieceAt(target);
            if (targetPiece != null && targetPiece.IsPlayer1 != piece.IsPlayer1)
            {
                Debug.Log($"GameManager: Valid attack on piece {targetPiece.GetType().Name} at {target}");
                piece.Attack(target, boardManager);
                SwitchTurn();
            }
            else
            {
                Debug.LogWarning($"GameManager: No valid enemy piece at {target} to attack!");
            }
        }
        else if (validMoves.Contains(target))
        {
            Debug.Log($"GameManager: Valid move to {target}");
            piece.GetComponent<PieceAnimator>().MoveTo(target, () =>
            {
                boardManager.MovePiece(piece, piece.Position, target);
                SwitchTurn();
            });
        }
        else
        {
            Debug.LogWarning($"GameManager: Invalid move or attack to {target}");
        }
    }

    /// <summary>
    /// Переключает ход между игроками и уведомляет подписчиков.
    /// </summary>
    private void SwitchTurn()
    {
        isPlayer1Turn = !isPlayer1Turn;
        OnTurnChanged?.Invoke(isPlayer1Turn);
        Debug.Log($"GameManager: Turn switched to Player {(isPlayer1Turn ? 1 : 2)}");
    }
}