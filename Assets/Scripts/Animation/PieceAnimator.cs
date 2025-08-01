using UnityEngine;
using System;
using System.Collections;
using DG.Tweening;
using Zenject;

public class PieceAnimator : MonoBehaviour
{
    [Inject] private IBoardManager boardManager;
    [Inject] private IPieceFactory pieceFactory; // Фабрика для создания фигур

    [SerializeField, Tooltip("Высота прыжка при перемещении")] private float jumpHeight = 1f;
    [SerializeField, Tooltip("Длительность поворота фигуры")] private float rotationDuration = 0.3f;
    [SerializeField, Tooltip("Конфигурация анимаций для фигуры")] private PieceAnimationConfig animationConfig;
    [SerializeField, Tooltip("Высота дуги для параболического полёта снаряда")] private float projectileArcHeight = 1f;

    public float ProjectileFlightDuration => animationConfig?.RangedAttackDuration ?? 0.5f;

    private bool isAnimating;

    public float JumpHeight => jumpHeight;
    public float RotationDuration => rotationDuration;

    public static event Action<Piece, Vector3Int, bool, bool> OnAnimationStarted; // Piece, target, isMove, isRangedAttack
    public static event Action<GameObject> OnProjectileFlying; // Projectile GameObject
    public static event Action<Piece> OnAnimationCompleted; // Piece

    public static event Action<Piece> OnAnimationStartedLegacy; // Для обратной совместимости
    public static event Action<Piece> OnAnimationFinishedLegacy; // Для обратной совместимости

    private void Awake()
    {
        if (animationConfig == null)
        {
            Debug.LogWarning($"PieceAnimator: AnimationConfig not assigned on {gameObject.name}, will use default from PieceFactory");
        }
    }

    public void MoveTo(Vector3Int targetPos, Vector3Int animationTarget, Action onStart, Action onComplete)
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
        OnAnimationStarted?.Invoke(piece, targetPos, true, false);
        OnAnimationStartedLegacy?.Invoke(piece);

