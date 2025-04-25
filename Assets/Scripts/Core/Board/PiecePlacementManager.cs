using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

/// <summary>
/// ��������� ��� ���������� ������������ ��� � �����.
/// </summary>
public interface IPiecePlacementManager
{
    void PlaceMountains(int mountainsPerSide);
    void PlacePiecesForPlayer(bool isPlayer1);
}

/// <summary>
/// ��������� ������� ����������� ��� � ����� �� �����.
/// ������� �������� ������������ ��������������� (SOLID).
/// </summary>
public class PiecePlacementManager : MonoBehaviour, IPiecePlacementManager
{
    [Inject] private IBoardManager boardManager;
    [Inject] private IPieceFactory pieceFactory;

    [SerializeField] private int kingsPerSide = 1; // ������ �� �������
    [SerializeField] private int dragonsPerSide = 1; // ������� �� �������
    [SerializeField] private int heavyCavalryPerSide = 2; // ������ ��������� �� �������
    [SerializeField] private int elephantsPerSide = 2; // ����� �� �������
    [SerializeField] private int lightHorsesPerSide = 3; // ˸���� ��������� �� �������
    [SerializeField] private int spearmenPerSide = 3; // ��������� �� �������
    [SerializeField] private int crossbowmenPerSide = 3; // ����������� �� �������
    [SerializeField] private int rabblePerSide = 3; // ��������� �� �������
    [SerializeField] private int catapultsPerSide = 1; // ���������� �� �������
    [SerializeField] private int trebuchetsPerSide = 1; // ��������� �� �������

    private readonly List<Vector3Int> occupiedPositions = new List<Vector3Int>(); // ������� ������
    private readonly List<Vector3Int> blockedPositions = new List<Vector3Int>(); // ��������������� ����� �����
    private List<int> reservedPassagesPlayer1 = new List<int>(); // ����������������� ������� ��� ������ 1
    private List<int> reservedPassagesPlayer2 = new List<int>(); // ����������������� ������� ��� ������ 2

    /// <summary>
    /// ��������� ����, �������� 8 �� �������, �������� ����������������� �������.
    /// </summary>
    public void PlaceMountains(int mountainsPerSide)
    {
        if (boardManager == null)
        {
            Debug.LogError("PiecePlacementManager: boardManager is null in PlaceMountains!");
            return;
        }

        // ������������ ������������ ���������� ��� �� 8
        mountainsPerSide = Mathf.Min(mountainsPerSide, 8);

        // ������� ������� ������� � ����������������� �������
        occupiedPositions.Clear();
        reservedPassagesPlayer1.Clear();
        reservedPassagesPlayer2.Clear();

        // ����������� ��� ������� ��� ������ 1
        List<int> availableXPlayer1 = Enumerable.Range(0, 10).ToList();
        for (int i = 0; i < 2 && availableXPlayer1.Count > 0; i++)
        {
            int xPassage = availableXPlayer1[UnityEngine.Random.Range(0, availableXPlayer1.Count)];
            reservedPassagesPlayer1.Add(xPassage);
            availableXPlayer1.Remove(xPassage);
            Debug.Log($"PiecePlacementManager: Reserved passage {i + 1} for Player 1 at x={xPassage} for {(i == 0 ? "catapult" : "trebuchet")}");
        }

        // ����������� ��� ������� ��� ������ 2
        List<int> availableXPlayer2 = Enumerable.Range(0, 10).ToList();
        for (int i = 0; i < 2 && availableXPlayer2.Count > 0; i++)
        {
            int xPassage = availableXPlayer2[UnityEngine.Random.Range(0, availableXPlayer2.Count)];
            reservedPassagesPlayer2.Add(xPassage);
            availableXPlayer2.Remove(xPassage);
            Debug.Log($"PiecePlacementManager: Reserved passage {i + 1} for Player 2 at x={xPassage} for {(i == 0 ? "catapult" : "trebuchet")}");
        }

        // ���������� ����� ��� ��� (z=0 � ������ �����)
        int[] zLinesPlayer1, zLinesPlayer2;
        if (mountainsPerSide <= 4)
        {
            zLinesPlayer1 = new[] { 3 }; // 4-� ����� (z=3)
            zLinesPlayer2 = new[] { 6 }; // 7-� ����� (z=6)
        }
        else
        {
            zLinesPlayer1 = new[] { 2, 3 }; // 3�4-� ����� (z=2,3)
            zLinesPlayer2 = new[] { 6, 7 }; // 7�8-� ����� (z=6,7)
        }

        // ��������� ������� ��� ���, �������� ����������������� ������� ����� �������
        List<Vector3Int> player1Positions = GetMountainPositions(zLinesPlayer1);
        List<Vector3Int> player2Positions = GetMountainPositions(zLinesPlayer2);

        // ������������ ������� 2 ������� �� ������ ���
        EnsurePassages(player1Positions, mountainsPerSide, zLinesPlayer1, true);
        EnsurePassages(player2Positions, mountainsPerSide, zLinesPlayer2, false);

        // ��������� ����
        PlaceMountainsForPlayer(player1Positions, mountainsPerSide, true);
        PlaceMountainsForPlayer(player2Positions, mountainsPerSide, false);

        // ��������� � ������� ���� �� ����������������� ��������
        foreach (int x in reservedPassagesPlayer1.Concat(reservedPassagesPlayer2).Distinct())
        {
            foreach (int z in new[] { 1, 2, 3, 6, 7, 8 })
            {
                Vector3Int pos = new Vector3Int(x, 0, z);
                if (boardManager.IsMountain(pos))
                {
                    Debug.LogError($"PiecePlacementManager: Mountain found at {pos} in reserved passage! Removing...");
                    boardManager.RemovePiece(pos); // ������� ����
                }
            }
        }
    }

