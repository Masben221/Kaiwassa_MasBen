using UnityEngine;

/// <summary>
/// ������������ �������� ��� �����, ������������� ����� ���������.
/// ������ ��������� ��� ������� �����, ������� �����, ��������� �����, ������ � ������� ������.
/// </summary>
[CreateAssetMenu(fileName = "PieceAnimationConfig", menuName = "Cyvasse/PieceAnimationConfig", order = 1)]
public class PieceAnimationConfig : ScriptableObject
{
    [Header("Movement Animation Settings")]
    [SerializeField, Tooltip("������������ �������� �������� ������ (� ��������)")]
    private float moveDuration = 0.5f;
    [SerializeField, Tooltip("������������ ����� � ���� ������ ��� �������� (� ��������, 0 = ��� �����)")]
    private float jumpPeakPauseDuration = 0f; // ����� ����: ����� � ���� ������

    [Header("Melee Attack Settings")]
    [SerializeField, Tooltip("������������ �������� ������� ����� (����� ����� � �����)")]
    private float meleeAttackDuration = 0.3f;
    [SerializeField, Tooltip("��������� ����� ��� ������� �����")]
    private float meleePunchDistance = 0.2f;
    [SerializeField, Tooltip("������ ������� ��� ������� ��������� ��� ������� �����")]
    private GameObject hitEffectPrefab;
    [SerializeField, Tooltip("������ ������� ��� ������� ������ ��� ������� ����� (��������, ����� �������)")]
    private GameObject meleeWeaponEffectPrefab;
    [SerializeField, Range(0f, 1f), Tooltip("������ ������������ ������� ������ ������������ ������������ �������� (0 = ������ ��������, 0.5 = ��� ������, 1 = ����� ��������)")]
    private float meleeWeaponEffectTiming = 0.5f; // ����� ����: ���������� �������� �������

    [Header("Ranged Attack Settings")]
    [SerializeField, Tooltip("������������ �������� ������� ����� (���� �������)")]
    private float rangedAttackDuration = 0.5f;
    [SerializeField, Tooltip("��������� ������ ������ ��� ��������")]
    private float recoilDistance = 0.1f;
    [SerializeField, Tooltip("������������ ������ ������ ��� ��������")]
    private float recoilDuration = 0.2f;
    [SerializeField, Tooltip("������ �������� ������� ��� ������� ����� (������� �� ���� ������)")]
    private GameObject projectileModelPrefab;

    [Header("Hit and Death Settings")]
    [SerializeField, Tooltip("������������ �������� ��������� ����� (������������)")]
    private float hitDuration = 0.2f;
    [SerializeField, Tooltip("��������� ������������ ��� ��������� �����")]
    private float hitPunchDistance = 0.15f;
    [SerializeField, Tooltip("������������ �������� ������ (�����������/�������)")]
    private float deathDuration = 0.5f;
    [SerializeField, Tooltip("������ ������� ��� ������� ������")]
    private GameObject deathEffectPrefab;

    /// <summary>
    /// ������������ �������� ��������.
    /// </summary>
    public float MoveDuration => moveDuration;
    public float JumpPeakPauseDuration => jumpPeakPauseDuration; // ����� ��������: ����� � ����
    public float MeleeAttackDuration => meleeAttackDuration;
    public float MeleePunchDistance => meleePunchDistance;
    public GameObject HitEffectPrefab => hitEffectPrefab;
    public GameObject MeleeWeaponEffectPrefab => meleeWeaponEffectPrefab;
    public float MeleeWeaponEffectTiming => meleeWeaponEffectTiming; // ����� ��������: ���������� ��������
    public float RangedAttackDuration => rangedAttackDuration;
    public float RecoilDistance => recoilDistance;
    public float RecoilDuration => recoilDuration;
    public GameObject ProjectileModelPrefab => projectileModelPrefab;
    public float HitDuration => hitDuration;
    public float HitPunchDistance => hitPunchDistance;
    public float DeathDuration => deathDuration;
    public GameObject DeathEffectPrefab => deathEffectPrefab;
}