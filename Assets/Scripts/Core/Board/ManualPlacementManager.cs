using UnityEngine;
using System.Collections.Generic;
using Zenject;

/// <summary>
/// Управляет ручной расстановкой фигур и гор на доске.
/// Отвечает за проверку допустимости размещения, управление счётчиками фигур и перемещение.
/// </summary>
public class ManualPlacementManager : MonoBehaviour, IPiecePlacementManager
{
    // Зависимости, инъектируемые через Zenject
    [Inject] private IBoardManager boardManager; // Менеджер доски для проверки клеток и размещения фигур
    [Inject] private IPieceFactory pieceFactory; // Фабрика для создания фигур

    // Счётчики оставшихся фигур для каждого игрока
    private Dictionary<PieceType, int> player1Pieces = new Dictionary<PieceType, int>(); // Фигуры игрока 1
    private Dictionary<PieceType, int> player2Pieces = new Dictionary<PieceType, int>(); // Фигуры игрока 2

    // Количество гор на сторону (настраивается через UI)
    private int mountainsPerSide;

    // Свойство для получения количества гор на сторону
    public int GetMountainsPerSide => mountainsPerSide;

    /// <summary>
    /// Инициализирует счётчики фигур и гор для обоих игроков.
    /// </summary>
    /// <param name="mountainsPerSide">Количество гор на сторону.</param>
    public void Initialize(int mountainsPerSide)
    {
        this.mountainsPerSide = mountainsPerSide;
        // Инициализируем счётчики фигур для игрока 1
        player1Pieces = new Dictionary<PieceType, int>
        {
            { PieceType.King, 1 },
            { PieceType.Dragon, 1 },
            { PieceType.Elephant, 2 },
            { PieceType.HeavyCavalry, 2 },
            { PieceType.LightHorse, 2 }, // Уменьшено до 2
            { PieceType.Spearman, 2 },   // Уменьшено до 2
            { PieceType.Crossbowman, 2 }, // Уменьшено до 2
            { PieceType.Rabble, 2 },     // Уменьшено до 2
            { PieceType.Catapult, 1 },
            { PieceType.Trebuchet, 1 },
            { PieceType.Swordsman, 2 },  // Новая фигура: Мечник
            { PieceType.Archer, 2 },     // Новая фигура: Лучник
            { PieceType.Mountain, mountainsPerSide }
        };
        // Копируем счётчики для игрока 2
        player2Pieces = new Dictionary<PieceType, int>(player1Pieces);
        Debug.Log($"ManualPlacementManager: Initialized with {mountainsPerSide} mountains per side.");
    }

