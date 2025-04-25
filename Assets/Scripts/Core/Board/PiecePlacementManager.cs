using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

/// <summary>
/// Интерфейс для управления расстановкой гор и фигур.
/// </summary>
public interface IPiecePlacementManager
{
    void PlaceMountains(int mountainsPerSide);
    void PlacePiecesForPlayer(bool isPlayer1);
}

/// <summary>
/// Управляет логикой расстановки гор и фигур на доске.
/// Следует принципу единственной ответственности (SOLID).
/// </summary>
public class PiecePlacementManager : MonoBehaviour, IPiecePlacementManager
{
    [Inject] private IBoardManager boardManager;
    [Inject] private IPieceFactory pieceFactory;

    [SerializeField] private int kingsPerSide = 1; // Короли на сторону
    [SerializeField] private int dragonsPerSide = 1; // Драконы на сторону
    [SerializeField] private int heavyCavalryPerSide = 2; // Тяжёлая кавалерия на сторону
    [SerializeField] private int elephantsPerSide = 2; // Слоны на сторону
    [SerializeField] private int lightHorsesPerSide = 3; // Лёгкая кавалерия на сторону
    [SerializeField] private int spearmenPerSide = 3; // Копейщики на сторону
    [SerializeField] private int crossbowmenPerSide = 3; // Арбалетчики на сторону
    [SerializeField] private int rabblePerSide = 3; // Ополчение на сторону
    [SerializeField] private int catapultsPerSide = 1; // Катапульты на сторону
    [SerializeField] private int trebuchetsPerSide = 1; // Требушеты на сторону

    private readonly List<Vector3Int> occupiedPositions = new List<Vector3Int>(); // Занятые клетки
    private readonly List<Vector3Int> blockedPositions = new List<Vector3Int>(); // Заблокированные линии атаки

    /// <summary>
    /// Размещает горы в зависимости от их количества, исключая нейтральную зону (z=4–5).
    /// </summary>
    public void PlaceMountains(int mountainsPerSide)
    {
        if (boardManager == null)
        {
            Debug.LogError("PiecePlacementManager: boardManager is null in PlaceMountains!");
            return;
        }

        // Очищаем занятые позиции
        occupiedPositions.Clear();

        // Определяем линии для гор (z=0 — первая линия)
        int[] zLinesPlayer1, zLinesPlayer2;
        if (mountainsPerSide <= 4)
        {
            zLinesPlayer1 = new[] { 3 }; // 4-я линия (z=3)
            zLinesPlayer2 = new[] { 6 }; // 7-я линия (z=6)
        }
        else if (mountainsPerSide <= 7)
        {
            zLinesPlayer1 = new[] { 2, 3 }; // 3–4-я линии (z=2,3)
            zLinesPlayer2 = new[] { 6, 7 }; // 7–8-я линии (z=6,7)
        }
        else
        {
            zLinesPlayer1 = new[] { 1, 2, 3 }; // 2–4-я линии (z=1,2,3)
            zLinesPlayer2 = new[] { 6, 7, 8 }; // 7–9-я линии (z=6,7,8)
        }

        // Зоны для гор
        List<Vector3Int> player1Positions = GetMountainPositions(zLinesPlayer1);
        List<Vector3Int> player2Positions = GetMountainPositions(zLinesPlayer2);

        // Обеспечиваем проходы
        EnsurePassages(player1Positions, mountainsPerSide, zLinesPlayer1);
        EnsurePassages(player2Positions, mountainsPerSide, zLinesPlayer2);

        // Размещаем горы
        PlaceMountainsForPlayer(player1Positions, mountainsPerSide, true);
        PlaceMountainsForPlayer(player2Positions, mountainsPerSide, false);
    }

