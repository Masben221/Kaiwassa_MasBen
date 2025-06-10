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
    protected IMovable movementStrategy; // Стратегия движения
    protected IAttackable attackStrategy; // Стратегия атаки
    private Vector3Int position; // Позиция на доске
    private bool isPlayer1; // Принадлежность игроку
    private Quaternion initialRotation; // Начальная ротация фигуры
    [SerializeField] private PieceType type; // Тип фигуры
    [SerializeField] private Sprite iconSprite; // Иконка для UI

    public Vector3Int Position => position;
    public bool IsPlayer1 => isPlayer1;
    public PieceType Type => type;
    public Sprite IconSprite => iconSprite;
    public IAttackable AttackStrategy => attackStrategy;
    public Quaternion InitialRotation => initialRotation;

    private void Awake()
    {
        initialRotation = transform.rotation; // Сохраняем начальную ротацию
        SetupStrategies(); // Инициализация стратегий
    }

    // Реализуется в дочерних классах для установки стратегий
    protected abstract void SetupStrategies();

    // Инициализация фигуры
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

    // Установка позиции
    public void SetPosition(Vector3Int newPosition)
    {
        position = newPosition;
        transform.position = new Vector3(newPosition.x, 0.5f, newPosition.z);
        // Обновление коллайдера (если есть)
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
            collider.enabled = true; // Перезапускаем коллайдер для синхронизации
        }
        Debug.Log($"Piece {GetType().Name} set to position {newPosition}");
    }

    // Получение допустимых ходов
    public List<Vector3Int> GetValidMoves(IBoardManager board)
    {
        return movementStrategy?.CalculateMoves(board, this) ?? new List<Vector3Int>();
    }

    // Получение целей атаки
    public List<Vector3Int> GetAttackMoves(IBoardManager board)
    {
        return attackStrategy?.CalculateAttacks(board, this) ?? new List<Vector3Int>();
    }

    // Получение всех возможных целей атаки
    public List<Vector3Int> GetAllPotentialAttackMoves(IBoardManager board)
    {
        return attackStrategy?.CalculateAllAttacks(board, this) ?? new List<Vector3Int>();
    }

    // Запуск атаки (пустой, так как атака обрабатывается в PerformAction)
    public void Attack(Vector3Int target, IBoardManager boardManager)
    {
        Debug.Log($"Piece {GetType().Name}: Attack called for {target}");
    }

    // Выполняет действие (движение или атака) с анимацией
    public void PerformAction(Vector3Int target, bool isMove, bool isRangedAttack, IBoardManager boardManager, Action onComplete)
    {
        PieceAnimator animator = GetComponent<PieceAnimator>();
        if (animator == null)
        {
            Debug.LogError($"Piece {GetType().Name}: No PieceAnimator found");
            onComplete?.Invoke();
            return;
        }

        // Для дальней атаки остаёмся на месте
        Vector3Int animationTarget = isRangedAttack ? position : target;
        Debug.Log($"Piece {GetType().Name}: Performing {(isMove ? "move" : isRangedAttack ? "ranged attack" : "melee attack")} to {target}");

        animator.MoveTo(target, animationTarget, null, () => // ИЗМЕНЕНИЕ: передаём target и animationTarget
        {
            if (isMove)
            {
                boardManager.MovePiece(this, position, target);
            }
            else
            {
                if (attackStrategy != null)
                {
                    attackStrategy.ExecuteAttack(this, target, boardManager);
                    // Для ближней атаки перемещаем фигуру на клетку цели
                    if (!isRangedAttack)
                    {
                        boardManager.MovePiece(this, position, target);
                    }
                }
                else
                {
                    Debug.LogWarning($"Piece {GetType().Name}: No attack strategy assigned");
                }
            }
            onComplete?.Invoke();
        });
    }
}