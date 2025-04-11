using UnityEngine;
using Zenject;
using System;

/// <summary>
/// Управляет логикой игры: ходы, смена игроков, начальная расстановка.
/// </summary>
public class GameManager : MonoBehaviour, IGameManager
{
    // Зависимости, инжектируемые через Zenject
    [Inject] private IBoardManager boardManager;
    [Inject] private PieceFactory pieceFactory;

    // Текущий ход (true - игрок 1, false - игрок 2)
    public bool IsPlayer1Turn { get; private set; } = true;

    // Событие для уведомления о смене хода
    public event Action<bool> OnTurnChanged;

    /// <summary>
    /// Метод инициализации, вызываемый Zenject после инъекции зависимостей.
    /// </summary>
    [Inject]
    private void Initialize()
    {
        StartGame();
    }

    /// <summary>
    /// Инициализирует игру: создаёт доску и расставляет начальные фигуры.
    /// </summary>
    public void StartGame()
    {
        // Инициализируем доску размером 10x10
        boardManager.InitializeBoard(10);

        // Расставляем начальные фигуры
        SetupInitialPieces();

        Debug.Log("Game started.");
    }

    /// <summary>
    /// Обрабатывает ход или атаку фигуры.
    /// </summary>
    public void MakeMove(IPiece piece, Vector3Int target)
    {
        // Проверяем, соответствует ли ход текущему игроку
        if (piece.IsPlayer1 != IsPlayer1Turn) return;

        // Получаем доступные ходы и атаки
        var validMoves = piece.GetValidMoves(boardManager);
        var attackMoves = piece.GetAttackMoves(boardManager);

        if (validMoves.Contains(target))
        {
            // Перемещаем фигуру на новую позицию
            boardManager.RemovePiece(piece.Position);
            boardManager.PlacePiece(piece, target);
            Debug.Log($"Moved piece to {target}");
        }
        else if (attackMoves.Contains(target))
        {
            // Уничтожаем фигуру противника на целевой позиции
            boardManager.RemovePiece(target);
            Debug.Log($"Attacked and removed piece at {target}");
        }
        else
        {
            return; // Недопустимый ход
        }

        // Меняем ход
        IsPlayer1Turn = !IsPlayer1Turn;
        OnTurnChanged?.Invoke(IsPlayer1Turn);
    }

    /// <summary>
    /// Расставляет начальные фигуры на доске.
    /// </summary>
    private void SetupInitialPieces()
    {
        // Создаём короля для игрока 1
        var king1 = pieceFactory.CreatePiece(PieceType.King, true, new Vector3Int(5, 0, 0));
        boardManager.PlacePiece(king1, king1.Position);

        // Создаём короля для игрока 2
        var king2 = pieceFactory.CreatePiece(PieceType.King, false, new Vector3Int(5, 0, 9));
        boardManager.PlacePiece(king2, king2.Position);

        // Создаём дракона для игрока 1
        var dragon1 = pieceFactory.CreatePiece(PieceType.Dragon, true, new Vector3Int(4, 0, 0));
        boardManager.PlacePiece(dragon1, dragon1.Position);
    }
}