        StartCoroutine(AnimateMoveAndRotate(piece, targetPos, animationTarget, onComplete));
    }

    /// <summary>
    /// Запускает анимацию ближней атаки для фигуры.
    /// </summary>
    /// <param name="targetPos">Целевая клетка для атаки.</param>
    /// <param name="onComplete">Действие после завершения анимации.</param>
    public void AnimateMeleeAttack(Vector3Int targetPos, Action onComplete)
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

        isAnimating = true;
        OnAnimationStarted?.Invoke(piece, targetPos, false, false);
        OnAnimationStartedLegacy?.Invoke(piece);

        StartCoroutine(AnimateMeleeAttackCoroutine(piece, targetPos, onComplete));
    }

    public void AnimateRangedAttack(Vector3Int targetPos, Action onComplete)
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

        isAnimating = true;
        OnAnimationStarted?.Invoke(piece, targetPos, false, true);
        OnAnimationStartedLegacy?.Invoke(piece);

        StartCoroutine(AnimateRangedAttackCoroutine(piece, targetPos, onComplete));
    }

    public void AnimateHitAndDeath(bool isDeath, Action onComplete, Vector3? hitDirection = null)
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
            Debug.Log($"PieceAnimator: Cannot animate Mountain at {piece.Position}");
            onComplete?.Invoke();
            return;
        }

        isAnimating = true;
        OnAnimationStartedLegacy?.Invoke(piece);

        StartCoroutine(AnimateHitAndDeathCoroutine(piece, isDeath, hitDirection, onComplete));
    }

    public void AnimateDeath(Action onComplete)
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
            Debug.Log($"PieceAnimator: Cannot animate Mountain at {piece.Position}");
            onComplete?.Invoke();
            return;
        }

        isAnimating = true;
        OnAnimationStartedLegacy?.Invoke(piece);

        StartCoroutine(AnimateDeathCoroutine(piece, onComplete));
    }

    private IEnumerator AnimateMoveAndRotate(Piece piece, Vector3Int targetPos, Vector3Int animationTarget, Action onComplete)
    {
        PieceAnimationConfig config = GetAnimationConfig(piece);
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(animationTarget.x, 0.5f, animationTarget.z);
        Quaternion startRotation = transform.rotation;
        Quaternion initialRotation = piece.InitialRotation;

        Vector3 direction = new Vector3(targetPos.x - piece.Position.x, 0, targetPos.z - piece.Position.z);
        Quaternion targetRotation = direction.sqrMagnitude > 0.01f ? Quaternion.LookRotation(direction, Vector3.up) : startRotation;

        float elapsedTime = 0f;
        while (elapsedTime < rotationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / rotationDuration);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }
        transform.rotation = targetRotation;

        elapsedTime = 0f;
        float moveDuration = config?.MoveDuration ?? 0.5f;
        float peakPause = config?.JumpPeakPauseDuration ?? 0f;
        if (Vector3.Distance(startPos, endPos) > 0.1f)
        {
            while (elapsedTime < moveDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / moveDuration);
                float adjustedT = t;

                // Пауза в пике прыжка (t = 0.5)
                if (peakPause > 0f && t >= 0.5f && elapsedTime < (moveDuration * 0.5f + peakPause))
                {
                    adjustedT = 0.5f; // Замораживаем интерполяцию на пике
                }
                else if (peakPause > 0f && t > 0.5f)
                {
                    // Смещаем t после паузы
                    float timeAfterPause = elapsedTime - (moveDuration * 0.5f + peakPause);
                    adjustedT = Mathf.Clamp01(0.5f + timeAfterPause / (moveDuration * 0.5f));
                }

                float height = jumpHeight * Mathf.Sin(adjustedT * Mathf.PI);
                transform.position = Vector3.Lerp(startPos, endPos, adjustedT) + new Vector3(0, height, 0);
                yield return null;
            }
            transform.position = endPos;
        }
        else
        {
            yield return new WaitForSeconds(moveDuration + peakPause);
        }

        elapsedTime = 0f;
        while (elapsedTime < rotationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / rotationDuration);
            transform.rotation = Quaternion.Slerp(targetRotation, initialRotation, t);
            yield return null;
        }
        transform.rotation = initialRotation;

        isAnimating = false;
        OnAnimationCompleted?.Invoke(piece);
        OnAnimationFinishedLegacy?.Invoke(piece);
        onComplete?.Invoke();
        Debug.Log($"PieceAnimator: Animation completed for {piece.Type} to target {targetPos}");
    }

    /// <summary>
    /// Корутина для анимации ближней атаки.
    /// Поворачивает фигуру к цели, выполняет движение к цели с фиксированной длительностью,
    /// с паузой в пике прыжка, создаёт эффект оружия с заданной процентной задержкой от длительности движения,
    /// выполняет рывок атаки, эффект попадания, анимацию попадания для целевой фигуры, затем возвращает исходную ротацию.
    /// </summary>
    private IEnumerator AnimateMeleeAttackCoroutine(Piece piece, Vector3Int targetPos, Action onComplete)
    {
        try
        {
            PieceAnimationConfig config = GetAnimationConfig(piece);
            Vector3 startPos = transform.position;
            Vector3 endPos = new Vector3(targetPos.x, 0.5f, targetPos.z);
            Quaternion startRotation = transform.rotation;
            Quaternion initialRotation = piece.InitialRotation;

            // Рассчитываем фиксированную длительность движения
            float moveDurationAdjusted = config?.MoveDuration ?? 0.5f;
            float peakPause = config?.JumpPeakPauseDuration ?? 0f;

            // Запуск корутины для создания эффекта оружия с процентной задержкой от длительности движения
            if (config?.MeleeWeaponEffectPrefab != null)
            {
                float effectDelay = moveDurationAdjusted * Mathf.Clamp01(config.MeleeWeaponEffectTiming);
                StartCoroutine(CreateWeaponEffectWithDelay(piece, effectDelay));
            }

            // Поворот к цели
            Vector3 direction = new Vector3(targetPos.x - piece.Position.x, 0, targetPos.z - piece.Position.z);
            Quaternion targetRotation = direction.sqrMagnitude > 0.01f ? Quaternion.LookRotation(direction, Vector3.up) : startRotation;

            float elapsedTime = 0f;
            while (elapsedTime < rotationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / rotationDuration);
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                yield return null;
            }
            transform.rotation = targetRotation;

            // Движение к цели с паузой в пике
            elapsedTime = 0f;
            if (Vector3.Distance(startPos, endPos) > 0.1f)
            {
                while (elapsedTime < moveDurationAdjusted)
                {
                    elapsedTime += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsedTime / moveDurationAdjusted);
                    float adjustedT = t;

                    // Пауза в пике прыжка (t = 0.5)
                    if (peakPause > 0f && t >= 0.5f && elapsedTime < (moveDurationAdjusted * 0.5f + peakPause))
                    {
                        adjustedT = 0.5f; // Замораживаем интерполяцию на пике
                    }
                    else if (peakPause > 0f && t > 0.5f)
                    {
                        // Смещаем t после паузы
                        float timeAfterPause = elapsedTime - (moveDurationAdjusted * 0.5f + peakPause);
                        adjustedT = Mathf.Clamp01(0.5f + timeAfterPause / (moveDurationAdjusted * 0.5f));
                    }

                    float height = jumpHeight * Mathf.Sin(adjustedT * Mathf.PI);
                    transform.position = Vector3.Lerp(startPos, endPos, adjustedT) + new Vector3(0, height, 0);
                    yield return null;
                }
                transform.position = endPos;
            }
            else
            {
                yield return new WaitForSeconds(moveDurationAdjusted + peakPause);
            }

            // Анимация атаки и эффекты
            if (config != null)
            {
                Vector3 punchDirection = direction.normalized * config.MeleePunchDistance;
                transform.DOPunchPosition(punchDirection, config.MeleeAttackDuration, 10, 1f)
                    .SetEase(Ease.InOutSine);

                // Эффект попадания
                if (config.HitEffectPrefab != null)
                {
                    GameObject hitEffect = Instantiate(config.HitEffectPrefab, endPos + Vector3.up * 0.5f, Quaternion.identity);
                    ImpactEffect impactEffect = hitEffect.GetComponent<ImpactEffect>();
                    if (impactEffect == null)
                    {
                        Debug.LogWarning($"PieceAnimator: HitEffectPrefab on {piece.Type} does not have ImpactEffect component!");
                        Destroy(hitEffect);
                    }

                    Piece targetPiece = boardManager.GetPieceAt(targetPos);
                    if (targetPiece != null)
                    {
                        PieceAnimator targetAnimator = targetPiece.GetComponent<PieceAnimator>();
                        if (targetAnimator != null)
                        {
                            targetAnimator.AnimateHitAndDeath(false, null, -direction.normalized);
                        }
                    }
                }

                // Задержка после анимации атаки для синхронизации
                yield return new WaitForSeconds(config.MeleeAttackDuration);
            }
            else
            {
                yield return new WaitForSeconds(0.3f);
            }

            // Возврат исходной ротации
            elapsedTime = 0f;
            while (elapsedTime < rotationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / rotationDuration);
                transform.rotation = Quaternion.Slerp(targetRotation, initialRotation, t);
                yield return null;
            }
            transform.rotation = initialRotation;
        }
        finally
        {
            isAnimating = false;
            OnAnimationCompleted?.Invoke(piece);
            OnAnimationFinishedLegacy?.Invoke(piece);
            onComplete?.Invoke();
            Debug.Log($"PieceAnimator: Melee attack animation completed for {piece.Type} to target {targetPos}");
        }
    }

    /// <summary>
    /// Корутина для создания эффекта оружия с заданной задержкой от начала движения.
    /// </summary>
    private IEnumerator CreateWeaponEffectWithDelay(Piece piece, float delay)
    {
        yield return new WaitForSeconds(delay);

        PieceAnimationConfig config = GetAnimationConfig(piece);
        if (config.MeleeWeaponEffectPrefab != null)
        {
            // Смещение задаётся в префабе (например, для пасти дракона)
            GameObject weaponEffect = Instantiate(config.MeleeWeaponEffectPrefab, transform);
            ImpactEffect impactEffect = weaponEffect.GetComponent<ImpactEffect>();
            if (impactEffect == null)
            {
                Debug.LogWarning($"PieceAnimator: MeleeWeaponEffectPrefab on {piece.Type} does not have ImpactEffect component!");
                Destroy(weaponEffect);
            }
            else
            {
                Debug.Log($"PieceAnimator: Created melee weapon effect for {piece.Type} as child of {gameObject.name} after {delay}s delay");
            }
        }
    }

    private IEnumerator AnimateRangedAttackCoroutine(Piece piece, Vector3Int targetPos, Action onComplete)
    {
        try
        {
            PieceAnimationConfig config = GetAnimationConfig(piece);
            Vector3 startPos = transform.position;
            Quaternion startRotation = transform.rotation;
            Quaternion initialRotation = piece.InitialRotation;

            Vector3 direction = new Vector3(targetPos.x - piece.Position.x, 0, targetPos.z - piece.Position.z).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            float distance = Vector3.Distance(startPos, new Vector3(targetPos.x, 0.5f, targetPos.z));

            float theta = Mathf.Atan2(4f * projectileArcHeight, distance) * Mathf.Rad2Deg;
            float startPitch = 90f - theta;
            float endPitch = 90f + theta;

            float elapsedTime = 0f;
            while (elapsedTime < rotationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / rotationDuration);
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                yield return null;
            }
            transform.rotation = targetRotation;

            if (config != null)
            {
                Vector3 recoilDirection = -direction * config.RecoilDistance;
                transform.DOPunchPosition(recoilDirection, config.RecoilDuration, 10, 1f)
                    .SetEase(Ease.InOutSine);

                if (config.ProjectileModelPrefab != null)
                {
                    Vector3 targetWorldPos = new Vector3(targetPos.x, 0.5f, targetPos.z);
                    Vector3 launchOffset = direction * 0.2f + Vector3.up * 0.5f;
                    GameObject projectile = Instantiate(
                        config.ProjectileModelPrefab,
                        startPos + launchOffset,
                        Quaternion.Euler(startPitch, 0f, 0f)
                    );

                    Vector3 midPoint = (startPos + targetWorldPos) * 0.5f + Vector3.up * projectileArcHeight;
                    Vector3[] path = { startPos + launchOffset, midPoint, targetWorldPos };

                    float flightElapsed = 0f;

                    projectile.transform.DOPath(path, ProjectileFlightDuration, PathType.CatmullRom)
                        .SetEase(Ease.Linear)
                        .OnStart(() => OnProjectileFlying?.Invoke(projectile))
                        .OnUpdate(() =>
                        {
                            flightElapsed += Time.deltaTime;
                            float progress = Mathf.Clamp01(flightElapsed / ProjectileFlightDuration);
                            float parabola = 4f * progress * (1f - progress);
                            float pitch;
                            if (progress < 0.5f)
                                pitch = Mathf.Lerp(startPitch, 90f, progress * 2f);
                            else
                                pitch = Mathf.Lerp(90f, endPitch, (progress - 0.5f) * 2f);

                            Vector3 dirFlat = (targetWorldPos - projectile.transform.position);
                            dirFlat.y = 0f;
                            float yaw = dirFlat.sqrMagnitude > 0.001f
                                ? Mathf.Atan2(dirFlat.x, dirFlat.z) * Mathf.Rad2Deg
                                : projectile.transform.eulerAngles.y;

                            projectile.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
                        })
                        .OnComplete(() =>
                        {
                            if (config.HitEffectPrefab != null)
                            {
                                GameObject hitEffect = Instantiate(config.HitEffectPrefab, targetWorldPos, Quaternion.identity);
                                ImpactEffect impactEffect = hitEffect.GetComponent<ImpactEffect>();
                                if (impactEffect == null)
                                {
                                    Debug.LogWarning($"PieceAnimator: HitEffectPrefab on {piece.Type} does not have ImpactEffect component!");
                                    Destroy(hitEffect);
                                }

                                Piece targetPiece = boardManager?.GetPieceAt(targetPos);
                                if (targetPiece != null)
                                {
                                    PieceAnimator targetAnimator = targetPiece.GetComponent<PieceAnimator>();
                                    targetAnimator?.AnimateHitAndDeath(false, null, -direction);
                                }
                            }
                            Destroy(projectile);
                        });
                }

                yield return new WaitForSeconds(ProjectileFlightDuration);
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }

            elapsedTime = 0f;
            while (elapsedTime < rotationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / rotationDuration);
                transform.rotation = Quaternion.Slerp(targetRotation, initialRotation, t);
                yield return null;
            }
            transform.rotation = initialRotation;
        }
        finally
        {
            isAnimating = false;
            OnAnimationCompleted?.Invoke(piece);
            OnAnimationFinishedLegacy?.Invoke(piece);
            onComplete?.Invoke();
        }
    }

    private IEnumerator AnimateHitAndDeathCoroutine(Piece piece, bool isDeath, Vector3? hitDirection, Action onComplete)
    {
        try
        {
            PieceAnimationConfig config = GetAnimationConfig(piece);

            if (config != null)
            {
                if (hitDirection.HasValue)
                {
                    Vector3 punchDirection = hitDirection.Value * config.HitPunchDistance;
                    transform.DOPunchPosition(punchDirection, config.HitDuration, 10, 1f)
                        .SetEase(Ease.InOutSine);
                }
                else
                {
                    transform.DOShakePosition(config.HitDuration, config.HitPunchDistance, 10, 90f, false)
                        .SetEase(Ease.InOutSine);
                }

                if (config.HitEffectPrefab != null)
                {
                    GameObject hitEffect = Instantiate(config.HitEffectPrefab, transform.position, Quaternion.identity);
                    ImpactEffect impactEffect = hitEffect.GetComponent<ImpactEffect>();
                    if (impactEffect == null)
                    {
                        Debug.LogWarning($"PieceAnimator: HitEffectPrefab on {piece.Type} does not have ImpactEffect component!");
                        Destroy(hitEffect);
                    }
                }

                yield return new WaitForSeconds(config.HitDuration);

                if (isDeath)
                {
                    Renderer renderer = GetComponentInChildren<Renderer>();
                    if (renderer != null && renderer.material != null)
                    {
                        Material mat = renderer.material;
                        mat.DOFade(0f, config.DeathDuration).SetEase(Ease.InOutSine);
                    }
                    transform.DOScale(Vector3.zero, config.DeathDuration).SetEase(Ease.InOutSine);

                    if (config.DeathEffectPrefab != null)
                    {
                        GameObject deathEffect = Instantiate(config.DeathEffectPrefab, transform.position, Quaternion.identity);
                        ImpactEffect impactEffect = deathEffect.GetComponent<ImpactEffect>();
                        if (impactEffect == null)
                        {
                            Debug.LogWarning($"PieceAnimator: DeathEffectPrefab on {piece.Type} does not have ImpactEffect component!");
                            Destroy(deathEffect);
                        }
                    }

                    yield return new WaitForSeconds(config.DeathDuration);
                }
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
        finally
        {
            isAnimating = false;
            OnAnimationCompleted?.Invoke(piece);
            OnAnimationFinishedLegacy?.Invoke(piece);
            onComplete?.Invoke();
            Debug.Log($"PieceAnimator: {(isDeath ? "Death" : "Hit")} animation completed for {piece.Type}");
        }
    }

    private IEnumerator AnimateDeathCoroutine(Piece piece, Action onComplete)
    {
        try
        {
            PieceAnimationConfig config = GetAnimationConfig(piece);

            if (config != null)
            {
                Renderer renderer = GetComponentInChildren<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    Material mat = renderer.material;
                    mat.DOFade(0f, config.DeathDuration).SetEase(Ease.InOutSine);
                }
                transform.DOScale(Vector3.zero, config.DeathDuration).SetEase(Ease.InOutSine);

                if (config.DeathEffectPrefab != null)
                {
                    GameObject deathEffect = Instantiate(config.DeathEffectPrefab, transform.position, Quaternion.identity);
                    ImpactEffect impactEffect = deathEffect.GetComponent<ImpactEffect>();
                    if (impactEffect == null)
                    {
                        Debug.LogWarning($"PieceAnimator: DeathEffectPrefab on {piece.Type} does not have ImpactEffect component!");
                        Destroy(deathEffect);
                    }
                }

                yield return new WaitForSeconds(config.DeathDuration);
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
        finally
        {
            isAnimating = false;
            OnAnimationCompleted?.Invoke(piece);
            OnAnimationFinishedLegacy?.Invoke(piece);
            onComplete?.Invoke();
            Debug.Log($"PieceAnimator: Death animation completed for {piece.Type}");
        }
    }

    public PieceAnimationConfig GetAnimationConfig(Piece piece)
    {
        if (animationConfig != null)
        {
            return animationConfig;
        }

        if (pieceFactory == null)
        {
            Debug.LogError("PieceAnimator: PieceFactory not found in scene!");
            return null;
        }

        PieceAnimationConfig defaultConfig = pieceFactory.GetDefaultAnimationConfig();
        if (defaultConfig == null)
        {
            Debug.LogWarning($"PieceAnimator: No animation config assigned for {piece.Type} and no default config in PieceFactory");
        }
        return defaultConfig;
    }
}