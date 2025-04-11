using UnityEngine;

/// <summary>
/// Интерфейс, определяющий методы для управления игровой доской.
/// Используется для соблюдения принципа Dependency Inversion (D из SOLID).
/// </summary>
public interface IBoardManager
{
    // Инициализирует доску заданного размера в 3D-пространстве
    void InitializeBoard(int size);

    // Проверяет, находится ли позиция в пределах доски
    bool IsWithinBounds(Vector3Int position);

    // Проверяет, занята ли клетка фигурой
    bool IsOccupied(Vector3Int position);

    // Размещает фигуру на заданной 3D-позиции
    void PlacePiece(IPiece piece, Vector3Int position);

    // Удаляет фигуру с заданной 3D-позиции
    void RemovePiece(Vector3Int position);

    // Возвращает фигуру, находящуюся на заданной позиции
    IPiece GetPieceAt(Vector3Int position);
}