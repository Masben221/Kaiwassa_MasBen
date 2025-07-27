using UnityEngine;

/// <summary>
/// ������������ �������� ��� �����, ������������� ����� ���������.
/// ������ ��������� ��� ������� �����, ������� �����, ��������� ����� � ������.
/// </summary>
[CreateAssetMenu(fileName = "PieceAnimationConfig", menuName = "Cyvasse/PieceAnimationConfig", order = 1)]
public class PieceAnimationConfig : ScriptableObject
{
    [Header("Movement Settings")]
    [SerializeField, Tooltip("������������ �������� �����������")]
    private float moveDuration = 0.5f;

    [Header("Melee Attack Settings")]
    [SerializeField, Tooltip("������������ �������� ������� ����� (����� ����� � �����)")]
    private float meleeAttackDuration = 0.3f;
    [SerializeField, Tooltip("��������� ����� ��� ������� �����")]
    private float meleePunchDistance = 0.2f;
    [SerializeField, Tooltip("������ ������ ��� ������� ��������� ��� ������� �����")]
    private ParticleSystem hitEffectPrefab;

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
    [SerializeField, Tooltip("������ ������ ��� ������� ������")]
    private ParticleSystem deathEffectPrefab;

    public float MoveDuration => moveDuration;
    public float MeleeAttackDuration => meleeAttackDuration;
    public float MeleePunchDistance => meleePunchDistance;
    public ParticleSystem HitEffectPrefab => hitEffectPrefab;
    public float RangedAttackDuration => rangedAttackDuration;
    public float RecoilDistance => recoilDistance;
    public float RecoilDuration => recoilDuration;
    public GameObject ProjectileModelPrefab => projectileModelPrefab;
    public float HitDuration => hitDuration;
    public float HitPunchDistance => hitPunchDistance;
    public float DeathDuration => deathDuration;
    public ParticleSystem DeathEffectPrefab => deathEffectPrefab;
}