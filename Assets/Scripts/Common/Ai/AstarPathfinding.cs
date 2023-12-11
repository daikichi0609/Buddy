using System;
using System.Collections.Generic;
using UnityEngine;

public static class AStarSearch
{
    private static readonly float SQUARE2 = 1.4f;

    /// <summary>
    /// ノード
    /// </summary>
    public class Node : IEquatable<Node>, IEquatable<Vector2Int>
    {
        /// <summary>
        /// X座標
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Y座標
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// 移動コスト
        /// </summary>
        private float MoveCost { get; }

        /// <summary>
        /// 推定コスト（ゴールまでの距離）
        /// </summary>
        public float HeuristicCost { get; }

        /// <summary>
        /// 親ノードのコスト
        /// </summary>
        private float ParentCost => Parent != null ? Parent.SumCost : 0f;

        /// <summary>
        /// 合計コスト（総コスト = 親コスト + 移動コスト + 推定コスト）
        /// </summary>
        public float SumCost => ParentCost + MoveCost + HeuristicCost;

        /// <summary>
        /// 親ノード
        /// </summary>
        public Node Parent { get; private set; }

        /// <summary>
        /// コンストラクタで座標情報と移動コストとヒューリスティックコストをセット
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Node(int x, int y, float mCost, float hCost)
        {
            this.X = x;
            this.Y = y;
            this.MoveCost = mCost;
            this.HeuristicCost = hCost;
        }
        public Node(int x, int y, float mCost, Vector2Int goal)
        {
            this.X = x;
            this.Y = y;
            this.MoveCost = mCost;
            this.HeuristicCost = new Vector2Int(x, y).GetDistance(goal);
        }

        /// <summary>
        /// 親ノード追加
        /// </summary>
        /// <param name="parent"></param>
        public void SetParent(Node parent) => Parent = parent;

        /// <summary>
        /// 座標同じならtrue
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Node other) => X == other.X && Y == other.Y;
        public bool Equals(Vector2Int other) => X == other.x && Y == other.y;
    }

    /// <summary>
    /// リストから同じ座標のノードを取得する
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="other"></param>
    /// <param name="old"></param>
    /// <returns></returns>
    public static bool TryGetSamePositionNode(this List<Node> nodes, Node other, out Node old)
    {
        old = null;

        foreach (var node in nodes)
            if (node.Equals(other) == true)
            {
                old = node;
                return true;
            }
        return false;
    }

    /// <summary>
    /// パス検索
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="goalPos"></param>
    /// <param name="grid"></param>
    /// <returns></returns>
    public static List<Node> FindPath(Vector2Int startPos, Vector2Int goalPos, int[,] grid)
    {
        // スタートとゴールのノード作成
        var startNode = new Node(startPos.x, startPos.y, 0, startPos.GetDistance(goalPos));

        List<Node> openList = new List<Node>(); // 調査対象ノード
        List<Node> closedList = new List<Node>(); // 調査済みノード
        openList.Add(startNode);

        // OpenListが空になるまで続ける
        while (openList.Count > 0)
        {
            Node currentNode = openList[0];

            // openListの中で、合計コストが最小のノードを探す
            for (int i = 0; i < openList.Count; i++)
            {
                if (openList[i].SumCost < currentNode.SumCost ||
                    openList[i].SumCost == currentNode.SumCost && openList[i].HeuristicCost < currentNode.HeuristicCost)
                {
                    currentNode = openList[i];
                }
            }

            // オープンリストからクローズリストに移動（調査済みとしてマーク）
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            // goalと同じノードなら終了
            if (currentNode.Equals(goalPos) == true)
                return RetracePath(startNode, currentNode);

            // 隣接するノードを作成する
            List<Node> neighbors = GetNeighbors(currentNode, grid, goalPos);

            // 隣接するノードを調査
            foreach (Node neighbor in neighbors)
            {
                // 親ノードをセット
                neighbor.SetParent(currentNode);

                bool existInOpen = openList.TryGetSamePositionNode(neighbor, out var open); // オープンリストに存在するか
                bool existInClose = closedList.TryGetSamePositionNode(neighbor, out var close); // クローズリストに存在するか

                // オープンリストとクローズリストに存在しないなら、このノードをオープンリストに追加
                if (existInOpen == false && existInClose == false)
                    openList.Add(neighbor);

                // クローズリストにコストの大きいノードがあるなら古いノードを削除して新しいノードをオープンリストに追加
                else if (existInClose == true && neighbor.SumCost < close.SumCost)
                {
                    closedList.Remove(close);
                    openList.Add(neighbor);
                }
                // オープンリストにコストの大きいノードがあるなら古いノードを削除して新しいノードをオープンリストに追加
                else if (existInOpen == true && neighbor.SumCost < open.SumCost)
                {
                    openList.Remove(open);
                    openList.Add(neighbor);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// パス復元
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="endNode"></param>
    /// <returns></returns>
    private static List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode.Equals(startNode) == false)
        {
            path.Add(currentNode);
            currentNode = currentNode.Parent;
        }

        path.Reverse();
        return path;
    }

    /// <summary>
    /// 近傍のノードを取得する
    /// </summary>
    /// <param name="node"></param>
    /// <param name="grid"></param>
    /// <returns></returns>
    private static List<Node> GetNeighbors(Node node, int[,] grid, Vector2Int goalPos)
    {
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // 中心のノードは除外
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.X + x;
                int checkY = node.Y + y;

                // マップの範囲外は除外
                if (checkX >= 0 && checkX < grid.GetLength(0) && checkY >= 0 && checkY < grid.GetLength(1))
                {
                    int moveCost = grid[checkX, checkY]; // 移動コスト
                    // 壁は除外
                    if (moveCost == 0)
                        continue;

                    // 斜め移動の場合
                    if (x != 0 && y != 0)
                        // 斜め移動の条件を満たすマップでないなら除外
                        if (grid[checkX, node.Y] == 0 || grid[node.X, checkY] == 0)
                            continue;

                    Node neighbor = new Node(checkX, checkY, moveCost, goalPos);
                    neighbors.Add(neighbor);
                }
            }
        }

        return neighbors;
    }

    /// <summary>
    /// 二点間のチェビシェフ距離を計算する（斜め移動可能）
    /// ヒューリスティックコストに使う
    /// </summary>
    /// <param name="distanceX"></param>
    /// <param name="distanceY"></param>
    /// <returns></returns>
    private static float GetDistance(int distanceX, int distanceY) => distanceX > distanceY ? distanceY * SQUARE2 + (distanceX - distanceY) : distanceX * SQUARE2 + (distanceY - distanceX);
    private static float GetDistance(this Node nodeA, Node nodeB) => GetDistance(Math.Abs(nodeA.X - nodeB.X), Math.Abs(nodeA.Y - nodeB.Y));
    private static float GetDistance(this Vector2Int a, Vector2Int b) => GetDistance(Math.Abs(a.x - b.x), Math.Abs(a.y - b.y));
}
