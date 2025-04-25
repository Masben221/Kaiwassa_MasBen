using UnityEngine;
using Zenject;
using System;

/// <summary>
/// ��������� ��� ���������� �����.
/// </summary>
public interface IGameManager
{
    void StartGame(int mountainsPerSide); // ������������� ���� � ������ ���
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
    [Inject] private IPiecePlacementManager piecePlacementManager; // ��������� �����������

    private bool isPlayer1Turn = true; // ������� ��� (true = ����� 1)
    public bool IsPlayer1Turn => isPlayer1Turn; // ������ ��� �������� ����
    public event Action<bool> OnTurnChanged; // ������� ����� ����

    private void Start()
    {
        Debug.Log("GameManager: Waiting for UI to start game.");
    }

    /// <summary>
    /// �������������� ����: ������ ����� 10x10, ��������� ���� � ������.
    /// </summary>
    public void StartGame(int mountainsPerSide)
    {
        Debug.Log($"GameManager: StartGame called with {mountainsPerSide} mountains per side.");
        boardManager.InitializeBoard(10);

        Debug.Log("GameManager: Placing mountains...");
        piecePlacementManager.PlaceMountains(mountainsPerSide);

        Debug.Log("GameManager: Placing pieces...");
        piecePlacementManager.PlacePiecesForPlayer(true); // ����� 1
        piecePlacementManager.PlacePiecesForPlayer(false); // ����� 2

        Debug.Log("GameManager: Game started successfully!");
        OnTurnChanged?.Invoke(isPlayer1Turn);
    }

    /// <summary>
    /// ������������ ��� ��� ����� ������.
    /// ������� ��������� ����������� �����, ����� ���.
    /// </summary>
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