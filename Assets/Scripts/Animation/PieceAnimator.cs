using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// ��������� ��� �������� �������� �����.
/// ��������� ������� ����������� � ������� � ���������� � ������/���������� ��������.
/// </summary>
public class PieceAnimator : MonoBehaviour
{
    [SerializeField] private float moveDuration = 0.5f; // ������������ ��������
    [SerializeField] private float jumpHeight = 1f; // ������ ������

    private bool isMoving; // ����, ����������� �� ��������

    // ������� ��� ����������� � ������ � ���������� ��������
    public static event Action OnAnimationStarted;
    public static event Action OnAnimationFinished;

    /// <summary>
    /// ���������� ������ � ������� ������� � ��������� ������.
    /// </summary>
    /// <param name="target">������� ������ � ��������� �����������.</param>
    /// <param name="onComplete">Callback, ���������� ����� ����������.</param>
    public void MoveTo(Vector3Int target, Action onComplete)
    {
        if (isMoving)
        {
            Debug.LogWarning($"PieceAnimator: Already moving for {GetComponent<Piece>().GetType().Name}");
            return;
        }

        Debug.Log($"PieceAnimator: MoveTo called for {GetComponent<Piece>().GetType().Name} to {target}");
        isMoving = true;

        // ���������� � ������ ��������
        OnAnimationStarted?.Invoke();

        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(target.x, 0.5f, target.z); // y=0.5f ��� ������
        float elapsedTime = 0f;

        StartCoroutine(MoveCoroutine(startPos, endPos, () =>
        {
            isMoving = false;
            transform.position = endPos;
            // ���������� � ���������� ��������
            OnAnimationFinished?.Invoke();
            onComplete?.Invoke();
        }));

        IEnumerator MoveCoroutine(Vector3 start, Vector3 end, Action callback)
        {
            while (elapsedTime < moveDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / moveDuration;
                float height = jumpHeight * Mathf.Sin(t * Mathf.PI);
                transform.position = Vector3.Lerp(start, end, t) + new Vector3(0, height, 0);
                yield return null;
            }
            callback?.Invoke();
        }
    }
}