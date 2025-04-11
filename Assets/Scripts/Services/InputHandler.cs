using Zenject;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [Inject] private GameManager gameManager;
    [Inject] private IBoardManager boardManager;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3Int target = new Vector3Int(Mathf.FloorToInt(hit.point.x), 0, Mathf.FloorToInt(hit.point.z));
                IPiece piece = boardManager.GetPieceAt(target);
                if (piece != null)
                {
                    gameManager.MakeMove(piece, target); // Пример вызова
                }
            }
        }
    }
}