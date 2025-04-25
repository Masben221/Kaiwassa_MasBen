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
    private List<int> reservedPassagesPlayer1 = new List<int>(); // Зарезервированные проходы для Игрока 1
    private List<int> reservedPassagesPlayer2 = new List<int>(); // Зарезервированные проходы для Игрока 2

    /// <summary>
    /// Размещает горы, максимум 8 на сторону, исключая зарезервированные проходы.
    /// </summary>
    public void PlaceMountains(int mountainsPerSide)
    {
        if (boardManager == null)
        {
            Debug.LogError("PiecePlacementManager: boardManager is null in PlaceMountains!");
            return;
        }

        // Ограничиваем максимальное количество гор до 8
        mountainsPerSide = Mathf.Min(mountainsPerSide, 8);

        // Очищаем занятые позиции и зарезервированные проходы
        occupiedPositions.Clear();
        reservedPassagesPlayer1.Clear();
        reservedPassagesPlayer2.Clear();

        // Резервируем два прохода для Игрока 1
        List<int> availableXPlayer1 = Enumerable.Range(0, 10).ToList();
        for (int i = 0; i < 2 && availableXPlayer1.Count > 0; i++)
        {
            int xPassage = availableXPlayer1[UnityEngine.Random.Range(0, availableXPlayer1.Count)];
            reservedPassagesPlayer1.Add(xPassage);
            availableXPlayer1.Remove(xPassage);
            Debug.Log($"PiecePlacementManager: Reserved passage {i + 1} for Player 1 at x={xPassage} for {(i == 0 ? "catapult" : "trebuchet")}");
        }

        // Резервируем два прохода для Игрока 2
        List<int> availableXPlayer2 = Enumerable.Range(0, 10).ToList();
        for (int i = 0; i < 2 && availableXPlayer2.Count > 0; i++)
        {
            int xPassage = availableXPlayer2[UnityEngine.Random.Range(0, availableXPlayer2.Count)];
            reservedPassagesPlayer2.Add(xPassage);
            availableXPlayer2.Remove(xPassage);
            Debug.Log($"PiecePlacementManager: Reserved passage {i + 1} for Player 2 at x={xPassage} for {(i == 0 ? "catapult" : "trebuchet")}");
        }

        // Определяем линии для гор (z=0 — первая линия)
        int[] zLinesPlayer1, zLinesPlayer2;
        if (mountainsPerSide <= 4)
        {
            zLinesPlayer1 = new[] { 3 }; // 4-я линия (z=3)
            zLinesPlayer2 = new[] { 6 }; // 7-я линия (z=6)
        }
        else
        {
            zLinesPlayer1 = new[] { 2, 3 }; // 3–4-я линии (z=2,3)
            zLinesPlayer2 = new[] { 6, 7 }; // 7–8-я линии (z=6,7)
        }

        // Формируем позиции для гор, исключая зарезервированные проходы обоих игроков
        List<Vector3Int> player1Positions = GetMountainPositions(zLinesPlayer1);
        List<Vector3Int> player2Positions = GetMountainPositions(zLinesPlayer2);

        // Обеспечиваем минимум 2 прохода на линиях гор
        EnsurePassages(player1Positions, mountainsPerSide, zLinesPlayer1, true);
        EnsurePassages(player2Positions, mountainsPerSide, zLinesPlayer2, false);

        // Размещаем горы
        PlaceMountainsForPlayer(player1Positions, mountainsPerSide, true);
        PlaceMountainsForPlayer(player2Positions, mountainsPerSide, false);

        // Проверяем и удаляем горы на зарезервированных проходах
        foreach (int x in reservedPassagesPlayer1.Concat(reservedPassagesPlayer2).Distinct())
        {
            foreach (int z in new[] { 1, 2, 3, 6, 7, 8 })
            {
                Vector3Int pos = new Vector3Int(x, 0, z);
                if (boardManager.IsMountain(pos))
                {
                    Debug.LogError($"PiecePlacementManager: Mountain found at {pos} in reserved passage! Removing...");
                    boardManager.RemovePiece(pos); // Удаляем гору
                }
            }
        }
    }

    /// <summary>
    /// Получает позиции для гор в заданных линиях z, исключая зарезервированные проходы обоих игроков.
    /// </summary>
    private List<Vector3Int> GetMountainPositions(int[] zLines)
    {
        List<Vector3Int> positions = new List<Vector3Int>();
        foreach (int z in zLines)
        {
            for (int x = 0; x < 10; x++)
            {
                if (reservedPassagesPlayer1.Contains(x) || reservedPassagesPlayer2.Contains(x)) continue; // Исключаем проходы
                Vector3Int pos = new Vector3Int(x, 0, z);
                if (!boardManager.IsBlocked(pos) && !occupiedPositions.Contains(pos))
                {
                    positions.Add(pos);
                }
            }
        }
        Debug.Log($"PiecePlacementManager: Mountain positions for z={string.Join(",", zLines)}: {string.Join(", ", positions)}");
        return positions;
    }

    /// <summary>
    /// Обеспечивает минимум 2 прохода на каждой линии z.
    /// </summary>
    private void EnsurePassages(List<Vector3Int> positions, int mountainsPerSide, int[] zLines, bool isPlayer1)
    {
        int[] passageZLines = isPlayer1 ? new[] { 1, 2, 3 } : new[] { 6, 7, 8 }; // Линии для проходов
        List<int> reservedPassages = isPlayer1 ? reservedPassagesPlayer1 : reservedPassagesPlayer2;

        // Удаляем горы на зарезервированных проходах
        foreach (int x in reservedPassages)
        {
            foreach (int z in passageZLines)
            {
                Vector3Int pos = new Vector3Int(x, 0, z);
                if (boardManager.IsMountain(pos))
                {
                    boardManager.RemovePiece(pos);
                    Debug.Log($"PiecePlacementManager: Removed mountain at {pos} to ensure passage");
                }
                var passagePos = positions.FirstOrDefault(p => p.x == x && p.z == z);
                if (passagePos != default)
                {
                    positions.Remove(passagePos);
                    Debug.Log($"PiecePlacementManager: Ensured no mountain at {passagePos} for passage");
                }
            }
        }

        // Обеспечиваем минимум 2 прохода на линиях гор
        foreach (int z in zLines)
        {
            var zPositions = positions.Where(p => p.z == z).ToList();
            if (zPositions.Count <= 2) continue; // Уже есть проходы

            int maxMountains = zPositions.Count - 2; // Минимум 2 прохода
            int remainingMountains = mountainsPerSide;
            while (zPositions.Count > maxMountains && remainingMountains > 0)
            {
                int index = UnityEngine.Random.Range(0, zPositions.Count);
                positions.Remove(zPositions[index]);
                zPositions.RemoveAt(index);
                remainingMountains--;
            }
        }
        Debug.Log($"PiecePlacementManager: Final reservedPassages for Player {(isPlayer1 ? 1 : 2)}={string.Join(", ", reservedPassages)}");
    }

    /// <summary>
    /// Размещает горы для игрока, исключая зарезервированные проходы.
    /// </summary>
    private void PlaceMountainsForPlayer(List<Vector3Int> positions, int mountainsPerSide, bool isPlayer1)
    {
        List<int> reservedPassages = isPlayer1 ? reservedPassagesPlayer1 : reservedPassagesPlayer2;
        mountainsPerSide = Mathf.Min(mountainsPerSide, positions.Count);
        for (int i = 0; i < mountainsPerSide; i++)
        {
            if (positions.Count == 0) break;
            int index = UnityEngine.Random.Range(0, positions.Count);
            Vector3Int pos = positions[index];
            if (reservedPassages.Contains(pos.x))
            {
                Debug.LogWarning($"PiecePlacementManager: Attempted to place mountain at {pos} in reserved passage! Skipping...");
                positions.RemoveAt(index);
                continue;
            }
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
        int[] passageZLines = isPlayer1 ? new[] { 1, 2, 3 } : new[] { 6, 7, 8 }; // Линии для проходов
        List<int> reservedPassages = isPlayer1 ? reservedPassagesPlayer1 : reservedPassagesPlayer2;

        // Очищаем зарезервированные проходы от гор
        foreach (int x in reservedPassages)
        {
            foreach (int z in passageZLines)
            {
                Vector3Int pos = new Vector3Int(x, 0, z);
                if (boardManager.IsMountain(pos))
                {
                    boardManager.RemovePiece(pos);
                    Debug.Log($"PiecePlacementManager: Cleared mountain at {pos} for {(reservedPassages.IndexOf(x) == 0 ? "catapult" : "trebuchet")}");
                }
            }
        }

        // 1. Катапульты: z=1 или z=0 (z=8 или z=9), на первом зарезервированном проходе
        PlacePieceWithGuarantee(PieceType.Catapult, isPlayer1,
            x => reservedPassages.Count > 0 && x == reservedPassages[0],
            isPlayer1 ? new[] { zLine2, zLine1 } : new[] { zLine2, zLine1 }, catapultsPerSide,
            pos => passageZLines.All(z => !boardManager.IsMountain(new Vector3Int(pos.x, 0, z))),
            blockLineOfSight: true);

        // 2. Требушеты: z=1 или z=0 (z=8 или z=9), на втором зарезервированном проходе
        PlacePieceWithGuarantee(PieceType.Trebuchet, isPlayer1,
            x => reservedPassages.Count > 1 && x == reservedPassages[1],
            isPlayer1 ? new[] { zLine2, zLine1 } : new[] { zLine2, zLine1 }, trebuchetsPerSide,
            pos => passageZLines.All(z => !boardManager.IsMountain(new Vector3Int(pos.x, 0, z))),
            blockLineOfSight: true);

        // 3. Король: первая линия (z=0 или z=9), x=3–6
        PlacePieceInZoneWithFallback(PieceType.King, isPlayer1, x => x >= 3 && x <= 6, new[] { zLine1 }, kingsPerSide);

        // 4. Тяжёлая кавалерия: z=2,3 или z=6,7, за горами
        PlacePieceInZoneWithFallback(PieceType.HeavyCavalry, isPlayer1, x => true, new[] { zLine4, zLine3 },
            heavyCavalryPerSide, pos => boardManager.IsMountain(new Vector3Int(pos.x, 0, isPlayer1 ? 3 : 6)));

        // 5. Дракон: z=1,2 или z=7,8, ближе к центру (x=3–6)
        PlacePieceInZoneWithFallback(PieceType.Dragon, isPlayer1, x => x >= 3 && x <= 6, new[] { zLine2, zLine3 }, dragonsPerSide);

        // 6. Слоны: z=2,3 или z=6,7 (без условия горы)
        PlacePieceInZoneWithFallback(PieceType.Elephant, isPlayer1, x => true, new[] { zLine4, zLine3 }, elephantsPerSide);

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
    /// Размещает фигуры с гарантией (очищает клетку, если нужно).
    /// </summary>
    private void PlacePieceWithGuarantee(PieceType type, bool isPlayer1, Func<int, bool> xCondition, int[] preferredZLines,
        int count, Func<Vector3Int, bool> extraCondition = null, bool prioritizeLineOfSight = false, bool blockLineOfSight = false)
    {
        List<Vector3Int> availablePositions = GetAvailablePositions(xCondition, preferredZLines, extraCondition, prioritizeLineOfSight);
        Debug.Log($"PiecePlacementManager: Available positions for {type}: {string.Join(", ", availablePositions)}");

        int placedCount = PlacePieces(type, isPlayer1, availablePositions, Mathf.Min(count, availablePositions.Count), blockLineOfSight);
        count -= placedCount;

        // Если не разместили, очищаем клетку
        if (count > 0)
        {
            foreach (int z in preferredZLines)
            {
                for (int x = 0; x < 10; x++)
                {
                    if (!xCondition(x)) continue;
                    Vector3Int pos = new Vector3Int(x, 0, z);
                    if (extraCondition != null && !extraCondition(pos)) continue;

                    // Проверяем, занята ли клетка
                    if (boardManager.IsBlocked(pos))
                    {
                        // Удаляем гору или фигуру
                        if (boardManager.IsMountain(pos))
                        {
                            boardManager.RemovePiece(pos);
                            Debug.Log($"PiecePlacementManager: Cleared mountain at {pos} to place {type}");
                        }
                        else if (occupiedPositions.Contains(pos))
                        {
                            var piece = boardManager.GetPieceAt(pos);
                            if (piece != null)
                            {
                                boardManager.RemovePiece(pos);
                                occupiedPositions.Remove(pos);
                                Debug.Log($"PiecePlacementManager: Cleared {piece.Type} at {pos} to place {type}");
                            }
                        }
                    }
                    // Размещаем фигуру
                    PlacePiece(type, isPlayer1, pos);
                    if (blockLineOfSight)
                    {
                        BlockLineOfSight(pos, isPlayer1);
                    }
                    occupiedPositions.Add(pos); // Добавляем в занятые
                    placedCount++;
                    count--;
                    Debug.Log($"PiecePlacementManager: Force-placed {type} for Player {(isPlayer1 ? 1 : 2)} at {pos}");
                    if (count == 0) break;
                }
                if (count == 0) break;
            }
        }

        if (count > 0)
        {
            Debug.LogError($"PiecePlacementManager: Failed to place all {type} for Player {(isPlayer1 ? 1 : 2)}, remaining {count}");
        }
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
            count -= placedCount;
        }

        // Если всё ещё не разместили, очищаем клетку
        if (count > 0)
        {
            foreach (int z in isPlayer1 ? new[] { 3, 2, 1, 0 } : new[] { 6, 7, 8, 9 })
            {
                for (int x = 0; x < 10; x++)
                {
                    if (!xCondition(x)) continue;
                    Vector3Int pos = new Vector3Int(x, 0, z);
                    if (extraCondition == null || extraCondition(pos))
                    {
                        // Проверяем, занята ли клетка
                        if (boardManager.IsBlocked(pos))
                        {
                            // Удаляем гору или фигуру
                            if (boardManager.IsMountain(pos))
                            {
                                boardManager.RemovePiece(pos);
                                Debug.Log($"PiecePlacementManager: Cleared mountain at {pos} to place {type}");
                            }
                            else if (occupiedPositions.Contains(pos))
                            {
                                var piece = boardManager.GetPieceAt(pos);
                                if (piece != null)
                                {
                                    boardManager.RemovePiece(pos);
                                    occupiedPositions.Remove(pos);
                                    Debug.Log($"PiecePlacementManager: Cleared {piece.Type} at {pos} to place {type}");
                                }
                            }
                        }
                        // Размещаем фигуру
                        PlacePiece(type, isPlayer1, pos);
                        if (blockLineOfSight)
                        {
                            BlockLineOfSight(pos, isPlayer1);
                        }
                        occupiedPositions.Add(pos); // Добавляем в занятые
                        placedCount++;
                        count--;
                        Debug.Log($"PiecePlacementManager: Force-placed {type} for Player {(isPlayer1 ? 1 : 2)} at {pos}");
                        if (count == 0) break;
                    }
                }
                if (count == 0) break;
            }
        }

        if (count > 0)
        {
            Debug.LogError($"PiecePlacementManager: Failed to place all {type} for Player {(isPlayer1 ? 1 : 2)}, remaining {count}");
        }
    }

    /// <summary>
    /// Получает доступные позиции для размещения, упрощая для катапульт и требушетов.
    /// </summary>
    private List<Vector3Int> GetAvailablePositions(Func<int, bool> xCondition, int[] zLines, Func<Vector3Int, bool> extraCondition, bool prioritizeLineOfSight)
    {
        List<Vector3Int> availablePositions = new List<Vector3Int>();
        foreach (int z in zLines)
        {
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
        Debug.Log($"PiecePlacementManager: Available positions for z={string.Join(",", zLines)}: {string.Join(", ", availablePositions)}");
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
                Debug.LogWarning($"PiecePlacementManager: No available positions for {type} for Player {(isPlayer1 ? 1 : 2)}");
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
        return blockedPositions.Any(blocked => blocked.x == pos.x);
    }

    /// <summary>
    /// Блокирует линию атаки для катапульт и требушетов.
    /// </summary>
    private void BlockLineOfSight(Vector3Int pos, bool isPlayer1)
    {
        int zStart = isPlayer1 ? 1 : 6;
        int zEnd = isPlayer1 ? 3 : 8; // Блокируем до z=3/8
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