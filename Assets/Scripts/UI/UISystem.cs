using UnityEngine;

/// <summary>
/// ��������� ��� ������� ����������.
/// </summary>
public interface IUISystem
{
    void UpdateTurnDisplay(bool isPlayer1Turn); // ���������� ����������� �������� ����
}

/// <summary>
/// ���������� ������� ����������. ���� ������ �������� ����.
/// </summary>
public class UISystem : IUISystem
{
    /// <summary>
    /// ��������� ����������� �������� ����.
    /// </summary>
    /// <param name="isPlayer1Turn">true, ���� ��� ������� ������</param>
    public void UpdateTurnDisplay(bool isPlayer1Turn)
    {
        // ���� ������ ��������, �� ����� ����� �������� �������� UI
        Debug.Log($"Turn: Player {(isPlayer1Turn ? 1 : 2)}");
    }
}

