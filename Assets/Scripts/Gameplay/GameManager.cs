using UnityEngine;
using Zenject;
using System;

/// <summary>
/// ��������� ������� ����: ����, ����� �������, ��������� �����������.
/// </summary>
public class GameManager : MonoBehaviour, IGameManager
{
    // �����������, ������������� ����� Zenject
    [Inject] private IBoardManager boardManager;
    [Inject] private PieceFactory pieceFactory;

    // ������� ��� (true - ����� 1, false - ����� 2)
    public bool IsPlayer1Turn { get; private set; } = true;

    // ������� ��� ����������� � ����� ����
    public event Action<bool> OnTurnChanged;

    /// <summary>
    /// ����� �������������, ���������� Zenject ����� �������� ������������.
    /// </summary>
    [Inject]
    private void Initialize()
    {
        StartGame();
    }

    /// <summary>
    /// �������������� ����: ������ ����� � ����������� ��������� ������.
    /// </summary>
    public void StartGame()
    {
        // �������������� ����� �������� 10x10
        boardManager.InitializeBoard(10);

        // ����������� ��������� ������
        SetupInitialPieces();

        Debug.Log("Game started.");
    }

    /// <summary>
    /// ������������ ��� ��� ����� ������.
    /// </summary>
    public void MakeMove(IPiece piece, Vector3Int target)
    {
        // ���������, ������������� �� ��� �������� ������
        if (piece.IsPlayer1 != IsPlayer1Turn) return;

        // �������� ��������� ���� � �����
        var validMoves = piece.GetValidMoves(boardManager);
        var attackMoves = piece.GetAttackMoves(boardManager);

        if (validMoves.Contains(target))
        {
            // ���������� ������ �� ����� �������
            boardManager.RemovePiece(piece.Position);
            boardManager.PlacePiece(piece, target);
            Debug.Log($"Moved piece to {target}");
        }
        else if (attackMoves.Contains(target))
        {
            // ���������� ������ ���������� �� ������� �������
            boardManager.RemovePiece(target);
            Debug.Log($"Attacked and removed piece at {target}");
        }
        else
        {
            return; // ������������ ���
        }

        // ������ ���
        IsPlayer1Turn = !IsPlayer1Turn;
        OnTurnChanged?.Invoke(IsPlayer1Turn);
    }

    /// <summary>
    /// ����������� ��������� ������ �� �����.
    /// </summary>
    private void SetupInitialPieces()
    {
        // ������ ������ ��� ������ 1
        var king1 = pieceFactory.CreatePiece(PieceType.King, true, new Vector3Int(5, 0, 0));
        boardManager.PlacePiece(king1, king1.Position);

        // ������ ������ ��� ������ 2
        var king2 = pieceFactory.CreatePiece(PieceType.King, false, new Vector3Int(5, 0, 9));
        boardManager.PlacePiece(king2, king2.Position);

        // ������ ������� ��� ������ 1
        var dragon1 = pieceFactory.CreatePiece(PieceType.Dragon, true, new Vector3Int(4, 0, 0));
        boardManager.PlacePiece(dragon1, dragon1.Position);
    }
}