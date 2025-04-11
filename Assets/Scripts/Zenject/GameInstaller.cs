using UnityEngine;
using Zenject;

/// <summary>
/// Установщик зависимостей для игры. Настраивает глобальные сервисы через Zenject.
/// Все компоненты создаются из префабов для корректной работы с MonoBehaviour.
/// </summary>
public class GameInstaller : MonoInstaller
{
    // Префабы для основных компонентов
    [SerializeField] private GameObject boardManagerPrefab;
    [SerializeField] private GameObject pieceFactoryPrefab;
    [SerializeField] private GameObject gameManagerPrefab;

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

        Debug.Log("GameInstaller: Dependencies bound successfully.");
    }
}