using UnityEngine;
using Zenject;
using System.Collections;
using System;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Default Camera Settings")]
    [SerializeField] private Vector3 defaultPosition = new Vector3(7f, 2.8f, 4.5f);
    [SerializeField] private Vector3 defaultRotation = new Vector3(45f, -90f, 0f);
    [SerializeField] private float defaultOrthographicSize = 3.8f;
    [SerializeField] private float nearClippingPlane = -3f;
    [SerializeField] private float defaultTransitionDuration = 1f;

    [Header("Piece Follow Settings")]
    [SerializeField] private Vector3 followOffset = new Vector3(2f, 2f, 0f);
    [SerializeField] private Vector3 followRotationPlayer1 = new Vector3(45f, 0f, 0f);
    [SerializeField] private Vector3 followRotationPlayer2 = new Vector3(45f, 180f, 0f);
    [SerializeField] private float followOrthographicSize = 2.5f;
    [SerializeField] private float preFollowTransitionDuration = 0.5f;

    private Camera mainCamera;
    private bool isPlayer1Turn = true;
    private bool isFollowingPiece;
    private Coroutine currentTransition;

    [Inject] private IGameManager gameManager;
    [Inject] private IBoardManager boardManager;

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

    private void Start()
    {
        SetupDefaultCamera(instant: true);
        gameManager.OnTurnChanged += HandleTurnChanged;
    }

    private void OnDestroy()
    {
        gameManager.OnTurnChanged -= HandleTurnChanged;
    }

    private void SetupDefaultCamera(bool instant = false)
    {
        if (isFollowingPiece) return;

        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
        }

        if (instant)
        {
            transform.position = defaultPosition;
            transform.rotation = Quaternion.Euler(defaultRotation);
            mainCamera.orthographicSize = defaultOrthographicSize;
        }
        else
        {
            currentTransition = StartCoroutine(SmoothTransition(
                defaultPosition,
                Quaternion.Euler(defaultRotation),
                defaultOrthographicSize,
                defaultTransitionDuration
            ));
        }

        mainCamera.nearClipPlane = nearClippingPlane;
        Debug.Log($"CameraController: Set default camera at Position: {defaultPosition}, Rotation: {defaultRotation}, OrthographicSize: {defaultOrthographicSize}");
    }

    private void HandleTurnChanged(bool isPlayer1)
    {
        isPlayer1Turn = isPlayer1;
        SetupDefaultCamera();
    }

    public void PrepareToFollowPiece(Piece piece, Vector3Int target, bool isMove, bool isRangedAttack, Action onAnimationComplete)
    {
        if (piece.Type == PieceType.Mountain)
        {
            Debug.Log("CameraController: Ignoring animation for Mountain piece.");
            onAnimationComplete?.Invoke();
            return;
        }

        isFollowingPiece = true;

        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
        }

        currentTransition = StartCoroutine(PrepareAndFollowPiece(piece, target, isMove, isRangedAttack, onAnimationComplete));
    }

    private IEnumerator SmoothTransition(Vector3 targetPosition, Quaternion targetRotation, float targetOrthographicSize, float duration)
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        float startOrthographicSize = mainCamera.orthographicSize;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            mainCamera.orthographicSize = Mathf.Lerp(startOrthographicSize, targetOrthographicSize, t);
            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;
        mainCamera.orthographicSize = targetOrthographicSize;
    }

    private IEnumerator PrepareAndFollowPiece(Piece piece, Vector3Int target, bool isMove, bool isRangedAttack, Action onAnimationComplete)
    {
        PieceAnimator animator = piece.GetComponent<PieceAnimator>();
        if (animator == null)
        {
            Debug.LogError($"CameraController: No PieceAnimator found on {piece.Type} at {piece.Position}");
            isFollowingPiece = false;
            onAnimationComplete?.Invoke();
            yield break;
        }

        // Шаг 1: Перемещение за спину фигуры
        Vector3 piecePosition = piece.transform.position;
        Vector3 targetCameraPosition = piecePosition + (isPlayer1Turn ? followOffset : new Vector3(-followOffset.x, followOffset.y, followOffset.z));
        Quaternion targetRotation = Quaternion.Euler(isPlayer1Turn ? followRotationPlayer1 : followRotationPlayer2);

        yield return StartCoroutine(SmoothTransition(
            targetCameraPosition,
            targetRotation,
            followOrthographicSize,
            preFollowTransitionDuration
        ));

        // Шаг 2: Выполнение действия с анимацией
        Debug.Log($"CameraController: Processing {(isMove ? "move" : isRangedAttack ? "ranged attack" : "melee attack")} for {piece.Type}");
        Vector3 startPiecePosition = piecePosition;
        Vector3 endPiecePosition = isRangedAttack ? piecePosition : new Vector3(target.x, 0.5f, target.z);
        float animationDuration = animator.MoveDuration;

        onAnimationComplete?.Invoke(); // Запускаем PerformAction, который вызывает MoveTo

        // Шаг 3: Слежение за фигурой во время анимации
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration && isFollowingPiece)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            Vector3 currentPiecePosition = Vector3.Lerp(startPiecePosition, endPiecePosition, t);
            float height = isRangedAttack ? 0f : animator.JumpHeight * Mathf.Sin(t * Mathf.PI);
            currentPiecePosition.y += height;
            Vector3 cameraPosition = currentPiecePosition + (isPlayer1Turn ? followOffset : new Vector3(-followOffset.x, followOffset.y, followOffset.z));

            transform.position = cameraPosition;
            transform.rotation = targetRotation;
            mainCamera.orthographicSize = followOrthographicSize;

            yield return null;
        }

        isFollowingPiece = false;
        SetupDefaultCamera();
    }
}