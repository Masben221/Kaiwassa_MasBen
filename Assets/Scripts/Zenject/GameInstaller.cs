using UnityEngine;
using Zenject;

/// <summary>
/// ���������� ������������ ��� ����. ����������� ���������� ������� ����� Zenject.
/// </summary>
public class GameInstaller : MonoInstaller
{
    // ������� ��� �������� �����������
    [SerializeField] private GameObject boardManagerPrefab;
    [SerializeField] private GameObject pieceFactoryPrefab;
    [SerializeField] private GameObject gameManagerPrefab;
    [SerializeField] private GameObject inputHandlerPrefab;
    [SerializeField] private GameObject piecePlacementManagerPrefab;
    [SerializeField] private GameObject manualPlacementManagerPrefab;

    /// <summary>
    /// �����, ���������� Zenject ��� ��������� ������������.
    /// </summary>
    public override void InstallBindings()
    {
        // ����������� IGameManager � GameManager �� �������
        Container.Bind<IGameManager>()
            .To<GameManager>()
            .FromComponentInNewPrefab(gameManagerPrefab)
            .AsSingle()
            .NonLazy();

        // ����������� IBoardManager � BoardManager �� �������
        Container.Bind<IBoardManager>()
            .To<BoardManager>()
            .FromComponentInNewPrefab(boardManagerPrefab)
            .AsSingle()
            .NonLazy();

        // ����������� IPieceFactory � PieceFactory �� �������
        Container.Bind<IPieceFactory>()
            .To<PieceFactory>()
            .FromComponentInNewPrefab(pieceFactoryPrefab)
            .AsSingle()
            .NonLazy();

        // ����������� InputHandler �� �������
        Container.Bind<InputHandler>()
            .FromComponentInNewPrefab(inputHandlerPrefab)
            .AsSingle()
            .NonLazy();

        // ����������� IPiecePlacementManager ��� ��������� �����������
        Container.Bind<IPiecePlacementManager>()
            .WithId("Random")
            .To<PiecePlacementManager>()
            .FromComponentInNewPrefab(piecePlacementManagerPrefab)
            .AsSingle()
            .NonLazy();

        // ����������� IPiecePlacementManager ��� ������ �����������
        Container.Bind<IPiecePlacementManager>()
            .WithId("Manual")
            .To<ManualPlacementManager>()
            .FromComponentInNewPrefab(manualPlacementManagerPrefab)
            .AsSingle()
            .NonLazy();

        Debug.Log("GameInstaller: Dependencies bound successfully.");
    }
}