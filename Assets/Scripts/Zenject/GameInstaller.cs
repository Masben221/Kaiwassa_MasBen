using UnityEngine;
using Zenject;

/// <summary>
/// ���������� ������������ ��� ����. ����������� ���������� ������� ����� Zenject.
/// ��� ���������� ��������� �� �������� ��� ���������� ������ � MonoBehaviour.
/// </summary>
public class GameInstaller : MonoInstaller
{
    // ������� ��� �������� �����������
    [SerializeField] private GameObject boardManagerPrefab;
    [SerializeField] private GameObject pieceFactoryPrefab;
    [SerializeField] private GameObject gameManagerPrefab;

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

        // ����������� PieceFactory �� �������
        Container.Bind<PieceFactory>()
            .FromComponentInNewPrefab(pieceFactoryPrefab)
            .AsSingle()
            .NonLazy();

        Debug.Log("GameInstaller: Dependencies bound successfully.");
    }
}