    /// <summary>
    /// Получает позиции для гор в заданных линиях z, исключая нейтральную зону (z=4–5).
    /// </summary>
    private List<Vector3Int> GetMountainPositions(int[] zLines)
    {
        List<Vector3Int> positions = new List<Vector3Int>();
        foreach (int z in zLines)
        {
            if (z == 4 || z == 5)
            {
                Debug.LogWarning($"PiecePlacementManager: Attempted to place mountains in neutral zone (z={z}). Skipping.");
                continue;
            }
            for (int x = 0; x < 10; x++)
            {
                Vector3Int pos = new Vector3Int(x, 0, z);
                if (!boardManager.IsBlocked(pos) && !occupiedPositions.Contains(pos))
                {
                    positions.Add(pos);
                }
            }
        }
        return positions;
    }

    /// <summary>
    /// Обеспечивает минимум 2 прохода на каждой линии z.
    /// </summary>
    private void EnsurePassages(List<Vector3Int> positions, int mountainsPerSide, int[] zLines)
    {
        foreach (int z in zLines)
        {
            var zPositions = positions.Where(p => p.z == z).ToList();
            if (zPositions.Count <= 2) continue; // Уже есть проходы

            int maxMountains = zPositions.Count - 2; // Оставляем 2 клетки без гор
            while (zPositions.Count > maxMountains && mountainsPerSide > 0)
            {
                int index = UnityEngine.Random.Range(0, zPositions.Count);
                positions.Remove(zPositions[index]);
                zPositions.RemoveAt(index);
                mountainsPerSide--;
            }
        }
    }

    /// <summary>
    /// Размещает горы для игрока.
    /// </summary>
    private void PlaceMountainsForPlayer(List<Vector3Int> positions, int mountainsPerSide, bool isPlayer1)
    {
        mountainsPerSide = Mathf.Min(mountainsPerSide, positions.Count);
        for (int i = 0; i < mountainsPerSide; i++)
        {
            if (positions.Count == 0) break;
            int index = UnityEngine.Random.Range(0, positions.Count);
            Vector3Int pos = positions[index];
            GameObject mountain = pieceFactory.CreateMountain(pos);
            if (mountain != null)
            {
                boardManager.PlaceMountain(pos, mountain);
                occupiedPositions.Add(pos);
                Debug.Log($"PiecePlacementManager: Placed mountain for Player {(isPlayer1 ? 1 : 2)} at {pos}");
            }
            else
            {
                Debug.LogWarning($"PiecePlacementManager: Failed to create mountain at {pos}");
            }
            positions.RemoveAt(index);
        }
    }

