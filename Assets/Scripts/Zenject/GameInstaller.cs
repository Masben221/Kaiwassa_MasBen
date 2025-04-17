using UnityEngine;
using Zenject;

/// <summary>
/// Установщик зависимостей для игры. Настраивает глобальные сервисы через Zenject.
/// </summary>
public class GameInstaller : MonoInstaller
{
    // Префабы для основных компонентов
    [SerializeField] private GameObject boardManagerPrefab;
    [SerializeField] private GameObject pieceFactoryPrefab;
    [SerializeField] private GameObject gameManagerPrefab;
    [SerializeField] private GameObject inputHandlerPrefab;

    /// <summary>
    /// Метод, вызываемый Zenject для настройки зависимостей.
    /// </summary>
    public override void InstallBindings()
    {
        // Привязываем IGameManager к GameManager из префаба
        Container.Bind<IGameManager>()
            .To<GameManager>()
            .FromComponentInNewPrefab(gameManagerPrefab)
            .AsSingle()
            .NonLazy();

        // Привязываем IBoardManager к BoardManager из префаба
        Container.Bind<IBoardManager>()
            .To<BoardManager>()
            .FromComponentInNewPrefab(boardManagerPrefab)
            .AsSingle()
            .NonLazy();

        // Привязываем PieceFactory из префаба
        Container.Bind<PieceFactory>()
            .FromComponentInNewPrefab(pieceFactoryPrefab)
            .AsSingle()
            .NonLazy();

        // Привязываем InputHandler из префаба
        Container.Bind<InputHandler>()
            .FromComponentInNewPrefab(inputHandlerPrefab)
            .AsSingle()
            .NonLazy();

        Debug.Log("GameInstaller: Dependencies bound successfully.");
    }
}