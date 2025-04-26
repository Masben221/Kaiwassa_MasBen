using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Zenject;

/// <summary>
/// Управляет логикой ручной расстановки фигур и гор.
/// Реализует интерфейс IPiecePlacementManager для совместимости с системой.
/// </summary>
public class ManualPlacementManager : MonoBehaviour, IPiecePlacementManager
{
    [Inject] private IBoardManager boardManager; // Интерфейс для управления доской
    [Inject] private IPieceFactory pieceFactory; // Фабрика для создания фигур и гор

    private Dictionary<PieceType, int> player1Pieces = new Dictionary<PieceType, int>(); // Фигуры игрока 1
    private Dictionary<PieceType, int> player2Pieces = new Dictionary<PieceType, int>(); // Фигуры игрока 2
    private int mountainsPerSide; // Количество гор на сторону
    private int player1MountainsRemaining; // Оставшиеся горы для игрока 1
    private int player2MountainsRemaining; // Оставшиеся горы для игрока 2

    /// <summary>
    /// Геттер для количества гор на сторону.
    /// </summary>
    public int GetMountainsPerSide => mountainsPerSide;

    /// <summary>
    /// Инициализирует количество фигур и гор для ручной расстановки.
    /// </summary>
    /// <param name="mountainsPerSide">Количество гор на сторону, выбранное на экране настроек.</param>
    public void Initialize(int mountainsPerSide)
    {
        this.mountainsPerSide = mountainsPerSide;
        player1MountainsRemaining = mountainsPerSide;
        player2MountainsRemaining = mountainsPerSide;

        // Инициализация фигур для обоих игроков (соответствует PiecePlacementManager)
        player1Pieces = new Dictionary<PieceType, int>
        {
            { PieceType.King, 1 },
            { PieceType.Dragon, 1 },
            { PieceType.Elephant, 2 },
            { PieceType.HeavyCavalry, 2 },
            { PieceType.LightHorse, 3 },
            { PieceType.Spearman, 3 },
            { PieceType.Crossbowman, 3 },
            { PieceType.Rabble, 3 },
            { PieceType.Catapult, 1 },
            { PieceType.Trebuchet, 1 }
        };
        player2Pieces = new Dictionary<PieceType, int>(player1Pieces); // Копия для игрока 2
        Debug.Log($"ManualPlacementManager: Initialized with {mountainsPerSide} mountains per side.");
    }

    /// <summary>
    /// Проверяет, можно ли разместить фигуру или гору на указанной позиции.
    /// </summary>
    /// <param name="isPlayer1">true, если размещает игрок 1.</param>
    /// <param name="position">Позиция на доске.</param>
    /// <param name="isMountain">true, если размещается гора.</param>
    /// <returns>true, если размещение возможно.</returns>
    public bool CanPlace(bool isPlayer1, Vector3Int position, bool isMountain)
    {
        // Проверка границ доски и занятости клетки
        if (!boardManager.IsWithinBounds(position) || boardManager.IsBlocked(position))
            return false;

        // Ограничение по z: игрок 1 — z=0-3, игрок 2 — z=6-9
        if (isPlayer1 && (position.z < 0 || position.z > 3))
            return false;
        if (!isPlayer1 && (position.z < 6 || position.z > 9))
            return false;

        // Проверка доступности гор
        if (isMountain)
        {
            return isPlayer1 ? player1MountainsRemaining > 0 : player2MountainsRemaining > 0;
        }

        return true;
    }

    /// <summary>
    /// Размещает фигуру или гору на доске.
    /// </summary>
    /// <param name="isPlayer1">true, если размещает игрок 1.</param>
    /// <param name="position">Позиция на доске.</param>
    /// <param name="type">Тип фигуры (игнорируется для гор).</param>
    /// <param name="isMountain">true, если размещается гора.</param>
    /// <returns>true, если размещение успешно.</returns>
    public bool PlacePieceOrMountain(bool isPlayer1, Vector3Int position, PieceType type, bool isMountain)
    {
        if (!CanPlace(isPlayer1, position, isMountain))
        {
            Debug.LogWarning($"ManualPlacementManager: Cannot place {(isMountain ? "mountain" : type.ToString())} at {position} for Player {(isPlayer1 ? 1 : 2)}");
            return false;
        }

        if (isMountain)
        {
            var mountain = pieceFactory.CreateMountain(position);
            if (mountain != null)
            {
                boardManager.PlaceMountain(position, mountain);
                if (isPlayer1)
                    player1MountainsRemaining--;
                else
                    player2MountainsRemaining--;
                Debug.Log($"ManualPlacementManager: Placed mountain at {position} for Player {(isPlayer1 ? 1 : 2)}");
                return true;
            }
        }
        else
        {
            var pieces = isPlayer1 ? player1Pieces : player2Pieces;
            if (pieces.ContainsKey(type) && pieces[type] > 0)
            {
                var piece = pieceFactory.CreatePiece(type, isPlayer1, position);
                if (piece != null)
                {
                    boardManager.PlacePiece(piece, position);
                    pieces[type]--;
                    Debug.Log($"ManualPlacementManager: Placed {type} at {position} for Player {(isPlayer1 ? 1 : 2)}");
                    return true;
                }
            }
        }
        Debug.LogWarning($"ManualPlacementManager: Failed to place {(isMountain ? "mountain" : type.ToString())} at {position}");
        return false;
    }

    /// <summary>
    /// Возвращает количество оставшихся фигур или гор для игрока.
    /// </summary>
    /// <param name="isPlayer1">true, если проверяется игрок 1.</param>
    /// <param name="type">Тип фигуры (игнорируется для гор).</param>
    /// <param name="isMountain">true, если проверяются горы.</param>
    /// <returns>Количество оставшихся элементов.</returns>
    public int GetRemainingCount(bool isPlayer1, PieceType type, bool isMountain)
    {
        if (isMountain)
            return isPlayer1 ? player1MountainsRemaining : player2MountainsRemaining;
        var pieces = isPlayer1 ? player1Pieces : player2Pieces;
        return pieces.ContainsKey(type) ? pieces[type] : 0;
    }

    /// <summary>
    /// Проверяет, завершил ли игрок расстановку (все фигуры и горы размещены).
    /// </summary>
    /// <param name="isPlayer1">true, если проверяется игрок 1.</param>
    /// <returns>true, если расстановка завершена.</returns>
    public bool HasCompletedPlacement(bool isPlayer1)
    {
        var pieces = isPlayer1 ? player1Pieces : player2Pieces;
        // Проверяем, что все фигуры размещены (count == 0) и горы тоже
        return pieces.Values.All(count => count == 0) && (isPlayer1 ? player1MountainsRemaining : player2MountainsRemaining) == 0;
    }

    /// <summary>
    /// Проверяет, размещён ли король игроком.
    /// </summary>
    /// <param name="isPlayer1">true, если проверяется игрок 1.</param>
    /// <returns>true, если король ещё не размещён.</returns>
    public bool IsKingNotPlaced(bool isPlayer1)
    {
        var pieces = isPlayer1 ? player1Pieces : player2Pieces;
        return pieces.ContainsKey(PieceType.King) && pieces[PieceType.King] > 0;
    }

    // Реализация интерфейса IPiecePlacementManager (заглушки для совместимости с автоматической расстановкой)
    public void PlaceMountains(int mountainsPerSide) { /* Заглушка: горы уже размещены вручную */ }
    public void PlacePiecesForPlayer(bool isPlayer1) { /* Заглушка: фигуры уже размещены вручную */ }
}