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
    [SerializeField, Tooltip("������ ������ ��� ������� ��� ���������� (����������)")]
    private float followHeightOffset = 2.5f; // ���������� ����� Y
    [SerializeField, Tooltip("����� Z ��� ������ 1 (�����) ������������ Z ������")]
    private float followZOffsetPlayer1 = -2.5f; // ����� Z ��� ������ 1
    [SerializeField, Tooltip("����� Z ��� ������ 2 ������������ Z ������")]
    private float followZOffsetPlayer2 = 2.5f; // ����� Z ��� ������ 2
    [SerializeField, Tooltip("���� �������� ������ ��� ������ 1 ��� ���������� (45� ������, 0� �������)")]
    private Vector3 followRotationPlayer1 = new Vector3(45f, 0f, 0f);
    [SerializeField, Tooltip("���� �������� ������ ��� ������ 2 ��� ���������� (45� ������, 180� �������)")]
    private Vector3 followRotationPlayer2 = new Vector3(45f, 180f, 0f);
    [SerializeField, Tooltip("������ ��������������� ������ ��� ���������� �� �������")]
    private float followOrthographicSize = 2.5f;
    [SerializeField, Tooltip("������������ �������� � ��������� ������� ���������� (� ��������)")]
    private float preFollowTransitionDuration = 0.5f;
    [SerializeField, Tooltip("�������� ����������� ������ �� ������� (� �������� �� �������)")]
    private float followSpeed = 5f; // ����� �������� ������������

    private Camera mainCamera; // ������ �� ��������� ������
    private bool isPlayer1Turn = true; // ���� �������� ���� (true ��� ������ 1, false ��� ������ 2)
    private bool isFollowingPiece; // ����, �����������, ��� ������ ������� �� �������
    private Coroutine currentTransition; // ������� �������� �������� ������
    private int pendingAnimations; // ������� �������� ��������

    [Inject] private IGameManager gameManager; // �����������: �������� ���� ��� ��������� ������� ����� ����
    [Inject] private IBoardManager boardManager; // �����������: �������� ����� ��� ������� � ��������� ����

    /// <summary>
    /// �������������� ��������� ������ � ��������� ��� �������.
    /// ������������� ������ � ��������������� ����� � ������������� �� ������� ��������.
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
        gameManager.OnMoveInitiated += HandleMoveInitiated; // �������� �� ����� �������        
    }

    /// <summary>
    /// ������������ �� ������� ��� ����������� �������.
    /// </summary>
    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnTurnChanged -= HandleTurnChanged;
            gameManager.OnMoveInitiated -= HandleMoveInitiated;
        }        
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
    /// ������������ ������� ������ ����, �������� ����������� ������ � ������.
    /// </summary>
    private void HandleMoveInitiated(Piece piece, Vector3Int target, bool isMove, bool isRangedAttack)
    {
        if (piece.Type == PieceType.Mountain)
        {
            Debug.Log("CameraController: Ignoring animation for Mountain");
            return;
        }

        pendingAnimations++;
        isFollowingPiece = true;

        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
            currentTransition = null;
        }

        currentTransition = StartCoroutine(PrepareAndFollowPiece(piece, target, isMove, isRangedAttack, null));
    }

    /// <summary>
    /// ����������� ������� �������� ��������.
    /// </summary>
    public void IncrementPendingAnimations()
    {
        pendingAnimations++;
    }

    /// <summary>
    /// ������������ ������� ���������� ��������, �������� ������� �������� ��������.
    /// ���������� ������ � ��������� ���������, ����� ��� �������� ���������.
    /// </summary>
    public void HandleAnimationCompleted()
    {
        pendingAnimations--;
        if (pendingAnimations == 0)
        {
            isFollowingPiece = false;
            SetupDefaultCamera(instant: false);
            currentTransition = null;
        }
    }

    /// <summary>
    /// ��������� ������� ������� ������ � ������� �������, ������� � �������.
    /// ������������ ��� ���� ��������� ������ (��������� ���������, ���������� �� �������, ���� �������).
    /// </summary>
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
    /// ������ ������� �� ������� � ������������� �������� Y � Z, ���������� �������� �� ��������.
    /// </summary>
    private IEnumerator PrepareAndFollowPiece(Piece piece, Vector3Int target, bool isMove, bool isRangedAttack, Action onAnimationComplete)
    {
        PieceAnimator animator = piece.GetComponent<PieceAnimator>();
        if (animator == null)
        {
            Debug.LogError($"CameraController: No PieceAnimator on {piece.Type}");
            isFollowingPiece = false;
            pendingAnimations--;
            yield break;
        }

        PieceAnimationConfig config = animator.GetAnimationConfig(piece); // ������������ ��������
        if (config == null)
        {
            Debug.LogError($"CameraController: No PieceAnimationConfig for {piece.Type}");
            isFollowingPiece = false;
            pendingAnimations--;
            yield break;
        }

        Vector3 piecePosition = piece.transform.position; // ��������� ������� ������
        Vector3 targetPosition = new Vector3(target.x, 0.5f, target.z); // ������� �������

        // ��������� ��������� ������� ������ � ������������� ��������
        float zOffset = isPlayer1Turn ? followZOffsetPlayer1 : followZOffsetPlayer2;
        Vector3 attackerCameraPosition = new Vector3(piecePosition.x, piecePosition.y + followHeightOffset, piecePosition.z + zOffset);
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
            float rotationAndRecoilDuration = (config?.RecoilDuration ?? 0.2f) + (config?.RotationDuration ?? 0.3f); // ����� �������� � ������
            float cameraProjectileFlightDuration = animator.ProjectileFlightDuration + (config?.HitDuration ?? 0.2f) + (config?.DeathDuration ?? 0.5f); // ����� ����� ������� + �������

            // ���� 1: ������� �� ���������� ������ �� ����� �������� � ������
            float elapsedTime = 0f;
            while (elapsedTime < rotationAndRecoilDuration && isFollowingPiece)
            {
                elapsedTime += Time.deltaTime;
                Vector3 currentPiecePos = piece.transform.position;
                transform.position = new Vector3(currentPiecePos.x, currentPiecePos.y + followHeightOffset, currentPiecePos.z + zOffset);
                transform.rotation = targetRotation;
                mainCamera.orthographicSize = followOrthographicSize;
                yield return null;
            }

            // ���� 2: ������ ������������ � ���� � ��� ������� ��������� � ������
            Vector3 targetCameraPosition = new Vector3(targetPosition.x, targetPosition.y + followHeightOffset, targetPosition.z + zOffset);
            yield return StartCoroutine(SmoothTransition(
                targetCameraPosition,
                targetRotation,
                followOrthographicSize,
                cameraProjectileFlightDuration / 2
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
            float totalAnimationDuration = (config?.MeleePunchDistance ?? 0.2f) * 2 + (config?.MeleeAttackDuration ?? 0.3f) * 2 + (config?.RotationDuration ?? 0.3f) * 2 + (config?.MoveDuration ?? 0.5f) * 2;
            Vector3 startPiecePosition = piecePosition;
            Vector3 endPiecePosition = targetPosition;

            float elapsedTime = 0f;
            while (elapsedTime < totalAnimationDuration && isFollowingPiece)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / totalAnimationDuration;
                Vector3 currentPiecePosition = Vector3.Lerp(startPiecePosition, endPiecePosition, t);
                float height = isMove ? (config?.JumpHeight ?? 1f) * Mathf.Sin(t * Mathf.PI) : 0f;
                Vector3 cameraPosition = new Vector3(currentPiecePosition.x, currentPiecePosition.y + followHeightOffset + height, currentPiecePosition.z + zOffset);
                Vector3 targetCameraPosition = Vector3.Lerp(transform.position, cameraPosition, followSpeed * Time.deltaTime);
                transform.position = targetCameraPosition;
                transform.rotation = targetRotation;
                mainCamera.orthographicSize = followOrthographicSize;
                yield return null;
            }
        }
    }
}