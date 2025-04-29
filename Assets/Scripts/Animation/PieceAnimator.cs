using UnityEngine;
using System;

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
            Debug.LogWarning($"PieceAnimator: Already moving for {gameObject.name}");
            return;
        }

        Piece piece = GetComponent<Piece>();
        if (piece == null)
        {
            Debug.LogError($"PieceAnimator: No Piece component on {gameObject.name}");
            return;
        }

        if (piece.Type == PieceType.Mountain)
        {
            Debug.LogWarning($"PieceAnimator: Cannot animate Mountain at {target}");
            onComplete?.Invoke();
            return;
        }

        Debug.Log($"PieceAnimator: MoveTo called for {piece.Type} to {target}");
        isMoving = true;

        // ���������� � ������ ��������
        OnAnimationStarted?.Invoke();

        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(target.x, 0.5f, target.z); // y=0.5f ��� ������
        StartCoroutine(MoveCoroutine(startPos, endPos, () =>
        {
            isMoving = false;
            transform.position = endPos;
            // ���������� � ���������� ��������
            OnAnimationFinished?.Invoke();
            onComplete?.Invoke();
        }));
    }

    private System.Collections.IEnumerator MoveCoroutine(Vector3 start, Vector3 end, Action callback)
    {
        float elapsedTime = 0f;
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