using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// ��������� ����� ���������� � ���� � ������������ �������� ������.
/// ����������� �������� �������� � ���������� � ���������� ���� ��������.
/// </summary>
public class AnimationManager : MonoBehaviour
{
    private int activeAnimationsCount = 0; // ������� �������� ��������
    private bool isMovePrepared = false; // ���� ���������� ��������

    public event Action<Piece, Vector3Int, bool, bool> OnMovePrepared; // ������� ������ ���������� ��������
    public event Action OnAllAnimationsCompleted; // ������� ���������� ���� ��������

    /// <summary>
    /// �������������� �������� ��������.
    /// </summary>
    private void Awake()
    {
        if (FindObjectsOfType<AnimationManager>().Length > 1)
        {
            Debug.LogWarning("AnimationManager: Multiple instances detected, destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// �������� �������� �������� ������.
    /// </summary>
    public void StartMoveAnimation(Piece piece, Vector3Int target, bool isMove, bool isRangedAttack, Action onComplete)
    {
        if (!isMovePrepared)
        {
            isMovePrepared = true;
            OnMovePrepared?.Invoke(piece, target, isMove, isRangedAttack);
        }

        activeAnimationsCount++;
        piece.PerformAction(target, isMove, isRangedAttack, () =>
        {
            activeAnimationsCount--;
            CheckAnimationsCompleted();
            onComplete?.Invoke();
        });
    }

    /// <summary>
    /// �������� �������� ����� ��� ����� ��� ���������� �����.
    /// </summary>
    public void StartAttackAnimation(Piece piece, List<Vector3Int> targets, bool isRangedAttack, IBoardManager boardManager)
    {
        if (boardManager == null)
        {
            Debug.LogError("AnimationManager: boardManager is null in StartAttackAnimation!");
            return;
        }

        if (!isMovePrepared)
        {
            isMovePrepared = true;
            OnMovePrepared?.Invoke(piece, targets[0], false, isRangedAttack);
        }

        activeAnimationsCount++;
        piece.AttackStrategy.ExecuteAttack(piece, targets[0], boardManager, isRangedAttack);

        if (targets.Count > 1)
        {
            StartCoroutine(WaitForFirstAttack(piece, targets[1], isRangedAttack, boardManager));
        }
        else
        {
            CheckAnimationsCompleted();
        }
    }

    /// <summary>
    /// ������� ���������� �������� ������ ����� ����� ������� ������.
    /// </summary>
    private IEnumerator<object> WaitForFirstAttack(Piece piece, Vector3Int secondTarget, bool isRangedAttack, IBoardManager boardManager)
    {
        if (boardManager == null)
        {
            Debug.LogError("AnimationManager: boardManager is null in WaitForFirstAttack!");
            yield break;
        }

        PieceAnimator animator = piece.GetComponent<PieceAnimator>();
        PieceAnimationConfig config = animator?.GetAnimationConfig(piece);
        float animationDuration = (config?.RotationDuration ?? 0.3f) + (config?.MoveDuration ?? 0.5f) +
                                (config?.MeleeAttackDuration ?? 0.3f) + (config?.HitDuration ?? 0.2f) +
                                (config?.DeathDuration ?? 0.5f) + 0.1f;

        yield return new WaitForSeconds(animationDuration);
        activeAnimationsCount++;
        piece.AttackStrategy.ExecuteAttack(piece, secondTarget, boardManager, isRangedAttack);
        CheckAnimationsCompleted();
    }

    /// <summary>
    /// ���������, ��������� �� ��� ��������, � �������� ������� ��� �������������.
    /// </summary>
    private void CheckAnimationsCompleted()
    {
        if (activeAnimationsCount <= 0 && isMovePrepared)
        {
            isMovePrepared = false;
            OnAllAnimationsCompleted?.Invoke();
        }
    }
}