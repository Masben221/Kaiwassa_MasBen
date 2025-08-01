using UnityEngine;

/// <summary>
/// Конфигурация анимаций для фигур, настраиваемая через инспектор.
/// Хранит параметры для движения, поворота, ближней и дальней атаки, получения удара и смерти.
/// </summary>
[CreateAssetMenu(fileName = "PieceAnimationConfig", menuName = "Cyvasse/PieceAnimationConfig", order = 1)]
public class PieceAnimationConfig : ScriptableObject
{
    [Header("General Animation Settings")]
    [SerializeField, Tooltip("Длительность поворота фигуры (в секундах)")]
    private float rotationDuration = 0.3f; // НОВОЕ ПОЛЕ: Длительность поворота

    [Header("Movement Animation Settings")]
    [SerializeField, Tooltip("Длительность анимации движения фигуры (в секундах)")]
    private float moveDuration = 0.5f;
    [SerializeField, Tooltip("Высота прыжка при перемещении и атаке")]
    private float jumpHeight = 1f; // НОВОЕ ПОЛЕ: Высота прыжка

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
    private float meleeWeaponEffectTiming = 0.5f;
    [SerializeField, Tooltip("Длительность паузы в пике прыжка при ближней атаке (в секундах, 0 = без паузы)")]
    private float jumpPeakPauseDuration = 0f;

    [Header("Ranged Attack Settings")]
    [SerializeField, Tooltip("Длительность анимации дальней атаки (полёт снаряда)")]
    private float rangedAttackDuration = 0.5f;
    [SerializeField, Tooltip("Амплитуда отдачи фигуры при выстреле")]
    private float recoilDistance = 0.1f;
    [SerializeField, Tooltip("Длительность отдачи фигуры при выстреле")]
    private float recoilDuration = 0.2f;
    [SerializeField, Tooltip("Префаб модельки снаряда для дальней атаки (зависит от типа фигуры)")]
    private GameObject projectileModelPrefab;
    [SerializeField, Tooltip("Высота дуги для параболического полёта снаряда")]
    private float projectileArcHeight = 1f; // НОВОЕ ПОЛЕ: Высота дуги снаряда

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
    /// Длительность поворота фигуры.
    /// </summary>
    public float RotationDuration => rotationDuration;
    public float MoveDuration => moveDuration;
    public float JumpHeight => jumpHeight;
    public float MeleeAttackDuration => meleeAttackDuration;
    public float MeleePunchDistance => meleePunchDistance;
    public GameObject HitEffectPrefab => hitEffectPrefab;
    public GameObject MeleeWeaponEffectPrefab => meleeWeaponEffectPrefab;
    public float MeleeWeaponEffectTiming => meleeWeaponEffectTiming;
    public float JumpPeakPauseDuration => jumpPeakPauseDuration;
    public float RangedAttackDuration => rangedAttackDuration;
    public float RecoilDistance => recoilDistance;
    public float RecoilDuration => recoilDuration;
    public GameObject ProjectileModelPrefab => projectileModelPrefab;
    public float ProjectileArcHeight => projectileArcHeight;
    public float HitDuration => hitDuration;
    public float HitPunchDistance => hitPunchDistance;
    public float DeathDuration => deathDuration;
    public GameObject DeathEffectPrefab => deathEffectPrefab;
}