    /// <summary>
    /// Проверяет, можно ли разместить фигуру или гору на указанной позиции.
    /// </summary>
    /// <param name="isPlayer1">True, если размещает игрок 1.</param>
    /// <param name="position">Координаты клетки на доске.</param>
    /// <param name="type">Тип фигуры (например, King, Mountain, Swordsman, Archer).</param>
    /// <param name="isMove">True, если это перемещение (не влияет на счётчик).</param>
    /// <returns>True, если размещение возможно.</returns>
    public bool CanPlace(bool isPlayer1, Vector3Int position, PieceType type, bool isMove = false)
    {
        // Проверяем, что позиция находится в пределах доски и не занята
        if (!boardManager.IsWithinBounds(position) || boardManager.IsOccupied(position))
        {
            Debug.Log($"ManualPlacementManager: Cannot place {type} at {position} - out of bounds or occupied.");
            return false;
        }

        // Ограничиваем зоны: z 0–3 для игрока 1, z 6–9 для игрока 2
        if (isPlayer1 && (position.z < 0 || position.z > 3))
        {
            Debug.Log($"ManualPlacementManager: Cannot place {type} at {position} - out of zone for Player 1.");
            return false;
        }
        if (!isPlayer1 && (position.z < 6 || position.z > 9))
        {
            Debug.Log($"ManualPlacementManager: Cannot place {type} at {position} - out of zone for Player 2.");
            return false;
        }

        // Проверяем, есть ли доступные фигуры указанного типа (если это не перемещение)
        var pieces = isPlayer1 ? player1Pieces : player2Pieces;
        if (!isMove && (!pieces.ContainsKey(type) || pieces[type] <= 0))
        {
            Debug.Log($"ManualPlacementManager: Cannot place {type} - none remaining for Player {(isPlayer1 ? 1 : 2)}.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Проверяет, можно ли переместить фигуру на новую позицию.
    /// </summary>
    /// <param name="isPlayer1">True, если перемещает игрок 1.</param>
    /// <param name="type">Тип фигуры.</param>
    /// <param name="newPosition">Новая позиция.</param>
    /// <returns>True, если перемещение возможно.</returns>
    public bool CanMove(bool isPlayer1, PieceType type, Vector3Int newPosition)
    {
        // Проверяем, что позиция в пределах доски и не занята
        if (!boardManager.IsWithinBounds(newPosition) || boardManager.IsOccupied(newPosition))
        {
            Debug.Log($"ManualPlacementManager: Cannot move {type} to {newPosition} - out of bounds or occupied.");
            return false;
        }

        // Ограничиваем зоны: z 0–3 для игрока 1, z 6–9 для игрока 2
        if (isPlayer1 && (newPosition.z < 0 || newPosition.z > 3))
        {
            Debug.Log($"ManualPlacementManager: Cannot move {type} to {newPosition} - out of zone for Player 1.");
            return false;
        }
        if (!isPlayer1 && (newPosition.z < 6 || newPosition.z > 9))
        {
            Debug.Log($"ManualPlacementManager: Cannot move {type} to {newPosition} - out of zone for Player 2.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Размещает фигуру или гору на доске.
    /// Используется только для ручной расстановки через UI.
    /// </summary>
    /// <param name="isPlayer1">True, если размещает игрок 1.</param>
    /// <param name="position">Координаты клетки на доске.</param>
    /// <param name="type">Тип фигуры (например, King, Mountain, Swordsman, Archer).</param>
    /// <param name="isMove">True, если это перемещение (не уменьшает счётчик).</param>
    /// <returns>True, если размещение успешно.</returns>
    public bool PlacePieceOrMountain(bool isPlayer1, Vector3Int position, PieceType type, bool isMove = false)
    {
        // Проверяем, можно ли разместить фигуру
        if (!CanPlace(isPlayer1, position, type, isMove))
        {
            Debug.LogWarning($"ManualPlacementManager: Cannot place {type} at {position} for Player {(isPlayer1 ? 1 : 2)}.");
            return false;
        }

        // Создаём фигуру и размещаем её на доске
        Piece piece = pieceFactory.CreatePiece(type, isPlayer1, position);
        if (piece != null)
        {
            boardManager.PlacePiece(piece, position);
            // Уменьшаем счётчик только если это не перемещение
            if (!isMove)
            {
                var pieces = isPlayer1 ? player1Pieces : player2Pieces;
                pieces[type]--;
                Debug.Log($"ManualPlacementManager: Placed {type} at {position} for Player {(isPlayer1 ? 1 : 2)}. Remaining: {pieces[type]}.");
            }
            return true;
        }

        Debug.LogWarning($"ManualPlacementManager: Failed to create {type} at {position} for Player {(isPlayer1 ? 1 : 2)}.");
        return false;
    }

    /// <summary>
    /// Уменьшает счётчик оставшихся фигур указанного типа.
    /// Используется для синхронизации счётчиков после случайной генерации.
    /// </summary>
    /// <param name="isPlayer1">True, если для игрока 1.</param>
    /// <param name="type">Тип фигуры.</param>
    /// <returns>True, если счётчик успешно уменьшен.</returns>
    public bool DecreasePieceCount(bool isPlayer1, PieceType type)
    {
        var pieces = isPlayer1 ? player1Pieces : player2Pieces;
        if (!pieces.ContainsKey(type) || pieces[type] <= 0)
        {
            Debug.LogWarning($"ManualPlacementManager: Cannot decrease count for {type} - none remaining for Player {(isPlayer1 ? 1 : 2)}.");
            return false;
        }

        pieces[type]--;
        Debug.Log($"ManualPlacementManager: Decreased count for {type} for Player {(isPlayer1 ? 1 : 2)}. Remaining: {pieces[type]}.");
        return true;
    }

    /// <summary>
    /// Удаляет фигуру или гору с доски и увеличивает счётчик.
    /// </summary>
    /// <param name="isPlayer1">True, если удаляет игрок 1.</param>
    /// <param name="position">Координаты клетки на доске.</param>
    /// <param name="type">Тип фигуры.</param>
    /// <returns>True, если удаление успешно.</returns>
    public bool RemovePiece(bool isPlayer1, Vector3Int position, PieceType type)
    {
        if (!boardManager.IsWithinBounds(position))
        {
            Debug.LogWarning($"ManualPlacementManager: Cannot remove piece at {position} - out of bounds.");
            return false;
        }

        var piece = boardManager.GetPieceAt(position);
        if (piece != null && piece.IsPlayer1 == isPlayer1 && piece.Type == type)
        {
            boardManager.RemovePiece(position);
            var pieces = isPlayer1 ? player1Pieces : player2Pieces;
            pieces[type]++;
            Destroy(piece.gameObject);
            Debug.Log($"ManualPlacementManager: Removed {type} at {position} for Player {(isPlayer1 ? 1 : 2)}. Remaining: {pieces[type]}.");
            return true;
        }

        Debug.LogWarning($"ManualPlacementManager: No piece {type} at {position} for Player {(isPlayer1 ? 1 : 2)}.");
        return false;
    }

    /// <summary>
    /// Удаляет указанную фигуру с доски и увеличивает счётчик.
    /// </summary>
    /// <param name="piece">Фигура для удаления.</param>
    /// <returns>True, если удаление успешно.</returns>
    public bool RemovePiece(Piece piece)
    {
        if (!boardManager.IsWithinBounds(piece.Position))
        {
            Debug.LogWarning($"ManualPlacementManager: Cannot remove piece at {piece.Position} - out of bounds.");
            return false;
        }

        var existingPiece = boardManager.GetPieceAt(piece.Position);
        if (existingPiece == piece)
        {
            boardManager.RemovePiece(piece.Position);
            var pieces = piece.IsPlayer1 ? player1Pieces : player2Pieces;
            pieces[piece.Type]++;
            Destroy(piece.gameObject);
            Debug.Log($"ManualPlacementManager: Removed {piece.Type} at {piece.Position} for Player {(piece.IsPlayer1 ? 1 : 2)}. Remaining: {pieces[piece.Type]}.");
            return true;
        }

        Debug.LogWarning($"ManualPlacementManager: No piece {piece.Type} at {piece.Position}.");
        return false;
    }

    /// <summary>
    /// Перемещает фигуру с одной позиции на другую.
    /// </summary>
    /// <param name="piece">Фигура для перемещения.</param>
    /// <param name="from">Исходная позиция.</param>
    /// <param name="to">Новая позиция.</param>
    /// <returns>True, если перемещение успешно.</returns>
    public bool MovePiece(Piece piece, Vector3Int from, Vector3Int to)
    {
        if (!boardManager.IsWithinBounds(from) || !boardManager.IsWithinBounds(to))
        {
            Debug.LogWarning($"ManualPlacementManager: Cannot move {piece.Type} from {from} to {to} - out of bounds.");
            return false;
        }

        var existingPiece = boardManager.GetPieceAt(from);
        if (existingPiece != piece || boardManager.IsOccupied(to))
        {
            Debug.LogWarning($"ManualPlacementManager: Cannot move {piece.Type} from {from} to {to} - piece mismatch or target occupied.");
            return false;
        }

        bool isPlayer1 = piece.IsPlayer1;
        if (isPlayer1 && (to.z < 0 || to.z > 3))
        {
            Debug.LogWarning($"ManualPlacementManager: Cannot move {piece.Type} to {to} - out of zone for Player 1.");
            return false;
        }
        if (!isPlayer1 && (to.z < 6 || to.z > 9))
        {
            Debug.LogWarning($"ManualPlacementManager: Cannot move {piece.Type} to {to} - out of zone for Player 2.");
            return false;
        }

        boardManager.MovePiece(piece, from, to);
        Debug.Log($"ManualPlacementManager: Moved {piece.Type} from {from} to {to} for Player {(isPlayer1 ? 1 : 2)}.");
        return true;
    }

    /// <summary>
    /// Возвращает количество оставшихся фигур указанного типа.
    /// </summary>
    /// <param name="isPlayer1">True, если для игрока 1.</param>
    /// <param name="type">Тип фигуры.</param>
    /// <returns>Количество оставшихся фигур.</returns>
    public int GetRemainingCount(bool isPlayer1, PieceType type)
    {
        var pieces = isPlayer1 ? player1Pieces : player2Pieces;
        return pieces.ContainsKey(type) ? pieces[type] : 0;
    }

    /// <summary>
    /// Проверяет, завершена ли расстановка для игрока (все счётчики равны 0).
    /// </summary>
    /// <param name="isPlayer1">True, если для игрока 1.</param>
    /// <returns>True, если все фигуры размещены.</returns>
    public bool HasCompletedPlacement(bool isPlayer1)
    {
        var pieces = isPlayer1 ? player1Pieces : player2Pieces;
        foreach (var count in pieces.Values)
        {
            if (count > 0) return false;
        }
        return true;
    }

    /// <summary>
    /// Проверяет, размещён ли король.
    /// </summary>
    /// <param name="isPlayer1">True, если для игрока 1.</param>
    /// <returns>True, если король ещё не размещён.</returns>
    public bool IsKingNotPlaced(bool isPlayer1)
    {
        var pieces = isPlayer1 ? player1Pieces : player2Pieces;
        return pieces.ContainsKey(PieceType.King) && pieces[PieceType.King] > 0;
    }

    /// <summary>
    /// Не поддерживается в ручной расстановке.
    /// Вызывается только для автоматической расстановки через PiecePlacementManager.
    /// </summary>
    public void PlacePiecesForPlayer(bool isPlayer1, int selectedMountains)
    {
        Debug.LogWarning("ManualPlacementManager: PlacePiecesForPlayer not supported in manual placement.");
    }
}