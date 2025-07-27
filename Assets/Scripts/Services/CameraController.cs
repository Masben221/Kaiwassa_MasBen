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
    [SerializeField] private float preFollowTransitionDuration = 0.5f; // Увеличено для плавности (ИСПРАВЛЕНИЕ)

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

    private void SetupDefaultCamera(bool instant = false)
    {
        if (isFollowingPiece)
            return;

        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
            currentTransition = null;
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

    private void HandleTurnChanged(bool isPlayer1)
    {
        isPlayer1Turn = isPlayer1;
        SetupDefaultCamera();
    }

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

        Vector3 piecePosition = piece.transform.position;
        Vector3 targetPosition = new Vector3(target.x, 0.5f, target.z);
        PieceAnimationConfig config = animator.GetAnimationConfig(piece);

        // Определяем начальную позицию камеры (над атакующей фигурой)
        Vector3 attackerCameraPosition = piecePosition + (isPlayer1Turn ? followOffset : new Vector3(-followOffset.x, followOffset.y, followOffset.z));
        Quaternion targetRotation = Quaternion.Euler(isPlayer1Turn ? followRotationPlayer1 : followRotationPlayer2);

        // Плавный переход к начальной позиции камеры
        yield return StartCoroutine(SmoothTransition(
            attackerCameraPosition,
            targetRotation,
            followOrthographicSize,
            preFollowTransitionDuration
        ));

        // Запускаем анимацию фигуры
        Debug.Log($"CameraController: Processing {(isMove ? "move" : isRangedAttack ? "ranged attack" : "melee attack")} for {piece.Type}");
        onAnimationComplete?.Invoke();

        if (isRangedAttack)
        {
            // Для дальней атаки: сначала смотрим на стреляющего, затем на цель
            float rotationAndRecoilDuration = (config?.RecoilDuration ?? 0.2f) + animator.RotationDuration;
            float projectileFlightDuration = config?.RangedAttackDuration ?? 0.5f;
            float deathAnimationDuration = config?.DeathDuration ?? 0.5f;

            // Фаза 1: Смотрим на стреляющего во время поворота и отдачи
            float elapsedTime = 0f;
            while (elapsedTime < rotationAndRecoilDuration && isFollowingPiece)
            {
                elapsedTime += Time.deltaTime;
                transform.position = attackerCameraPosition;
                transform.rotation = targetRotation;
                mainCamera.orthographicSize = followOrthographicSize;
                yield return null;
            }

            // Фаза 2: Плавно перемещаемся к цели во время полёта снаряда
            Vector3 targetCameraPosition = targetPosition + (isPlayer1Turn ? followOffset : new Vector3(-followOffset.x, followOffset.y, followOffset.z));
            yield return StartCoroutine(SmoothTransition(
                targetCameraPosition,
                targetRotation,
                followOrthographicSize,
                projectileFlightDuration
            ));

            // Ждём анимацию попадания и смерти
            elapsedTime = 0f;
            while (elapsedTime < deathAnimationDuration && isFollowingPiece)
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
            // Для перемещения или ближней атаки: следим за фигурой
            float totalAnimationDuration = animator.RotationDuration * 2 + (config?.MoveDuration ?? 0.5f);
            Vector3 startPiecePosition = piecePosition;
            Vector3 endPiecePosition = isMove ? targetPosition : piecePosition;

            float elapsedTime = 0f;
            while (elapsedTime < totalAnimationDuration && isFollowingPiece)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / totalAnimationDuration;
                Vector3 currentPiecePosition = Vector3.Lerp(startPiecePosition, endPiecePosition, t);
                float height = isMove ? animator.JumpHeight * Mathf.Sin(t * Mathf.PI) : 0f;
                currentPiecePosition.y += height;
                Vector3 cameraPosition = currentPiecePosition + (isPlayer1Turn ? followOffset : new Vector3(-followOffset.x, followOffset.y, followOffset.z));

                transform.position = cameraPosition;
                transform.rotation = targetRotation;
                mainCamera.orthographicSize = followOrthographicSize;
                yield return null;
            }
        }

        // Плавный возврат в дефолтное положение
        isFollowingPiece = false;
        SetupDefaultCamera(instant: false);
        currentTransition = null;
    }
}