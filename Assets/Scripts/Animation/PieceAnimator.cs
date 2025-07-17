using UnityEngine;
using System;
using System.Collections;
using DG.Tweening;

public class PieceAnimator : MonoBehaviour
{
    [SerializeField, Tooltip("Длительность анимации перемещения")] private float moveDuration = 0.5f;
    [SerializeField, Tooltip("Высота прыжка при перемещении")] private float jumpHeight = 1f;
    [SerializeField, Tooltip("Длительность поворота фигуры")] private float rotationDuration = 0.3f;
    [SerializeField, Tooltip("Конфигурация анимаций для фигуры")] private PieceAnimationConfig animationConfig;

    private bool isAnimating;

    public float MoveDuration => moveDuration;
    public float JumpHeight => jumpHeight;
    public float RotationDuration => rotationDuration;

    public static event Action<Piece> OnAnimationStarted;
    public static event Action<Piece> OnAnimationFinished;

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
        OnAnimationStarted?.Invoke(piece);

        StartCoroutine(AnimateMoveAndRotate(piece, targetPos, animationTarget, onComplete));
    }

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
        OnAnimationStarted?.Invoke(piece);

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
        OnAnimationStarted?.Invoke(piece);

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
        OnAnimationStarted?.Invoke(piece);

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
        OnAnimationStarted?.Invoke(piece);

        StartCoroutine(AnimateDeathCoroutine(piece, onComplete));
    }

    private IEnumerator AnimateMoveAndRotate(Piece piece, Vector3Int targetPos, Vector3Int animationTarget, Action onComplete)
    {
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
        if (Vector3.Distance(startPos, endPos) > 0.1f)
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
        else
        {
            yield return new WaitForSeconds(moveDuration);
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
        OnAnimationFinished?.Invoke(piece);
        onComplete?.Invoke();
        Debug.Log($"PieceAnimator: Animation completed for {piece.Type} to target {targetPos}");
    }

    private IEnumerator AnimateMeleeAttackCoroutine(Piece piece, Vector3Int targetPos, Action onComplete)
    {
        PieceAnimationConfig config = GetAnimationConfig(piece);
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(targetPos.x, 0.5f, targetPos.z);
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
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / moveDuration);
            float height = jumpHeight * Mathf.Sin(t * Mathf.PI);
            transform.position = Vector3.Lerp(startPos, endPos, t) + new Vector3(0, height, 0);
            yield return null;
        }
        transform.position = endPos;

        if (config != null)
        {
            Vector3 punchDirection = direction.normalized * config.MeleePunchDistance;
            transform.DOPunchPosition(punchDirection, config.MeleeAttackDuration, 10, 1f)
                .SetEase(Ease.InOutSine);

            if (config.HitEffectPrefab != null)
            {
                ParticleSystem hitEffect = Instantiate(config.HitEffectPrefab, endPos + Vector3.up * 0.5f, Quaternion.identity);
                hitEffect.Play();
                Destroy(hitEffect.gameObject, hitEffect.main.duration);

                Piece targetPiece = FindObjectOfType<BoardManager>().GetPieceAt(targetPos);
                if (targetPiece != null)
                {
                    PieceAnimator targetAnimator = targetPiece.GetComponent<PieceAnimator>();
                    if (targetAnimator != null)
                    {
                        targetAnimator.AnimateHitAndDeath(false, null, -direction.normalized);
                    }
                }
            }

            yield return new WaitForSeconds(config.MeleeAttackDuration);
        }
        else
        {
            yield return new WaitForSeconds(0.3f);
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
        OnAnimationFinished?.Invoke(piece);
        onComplete?.Invoke();
        Debug.Log($"PieceAnimator: Melee attack animation completed for {piece.Type} to target {targetPos}");
    }

    private IEnumerator AnimateRangedAttackCoroutine(Piece piece, Vector3Int targetPos, Action onComplete)
    {
        PieceAnimationConfig config = GetAnimationConfig(piece);
        Vector3 startPos = transform.position;
        Quaternion startRotation = transform.rotation;
        Quaternion initialRotation = piece.InitialRotation;

        Vector3 direction = new Vector3(targetPos.x - piece.Position.x, 0, targetPos.z - piece.Position.z).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

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
                // Создание с фиксированным локальным rotation.x = 90 градусов
                GameObject projectile = Instantiate(config.ProjectileModelPrefab, startPos + direction * 0.2f + Vector3.up * 0.5f, Quaternion.Euler(90f, 0f, 0f));

                // Прямолинейное движение к цели
                projectile.transform.DOMove(targetWorldPos, config.RangedAttackDuration)
                    .SetEase(Ease.Linear)
                    .OnUpdate(() =>
                    {
                    // Поворот только вокруг глобальной Y-оси по направлению к цели
                    Vector3 targetDirection = (targetWorldPos - projectile.transform.position).normalized;
                        float targetYRotation = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;
                        Quaternion yRotation = Quaternion.Euler(90f, targetYRotation, 0f); // Фиксируем X на 90, меняем только Y
                    projectile.transform.rotation = yRotation;
                    })
                    .OnComplete(() =>
                    {
                        if (config.HitEffectPrefab != null)
                        {
                            ParticleSystem hitEffect = Instantiate(config.HitEffectPrefab, targetWorldPos, Quaternion.identity);
                            hitEffect.Play();
                            Destroy(hitEffect.gameObject, hitEffect.main.duration);

                            Piece targetPiece = FindObjectOfType<BoardManager>().GetPieceAt(targetPos);
                            if (targetPiece != null)
                            {
                                PieceAnimator targetAnimator = targetPiece.GetComponent<PieceAnimator>();
                                if (targetAnimator != null)
                                {
                                    targetAnimator.AnimateHitAndDeath(false, null, -direction);
                                }
                            }
                        }
                        Destroy(projectile);
                    });
            }

            yield return new WaitForSeconds(config.RangedAttackDuration);
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

        isAnimating = false;
        OnAnimationFinished?.Invoke(piece);
        onComplete?.Invoke();
        Debug.Log($"PieceAnimator: Ranged attack animation completed for {piece.Type} to target {targetPos}");
    }

    private IEnumerator AnimateHitAndDeathCoroutine(Piece piece, bool isDeath, Vector3? hitDirection, Action onComplete)
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
                ParticleSystem hitEffect = Instantiate(config.HitEffectPrefab, transform.position, Quaternion.identity);
                hitEffect.Play();
                Destroy(hitEffect.gameObject, hitEffect.main.duration);
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
                    ParticleSystem deathEffect = Instantiate(config.DeathEffectPrefab, transform.position, Quaternion.identity);
                    deathEffect.Play();
                    Destroy(deathEffect.gameObject, deathEffect.main.duration);
                }

                yield return new WaitForSeconds(config.DeathDuration);
            }
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        isAnimating = false;
        OnAnimationFinished?.Invoke(piece);
        onComplete?.Invoke();
        Debug.Log($"PieceAnimator: {(isDeath ? "Death" : "Hit")} animation completed for {piece.Type}");
    }

    private IEnumerator AnimateDeathCoroutine(Piece piece, Action onComplete)
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
                ParticleSystem deathEffect = Instantiate(config.DeathEffectPrefab, transform.position, Quaternion.identity);
                deathEffect.Play();
                Destroy(deathEffect.gameObject, deathEffect.main.duration);
            }

            yield return new WaitForSeconds(config.DeathDuration);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        isAnimating = false;
        OnAnimationFinished?.Invoke(piece);
        onComplete?.Invoke();
        Debug.Log($"PieceAnimator: Death animation completed for {piece.Type}");
    }

    public PieceAnimationConfig GetAnimationConfig(Piece piece)
    {
        if (animationConfig != null)
        {
            return animationConfig;
        }

        PieceFactory factory = FindObjectOfType<PieceFactory>();
        if (factory == null)
        {
            Debug.LogError("PieceAnimator: PieceFactory not found in scene!");
            return null;
        }

        PieceAnimationConfig defaultConfig = factory.GetDefaultAnimationConfig();
        if (defaultConfig == null)
        {
            Debug.LogWarning($"PieceAnimator: No animation config assigned for {piece.Type} and no default config in PieceFactory");
        }
        return defaultConfig;
    }
}