using System;
using System.Collections.Generic;

public class AStarSearch
{
    // ノードのクラス
    public class Node
    {
        public int x;
        public int y;
        public int gCost; // スタート地点からの実コスト
        public int hCost; // ゴール地点までの予測コスト
        public int fCost; // gCost + hCost
        public Node parent;

        public Node(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    // パス検索のメソッド
    public static List<Node> FindPath(Node startNode, Node endNode, int[,] grid)
    {
        List<Node> openList = new List<Node>();
        List<Node> closedList = new List<Node>();

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node currentNode = openList[0];

            // openListの中で、fCostが最小のノードを探す
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < currentNode.fCost ||
                    openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost)
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode == endNode)
            {
                return RetracePath(startNode, endNode);
            }

            List<Node> neighbors = GetNeighbors(currentNode, grid);

            foreach (Node neighbor in neighbors)
            {
                if (closedList.Contains(neighbor))
                {
                    continue;
                }

                int newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);

                if (newCostToNeighbor < neighbor.gCost || !openList.Contains(neighbor))
                {
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, endNode);
                    neighbor.fCost = neighbor.gCost + neighbor.hCost;
                    neighbor.parent = currentNode;

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }

        return null;
    }

    // パスを再構築するメソッド
    private static List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    // 近傍のノードを取得するメソッド
    private static List<Node> GetNeighbors(Node node, int[,] grid)
    {
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // 中心のノードは除外
                if (x == 0 && y == 0)
                {
                    continue;
                }

                int checkX = node.x + x;
                int checkY = node.y + y;

                // マップの範囲外は除外
                if (checkX >= 0 && checkX < grid.GetLength(0) && checkY >= 0 && checkY < grid.GetLength(1))
                {
                    // 壁は除外
                    if (grid[checkX, checkY] == 0)
                    {
                        continue;
                    }

                    Node neighbor = new Node(checkX, checkY);
                    neighbors.Add(neighbor);
                }
            }
        }

        return neighbors;
    }

    // 二点間の距離を計算するメソッド
    private static int GetDistance(Node nodeA, Node nodeB)
    {
        int distanceX = Math.Abs(nodeA.x - nodeB.x);
        int distanceY = Math.Abs(nodeA.y - nodeB.y);

        if (distanceX > distanceY)
        {
            return distanceY + (distanceX - distanceY);
        }
        else
        {
            return distanceX + (distanceY - distanceX);
        }
    }
}