    /// <summary>
    /// Размещает фигуры для игрока на заданных линиях с учётом гор и линий атаки.
    /// </summary>
    public void PlacePiecesForPlayer(bool isPlayer1)
    {
        if (boardManager == null)
        {
            Debug.LogError("PiecePlacementManager: boardManager is null in PlacePiecesForPlayer!");
            return;
        }

        // Очищаем занятые и заблокированные позиции
        occupiedPositions.Clear();
        blockedPositions.Clear();

        // Линии (z=0 — первая линия)
        int zLine1 = isPlayer1 ? 0 : 9; // 1-я линия (z=0), 10-я линия (z=9)
        int zLine2 = isPlayer1 ? 1 : 8; // 2-я линия (z=1), 9-я линия (z=8)
        int zLine3 = isPlayer1 ? 2 : 7; // 3-я линия (z=2), 8-я линия (z=7)
        int zLine4 = isPlayer1 ? 3 : 6; // 4-я линия (z=3), 7-я линия (z=6)
        int zBorder = isPlayer1 ? 3 : 6; // Линия с горами: 4-я линия (z=3), 7-я линия (z=6)

        // 1. Король: первая линия (z=0 или z=9), x=3–6
        PlacePieceInZoneWithFallback(PieceType.King, isPlayer1, x => x >= 3 && x <= 6, new[] { zLine1 }, kingsPerSide);

        // 2. Тяжёлая кавалерия: z=2,3 или z=6,7, за горами
        PlacePieceInZoneWithFallback(PieceType.HeavyCavalry, isPlayer1, x => true, new[] { zLine4, zLine3 },
            heavyCavalryPerSide, pos => boardManager.IsMountain(new Vector3Int(pos.x, 0, zBorder)));

        // 3. Катапульты: z=1 или z=8, напротив проходов, свободная линия атаки
        PlacePieceInZoneWithFallback(PieceType.Catapult, isPlayer1, x => x >= 2 && x <= 7, new[] { zLine2 },
            catapultsPerSide, pos => !boardManager.IsMountain(new Vector3Int(pos.x, 0, zBorder)),
            blockLineOfSight: true);

        // 4. Требушеты: z=1 или z=8, напротив проходов, свободная линия атаки
        PlacePieceInZoneWithFallback(PieceType.Trebuchet, isPlayer1, x => x == 0 || x == 9, new[] { zLine2 },
            trebuchetsPerSide, pos => !boardManager.IsMountain(new Vector3Int(pos.x, 0, zBorder)),
            blockLineOfSight: true);

        // 5. Дракон: z=1,2 или z=7,8, ближе к центру (x=3–6)
        PlacePieceInZoneWithFallback(PieceType.Dragon, isPlayer1, x => x >= 3 && x <= 6, new[] { zLine2, zLine3 }, dragonsPerSide);

        // 6. Слоны: z=2,3 или z=6,7, за горами
        PlacePieceInZoneWithFallback(PieceType.Elephant, isPlayer1, x => true, new[] { zLine4, zLine3 },
            elephantsPerSide, pos => boardManager.IsMountain(new Vector3Int(pos.x, 0, zBorder)));

        // 7. Лёгкая кавалерия, арбалетчики, копейщики: z=2 или z=7,8, случайно
        var randomTypes = new[] { PieceType.LightHorse, PieceType.Crossbowman, PieceType.Spearman }.OrderBy(_ => UnityEngine.Random.value).ToList();
        foreach (var type in randomTypes)
        {
            int count = type switch
            {
                PieceType.LightHorse => lightHorsesPerSide,
                PieceType.Crossbowman => crossbowmenPerSide,
                PieceType.Spearman => spearmenPerSide,
                _ => 0
            };
            PlacePieceInZoneWithFallback(type, isPlayer1, x => true, isPlayer1 ? new[] { zLine3 } : new[] { zLine3, zLine2 }, count,
                prioritizeLineOfSight: type == PieceType.Crossbowman);
        }

        // 8. Ополченцы: z=2,3 или z=6,7
        PlacePieceInZoneWithFallback(PieceType.Rabble, isPlayer1, x => true, new[] { zLine4, zLine3 }, rabblePerSide);
    }

    /// <summary>
    /// Размещает фигуры в заданных линиях с возможностью сдвига в тыл.
    /// </summary>
    private void PlacePieceInZoneWithFallback(PieceType type, bool isPlayer1, Func<int, bool> xCondition, int[] preferredZLines,
        int count, Func<Vector3Int, bool> extraCondition = null, bool prioritizeLineOfSight = false, bool blockLineOfSight = false)
    {
        // Пробуем разместить в предпочтительных линиях
        List<Vector3Int> availablePositions = GetAvailablePositions(xCondition, preferredZLines, extraCondition, prioritizeLineOfSight);
        int placedCount = PlacePieces(type, isPlayer1, availablePositions, Mathf.Min(count, availablePositions.Count), blockLineOfSight);
        count -= placedCount;

        // Если остались фигуры, пробуем соседние линии в тыл
        if (count > 0)
        {
            int[] fallbackZLines = isPlayer1 ? new[] { 1, 0 } : new[] { 8, 9 };
            availablePositions = GetAvailablePositions(xCondition, fallbackZLines, extraCondition, prioritizeLineOfSight);
            placedCount = PlacePieces(type, isPlayer1, availablePositions, Mathf.Min(count, availablePositions.Count), blockLineOfSight);
            if (placedCount < count)
            {
                Debug.LogWarning($"PiecePlacementManager: Could not place all {type} for Player {(isPlayer1 ? 1 : 2)}, placed {placedCount}/{count}");
            }
        }
    }

