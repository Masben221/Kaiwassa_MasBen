using UnityEngine;

// <summary>
/// Интерфейс фабрики для создания фигур.
/// Следует паттерну Factory для централизованного создания объектов.
/// </summary>
public interface IPieceFactory
{
    IPiece CreatePiece(PieceType type, bool isPlayer1, Vector3Int position); // Метод создания фигуры
}

/// <summary>
/// Перечисление всех типов фигур в игре.
/// Используется для определения, какую фигуру создавать.
/// </summary>
public enum PieceType { King, Dragon, Elephant, HeavyHorse, LightHorse, Spearman, Crossbowman, Rabble, Catapult, Trebuchet }
