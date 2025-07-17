using UnityEngine;
using System.Collections.Generic;
using System;

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
    protected IMovable movementStrategy;
    protected IAttackable attackStrategy;
    private Vector3Int position;
    private bool isPlayer1;
    private Quaternion initialRotation;
    [SerializeField] private PieceType type;
    [SerializeField] private Sprite iconSprite;

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

    public List<Vector3Int> GetValidMoves(IBoardManager board)
    {
        return movementStrategy?.CalculateMoves(board, this) ?? new List<Vector3Int>();
    }

    public List<Vector3Int> GetAttackMoves(IBoardManager board)
    {
        return attackStrategy?.CalculateAttacks(board, this) ?? new List<Vector3Int>();
    }

    public List<Vector3Int> GetAllPotentialAttackMoves(IBoardManager board)
    {
        return attackStrategy?.CalculateAllAttacks(board, this) ?? new List<Vector3Int>();
    }

    public void Attack(Vector3Int target, IBoardManager boardManager)
    {
        Debug.Log($"Piece {GetType().Name}: Attack called for {target}");
    }

    /// <summary>
    /// Выполняет действие (движение, ближняя или дальняя атака) с соответствующей анимацией.
    /// </summary>
    /// <param name="target">Целевая клетка для действия.</param>
    /// <param name="isMove">Если true, выполняется перемещение.</param>
    /// <param name="isRangedAttack">Если true, выполняется дальняя атака; иначе ближняя.</param>
    /// <param name="boardManager">Менеджер доски для обновления состояния.</param>
    /// <param name="onComplete">Действие после завершения анимации.</param>
    public void PerformAction(Vector3Int target, bool isMove, bool isRangedAttack, IBoardManager boardManager, Action onComplete)
    {
        PieceAnimator animator = GetComponent<PieceAnimator>();
        if (animator == null)
        {
            Debug.LogError($"Piece {GetType().Name}: No PieceAnimator found");
            onComplete?.Invoke();
            return;
        }

        Vector3Int animationTarget = isRangedAttack ? position : target;
        Debug.Log($"Piece {GetType().Name}: Performing {(isMove ? "move" : isRangedAttack ? "ranged attack" : "melee attack")} to {target}");

        if (isMove)
        {
            // ИСПРАВЛЕНИЕ: Вызываем MovePiece после анимации для обновления логической позиции
            animator.MoveTo(target, target, null, () =>
            {
                boardManager.MovePiece(this, position, target);
                onComplete?.Invoke();
            });
        }
        else if (isRangedAttack)
        {
            animator.AnimateRangedAttack(target, () =>
            {
                attackStrategy?.ExecuteAttack(this, target, boardManager);
                onComplete?.Invoke();
            });
        }
        else
        {
            animator.AnimateMeleeAttack(target, () =>
            {
                attackStrategy?.ExecuteAttack(this, target, boardManager);
                boardManager.MovePiece(this, position, target);
                onComplete?.Invoke();
            });
        }
    }
}