using UnityEngine;
using Zenject;
using System.Collections;
using System;

/// <summary>
/// Управляет ортографической камерой в игре "Кайвасса".
/// Обеспечивает плавные переходы между дефолтным положением (обзор доски) и режимом следования за фигурой
/// во время движения, ближней или дальней атаки. Реагирует на смену хода и действия фигур.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Default Camera Settings")]
    [SerializeField, Tooltip("Позиция камеры по умолчанию (над центром доски)")]
    private Vector3 defaultPosition = new Vector3(7f, 2.8f, 4.5f);
    [SerializeField, Tooltip("Угол поворота камеры по умолчанию (45° наклон, -90° поворот)")]
    private Vector3 defaultRotation = new Vector3(45f, -90f, 0f);
    [SerializeField, Tooltip("Размер ортографической камеры для обзора всей доски")]
    private float defaultOrthographicSize = 3.8f;
    [SerializeField, Tooltip("Ближняя плоскость отсечения камеры")]
    private float nearClippingPlane = -3f;
    [SerializeField, Tooltip("Длительность плавного перехода к дефолтному положению (в секундах)")]
    private float defaultTransitionDuration = 1f;

    [Header("Piece Follow Settings")]
    [SerializeField, Tooltip("Высота камеры над фигурой при следовании (постоянная)")]
    private float followHeightOffset = 2.5f; // Постоянный офсет Y
    [SerializeField, Tooltip("Офсет Z для игрока 1 (белые) относительно Z фигуры")]
    private float followZOffsetPlayer1 = -2.5f; // Офсет Z для игрока 1
    [SerializeField, Tooltip("Офсет Z для игрока 2 относительно Z фигуры")]
    private float followZOffsetPlayer2 = 2.5f; // Офсет Z для игрока 2
    [SerializeField, Tooltip("Угол поворота камеры для игрока 1 при следовании (45° наклон, 0° поворот)")]
    private Vector3 followRotationPlayer1 = new Vector3(45f, 0f, 0f);
    [SerializeField, Tooltip("Угол поворота камеры для игрока 2 при следовании (45° наклон, 180° поворот)")]
    private Vector3 followRotationPlayer2 = new Vector3(45f, 180f, 0f);
    [SerializeField, Tooltip("Размер ортографической камеры при следовании за фигурой")]
    private float followOrthographicSize = 2.5f;
    [SerializeField, Tooltip("Длительность перехода к начальной позиции следования (в секундах)")]
    private float preFollowTransitionDuration = 0.5f;
    [SerializeField, Tooltip("Скорость перемещения камеры за фигурой (в единицах за секунду)")]
    private float followSpeed = 5f; // Новая скорость интерполяции

    private Camera mainCamera; // Ссылка на компонент камеры
    private bool isPlayer1Turn = true; // Флаг текущего хода (true для игрока 1, false для игрока 2)
    private bool isFollowingPiece; // Флаг, указывающий, что камера следует за фигурой
    private Coroutine currentTransition; // Текущая корутина перехода камеры
    private int pendingAnimations; // Счетчик активных анимаций

    [Inject] private IGameManager gameManager; // Зависимость: менеджер игры для получения событий смены хода
    [Inject] private IBoardManager boardManager; // Зависимость: менеджер доски для доступа к состоянию игры

    /// <summary>
    /// Инициализирует компонент камеры и проверяет его наличие.
    /// Устанавливает камеру в ортографический режим и подписывается на события анимации.
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
    /// Устанавливает дефолтное положение камеры при старте игры и подписывается на событие смены хода.
    /// </summary>
    private void Start()
    {
        SetupDefaultCamera(instant: true); // Устанавливаем дефолтное положение мгновенно
        gameManager.OnTurnChanged += HandleTurnChanged; // Подписываемся на событие смены хода
        gameManager.OnMoveInitiated += HandleMoveInitiated; // Подписка на новое событие        
    }

    /// <summary>
    /// Отписывается от событий при уничтожении объекта.
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
    /// Устанавливает камеру в дефолтное положение (над центром доски).
    /// Если instant = true, переход мгновенный; иначе — плавный за defaultTransitionDuration.
    /// </summary>
    /// <param name="instant">Если true, камера устанавливается мгновенно.</param>
    private void SetupDefaultCamera(bool instant = false)
    {
        if (isFollowingPiece)
            return; // Не меняем положение, если камера следует за фигурой

        if (currentTransition != null)
        {
            StopCoroutine(currentTransition); // Останавливаем текущий переход
            currentTransition = null;
        }

        if (instant)
        {
            // Мгновенная установка дефолтных параметров
            transform.position = defaultPosition;
            transform.rotation = Quaternion.Euler(defaultRotation);
            mainCamera.orthographicSize = defaultOrthographicSize;
        }
        else
        {
            // Плавный переход к дефолтному положению
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
    /// Обрабатывает событие смены хода, обновляя флаг isPlayer1Turn и возвращая камеру в дефолтное положение.
    /// </summary>
    /// <param name="isPlayer1">True, если ход игрока 1; false — игрока 2.</param>
    private void HandleTurnChanged(bool isPlayer1)
    {
        isPlayer1Turn = isPlayer1;
        SetupDefaultCamera(); // Возвращаем камеру в дефолтное положение с плавным переходом
    }

    /// <summary>
    /// Обрабатывает событие начала хода, запуская подплывание камеры к фигуре.
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
    /// Увеличивает счетчик активных анимаций.
    /// </summary>
    public void IncrementPendingAnimations()
    {
        pendingAnimations++;
    }

    /// <summary>
    /// Обрабатывает событие завершения анимации, уменьшая счетчик активных анимаций.
    /// Возвращает камеру в дефолтное положение, когда все анимации завершены.
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
    /// Выполняет плавный переход камеры к целевой позиции, ротации и размеру.
    /// Используется для всех переходов камеры (дефолтное положение, следование за фигурой, полёт снаряда).
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
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration); // Плавная интерполяция
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            mainCamera.orthographicSize = Mathf.Lerp(startOrthographicSize, targetSize, t);
            yield return null;
        }

        // Устанавливаем точные конечные значения
        transform.position = targetPosition;
        transform.rotation = targetRotation;
        mainCamera.orthographicSize = targetSize;
    }

    /// <summary>
    /// Корутина для подготовки и следования камеры за фигурой во время движения или атаки.
    /// Камера следует за фигурой с динамическими офсетами Y и Z, аналогично движению за снарядом.
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

        PieceAnimationConfig config = animator.GetAnimationConfig(piece); // Конфигурация анимаций
        if (config == null)
        {
            Debug.LogError($"CameraController: No PieceAnimationConfig for {piece.Type}");
            isFollowingPiece = false;
            pendingAnimations--;
            yield break;
        }

        Vector3 piecePosition = piece.transform.position; // Начальная позиция фигуры
        Vector3 targetPosition = new Vector3(target.x, 0.5f, target.z); // Целевая позиция

        // Вычисляем начальную позицию камеры с динамическими офсетами
        float zOffset = isPlayer1Turn ? followZOffsetPlayer1 : followZOffsetPlayer2;
        Vector3 attackerCameraPosition = new Vector3(piecePosition.x, piecePosition.y + followHeightOffset, piecePosition.z + zOffset);
        Quaternion targetRotation = Quaternion.Euler(isPlayer1Turn ? followRotationPlayer1 : followRotationPlayer2);

        // Плавный переход к начальной позиции камеры
        yield return StartCoroutine(SmoothTransition(
            attackerCameraPosition,
            targetRotation,
            followOrthographicSize,
            preFollowTransitionDuration
        ));

        // Вызываем onAnimationComplete для синхронизации с анимацией фигуры
        Debug.Log($"CameraController: Processing {(isMove ? "move" : isRangedAttack ? "ranged attack" : "melee attack")} for {piece.Type}");
        onAnimationComplete?.Invoke();

        if (isRangedAttack)
        {
            // Для дальней атаки: три фазы
            float rotationAndRecoilDuration = (config?.RecoilDuration ?? 0.2f) + (config?.RotationDuration ?? 0.3f); // Время поворота и отдачи
            float cameraProjectileFlightDuration = animator.ProjectileFlightDuration + (config?.HitDuration ?? 0.2f) + (config?.DeathDuration ?? 0.5f); // Время полёта снаряда + эффекты

            // Фаза 1: Смотрим на стреляющую фигуру во время поворота и отдачи
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

            // Фаза 2: Плавно перемещаемся к цели и ждём эффекты попадания и смерти
            Vector3 targetCameraPosition = new Vector3(targetPosition.x, targetPosition.y + followHeightOffset, targetPosition.z + zOffset);
            yield return StartCoroutine(SmoothTransition(
                targetCameraPosition,
                targetRotation,
                followOrthographicSize,
                cameraProjectileFlightDuration / 2
            ));

            // Ожидаем оставшееся время для эффектов
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
            // Для перемещения или ближней атаки: следим за фигурой
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