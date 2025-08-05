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
        // Получаем контроллер камеры
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
    /// Выполняет действие (движение, ближняя или дальняя атака) с соответствующей анимацией.
    /// </summary>
    /// <param name="target">Целевая клетка для действия.</param>
    /// <param name="isMove">Если true, выполняется перемещение.</param>
    /// <param name="isRangedAttack">Если true, выполняется дальняя атака; иначе ближняя.</param>
    /// <param name="boardManager">Менеджер доски для обновления состояния.</param>
    /// <param name="onComplete">Действие после завершения анимации.</param>
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
                    cameraController.HandleAnimationCompleted(); // запускаем возврат камеры если не первый ход у слона
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
    /// Выбирает тип атаки (ближняя или дальняя) и выполняет соответствующую анимацию.
    /// </summary>
    /// <param name="target">Целевая клетка для атаки.</param>
    /// <param name="isRangedAttack">Если true, выполняется дальняя атака; иначе ближняя.</param>
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
                    cameraController.HandleAnimationCompleted(); // запускаем возврат камеры если не первый ход у слона
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
                    cameraController.HandleAnimationCompleted(); // запускаем возврат камеры если не первый ход у слона
                }
            });
        }
    }
}