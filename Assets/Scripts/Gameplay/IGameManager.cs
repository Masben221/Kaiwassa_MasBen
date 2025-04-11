using UnityEngine;

/// <summary>
/// Интерфейс для управления логикой игры.
/// </summary>
public interface IGameManager
{
    void StartGame(); // Запуск игры
    void MakeMove(IPiece piece, Vector3Int target); // Выполнение хода
    bool IsPlayer1Turn { get; } // Чей сейчас ход

    event System.Action<bool> OnTurnChanged; // Событие смены хода (паттерн Observer)
}