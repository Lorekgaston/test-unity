using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestPathFindingNode : MonoBehaviour
{
    [SerializeField] private PlayerGrid player;
    private PathFinding pathFinding;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pathFinding = new PathFinding(10, 10);
        GridTest<PathNode> grid = pathFinding.GetGrid();
        Vector3 centerOffset = Vector3.one * (grid.GetCellSize() * 0.5f);
        player.transform.position = grid.GetWorldPosition(0, 0) + centerOffset;
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 vec = Camera.main.ScreenToWorldPoint(mousePos);
            GridTest<PathNode> grid = pathFinding.GetGrid();
            grid.GetXY(vec, out int x, out int y);
            List<PathNode> path = pathFinding.FindPath(0, 0, x, y);
            if (path != null)
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Vector3 start = pathFinding.GetGrid().GetWorldPosition(path[i].x, path[i].y)
                 + new Vector3(5f, 5f);

                    Vector3 end = pathFinding.GetGrid().GetWorldPosition(path[i + 1].x, path[i + 1].y)
                                  + new Vector3(5f, 5f);

                    Debug.DrawLine(start, end, Color.white, 2f);

                }
            }
            player.SetTargetPosition(vec);
        }
    }
}
