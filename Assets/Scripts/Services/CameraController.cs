using UnityEngine;
using Zenject;
using System.Collections;
using System;

/// <summary>
/// ��������� ��������������� ������� � ���� "��������".
/// ������������ ������� �������� ����� ��������� ���������� (����� �����) � ������� ���������� �� �������
/// �� ����� ��������, ������� ��� ������� �����. ��������� �� ����� ���� � �������� �����.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Default Camera Settings")]
    [SerializeField, Tooltip("������� ������ �� ��������� (��� ������� �����)")]
    private Vector3 defaultPosition = new Vector3(7f, 2.8f, 4.5f);

    [SerializeField, Tooltip("���� �������� ������ �� ��������� (45� ������, -90� �������)")]
    private Vector3 defaultRotation = new Vector3(45f, -90f, 0f);

    [SerializeField, Tooltip("������ ��������������� ������ ��� ������ ���� �����")]
    private float defaultOrthographicSize = 3.8f;

    [SerializeField, Tooltip("������� ��������� ��������� ������")]
    private float nearClippingPlane = -3f;

    [SerializeField, Tooltip("������������ �������� �������� � ���������� ��������� (� ��������)")]
    private float defaultTransitionDuration = 1f;

    [Header("Piece Follow Settings")]
    [SerializeField, Tooltip("�������� ������ ������������ ������ ��� ����������")]
    private Vector3 followOffset = new Vector3(2f, 2f, 0f);

    [SerializeField, Tooltip("���� �������� ������ ��� ������ 1 ��� ���������� (45� ������, 0� �������)")]
    private Vector3 followRotationPlayer1 = new Vector3(45f, 0f, 0f);

    [SerializeField, Tooltip("���� �������� ������ ��� ������ 2 ��� ���������� (45� ������, 180� �������)")]
    private Vector3 followRotationPlayer2 = new Vector3(45f, 180f, 0f);

    [SerializeField, Tooltip("������ ��������������� ������ ��� ���������� �� �������")]
    private float followOrthographicSize = 2.5f;

    [SerializeField, Tooltip("������������ �������� � ��������� ������� ���������� (� ��������)")]
    private float preFollowTransitionDuration = 0.5f; // ��������� ��� ���������

    private Camera mainCamera; // ������ �� ��������� ������
    private bool isPlayer1Turn = true; // ���� �������� ���� (true ��� ������ 1, false ��� ������ 2)
    private bool isFollowingPiece; // ����, �����������, ��� ������ ������� �� �������
    private Coroutine currentTransition; // ������� �������� �������� ������

    [Inject] private IGameManager gameManager; // �����������: �������� ���� ��� ��������� ������� ����� ����
    [Inject] private IBoardManager boardManager; // �����������: �������� ����� ��� ������� � ��������� ����

    /// <summary>
    /// �������������� ��������� ������ � ��������� ��� �������.
    /// ������������� ������ � ��������������� �����.
    /// </summary>
    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("CameraController: No Camera component found!");
            return;
        }
        mainCamera.orthographic = true;
    }

    /// <summary>
    /// ������������� ��������� ��������� ������ ��� ������ ���� � ������������� �� ������� ����� ����.
    /// </summary>
    private void Start()
    {
        SetupDefaultCamera(instant: true); // ������������� ��������� ��������� ���������
        gameManager.OnTurnChanged += HandleTurnChanged; // ������������� �� ������� ����� ����
    }

    /// <summary>
    /// ������������ �� ������� ����� ���� ��� ����������� �������.
    /// </summary>
    private void OnDestroy()
    {
        if (gameManager != null)
            gameManager.OnTurnChanged -= HandleTurnChanged;
    }

    /// <summary>
    /// ������������� ������ � ��������� ��������� (��� ������� �����).
    /// ���� instant = true, ������� ����������; ����� � ������� �� defaultTransitionDuration.
    /// </summary>
    /// <param name="instant">���� true, ������ ��������������� ���������.</param>
    private void SetupDefaultCamera(bool instant = false)
    {
        if (isFollowingPiece)
            return; // �� ������ ���������, ���� ������ ������� �� �������

        if (currentTransition != null)
        {
            StopCoroutine(currentTransition); // ������������� ������� �������
            currentTransition = null;
        }

        if (instant)
        {
            // ���������� ��������� ��������� ����������
            transform.position = defaultPosition;
            transform.rotation = Quaternion.Euler(defaultRotation);
            mainCamera.orthographicSize = defaultOrthographicSize;
        }
        else
        {
            // ������� ������� � ���������� ���������
            currentTransition = StartCoroutine(SmoothTransition(
                defaultPosition,
                Quaternion.Euler(defaultRotation),
                defaultOrthographicSize,
                defaultTransitionDuration
            ));
        }

        mainCamera.nearClipPlane = nearClippingPlane;
        Debug.Log($"CameraController: Set default camera at {defaultPosition}");
    }

    /// <summary>
    /// ������������ ������� ����� ����, �������� ���� isPlayer1Turn � ��������� ������ � ��������� ���������.
    /// </summary>
    /// <param name="isPlayer1">True, ���� ��� ������ 1; false � ������ 2.</param>
    private void HandleTurnChanged(bool isPlayer1)
    {
        isPlayer1Turn = isPlayer1;
        SetupDefaultCamera(); // ���������� ������ � ��������� ��������� � ������� ���������
    }

    /// <summary>
    /// �������������� ������ � ���������� �� ������� �� ����� �������� ��� �����.
    /// ��������� �������� ��� �������� ������ � ����������� �� ���� �������� (��������, ������� ��� ������� �����).
    /// </summary>
    /// <param name="piece">������, �� ������� ������� ������.</param>
    /// <param name="target">������� ������ (��� �������� ��� �����).</param>
    /// <param name="isMove">���� true, ����������� ��������; ����� � �����.</param>
    /// <param name="isRangedAttack">���� true, ����������� ������� �����; ����� � �������.</param>
    /// <param name="onAnimationComplete">��������, ���������� ����� ���������� ��������.</param>
    public void PrepareToFollowPiece(Piece piece, Vector3Int target, bool isMove, bool isRangedAttack, Action onAnimationComplete)
    {
        if (piece.Type == PieceType.Mountain)
        {
            Debug.Log("CameraController: Ignoring animation for Mountain");
            onAnimationComplete?.Invoke(); // ���� �� �����������, ����� �������� ����������
            return;
        }

        isFollowingPiece = true; // ������������� ���� ����������

        if (currentTransition != null)
        {
            StopCoroutine(currentTransition); // ������������� ������� �������
            currentTransition = null;
        }

        // ��������� �������� ��� ���������� �� �������
        currentTransition = StartCoroutine(PrepareAndFollowPiece(piece, target, isMove, isRangedAttack, onAnimationComplete));
    }

    /// <summary>
    /// ��������� ������� ������� ������ � ������� �������, ������� � �������.
    /// ������������ ��� ���� ��������� ������ (��������� ���������, ���������� �� �������, ���� �������).
    /// </summary>
    /// <param name="targetPosition">������� ������� ������.</param>
    /// <param name="targetRotation">������� ������� ������.</param>
    /// <param name="targetSize">������� ������ ��������������� ������.</param>
    /// <param name="duration">������������ ��������.</param>
    private IEnumerator SmoothTransition(Vector3 targetPosition, Quaternion targetRotation, float targetSize, float duration)
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        float startOrthographicSize = mainCamera.orthographicSize;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration); // ������� ������������
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            mainCamera.orthographicSize = Mathf.Lerp(startOrthographicSize, targetSize, t);
            yield return null;
        }

        // ������������� ������ �������� ��������
        transform.position = targetPosition;
        transform.rotation = targetRotation;
        mainCamera.orthographicSize = targetSize;
    }

    /// <summary>
    /// �������� ��� ���������� � ���������� ������ �� ������� �� ����� �������� ��� �����.
    /// ��� �������� � ������� �����: ������� �� ������� � ������ � ����������.
    /// ��� ������� �����: ������� ������� �� ���������� ������, ����� �� ����, ��������������� � ������ � ���������.
    /// </summary>
    /// <param name="piece">������, �� ������� ������� ������.</param>
    /// <param name="target">������� ������ (��� �������� ��� �����).</param>
    /// <param name="isMove">���� true, ����������� ��������; ����� � �����.</param>
    /// <param name="isRangedAttack">���� true, ����������� ������� �����; ����� � �������.</param>
    /// <param name="onAnimationComplete">��������, ���������� ����� ���������� ��������.</param>
    private IEnumerator PrepareAndFollowPiece(Piece piece, Vector3Int target, bool isMove, bool isRangedAttack, Action onAnimationComplete)
    {
        PieceAnimator animator = piece.GetComponent<PieceAnimator>();
        if (animator == null)
        {
            Debug.LogError($"CameraController: No PieceAnimator on {piece.Type}");
            isFollowingPiece = false;
            onAnimationComplete?.Invoke();
            yield break;
        }

        Vector3 piecePosition = piece.transform.position; // ��������� ������� ������
        Vector3 targetPosition = new Vector3(target.x, 0.5f, target.z); // ������� �������
        PieceAnimationConfig config = animator.GetAnimationConfig(piece); // ������������ ��������

        // ���������� ��������� ������� ������ ��� �������
        Vector3 attackerCameraPosition = piecePosition + (isPlayer1Turn ? followOffset : new Vector3(-followOffset.x, followOffset.y, followOffset.z));
        Quaternion targetRotation = Quaternion.Euler(isPlayer1Turn ? followRotationPlayer1 : followRotationPlayer2);

        // ������� ������� � ��������� ������� ������
        yield return StartCoroutine(SmoothTransition(
            attackerCameraPosition,
            targetRotation,
            followOrthographicSize,
            preFollowTransitionDuration
        ));

        // �������� onAnimationComplete ��� ������������� � ��������� ������
        Debug.Log($"CameraController: Processing {(isMove ? "move" : isRangedAttack ? "ranged attack" : "melee attack")} for {piece.Type}");
        onAnimationComplete?.Invoke();

        if (isRangedAttack)
        {
            // ��� ������� �����: ��� ����
            float rotationAndRecoilDuration = (config?.RecoilDuration ?? 0.2f) + animator.RotationDuration; // ����� �������� � ������
            float cameraProjectileFlightDuration = animator.ProjectileFlightDuration + (config?.HitDuration ?? 0.2f) + (config?.DeathDuration ?? 0.5f); // ����� ����� ������� + �������

            // ���� 1: ������� �� ���������� ������ �� ����� �������� � ������
            float elapsedTime = 0f;
            while (elapsedTime < rotationAndRecoilDuration && isFollowingPiece)
            {
                elapsedTime += Time.deltaTime;
                transform.position = attackerCameraPosition;
                transform.rotation = targetRotation;
                mainCamera.orthographicSize = followOrthographicSize;
                yield return null;
            }

            // ���� 2: ������ ������������ � ���� � ��� ������� ��������� � ������
            Vector3 targetCameraPosition = targetPosition + (isPlayer1Turn ? followOffset : new Vector3(-followOffset.x, followOffset.y, followOffset.z));
            yield return StartCoroutine(SmoothTransition(
                targetCameraPosition,
                targetRotation,
                followOrthographicSize,
                cameraProjectileFlightDuration / 2 // �������� ������� �� �����������
            ));

            // ������� ���������� ����� ��� ��������
            elapsedTime = 0f;
            while (elapsedTime < cameraProjectileFlightDuration / 2 && isFollowingPiece)
            {
                elapsedTime += Time.deltaTime;
                transform.position = targetCameraPosition;
                transform.rotation = targetRotation;
                mainCamera.orthographicSize = followOrthographicSize;
                yield return null;
            }
        }
        else
        {
            // ��� ����������� ��� ������� �����: ������ �� �������
            float totalAnimationDuration = animator.RotationDuration * 2 + (config?.MoveDuration ?? 0.5f); // ����� �������� (��� �������� + ��������)
            Vector3 startPiecePosition = piecePosition;
            Vector3 endPiecePosition = targetPosition;

            float elapsedTime = 0f;
            while (elapsedTime < totalAnimationDuration && isFollowingPiece)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / totalAnimationDuration;
                Vector3 currentPiecePosition = Vector3.Lerp(startPiecePosition, endPiecePosition, t); // ������������ �������
                float height = isMove ? animator.JumpHeight * Mathf.Sin(t * Mathf.PI) : 0f; // ���� ������ ������
                currentPiecePosition.y += height;
                Vector3 cameraPosition = currentPiecePosition + (isPlayer1Turn ? followOffset : new Vector3(-followOffset.x, followOffset.y, followOffset.z));

                transform.position = cameraPosition;
                transform.rotation = targetRotation;
                mainCamera.orthographicSize = followOrthographicSize;
                yield return null;
            }
        }

        // ������� ������� � ��������� ���������
        isFollowingPiece = false;
        SetupDefaultCamera(instant: false);
        currentTransition = null;
    }
}