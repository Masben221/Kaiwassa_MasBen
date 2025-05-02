using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

/// <summary>
/// Управляет логикой автоматической расстановки гор и фигур на доске.
/// Следует принципу единственной ответственности (SOLID).
/// </summary>
public class PiecePlacementManager : MonoBehaviour, IPiecePlacementManager
{
    // Зависимости, инъектируемые через Zenject
    [Inject] private IBoardManager boardManager; // Менеджер доски
    [Inject] private IPieceFactory pieceFactory; // Фабрика для создания фигур

    // Количество фигур каждого типа для одного игрока (настраивается в инспекторе)
    [SerializeField] private int kingsPerSide = 1;         // Количество королей
    [SerializeField] private int dragonsPerSide = 1;       // Количество драконов
    [SerializeField] private int heavyCavalryPerSide = 2;  // Количество тяжёлой кавалерии
    [SerializeField] private int elephantsPerSide = 2;     // Количество слонов
    [SerializeField] private int lightHorsesPerSide = 2;   // Количество лёгкой кавалерии
    [SerializeField] private int spearmenPerSide = 2;      // Количество копейщиков
    [SerializeField] private int crossbowmenPerSide = 2;   // Количество арбалетчиков
    [SerializeField] private int rabblePerSide = 2;        // Количество ополчения
    [SerializeField] private int catapultsPerSide = 1;     // Количество катапульт
    [SerializeField] private int trebuchetsPerSide = 1;    // Количество требушетов
    [SerializeField] private int swordsmenPerSide = 2;     // Количество мечников
    [SerializeField] private int archersPerSide = 2;       // Количество лучников

    // Списки для отслеживания занятых и заблокированных позиций
    private readonly List<Vector3Int> occupiedPositions = new List<Vector3Int>(); // Позиции, занятые фигурами
    private readonly List<Vector3Int> blockedPositions = new List<Vector3Int>();  // Позиции, заблокированные для размещения (например, линия обстрела)

    private int mountainsPerSide; // Количество гор на сторону

    // Свойство для получения количества гор на сторону
    public int GetMountainsPerSide => mountainsPerSide;

    /// <summary>
    /// Инициализирует менеджер расстановки, сбрасывая списки занятых и заблокированных позиций.
    /// </summary>
    /// <param name="mountainsPerSide">Количество гор на сторону.</param>
    public void Initialize(int mountainsPerSide)
    {
        this.mountainsPerSide = mountainsPerSide; // Сохраняем количество гор
        occupiedPositions.Clear(); // Очищаем занятые позиции
        blockedPositions.Clear(); // Очищаем заблокированные позиции
        Debug.Log($"PiecePlacementManager: Initialized with {mountainsPerSide} mountains per side.");
    }

    /// <summary>
    /// Проверяет, можно ли разместить фигуру на указанной позиции (не поддерживается в автоматической расстановке).
    /// </summary>
    public bool CanPlace(bool isPlayer1, Vector3Int position, PieceType type, bool isMove = false)
    {
        Debug.LogWarning("PiecePlacementManager: CanPlace not supported in automatic placement.");
        return false;
    }

    /// <summary>
    /// Размещает фигуру или гору на доске (не поддерживается в автоматической расстановке).
    /// </summary>
    public bool PlacePieceOrMountain(bool isPlayer1, Vector3Int position, PieceType type, bool isMove = false)
    {
        Debug.LogWarning("PiecePlacementManager: PlacePieceOrMountain not supported in automatic placement.");
        return false;
    }

    /// <summary>
    /// Удаляет фигуру с доски (не поддерживается в автоматической расстановке).
    /// </summary>
    public bool RemovePiece(bool isPlayer1, Vector3Int position, PieceType type)
    {
        Debug.LogWarning("PiecePlacementManager: RemovePiece not supported in automatic placement.");
        return false;
    }

    /// <summary>
    /// Удаляет указанную фигуру с доски (не поддерживается в автоматической расстановке).
    /// </summary>
    public bool RemovePiece(Piece piece)
    {
        Debug.LogWarning("PiecePlacementManager: RemovePiece(Piece) not supported in automatic placement.");
        return false;
    }

    /// <summary>
    /// Перемещает фигуру с одной позиции на другую (не поддерживается в автоматической расстановке).
    /// </summary>
    public bool MovePiece(Piece piece, Vector3Int from, Vector3Int to)
    {
        Debug.LogWarning("PiecePlacementManager: MovePiece not supported in automatic placement.");
        return false;
    }

