using UnityEngine;
using Zenject;
using System.Collections.Generic;

/// <summary>
/// Обрабатывает ввод игрока (мышь или тачскрин) для выбора фигур и ходов.
/// </summary>
public class InputHandler : MonoBehaviour
{
    // Зависимости, инжектируемые через Zenject
    [Inject] private IGameManager gameManager;
    [Inject] private IBoardManager boardManager;

    // Префабы для маркеров клеток
    [SerializeField] private GameObject moveMarkerPrefab; // Для ходов
    [SerializeField] private GameObject attackMarkerPrefab; // Для атак

    // Текущая выбранная фигура
    private Piece selectedPiece;

    // Список текущих маркеров на доске
    private List<GameObject> currentMarkers = new List<GameObject>();

    // Материал для подсветки выбранной фигуры
    [SerializeField] private Material highlightMaterial;

    // Исходной материал выбранной фигуры (для восстановления)
    private Material originalMaterial;

    /// <summary>
    /// Обрабатывает ввод игрока в каждом кадре.
    /// </summary>
    private void Update()
    {
        // Проверяем клик мыши или касание (работает для мыши и тачскрина)
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    /// <summary>
    /// Обрабатывает клик: выбирает фигуру, выполняет ход/атаку или отменяет выделение.
    /// </summary>
    private void HandleClick()
    {
        // Создаём луч из позиции мыши/касания
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Получаем 3D-позицию клика, учитывая центр клетки
            Vector3Int clickedPos = new Vector3Int(
                Mathf.FloorToInt(hit.point.x + 0.5f),
                0,
                Mathf.FloorToInt(hit.point.z + 0.5f)
            );

            // Проверяем, есть ли фигура на клетке
            Piece clickedPiece = boardManager.GetPieceAt(clickedPos);

            // Если фигура выбрана
            if (selectedPiece != null)
            {
                // Проверяем, является ли клетка допустимым ходом или атакой
                var validMoves = selectedPiece.GetValidMoves(boardManager);
                var attackMoves = selectedPiece.GetAttackMoves(boardManager);

                if (validMoves.Contains(clickedPos) || attackMoves.Contains(clickedPos))
                {
                    // Выполняем ход или атаку
                    gameManager.MakeMove(selectedPiece, clickedPos);
                }

                // Сбрасываем выделение
                ClearSelection();
            }
            // Если кликнули на фигуру текущего игрока
            else if (clickedPiece != null && clickedPiece.IsPlayer1 == gameManager.IsPlayer1Turn)
            {
                // Выбираем фигуру
                SelectPiece(clickedPiece);
            }
            // Если кликнули вне доски, отменяем выделение
            else if (!boardManager.IsWithinBounds(clickedPos))
            {
                ClearSelection();
            }
        }
        else
        {
            // Если кликнули вне объектов, отменяем выделение
            ClearSelection();
        }
    }

    /// <summary>
    /// Выбирает фигуру и подсвечивает клетки для ходов и атак.
    /// </summary>
    private void SelectPiece(Piece piece)
    {
        selectedPiece = piece;

        // Подсвечиваем фигуру
        Renderer renderer = piece.GetComponentInChildren<Renderer>();
        if (renderer != null && highlightMaterial != null)
        {
            originalMaterial = renderer.material;
            renderer.material = highlightMaterial;
        }

        // Показываем маркеры для доступных ходов
        var validMoves = piece.GetValidMoves(boardManager);
        foreach (var move in validMoves)
        {
            GameObject marker = Instantiate(
                moveMarkerPrefab,
                new Vector3(move.x, 0.1f, move.z),
                Quaternion.Euler(90, 0, 0)
            );
            currentMarkers.Add(marker);
        }

        // Показываем маркеры для доступных атак
        var attackMoves = piece.GetAttackMoves(boardManager);
        foreach (var attack in attackMoves)
        {
            GameObject marker = Instantiate(
                attackMarkerPrefab,
                new Vector3(attack.x, 0.1f, attack.z),
                Quaternion.Euler(90, 0, 0)
            );
            currentMarkers.Add(marker);
        }

        Debug.Log($"Selected piece at {piece.Position}");
    }

    /// <summary>
    /// Очищает текущее выделение и убирает маркеры.
    /// </summary>
    private void ClearSelection()
    {
        // Восстанавливаем материал фигуры
        if (selectedPiece != null)
        {
            Renderer renderer = selectedPiece.GetComponentInChildren<Renderer>();
            if (renderer != null && originalMaterial != null)
            {
                renderer.material = originalMaterial;
            }
        }

        // Удаляем все маркеры
        foreach (var marker in currentMarkers)
        {
            Destroy(marker);
        }
        currentMarkers.Clear();

        selectedPiece = null;
        Debug.Log("Selection cleared.");
    }

    /// <summary>
    /// Очистка при уничтожении объекта.
    /// </summary>
    private void OnDestroy()
    {
        ClearSelection();
    }
}