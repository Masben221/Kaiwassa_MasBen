using UnityEngine;

/// <summary>
/// Интерфейс для управления размещением фигур и гор на доске.
/// </summary>
public interface IPiecePlacementManager
{
    void Initialize(int mountainsPerSide);
    int GetMountainsPerSide { get; }
    bool CanPlace(bool isPlayer1, Vector3Int position, PieceType type, bool isMove = false);
    bool PlacePieceOrMountain(bool isPlayer1, Vector3Int position, PieceType type, bool isMove = false);
    bool RemovePiece(bool isPlayer1, Vector3Int position, PieceType type);
    bool RemovePiece(Piece piece);
    bool MovePiece(Piece piece, Vector3Int position, Vector3Int to);
    int GetRemainingCount(bool isPlayer1, PieceType type);
    bool HasCompletedPlacement(bool isPlayer1);
    bool IsKingNotPlaced(bool isPlayer1);
    void PlaceMountains(int mountainsPerSide);
    void PlacePiecesForPlayer(bool isPlayer1);

    // Устаревшие методы, сохранены для совместимости
    bool CanPlace(bool isPlayer1, Vector3Int position, bool isMountain);
    bool PlacePieceOrMountain(bool isPlayer1, Vector3Int position, PieceType type);
}