    /// <summary>
    /// Возвращает количество оставшихся фигур указанного типа (не поддерживается в автоматической расстановке).
    /// </summary>
    public int GetRemainingCount(bool isPlayer1, PieceType type)
    {
        Debug.LogWarning("PiecePlacementManager: GetRemainingCount not supported in automatic placement.");
        return 0;
    }

    /// <summary>
    /// Проверяет, завершена ли расстановка для игрока (в автоматической расстановке считается завершённой после PlacePiecesForPlayer).
    /// </summary>
    public bool HasCompletedPlacement(bool isPlayer1)
    {
        Debug.LogWarning("PiecePlacementManager: HasCompletedPlacement not supported in automatic placement.");
        return true; // Автоматическая расстановка считается завершённой
    }

    /// <summary>
    /// Проверяет, размещён ли король (не поддерживается в автоматической расстановке).
    /// </summary>
    public bool IsKingNotPlaced(bool isPlayer1)
    {
        Debug.LogWarning("PiecePlacementManager: IsKingNotPlaced not supported in automatic placement.");
        return false; // Предполагается, что король размещён автоматически
    }

    /// <summary>
    /// Получает список позиций для размещения гор на указанных линиях Z, исключая зарезервированные проходы.
    /// </summary>
    private List<Vector3Int> GetMountainPositions(int[] zLines, List<int> reservedPassages)
    {
        List<Vector3Int> positions = new List<Vector3Int>();
        foreach (int z in zLines)
        {
            for (int x = 0; x < 10; x++)
            {
                if (reservedPassages.Contains(x)) continue; // Пропускаем зарезервированные проходы
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
    /// Размещает горы для указанного игрока на заданных позициях.
    /// </summary>
    private void PlaceMountainsForPlayer(List<Vector3Int> positions, int mountainsPerSide, bool isPlayer1, List<int> reservedPassages)
    {
        Debug.Log($"PiecePlacementManager: Starting to place {mountainsPerSide} mountains for Player {(isPlayer1 ? 1 : 2)}");
        int mountainsToPlace = Mathf.Min(mountainsPerSide, positions.Count);
        for (int i = 0; i < mountainsToPlace; i++)
        {
            if (positions.Count == 0) break;
            int index = UnityEngine.Random.Range(0, positions.Count);
            Vector3Int pos = positions[index];
            if (reservedPassages.Contains(pos.x))
                continue;
            Piece mountain = pieceFactory.CreatePiece(PieceType.Mountain, isPlayer1, pos);
            if (mountain != null)
            {
                boardManager.PlacePiece(mountain, pos);
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
    /// Генерирует зарезервированные проходы (X координаты) для катапульты и требушета.
    /// </summary>
    private List<int> GetReservedPassages(bool isPlayer1)
    {
        List<int> availableX = Enumerable.Range(0, 10).ToList();
        List<int> reservedPassages = new List<int>();
        for (int i = 0; i < 2 && availableX.Count > 0; i++)
        {
            int xPassage = availableX[UnityEngine.Random.Range(0, availableX.Count)];
            reservedPassages.Add(xPassage);
            availableX.Remove(xPassage);
            Debug.Log($"PiecePlacementManager: Reserved passage {i + 1} for Player {(isPlayer1 ? 1 : 2)} at x={xPassage} for {(i == 0 ? "catapult" : "trebuchet")}");
        }
        return reservedPassages;
    }

    /// <summary>
    /// Размещает все фигуры и горы для указанного игрока.
    /// </summary>
    public void PlacePiecesForPlayer(bool isPlayer1, int selectedMountains)
    {
        if (boardManager == null)
        {
            Debug.LogError("PiecePlacementManager: boardManager is null in PlacePiecesForPlayer!");
            return;
        }

        mountainsPerSide = Mathf.Min(selectedMountains, 8); // Ограничиваем количество гор
        occupiedPositions.Clear(); // Очищаем занятые позиции

        // Генерируем зарезервированные проходы для катапульты и требушета
        List<int> reservedPassages = GetReservedPassages(isPlayer1);

        // Определяем линии Z для гор
        int[] zLines;
        if (mountainsPerSide <= 4)
        {
            zLines = isPlayer1 ? new[] { 3 } : new[] { 6 };
        }
        else
        {
            zLines = isPlayer1 ? new[] { 2, 3 } : new[] { 6, 7 };
        }

        // Размещаем горы
        List<Vector3Int> mountainPositions = GetMountainPositions(zLines, reservedPassages);
        PlaceMountainsForPlayer(mountainPositions, mountainsPerSide, isPlayer1, reservedPassages);

        blockedPositions.Clear(); // Очищаем заблокированные позиции

        // Определяем линии Z для фигур
        int zLine1 = isPlayer1 ? 0 : 9; // Самая дальняя линия
        int zLine2 = isPlayer1 ? 1 : 8;
        int zLine3 = isPlayer1 ? 2 : 7;
        int zLine4 = isPlayer1 ? 3 : 6;
        int[] passageZLines = isPlayer1 ? new[] { 1, 2, 3 } : new[] { 6, 7, 8 }; // Линии для проверки линии обстрела

        // Размещаем катапульту с учётом зарезервированного прохода
        PlacePieceWithGuarantee(PieceType.Catapult, isPlayer1,
            x => reservedPassages.Count > 0 && x == reservedPassages[0],
            isPlayer1 ? new[] { zLine2, zLine1 } : new[] { zLine2, zLine1 }, catapultsPerSide,
            pos => passageZLines.All(z => !boardManager.IsMountain(new Vector3Int(pos.x, 0, z))),
            blockLineOfSight: true);

        // Размещаем требушет с учётом зарезервированного прохода
        PlacePieceWithGuarantee(PieceType.Trebuchet, isPlayer1,
            x => reservedPassages.Count > 1 && x == reservedPassages[1],
            isPlayer1 ? new[] { zLine2, zLine1 } : new[] { zLine2, zLine1 }, trebuchetsPerSide,
            pos => passageZLines.All(z => !boardManager.IsMountain(new Vector3Int(pos.x, 0, z))),
            blockLineOfSight: true);

        // Размещаем короля на центральных позициях дальней линии
        Vector3Int? kingPosition = PlacePieceInZoneWithFallback(PieceType.King, isPlayer1, x => x >= 3 && x <= 6, new[] { zLine1 }, kingsPerSide);
        if (!kingPosition.HasValue)
        {
            Debug.LogError($"PiecePlacementManager: Failed to place King for Player {(isPlayer1 ? 1 : 2)}!");
            return;
        }

        // Размещаем тяжёлую кавалерию с учётом наличия или отсутствия гор
        Func<Vector3Int, bool> heavyCavalryCondition = mountainsPerSide > 0
            ? (Func<Vector3Int, bool>)(pos => boardManager.IsMountain(new Vector3Int(pos.x, 0, isPlayer1 ? 3 : 6)))
            : null;
        PlacePieceInZoneWithFallback(PieceType.HeavyCavalry, isPlayer1, x => true, new[] { zLine4, zLine3 },
            heavyCavalryPerSide, heavyCavalryCondition);

        // Размещаем дракона на средних линиях
        PlacePieceInZoneWithFallback(PieceType.Dragon, isPlayer1, x => x >= 3 && x <= 6, new[] { zLine2, zLine3 }, dragonsPerSide);

        // Размещаем слонов на передних линиях
        PlacePieceInZoneWithFallback(PieceType.Elephant, isPlayer1, x => true, new[] { zLine4, zLine3 }, elephantsPerSide);

        // Размещаем лёгкую кавалерию, арбалетчиков и копейщиков в случайном порядке
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

        // Размещаем ополчение на передних линиях
        PlacePieceInZoneWithFallback(PieceType.Rabble, isPlayer1, x => true, new[] { zLine4, zLine3 }, rabblePerSide);

        // Размещаем мечников рядом с королём
        PlaceSwordsmenNearKing(isPlayer1, kingPosition.Value, swordsmenPerSide);

        // Размещаем лучников на всех доступных линиях, избегая линий атаки
        PlacePieceInZoneWithFallback(PieceType.Archer, isPlayer1, x => !reservedPassages.Contains(x),
            isPlayer1 ? new[] { zLine1, zLine2, zLine3, zLine4 } : new[] { zLine4, zLine3, zLine2, zLine1 }, archersPerSide,
            prioritizeLineOfSight: true);
    }

    /// <summary>
    /// Размещает мечников рядом с королём, предпочтительно справа и слева, избегая заблокированных позиций.
    /// </summary>
    private void PlaceSwordsmenNearKing(bool isPlayer1, Vector3Int kingPosition, int count)
    {
        List<Vector3Int> preferredPositions = new List<Vector3Int>();
        int kingX = kingPosition.x;
        int kingZ = kingPosition.z;

        // Предпочтительные позиции: справа и слева от короля (x ± 1, тот же z)
        preferredPositions.Add(new Vector3Int(kingX - 1, 0, kingZ));
        preferredPositions.Add(new Vector3Int(kingX + 1, 0, kingZ));

        // Дополнительные позиции: соседние клетки на той же линии или следующей
        for (int dx = -2; dx <= 2; dx++)
        {
            if (dx == 0 || dx == -1 || dx == 1) continue; // Пропускаем короля и уже добавленные позиции
            preferredPositions.Add(new Vector3Int(kingX + dx, 0, kingZ));
        }
        for (int dx = -2; dx <= 2; dx++)
        {
            int nextZ = isPlayer1 ? kingZ + 1 : kingZ - 1; // z=1 для игрока 1, z=8 для игрока 2
            preferredPositions.Add(new Vector3Int(kingX + dx, 0, nextZ));
        }

        // Фильтруем позиции: в пределах доски, не заняты, не заблокированы
        List<Vector3Int> availablePositions = preferredPositions
            .Where(pos => boardManager.IsWithinBounds(pos) &&
                          !boardManager.IsBlocked(pos) &&
                          !occupiedPositions.Contains(pos) &&
                          !blockedPositions.Contains(pos))
            .ToList();

        Debug.Log($"PiecePlacementManager: Available positions for Swordsmen near King at {kingPosition}: {string.Join(", ", availablePositions)}");

        // Размещаем мечников
        int placedCount = PlacePieces(PieceType.Swordsman, isPlayer1, availablePositions, Mathf.Min(count, availablePositions.Count), blockLineOfSight: false);
        count -= placedCount;

        // Если не удалось разместить всех мечников, используем запасные зоны
        if (count > 0)
        {
            int[] fallbackZLines = isPlayer1 ? new[] { 1, 0 } : new[] { 8, 9 };
            availablePositions = GetAvailablePositions(x => x >= 2 && x <= 7, fallbackZLines, null, prioritizeLineOfSight: true);
            placedCount = PlacePieces(PieceType.Swordsman, isPlayer1, availablePositions, Mathf.Min(count, availablePositions.Count), blockLineOfSight: false);
            count -= placedCount;
        }

        // Если всё ещё остались мечники, размещаем принудительно
        if (count > 0)
        {
            foreach (int z in isPlayer1 ? new[] { 0, 1 } : new[] { 9, 8 })
            {
                for (int x = 0; x < 10; x++)
                {
                    Vector3Int pos = new Vector3Int(x, 0, z);
                    if (!boardManager.IsWithinBounds(pos) || blockedPositions.Contains(pos)) continue;

                    if (boardManager.IsBlocked(pos))
                    {
                        if (boardManager.IsMountain(pos))
                        {
                            boardManager.RemovePiece(pos);
                            Debug.Log($"PiecePlacementManager: Cleared mountain at {pos} to place Swordsman");
                        }
                        else if (occupiedPositions.Contains(pos))
                        {
                            var piece = boardManager.GetPieceAt(pos);
                            if (piece != null)
                            {
                                boardManager.RemovePiece(pos);
                                occupiedPositions.Remove(pos);
                                Debug.Log($"PiecePlacementManager: Cleared {piece.Type} at {pos} to place Swordsman");
                            }
                        }
                    }
                    PlacePiece(PieceType.Swordsman, isPlayer1, pos);
                    occupiedPositions.Add(pos);
                    placedCount++;
                    count--;
                    Debug.Log($"PiecePlacementManager: Force-placed Swordsman for Player {(isPlayer1 ? 1 : 2)} at {pos}");
                    if (count == 0) break;
                }
                if (count == 0) break;
            }
        }

        if (count > 0)
        {
            Debug.LogError($"PiecePlacementManager: Failed to place all Swordsmen for Player {(isPlayer1 ? 1 : 2)}, remaining {count}");
        }
    }

    /// <summary>
    /// Размещает фигуры с гарантированным размещением, если обычное размещение не удалось.
    /// Используется для фигур, которые должны быть размещены строго на определённых позициях (например, катапульта, требушет).
    /// </summary>
    private void PlacePieceWithGuarantee(PieceType type, bool isPlayer1, Func<int, bool> xCondition, int[] preferredZLines,
        int count, Func<Vector3Int, bool> extraCondition = null, bool prioritizeLineOfSight = false, bool blockLineOfSight = false)
    {
        List<Vector3Int> availablePositions = GetAvailablePositions(xCondition, preferredZLines, extraCondition, prioritizeLineOfSight);
        Debug.Log($"PiecePlacementManager: Available positions for {type}: {string.Join(", ", availablePositions)}");

        int placedCount = PlacePieces(type, isPlayer1, availablePositions, Mathf.Min(count, availablePositions.Count), blockLineOfSight);
        count -= placedCount;

        if (count > 0)
        {
            foreach (int z in preferredZLines)
            {
                for (int x = 0; x < 10; x++)
                {
                    if (!xCondition(x)) continue;
                    Vector3Int pos = new Vector3Int(x, 0, z);
                    if (extraCondition != null && !extraCondition(pos)) continue;

                    if (boardManager.IsBlocked(pos))
                    {
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
                    PlacePiece(type, isPlayer1, pos);
                    if (blockLineOfSight)
                    {
                        BlockLineOfSight(pos, isPlayer1);
                    }
                    occupiedPositions.Add(pos);
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
    /// Размещает фигуры на указанных линиях Z с возможностью перехода на запасные линии.
    /// Возвращает позицию размещённой фигуры, если count == 1.
    /// </summary>
    private Vector3Int? PlacePieceInZoneWithFallback(PieceType type, bool isPlayer1, Func<int, bool> xCondition, int[] preferredZLines,
        int count, Func<Vector3Int, bool> extraCondition = null, bool prioritizeLineOfSight = false, bool blockLineOfSight = false)
    {
        Vector3Int? lastPlacedPosition = null;
        List<Vector3Int> availablePositions = GetAvailablePositions(xCondition, preferredZLines, extraCondition, prioritizeLineOfSight);
        int placedCount = PlacePieces(type, isPlayer1, availablePositions, Mathf.Min(count, availablePositions.Count), blockLineOfSight, pos => lastPlacedPosition = pos);
        count -= placedCount;

        if (count > 0)
        {
            int[] fallbackZLines = isPlayer1 ? new[] { 1, 0 } : new[] { 8, 9 };
            availablePositions = GetAvailablePositions(xCondition, fallbackZLines, extraCondition, prioritizeLineOfSight);
            placedCount = PlacePieces(type, isPlayer1, availablePositions, Mathf.Min(count, availablePositions.Count), blockLineOfSight, pos => lastPlacedPosition = pos);
            count -= placedCount;
        }

        if (count > 0)
        {
            foreach (int z in isPlayer1 ? new[] { 3, 2, 1, 0 } : new[] { 6, 7, 8, 9 })
            {
                for (int x = 0; x < 10; x++)
                {
                    if (!xCondition(x)) continue;
                    Vector3Int pos = new Vector3Int(x, 0, z);
                    if (extraCondition != null && !extraCondition(pos)) continue;

                    if (boardManager.IsBlocked(pos))
                    {
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
                    PlacePiece(type, isPlayer1, pos);
                    if (blockLineOfSight)
                    {
                        BlockLineOfSight(pos, isPlayer1);
                    }
                    occupiedPositions.Add(pos);
                    lastPlacedPosition = pos;
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

        return lastPlacedPosition;
    }

    /// <summary>
    /// Получает список доступных позиций для размещения фигур с учётом условий.
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
    /// Размещает указанное количество фигур на доступных позициях.
    /// </summary>
    private int PlacePieces(PieceType type, bool isPlayer1, List<Vector3Int> availablePositions, int count, bool blockLineOfSight, Action<Vector3Int> onPlace = null)
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
            onPlace?.Invoke(pos);
            availablePositions.RemoveAt(index);
            placedCount++;
        }
        return placedCount;
    }

    /// <summary>
    /// Проверяет, блокирует ли указанная позиция линию обстрела.
    /// </summary>
    private bool BlocksLineOfSight(Vector3Int pos)
    {
        return blockedPositions.Any(blocked => blocked.x == pos.x);
    }

    /// <summary>
    /// Блокирует линию обстрела для указанной позиции.
    /// </summary>
    private void BlockLineOfSight(Vector3Int pos, bool isPlayer1)
    {
        int zStart = isPlayer1 ? 1 : 6;
        int zEnd = isPlayer1 ? 3 : 8;
        for (int z = zStart; z <= zEnd; z++)
        {
            Vector3Int blockPos = new Vector3Int(pos.x, 0, z);
            if (!blockedPositions.Contains(blockPos))
            {
                blockedPositions.Add(blockPos);
            }
        }
        Debug.Log($"PiecePlacementManager: Blocked line of sight for x={pos.x}, z={zStart} to {zEnd} for Player {(isPlayer1 ? 1 : 2)}");
    }

    /// <summary>
    /// Размещает одну фигуру на указанной позиции.
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
}