using UnityEngine;
using Zenject;
using System.Collections.Generic;

/// <summary>
/// ������������ ���� ������ (���� ��� ��������) ��� ������ ����� � �����.
/// </summary>
public class InputHandler : MonoBehaviour
{
    // �����������, ������������� ����� Zenject
    [Inject] private IGameManager gameManager;
    [Inject] private IBoardManager boardManager;

    // ������� ��� �������� ������
    [SerializeField] private GameObject moveMarkerPrefab; // ��� �����
    [SerializeField] private GameObject attackMarkerPrefab; // ��� ����

    // ������� ��������� ������
    private Piece selectedPiece;

    // ������ ������� �������� �� �����
    private List<GameObject> currentMarkers = new List<GameObject>();

    // �������� ��� ��������� ��������� ������
    [SerializeField] private Material highlightMaterial;

    // �������� �������� ��������� ������ (��� ��������������)
    private Material originalMaterial;

    /// <summary>
    /// ������������ ���� ������ � ������ �����.
    /// </summary>
    private void Update()
    {
        // ��������� ���� ���� ��� ������� (�������� ��� ���� � ���������)
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    /// <summary>
    /// ������������ ����: �������� ������, ��������� ���/����� ��� �������� ���������.
    /// </summary>
    private void HandleClick()
    {
        // ������ ��� �� ������� ����/�������
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // �������� 3D-������� �����, �������� ����� ������
            Vector3Int clickedPos = new Vector3Int(
                Mathf.FloorToInt(hit.point.x + 0.5f),
                0,
                Mathf.FloorToInt(hit.point.z + 0.5f)
            );

            // ���������, ���� �� ������ �� ������
            Piece clickedPiece = boardManager.GetPieceAt(clickedPos);

            // ���� ������ �������
            if (selectedPiece != null)
            {
                // ���������, �������� �� ������ ���������� ����� ��� ������
                var validMoves = selectedPiece.GetValidMoves(boardManager);
                var attackMoves = selectedPiece.GetAttackMoves(boardManager);

                if (validMoves.Contains(clickedPos) || attackMoves.Contains(clickedPos))
                {
                    // ��������� ��� ��� �����
                    gameManager.MakeMove(selectedPiece, clickedPos);
                }

                // ���������� ���������
                ClearSelection();
            }
            // ���� �������� �� ������ �������� ������
            else if (clickedPiece != null && clickedPiece.IsPlayer1 == gameManager.IsPlayer1Turn)
            {
                // �������� ������
                SelectPiece(clickedPiece);
            }
            // ���� �������� ��� �����, �������� ���������
            else if (!boardManager.IsWithinBounds(clickedPos))
            {
                ClearSelection();
            }
        }
        else
        {
            // ���� �������� ��� ��������, �������� ���������
            ClearSelection();
        }
    }

    /// <summary>
    /// �������� ������ � ������������ ������ ��� ����� � ����.
    /// </summary>
    private void SelectPiece(Piece piece)
    {
        selectedPiece = piece;

        // ������������ ������
        Renderer renderer = piece.GetComponentInChildren<Renderer>();
        if (renderer != null && highlightMaterial != null)
        {
            originalMaterial = renderer.material;
            renderer.material = highlightMaterial;
        }

        // ���������� ������� ��� ��������� �����
        var validMoves = piece.GetValidMoves(boardManager);
        foreach (var move in validMoves)
        {
            GameObject marker = Instantiate(
                moveMarkerPrefab,
                new Vector3(move.x, 0.1f, move.z),
                Quaternion.Euler(90, 0, 0)
            );
            currentMarkers.Add(marker);
        }

        // ���������� ������� ��� ��������� ����
        var attackMoves = piece.GetAttackMoves(boardManager);
        foreach (var attack in attackMoves)
        {
            GameObject marker = Instantiate(
                attackMarkerPrefab,
                new Vector3(attack.x, 0.1f, attack.z),
                Quaternion.Euler(90, 0, 0)
            );
            currentMarkers.Add(marker);
        }

        Debug.Log($"Selected piece at {piece.Position}");
    }

    /// <summary>
    /// ������� ������� ��������� � ������� �������.
    /// </summary>
    private void ClearSelection()
    {
        // ��������������� �������� ������
        if (selectedPiece != null)
        {
            Renderer renderer = selectedPiece.GetComponentInChildren<Renderer>();
            if (renderer != null && originalMaterial != null)
            {
                renderer.material = originalMaterial;
            }
        }

        // ������� ��� �������
        foreach (var marker in currentMarkers)
        {
            Destroy(marker);
        }
        currentMarkers.Clear();

        selectedPiece = null;
        Debug.Log("Selection cleared.");
    }

    /// <summary>
    /// ������� ��� ����������� �������.
    /// </summary>
    private void OnDestroy()
    {
        ClearSelection();
    }
}