using UnityEngine;
using System.Collections.Generic;
using System;
using Zenject;

public enum PieceType
{
    King,
    Dragon,
    Elephant,
    HeavyCavalry,
    LightHorse,
    Spearman,
    Crossbowman,
    Rabble,
    Catapult,
    Trebuchet,
    Mountain,
    Swordsman,
    Archer
}

public abstract class Piece : MonoBehaviour
{
    [Inject] protected IBoardManager boardManager;
    protected IMovable movementStrategy;
    protected IAttackable attackStrategy;
    private Vector3Int position;
    private bool isPlayer1;
    private Quaternion initialRotation;
    [SerializeField] private PieceType type;
    [SerializeField] private Sprite iconSprite;
    private CameraController cameraController;

    public Vector3Int Position => position;
    public bool IsPlayer1 => isPlayer1;
    public PieceType Type => type;
    public Sprite IconSprite => iconSprite;
    public IAttackable AttackStrategy => attackStrategy;
    public Quaternion InitialRotation => initialRotation;

    private void Awake()
    {
        initialRotation = transform.rotation;
        SetupStrategies();
        // �������� ���������� ������
        cameraController = Camera.main.GetComponent<CameraController>();
    }

    protected abstract void SetupStrategies();

    public void Initialize(bool isPlayer1, Material material)
    {
        this.isPlayer1 = isPlayer1;
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material = material;
        }
        else
        {
            Debug.LogWarning($"Piece {GetType().Name}: No Renderer found");
        }
        Debug.Log($"Piece {GetType().Name} initialized for Player {(isPlayer1 ? 1 : 2)}");
    }

    public void SetPosition(Vector3Int newPosition)
    {
        position = newPosition;
        transform.position = new Vector3(newPosition.x, 0.5f, newPosition.z);
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
            collider.enabled = true;
        }
        Debug.Log($"Piece {GetType().Name} set to position {newPosition}");
    }

    public List<Vector3Int> GetValidMoves()
    {
        return movementStrategy?.CalculateMoves(boardManager, this) ?? new List<Vector3Int>();
    }

    public List<Vector3Int> GetAttackMoves()
    {
        return attackStrategy?.CalculateAttacks(boardManager, this) ?? new List<Vector3Int>();
    }

    public List<Vector3Int> GetAllPotentialAttackMoves()
    {
        return attackStrategy?.CalculateAllAttacks(boardManager, this) ?? new List<Vector3Int>();
    }   

    /// <summary>
    /// ��������� �������� (��������, ������� ��� ������� �����) � ��������������� ���������.
    /// </summary>
    /// <param name="target">������� ������ ��� ��������.</param>
    /// <param name="isMove">���� true, ����������� �����������.</param>
    /// <param name="isRangedAttack">���� true, ����������� ������� �����; ����� �������.</param>
    /// <param name="boardManager">�������� ����� ��� ���������� ���������.</param>
    /// <param name="onComplete">�������� ����� ���������� ��������.</param>
    public void PerformAction(Vector3Int target, bool isMove, bool isRangedAttack, Action onComplete)
    {
        PieceAnimator animator = GetComponent<PieceAnimator>();
        if (animator == null)
        {
            Debug.LogError($"Piece {GetType().Name}: No PieceAnimator found");
            onComplete?.Invoke();
            return;
        }

        if (isMove)
        {
            animator.MoveTo(target, target, null, () =>
            {
                boardManager.MovePiece(this, position, target);
                onComplete?.Invoke();
                if (cameraController != null)
                {
                    cameraController.HandleAnimationCompleted(); // ��������� ������� ������ ���� �� ������ ��� � �����
                }
            });
        }
        else
        {
            attackStrategy?.ExecuteAttack(this, target, boardManager, isRangedAttack);
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// �������� ��� ����� (������� ��� �������) � ��������� ��������������� ��������.
    /// </summary>
    /// <param name="target">������� ������ ��� �����.</param>
    /// <param name="isRangedAttack">���� true, ����������� ������� �����; ����� �������.</param>
    public void SelectAttack(Vector3Int target, bool isRangedAttack)
    {
        PieceAnimator animator = GetComponent<PieceAnimator>();
        if (animator == null)
        {
            Debug.LogError($"Piece {GetType().Name}: No PieceAnimator found");
            return;
        }

        if (isRangedAttack)
        {
            animator.AnimateRangedAttack(target, () =>
            {
                boardManager.RemovePiece(target);
                if (cameraController != null)
                {
                    cameraController.HandleAnimationCompleted(); // ��������� ������� ������ ���� �� ������ ��� � �����
                }
            });
        }
        else
        {
            animator.AnimateMeleeAttack(target, () =>
            {
                boardManager.RemovePiece(target);
                boardManager.MovePiece(this, position, target);

                if (cameraController != null)
                {
                    cameraController.HandleAnimationCompleted(); // ��������� ������� ������ ���� �� ������ ��� � �����
                }
            });
        }
    }
}