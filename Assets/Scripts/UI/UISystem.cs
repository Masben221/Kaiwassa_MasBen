using UnityEngine;

/// <summary>
/// Реализация системы интерфейса. Пока просто логирует ходы.
/// </summary>
public class UISystem : IUISystem
{
    /// <summary>
    /// Обновляет отображение текущего хода.
    /// </summary>
    /// <param name="isPlayer1Turn">true, если ход первого игрока</param>
    public void UpdateTurnDisplay(bool isPlayer1Turn)
    {
        // Пока просто логируем, но здесь можно добавить реальный UI
        Debug.Log($"Turn: Player {(isPlayer1Turn ? 1 : 2)}");
    }
}

