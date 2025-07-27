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

    public void Attack(Vector3Int target, IBoardManager board)
    {
        Debug.Log($"Piece {GetType().Name}: Attack called for {target}");
    }

    /// <summary>
    /// ¬ыполн€ет действие (движение, ближн€€ или дальн€€ атака) с соответствующей анимацией.
    /// </summary>
    /// <param name="target">÷елева€ клетка дл€ действи€.</param>
    /// <param name="isMove">≈сли true, выполн€етс€ перемещение.</param>
    /// <param name="isRangedAttack">≈сли true, выполн€етс€ дальн€€ атака; иначе ближн€€.</param>
    /// <param name="boardManager">ћенеджер доски дл€ обновлени€ состо€ни€.</param>
    /// <param name="onComplete">ƒействие после завершени€ анимации.</param>
    public void PerformAction(Vector3Int target, bool isMove, bool isRangedAttack, IBoardManager boardManager, Action onComplete)
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
            });
        }
        else
        {
            attackStrategy?.ExecuteAttack(this, target, boardManager, isRangedAttack);
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// ¬ыбирает тип атаки (ближн€€ или дальн€€) и выполн€ет соответствующую анимацию.
    /// </summary>
    /// <param name="target">÷елева€ клетка дл€ атаки.</param>
    /// <param name="isRangedAttack">≈сли true, выполн€етс€ дальн€€ атака; иначе ближн€€.</param>
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
            });
        }
        else
        {
            animator.AnimateMeleeAttack(target, () =>
            {
                boardManager.RemovePiece(target);
                boardManager.MovePiece(this, position, target);
            });
        }
    }
}