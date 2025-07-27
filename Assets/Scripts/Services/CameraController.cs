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
    [SerializeField, Tooltip("Смещение камеры относительно фигуры при следовании")]
    private Vector3 followOffset = new Vector3(2f, 2f, 0f);

    [SerializeField, Tooltip("Угол поворота камеры для игрока 1 при следовании (45° наклон, 0° поворот)")]
    private Vector3 followRotationPlayer1 = new Vector3(45f, 0f, 0f);

    [SerializeField, Tooltip("Угол поворота камеры для игрока 2 при следовании (45° наклон, 180° поворот)")]
    private Vector3 followRotationPlayer2 = new Vector3(45f, 180f, 0f);

    [SerializeField, Tooltip("Размер ортографической камеры при следовании за фигурой")]
    private float followOrthographicSize = 2.5f;

    [SerializeField, Tooltip("Длительность перехода к начальной позиции следования (в секундах)")]
    private float preFollowTransitionDuration = 0.5f; // Увеличено для плавности

    private Camera mainCamera; // Ссылка на компонент камеры
    private bool isPlayer1Turn = true; // Флаг текущего хода (true для игрока 1, false для игрока 2)
    private bool isFollowingPiece; // Флаг, указывающий, что камера следует за фигурой
    private Coroutine currentTransition; // Текущая корутина перехода камеры

    [Inject] private IGameManager gameManager; // Зависимость: менеджер игры для получения событий смены хода
    [Inject] private IBoardManager boardManager; // Зависимость: менеджер доски для доступа к состоянию игры

    /// <summary>
    /// Инициализирует компонент камеры и проверяет его наличие.
    /// Устанавливает камеру в ортографический режим.
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
    }

    /// <summary>
    /// Отписывается от события смены хода при уничтожении объекта.
    /// </summary>
    private void OnDestroy()
    {
        if (gameManager != null)
            gameManager.OnTurnChanged -= HandleTurnChanged;
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
    /// Подготавливает камеру к следованию за фигурой во время движения или атаки.
    /// Запускает корутину для анимации камеры в зависимости от типа действия (движение, ближняя или дальняя атака).
    /// </summary>
    /// <param name="piece">Фигура, за которой следует камера.</param>
    /// <param name="target">Целевая клетка (для движения или атаки).</param>
    /// <param name="isMove">Если true, выполняется движение; иначе — атака.</param>
    /// <param name="isRangedAttack">Если true, выполняется дальняя атака; иначе — ближняя.</param>
    /// <param name="onAnimationComplete">Действие, вызываемое после завершения анимации.</param>
    public void PrepareToFollowPiece(Piece piece, Vector3Int target, bool isMove, bool isRangedAttack, Action onAnimationComplete)
    {
        if (piece.Type == PieceType.Mountain)
        {
            Debug.Log("CameraController: Ignoring animation for Mountain");
            onAnimationComplete?.Invoke(); // Горы не анимируются, сразу вызываем завершение
            return;
        }

        isFollowingPiece = true; // Устанавливаем флаг следования

        if (currentTransition != null)
        {
            StopCoroutine(currentTransition); // Останавливаем текущий переход
            currentTransition = null;
        }

        // Запускаем корутину для следования за фигурой
        currentTransition = StartCoroutine(PrepareAndFollowPiece(piece, target, isMove, isRangedAttack, onAnimationComplete));
    }

    /// <summary>
    /// Выполняет плавный переход камеры к целевой позиции, ротации и размеру.
    /// Используется для всех переходов камеры (дефолтное положение, следование за фигурой, полёт снаряда).
    /// </summary>
    /// <param name="targetPosition">Целевая позиция камеры.</param>
    /// <param name="targetRotation">Целевая ротация камеры.</param>
    /// <param name="targetSize">Целевой размер ортографической камеры.</param>
    /// <param name="duration">Длительность перехода.</param>
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
    /// Для движения и ближней атаки: следует за фигурой с учётом её траектории.
    /// Для дальней атаки: сначала смотрит на стреляющую фигуру, затем на цель, синхронизируясь с полётом и эффектами.
    /// </summary>
    /// <param name="piece">Фигура, за которой следует камера.</param>
    /// <param name="target">Целевая клетка (для движения или атаки).</param>
    /// <param name="isMove">Если true, выполняется движение; иначе — атака.</param>
    /// <param name="isRangedAttack">Если true, выполняется дальняя атака; иначе — ближняя.</param>
    /// <param name="onAnimationComplete">Действие, вызываемое после завершения анимации.</param>
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

        Vector3 piecePosition = piece.transform.position; // Начальная позиция фигуры
        Vector3 targetPosition = new Vector3(target.x, 0.5f, target.z); // Целевая позиция
        PieceAnimationConfig config = animator.GetAnimationConfig(piece); // Конфигурация анимаций

        // Определяем начальную позицию камеры над фигурой
        Vector3 attackerCameraPosition = piecePosition + (isPlayer1Turn ? followOffset : new Vector3(-followOffset.x, followOffset.y, followOffset.z));
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
            float rotationAndRecoilDuration = (config?.RecoilDuration ?? 0.2f) + animator.RotationDuration; // Время поворота и отдачи
            float cameraProjectileFlightDuration = animator.ProjectileFlightDuration + (config?.HitDuration ?? 0.2f) + (config?.DeathDuration ?? 0.5f); // Время полёта снаряда + эффекты

            // Фаза 1: Смотрим на стреляющую фигуру во время поворота и отдачи
            float elapsedTime = 0f;
            while (elapsedTime < rotationAndRecoilDuration && isFollowingPiece)
            {
                elapsedTime += Time.deltaTime;
                transform.position = attackerCameraPosition;
                transform.rotation = targetRotation;
                mainCamera.orthographicSize = followOrthographicSize;
                yield return null;
            }

            // Фаза 2: Плавно перемещаемся к цели и ждём эффекты попадания и смерти
            Vector3 targetCameraPosition = targetPosition + (isPlayer1Turn ? followOffset : new Vector3(-followOffset.x, followOffset.y, followOffset.z));
            yield return StartCoroutine(SmoothTransition(
                targetCameraPosition,
                targetRotation,
                followOrthographicSize,
                cameraProjectileFlightDuration / 2 // Половина времени на перемещение
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
            float totalAnimationDuration = animator.RotationDuration * 2 + (config?.MoveDuration ?? 0.5f); // Время анимации (два поворота + движение)
            Vector3 startPiecePosition = piecePosition;
            Vector3 endPiecePosition = targetPosition;

            float elapsedTime = 0f;
            while (elapsedTime < totalAnimationDuration && isFollowingPiece)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / totalAnimationDuration;
                Vector3 currentPiecePosition = Vector3.Lerp(startPiecePosition, endPiecePosition, t); // Интерполяция позиции
                float height = isMove ? animator.JumpHeight * Mathf.Sin(t * Mathf.PI) : 0f; // Учёт высоты прыжка
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