    /// <summary>
    /// Получает доступные позиции для размещения, исключая нейтральную зону (z=4–5).
    /// </summary>
    private List<Vector3Int> GetAvailablePositions(Func<int, bool> xCondition, int[] zLines, Func<Vector3Int, bool> extraCondition, bool prioritizeLineOfSight)
    {
        List<Vector3Int> availablePositions = new List<Vector3Int>();
        foreach (int z in zLines)
        {
            if (z == 4 || z == 5)
            {
                Debug.LogWarning($"PiecePlacementManager: Attempted to place pieces in neutral zone (z={z}). Skipping.");
                continue;
            }
            for (int x = 0; x < 10; x++)
            {
                Vector3Int pos = new Vector3Int(x, 0, z);
                if (xCondition(x) && !boardManager.IsBlocked(pos) && !occupiedPositions.Contains(pos) && !blockedPositions.Contains(pos) &&
                    (extraCondition == null || extraCondition(pos)))
                {
                    if (prioritizeLineOfSight && BlocksLineOfSight(pos))
                    {
                        continue;
                    }
                    availablePositions.Add(pos);
                }
            }
        }
        return availablePositions;
    }

    /// <summary>
    /// Размещает заданное количество фигур и возвращает число размещённых.
    /// </summary>
    private int PlacePieces(PieceType type, bool isPlayer1, List<Vector3Int> availablePositions, int count, bool blockLineOfSight)
    {
        int placedCount = 0;
        for (int i = 0; i < count; i++)
        {
            if (availablePositions.Count == 0)
            {
                break;
            }
            int index = UnityEngine.Random.Range(0, availablePositions.Count);
            Vector3Int pos = availablePositions[index];
            PlacePiece(type, isPlayer1, pos);
            if (blockLineOfSight)
            {
                BlockLineOfSight(pos, isPlayer1);
            }
            availablePositions.RemoveAt(index);
            placedCount++;
        }
        return placedCount;
    }

    /// <summary>
    /// Проверяет, блокирует ли позиция линии атаки союзников.
    /// </summary>
    private bool BlocksLineOfSight(Vector3Int pos)
    {
        // Проверяем, не пересекается ли позиция с заблокированными линиями атаки
        return blockedPositions.Any(blocked => blocked.x == pos.x);
    }

    /// <summary>
    /// Блокирует линию атаки для катапульт и требушетов.
    /// </summary>
    private void BlockLineOfSight(Vector3Int pos, bool isPlayer1)
    {
        int zStart = isPlayer1 ? 2 : 6;
        int zEnd = isPlayer1 ? 3 : 7; // Ограничиваем до z=3 (Игрок 1) и z=7 (Игрок 2), так как z=4,5 — нейтральная зона
        for (int z = zStart; z <= zEnd; z++)
        {
            Vector3Int blockPos = new Vector3Int(pos.x, 0, z);
            if (!blockedPositions.Contains(blockPos))
            {
                blockedPositions.Add(blockPos);
            }
        }
    }

    /// <summary>
    /// Создаёт и размещает фигуру.
    /// </summary>
    private void PlacePiece(PieceType type, bool isPlayer1, Vector3Int position)
    {
        Piece piece = pieceFactory.CreatePiece(type, isPlayer1, position);
        if (piece != null)
        {
            boardManager.PlacePiece(piece, position);
            occupiedPositions.Add(position);
            Debug.Log($"PiecePlacementManager: Placed {type} for Player {(isPlayer1 ? 1 : 2)} at {position}");
        }
        else
        {
            Debug.LogWarning($"PiecePlacementManager: Failed to create {type} at {position}");
        }
    }

    /// <summary>
    /// Находит позицию фигуры указанного типа.
    /// </summary>
    private Vector3Int? GetPiecePosition(PieceType type, bool isPlayer1)
    {
        foreach (var pair in boardManager.GetAllPieces())
        {
            if (pair.Value.Type == type && pair.Value.IsPlayer1 == isPlayer1)
                return pair.Key;
        }
        return null;
    }
}