    /// <summary>
    /// �������� ������� ��� ��� � �������� ������ z, �������� ����������������� ������� ����� �������.
    /// </summary>
    private List<Vector3Int> GetMountainPositions(int[] zLines)
    {
        List<Vector3Int> positions = new List<Vector3Int>();
        foreach (int z in zLines)
        {
            for (int x = 0; x < 10; x++)
            {
                if (reservedPassagesPlayer1.Contains(x) || reservedPassagesPlayer2.Contains(x)) continue; // ��������� �������
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
    /// ������������ ������� 2 ������� �� ������ ����� z.
    /// </summary>
    private void EnsurePassages(List<Vector3Int> positions, int mountainsPerSide, int[] zLines, bool isPlayer1)
    {
        int[] passageZLines = isPlayer1 ? new[] { 1, 2, 3 } : new[] { 6, 7, 8 }; // ����� ��� ��������
        List<int> reservedPassages = isPlayer1 ? reservedPassagesPlayer1 : reservedPassagesPlayer2;

        // ������� ���� �� ����������������� ��������
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

        // ������������ ������� 2 ������� �� ������ ���
        foreach (int z in zLines)
        {
            var zPositions = positions.Where(p => p.z == z).ToList();
            if (zPositions.Count <= 2) continue; // ��� ���� �������

            int maxMountains = zPositions.Count - 2; // ������� 2 �������
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
    /// ��������� ���� ��� ������, �������� ����������������� �������.
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
    /// ��������� ������ ��� ������ �� �������� ������ � ������ ��� � ����� �����.
    /// </summary>
    public void PlacePiecesForPlayer(bool isPlayer1)
    {
        if (boardManager == null)
        {
            Debug.LogError("PiecePlacementManager: boardManager is null in PlacePiecesForPlayer!");
            return;
        }

        // ������� ������� � ��������������� �������
        occupiedPositions.Clear();
        blockedPositions.Clear();

        // ����� (z=0 � ������ �����)
        int zLine1 = isPlayer1 ? 0 : 9; // 1-� ����� (z=0), 10-� ����� (z=9)
        int zLine2 = isPlayer1 ? 1 : 8; // 2-� ����� (z=1), 9-� ����� (z=8)
        int zLine3 = isPlayer1 ? 2 : 7; // 3-� ����� (z=2), 8-� ����� (z=7)
        int zLine4 = isPlayer1 ? 3 : 6; // 4-� ����� (z=3), 7-� ����� (z=6)
        int[] passageZLines = isPlayer1 ? new[] { 1, 2, 3 } : new[] { 6, 7, 8 }; // ����� ��� ��������
        List<int> reservedPassages = isPlayer1 ? reservedPassagesPlayer1 : reservedPassagesPlayer2;

        // ������� ����������������� ������� �� ���
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

        // 1. ����������: z=1 ��� z=0 (z=8 ��� z=9), �� ������ ����������������� �������
        PlacePieceWithGuarantee(PieceType.Catapult, isPlayer1,
            x => reservedPassages.Count > 0 && x == reservedPassages[0],
            isPlayer1 ? new[] { zLine2, zLine1 } : new[] { zLine2, zLine1 }, catapultsPerSide,
            pos => passageZLines.All(z => !boardManager.IsMountain(new Vector3Int(pos.x, 0, z))),
            blockLineOfSight: true);

        // 2. ���������: z=1 ��� z=0 (z=8 ��� z=9), �� ������ ����������������� �������
        PlacePieceWithGuarantee(PieceType.Trebuchet, isPlayer1,
            x => reservedPassages.Count > 1 && x == reservedPassages[1],
            isPlayer1 ? new[] { zLine2, zLine1 } : new[] { zLine2, zLine1 }, trebuchetsPerSide,
            pos => passageZLines.All(z => !boardManager.IsMountain(new Vector3Int(pos.x, 0, z))),
            blockLineOfSight: true);

        // 3. ������: ������ ����� (z=0 ��� z=9), x=3�6
        PlacePieceInZoneWithFallback(PieceType.King, isPlayer1, x => x >= 3 && x <= 6, new[] { zLine1 }, kingsPerSide);

        // 4. ������ ���������: z=2,3 ��� z=6,7, �� ������
        PlacePieceInZoneWithFallback(PieceType.HeavyCavalry, isPlayer1, x => true, new[] { zLine4, zLine3 },
            heavyCavalryPerSide, pos => boardManager.IsMountain(new Vector3Int(pos.x, 0, isPlayer1 ? 3 : 6)));

        // 5. ������: z=1,2 ��� z=7,8, ����� � ������ (x=3�6)
        PlacePieceInZoneWithFallback(PieceType.Dragon, isPlayer1, x => x >= 3 && x <= 6, new[] { zLine2, zLine3 }, dragonsPerSide);

        // 6. �����: z=2,3 ��� z=6,7 (��� ������� ����)
        PlacePieceInZoneWithFallback(PieceType.Elephant, isPlayer1, x => true, new[] { zLine4, zLine3 }, elephantsPerSide);

        // 7. ˸���� ���������, �����������, ���������: z=2 ��� z=7,8, ��������
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

        // 8. ���������: z=2,3 ��� z=6,7
        PlacePieceInZoneWithFallback(PieceType.Rabble, isPlayer1, x => true, new[] { zLine4, zLine3 }, rabblePerSide);
    }

    /// <summary>
    /// ��������� ������ � ��������� (������� ������, ���� �����).
    /// </summary>
    private void PlacePieceWithGuarantee(PieceType type, bool isPlayer1, Func<int, bool> xCondition, int[] preferredZLines,
        int count, Func<Vector3Int, bool> extraCondition = null, bool prioritizeLineOfSight = false, bool blockLineOfSight = false)
    {
        List<Vector3Int> availablePositions = GetAvailablePositions(xCondition, preferredZLines, extraCondition, prioritizeLineOfSight);
        Debug.Log($"PiecePlacementManager: Available positions for {type}: {string.Join(", ", availablePositions)}");

        int placedCount = PlacePieces(type, isPlayer1, availablePositions, Mathf.Min(count, availablePositions.Count), blockLineOfSight);
        count -= placedCount;

        // ���� �� ����������, ������� ������
        if (count > 0)
        {
            foreach (int z in preferredZLines)
            {
                for (int x = 0; x < 10; x++)
                {
                    if (!xCondition(x)) continue;
                    Vector3Int pos = new Vector3Int(x, 0, z);
                    if (extraCondition != null && !extraCondition(pos)) continue;

                    // ���������, ������ �� ������
                    if (boardManager.IsBlocked(pos))
                    {
                        // ������� ���� ��� ������
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
                    // ��������� ������
                    PlacePiece(type, isPlayer1, pos);
                    if (blockLineOfSight)
                    {
                        BlockLineOfSight(pos, isPlayer1);
                    }
                    occupiedPositions.Add(pos); // ��������� � �������
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
    /// ��������� ������ � �������� ������ � ������������ ������ � ���.
    /// </summary>
    private void PlacePieceInZoneWithFallback(PieceType type, bool isPlayer1, Func<int, bool> xCondition, int[] preferredZLines,
        int count, Func<Vector3Int, bool> extraCondition = null, bool prioritizeLineOfSight = false, bool blockLineOfSight = false)
    {
        // ������� ���������� � ���������������� ������
        List<Vector3Int> availablePositions = GetAvailablePositions(xCondition, preferredZLines, extraCondition, prioritizeLineOfSight);
        int placedCount = PlacePieces(type, isPlayer1, availablePositions, Mathf.Min(count, availablePositions.Count), blockLineOfSight);
        count -= placedCount;

        // ���� �������� ������, ������� �������� ����� � ���
        if (count > 0)
        {
            int[] fallbackZLines = isPlayer1 ? new[] { 1, 0 } : new[] { 8, 9 };
            availablePositions = GetAvailablePositions(xCondition, fallbackZLines, extraCondition, prioritizeLineOfSight);
            placedCount = PlacePieces(type, isPlayer1, availablePositions, Mathf.Min(count, availablePositions.Count), blockLineOfSight);
            count -= placedCount;
        }

        // ���� �� ��� �� ����������, ������� ������
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
                        // ���������, ������ �� ������
                        if (boardManager.IsBlocked(pos))
                        {
                            // ������� ���� ��� ������
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
                        // ��������� ������
                        PlacePiece(type, isPlayer1, pos);
                        if (blockLineOfSight)
                        {
                            BlockLineOfSight(pos, isPlayer1);
                        }
                        occupiedPositions.Add(pos); // ��������� � �������
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
    /// �������� ��������� ������� ��� ����������, ������� ��� ��������� � ����������.
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
    /// ��������� �������� ���������� ����� � ���������� ����� �����������.
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
    /// ���������, ��������� �� ������� ����� ����� ���������.
    /// </summary>
    private bool BlocksLineOfSight(Vector3Int pos)
    {
        return blockedPositions.Any(blocked => blocked.x == pos.x);
    }

    /// <summary>
    /// ��������� ����� ����� ��� ��������� � ����������.
    /// </summary>
    private void BlockLineOfSight(Vector3Int pos, bool isPlayer1)
    {
        int zStart = isPlayer1 ? 1 : 6;
        int zEnd = isPlayer1 ? 3 : 8; // ��������� �� z=3/8
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
    /// ������ � ��������� ������.
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
    /// ������� ������� ������ ���������� ����.
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