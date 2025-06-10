using UnityEngine;
using System;
using System.Collections;

public class PieceAnimator : MonoBehaviour
{
    [SerializeField] private float moveDuration = 0.5f; // ������������ �������� �����������
    [SerializeField] private float jumpHeight = 1f; // ������ ������
    [SerializeField] private float rotationDuration = 0.3f; // ������������ ��������

    private bool isAnimating; // ���� ��������

    public float MoveDuration => moveDuration;
    public float JumpHeight => jumpHeight;
    public float RotationDuration => rotationDuration;

    public static event Action<Piece> OnAnimationStarted;
    public static event Action<Piece> OnAnimationFinished;

    // ����������� � ��������� � ���������
    public void MoveTo(Vector3Int targetPos, Vector3Int animationTarget, Action onStart, Action onComplete) // ���������: �������� targetPos
    {
        if (isAnimating)
        {
            Debug.LogWarning($"PieceAnimator: Already animating for {gameObject.name}");
            return;
        }

        Piece piece = GetComponent<Piece>();
        if (piece == null)
        {
            Debug.LogError($"PieceAnimator: No Piece component on {gameObject.name}");
            onComplete?.Invoke();
            return;
        }

        if (piece.Type == PieceType.Mountain)
        {
            Debug.Log($"PieceAnimator: Cannot animate Mountain at {targetPos}");
            onComplete?.Invoke();
            return;
        }

        Debug.Log($"PieceAnimator: MoveTo called for {piece.Type} to target {targetPos}, animationTarget {animationTarget}");
        isAnimating = true;

        onStart?.Invoke();
        OnAnimationStarted?.Invoke(piece);

        StartCoroutine(AnimateMoveAndRotate(piece, targetPos, animationTarget, () =>
        {
            isAnimating = false;
            OnAnimationFinished?.Invoke(piece);
            onComplete?.Invoke();
        }));
    }

    // �������� ��� �������� ����������� � ��������
    private IEnumerator AnimateMoveAndRotate(Piece piece, Vector3Int targetPos, Vector3Int animationTarget, Action callback)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(animationTarget.x, 0.5f, animationTarget.z); // ���������� animationTarget ��� �����������
        Quaternion startRotation = transform.rotation;
        Quaternion initialRotation = piece.InitialRotation;

        // ��������� ����������� �������� �� ������ targetPos
        Vector3 direction = new Vector3(targetPos.x - piece.Position.x, 0, targetPos.z - piece.Position.z);
        Quaternion targetRotation = direction.sqrMagnitude > 0.01f ? Quaternion.LookRotation(direction, Vector3.up) : startRotation;

        // ������� � ����
        float elapsedTime = 0f;
        while (elapsedTime < rotationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / rotationDuration);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }
        transform.rotation = targetRotation;

        // ����������� (��� �������� ��� ������� �����)
        elapsedTime = 0f;
        if (Vector3.Distance(startPos, endPos) > 0.1f) // ����������, ���� animationTarget ����������
        {
            while (elapsedTime < moveDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / moveDuration);
                float height = jumpHeight * Mathf.Sin(t * Mathf.PI);
                transform.position = Vector3.Lerp(startPos, endPos, t) + new Vector3(0, height, 0);
                yield return null;
            }
            transform.position = endPos;
        }
        else // ��� ������� ����� ���
        {
            yield return new WaitForSeconds(moveDuration);
        }

        // ������� ������� � ��������� �������
        elapsedTime = 0f;
        while (elapsedTime < rotationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / rotationDuration);
            transform.rotation = Quaternion.Slerp(targetRotation, initialRotation, t);
            yield return null;
        }
        transform.rotation = initialRotation;

        Debug.Log($"PieceAnimator: Animation completed for {piece.Type} to target {targetPos}");
        callback?.Invoke();
    }
}