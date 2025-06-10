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
    [SerializeField] private float preFollowTransitionDuration = 0.3f; // Уменьшено для синхронизации (ИСПРАВЛЕНО)

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
        if (gameManager != null)
            gameManager.OnTurnChanged -= HandleTurnChanged;
    }

    // Установка дефолтной позиции камеры
    private void SetupDefaultCamera(bool instant = false)
    {
        if (isFollowingPiece)
            return;

        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
            currentTransition = null; // Сброс для немедленного завершения
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
        Debug.Log($"CameraController: Set default camera at {defaultPosition}");
    }

    // Обработка смены хода
    private void HandleTurnChanged(bool isPlayer1)
    {
        isPlayer1Turn = isPlayer1;
        SetupDefaultCamera();
    }

    // Подготовка камеры к слежению за фигурой
    public void PrepareToFollowPiece(Piece piece, Vector3Int target, bool isMove, bool isRangedAttack, Action onAnimationComplete)
    {
        if (piece.Type == PieceType.Mountain)
        {
            Debug.Log("CameraController: Ignoring animation for Mountain");
            onAnimationComplete?.Invoke();
            return;
        }

        isFollowingPiece = true;

        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
            currentTransition = null;
        }

        currentTransition = StartCoroutine(PrepareAndFollowPiece(piece, target, isMove, isRangedAttack, onAnimationComplete));
    }

    // Плавный переход камеры
    private IEnumerator SmoothTransition(Vector3 targetPosition, Quaternion targetRotation, float targetSize, float duration)
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
            mainCamera.orthographicSize = Mathf.Lerp(startOrthographicSize, targetSize, t);
            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;
        mainCamera.orthographicSize = targetSize;
    }

    // Слежение за фигурой
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

        // Перемещение за спину (синхронизировано с началом поворота фигуры)
        Vector3 piecePosition = piece.transform.position;
        Vector3 targetCameraPosition = piecePosition + (isPlayer1Turn ? followOffset : new Vector3(-followOffset.x, followOffset.y, followOffset.z));
        Quaternion targetRotation = Quaternion.Euler(isPlayer1Turn ? followRotationPlayer1 : followRotationPlayer2);

        yield return StartCoroutine(SmoothTransition(
            targetCameraPosition,
            targetRotation,
            followOrthographicSize,
            preFollowTransitionDuration
        ));

        // Выполнение действия
        Debug.Log($"CameraController: Processing {(isMove ? "move" : isRangedAttack ? "ranged attack" : "melee attack")} for {piece.Type}");

        // Полная длительность анимации фигуры: поворот + перемещение/пауза + поворот обратно
        float totalAnimationDuration = animator.RotationDuration * 2 + animator.MoveDuration;

        Vector3 startPiecePosition = piecePosition;
        Vector3 endPiecePosition = isRangedAttack ? piecePosition : new Vector3(target.x, 0.5f, target.z);

        onAnimationComplete?.Invoke();

        // Слежение за фигурой
        float elapsedTime = 0f;
        while (elapsedTime < totalAnimationDuration && isFollowingPiece)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / totalAnimationDuration;
            Vector3 currentPiecePosition = Vector3.Lerp(startPiecePosition, endPiecePosition, t);
            float height = isRangedAttack ? 0f : animator.JumpHeight * Mathf.Sin(t * Mathf.PI);
            currentPiecePosition.y += height;
            Vector3 cameraPosition = currentPiecePosition + (isPlayer1Turn ? followOffset : new Vector3(-followOffset.x, followOffset.y, followOffset.z));

            transform.position = cameraPosition;
            transform.rotation = targetRotation;
            mainCamera.orthographicSize = followOrthographicSize;

            yield return null;
        }

        // Немедленный возврат в дефолтное положение
        isFollowingPiece = false;
        SetupDefaultCamera(instant: false); // Плавный переход без задержки
        currentTransition = null; // Сбрасываем для следующей анимации
    }
}