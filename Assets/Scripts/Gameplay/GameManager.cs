using UnityEngine;
using Zenject;
using System;
using System.Collections.Generic;

/// <summary>
/// ��������� ��� ���������� �����.
/// </summary>
public interface IGameManager
{
    void StartGame(); // ������������� ����
    void MakeMove(Piece piece, Vector3Int target); // ���������� ���� ��� �����
    bool IsPlayer1Turn { get; } // ��� ���
    event Action<bool> OnTurnChanged; // ������� ����� ����
}

/// <summary>
/// ��������� ������� ����: ������������� �����, ���������� �����, ��������� ����� � ����, ����� �������.
/// </summary>
public class GameManager : MonoBehaviour, IGameManager
{
    [Inject] private IBoardManager boardManager; // ��������� �����
    [Inject] private IPieceFactory pieceFactory; // ������� �����

    [SerializeField] private int mountainsPerSide = 4; // ���������� ��� �� �������
    [SerializeField] private int kingsPerSide = 1; // ������ �� �������
    [SerializeField] private int dragonsPerSide = 1; // ������� �� �������
    [SerializeField] private int heavyCavalryPerSide = 2; // ������ ��������� �� �������
    [SerializeField] private int elephantsPerSide = 0; // ����� �� �������
    [SerializeField] private int lightHorsesPerSide = 0; // ˸���� ��������� �� �������
    [SerializeField] private int spearmenPerSide = 0; // ��������� �� �������
    [SerializeField] private int crossbowmenPerSide = 0; // ����������� �� �������
    [SerializeField] private int rabblePerSide = 0; // ����� �� �������
    [SerializeField] private int catapultsPerSide = 0; // ���������� �� �������
    [SerializeField] private int trebuchetsPerSide = 0; // ��������� �� �������

    private bool isPlayer1Turn = true; // ������� ��� (true = ����� 1)
    public bool IsPlayer1Turn => isPlayer1Turn; // ������ ��� �������� ����
    public event Action<bool> OnTurnChanged; // ������� ����� ����

    private void Start()
    {
        Debug.Log("GameManager: Start called.");
        StartGame();
    }

    /// <summary>
    /// �������������� ����: ������ ����� 10x10, ��������� ���� � ������.
    /// </summary>
    public void StartGame()
    {
        Debug.Log("GameManager: StartGame called.");
        boardManager.InitializeBoard(10);

        Debug.Log("GameManager: Placing mountains...");
        boardManager.PlaceMountains(mountainsPerSide);

        Debug.Log("GameManager: Placing pieces...");
        PlacePiecesForPlayer(true); // ����� 1
        PlacePiecesForPlayer(false); // ����� 2

        Debug.Log("GameManager: Game started successfully!");
        OnTurnChanged?.Invoke(isPlayer1Turn);
    }

    /// <summary>
    /// ��������� ������ ��� ���������� ������ �� ��� �������� ����� (z=0�4 ��� ������ 1, z=5�9 ��� ������ 2).
    /// </summary>
    /// <param name="isPlayer1">true, ���� ������ ��� ������ 1.</param>
    private void PlacePiecesForPlayer(bool isPlayer1)
    {
        List<Vector3Int> availablePositions = new List<Vector3Int>();
        int zStart = isPlayer1 ? 0 : 5;
        int zEnd = isPlayer1 ? 5 : 10;

        // �������� ��������� �������
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

        // ������ ����� � �� ����������
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

        // ��������� ������ ��������� �������
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
    /// ������ � ��������� ������ �� ����� ����� �������.
    /// </summary>
    /// <param name="type">��� ������.</param>
    /// <param name="isPlayer1">true, ���� ��� ������ 1.</param>
    /// <param name="position">������� �� �����.</param>
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
    /// ������������ ��� ��� ����� ������.
    /// ������� ��������� ����������� �����, ����� ���.
    /// </summary>
    /// <param name="piece">������, ������� ���������.</param>
    /// <param name="target">������� ������.</param>
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
    /// ����������� ��� ����� �������� � ���������� �����������.
    /// </summary>
    private void SwitchTurn()
    {
        isPlayer1Turn = !isPlayer1Turn;
        OnTurnChanged?.Invoke(isPlayer1Turn);
        Debug.Log($"GameManager: Turn switched to Player {(isPlayer1Turn ? 1 : 2)}");
    }
}