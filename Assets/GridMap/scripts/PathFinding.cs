using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PathFinding
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    public static PathFinding Instance { get; private set; }

    private GridTest<PathNode> grid;
    private List<PathNode> openList;
    private List<PathNode> closedList;
    private bool hasDiagonalMovement;
    public PathFinding(int width, int heigth, bool activeDiagonalMovement = false)
    {
        Instance = this;
        // Make originPosition configuirable not hardcoded
        float Cameraheight = Camera.main.orthographicSize * 2;
        float Camerawidth = Cameraheight * Camera.main.aspect;
        Vector3 originPosition = new Vector3(-Camerawidth / 2f, -Cameraheight / 2f);
        // end of calculation originPosition
        grid = new GridTest<PathNode>(width, heigth, 10f, originPosition, (GridTest<PathNode> g, int x, int y) => new PathNode(g, x, y));
        hasDiagonalMovement = activeDiagonalMovement;
    }

    public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition)
    {
        grid.GetXY(startWorldPosition, out int startX, out int startY);
        grid.GetXY(endWorldPosition, out int endX, out int endY);

        List<PathNode> path = FindPath(startX, startY, endX, endY);

        if (path == null)
        {
            return null;
        }
        else
        {
            List<Vector3> vectorPath = new List<Vector3>();
            foreach (PathNode pathnode in path)
            {
                /* vectorPath.Add(new Vector3(pathnode.x, pathnode.y) * grid.GetCellSize() + Vector3.one * grid.GetCellSize() * .5f);
             */
                vectorPath.Add(
       grid.GetWorldPosition(pathnode.x, pathnode.y)
       + Vector3.one * grid.GetCellSize() * 0.5f
   );
            }
            return vectorPath;
        }
    }

    public GridTest<PathNode> GetGrid()
    {
        return grid;
    }

    public List<PathNode> FindPath(int startX, int startY, int endX, int endY)
    {
        PathNode startNode = grid.GetGridObject(startX, startY);
        PathNode endNode = grid.GetGridObject(endX, endY);

        if (startNode == null || endNode == null)
        {
            // Invalid Path
            Debug.Log("Invalid Path");
            return null;
        }

        // queeing node for searching - start node intial position
        openList = new List<PathNode> { startNode };
        // already searched nodes
        closedList = new List<PathNode>();

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                PathNode pathnode = grid.GetGridObject(x, y);
                pathnode.gCost = int.MaxValue;
                pathnode.CalculateFCost();
                // CameFromNode
                pathnode.previousVisitedNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode)
            {
                return CalculatePath(endNode);
            }
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode surroundingNode in GetSurroundingNodeList(currentNode))
            {
                if (closedList.Contains(surroundingNode)) continue;
                if (!surroundingNode.isWalkable)
                {
                    closedList.Add(surroundingNode);
                    continue;
                }

                int tentativeGcost = currentNode.gCost + CalculateDistanceCost(currentNode, surroundingNode);

                if (tentativeGcost < surroundingNode.gCost)
                {
                    surroundingNode.previousVisitedNode = currentNode;
                    surroundingNode.gCost = tentativeGcost;
                    surroundingNode.hCost = CalculateDistanceCost(surroundingNode, endNode);
                    surroundingNode.CalculateFCost();
                    if (!openList.Contains(surroundingNode))
                    {
                        openList.Add(surroundingNode);
                    }
                }
            }
            ;
        }

        // Out of node on the openList
        return null;
    }

    private List<PathNode> GetSurroundingNodeList(PathNode currentNode)
    {
        List<PathNode> surroundingNodeList = new List<PathNode>();
        if (currentNode.x - 1 >= 0)
        {
            // Left
            surroundingNodeList.Add(GetNode(currentNode.x - 1, currentNode.y));
            if (hasDiagonalMovement)
            {
                // Left Down
                if (currentNode.y - 1 >= 0) surroundingNodeList.Add(GetNode(currentNode.x - 1, currentNode.y - 1));
                // Left Up
                if (currentNode.y + 1 < grid.GetHeight()) surroundingNodeList.Add(GetNode(currentNode.x - 1, currentNode.y + 1));
            }
        }
        if (currentNode.x + 1 < grid.GetWidth())
        {
            // Right
            surroundingNodeList.Add(GetNode(currentNode.x + 1, currentNode.y));
            if (hasDiagonalMovement)
            {
                // Right Down
                if (currentNode.y - 1 >= 0) surroundingNodeList.Add(GetNode(currentNode.x + 1, currentNode.y - 1));
                // Right Up
                if (currentNode.y + 1 < grid.GetHeight()) surroundingNodeList.Add(GetNode(currentNode.x + 1, currentNode.y + 1));
            }
        }
        // Down
        if (currentNode.y - 1 >= 0) surroundingNodeList.Add(GetNode(currentNode.x, currentNode.y - 1));
        // Up
        if (currentNode.y + 1 < grid.GetHeight()) surroundingNodeList.Add(GetNode(currentNode.x, currentNode.y + 1));

        return surroundingNodeList;
    }

    private PathNode GetNode(int x, int y)
    {
        return grid.GetGridObject(x, y);
    }

    private List<PathNode> CalculatePath(PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        path.Add(endNode);
        PathNode currentNode = endNode;
        while (currentNode.previousVisitedNode != null)
        {
            path.Add(currentNode.previousVisitedNode);
            currentNode = currentNode.previousVisitedNode;

        }
        path.Reverse();
        return path;
    }

    private int CalculateDistanceCost(PathNode a, PathNode b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);

        if (hasDiagonalMovement)
        {
            int remaining = Mathf.Abs(xDistance - yDistance);
            return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }
        return MOVE_STRAIGHT_COST * (xDistance + yDistance);
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }


}
