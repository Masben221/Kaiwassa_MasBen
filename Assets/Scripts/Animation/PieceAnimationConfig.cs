using UnityEngine;

/// <summary>
/// Конфигурация анимаций для фигур, настраиваемая через инспектор.
/// Хранит параметры для ближней атаки, дальней атаки, получения удара, смерти и эффекта оружия.
/// </summary>
[CreateAssetMenu(fileName = "PieceAnimationConfig", menuName = "Cyvasse/PieceAnimationConfig", order = 1)]
public class PieceAnimationConfig : ScriptableObject
{
    [Header("Movement Animation Settings")]
    [SerializeField, Tooltip("Длительность анимации движения фигуры (в секундах)")]
    private float moveDuration = 0.5f;
    [SerializeField, Tooltip("Длительность паузы в пике прыжка при движении (в секундах, 0 = без паузы)")]
    private float jumpPeakPauseDuration = 0f; // НОВОЕ ПОЛЕ: Пауза в пике прыжка

    [Header("Melee Attack Settings")]
    [SerializeField, Tooltip("Длительность анимации ближней атаки (рывок вперёд и назад)")]
    private float meleeAttackDuration = 0.3f;
    [SerializeField, Tooltip("Амплитуда рывка для ближней атаки")]
    private float meleePunchDistance = 0.2f;
    [SerializeField, Tooltip("Префаб объекта для эффекта попадания при ближней атаке")]
    private GameObject hitEffectPrefab;
    [SerializeField, Tooltip("Префаб объекта для эффекта оружия при ближней атаке (например, огонь дракона)")]
    private GameObject meleeWeaponEffectPrefab;
    [SerializeField, Range(0f, 1f), Tooltip("Момент срабатывания эффекта оружия относительно длительности движения (0 = начало движения, 0.5 = пик прыжка, 1 = конец движения)")]
    private float meleeWeaponEffectTiming = 0.5f; // НОВОЕ ПОЛЕ: Процентная задержка эффекта

    [Header("Ranged Attack Settings")]
    [SerializeField, Tooltip("Длительность анимации дальней атаки (полёт снаряда)")]
    private float rangedAttackDuration = 0.5f;
    [SerializeField, Tooltip("Амплитуда отдачи фигуры при выстреле")]
    private float recoilDistance = 0.1f;
    [SerializeField, Tooltip("Длительность отдачи фигуры при выстреле")]
    private float recoilDuration = 0.2f;
    [SerializeField, Tooltip("Префаб модельки снаряда для дальней атаки (зависит от типа фигуры)")]
    private GameObject projectileModelPrefab;

    [Header("Hit and Death Settings")]
    [SerializeField, Tooltip("Длительность анимации получения удара (отбрасывание)")]
    private float hitDuration = 0.2f;
    [SerializeField, Tooltip("Амплитуда отбрасывания при получении удара")]
    private float hitPunchDistance = 0.15f;
    [SerializeField, Tooltip("Длительность анимации смерти (растворение/падение)")]
    private float deathDuration = 0.5f;
    [SerializeField, Tooltip("Префаб объекта для эффекта смерти")]
    private GameObject deathEffectPrefab;

    /// <summary>
    /// Длительность анимации движения.
    /// </summary>
    public float MoveDuration => moveDuration;
    public float JumpPeakPauseDuration => jumpPeakPauseDuration; // НОВОЕ СВОЙСТВО: Пауза в пике
    public float MeleeAttackDuration => meleeAttackDuration;
    public float MeleePunchDistance => meleePunchDistance;
    public GameObject HitEffectPrefab => hitEffectPrefab;
    public GameObject MeleeWeaponEffectPrefab => meleeWeaponEffectPrefab;
    public float MeleeWeaponEffectTiming => meleeWeaponEffectTiming; // НОВОЕ СВОЙСТВО: Процентная задержка
    public float RangedAttackDuration => rangedAttackDuration;
    public float RecoilDistance => recoilDistance;
    public float RecoilDuration => recoilDuration;
    public GameObject ProjectileModelPrefab => projectileModelPrefab;
    public float HitDuration => hitDuration;
    public float HitPunchDistance => hitPunchDistance;
    public float DeathDuration => deathDuration;
    public GameObject DeathEffectPrefab => deathEffectPrefab;
}