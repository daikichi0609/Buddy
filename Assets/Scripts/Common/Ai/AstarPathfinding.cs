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
        /// 親ノード
        /// </summary>
        public Node Parent { get; }

        /// <summary>
        /// X座標
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Y座標
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// 今までの移動コスト
        /// </summary>
        public float TotalMoveCost { get; }

        /// <summary>
        /// 推定コスト（ゴールまでの距離）
        /// </summary>
        public float HeuristicCost { get; }

        /// <summary>
        /// スコア（スコア = 今までの移動コスト + 推定コスト）
        /// </summary>
        public float Score => TotalMoveCost + HeuristicCost;

        /// <summary>
        /// コンストラクタで座標情報と移動コストとヒューリスティックコストをセット
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Node(Node parent, int x, int y, float mCost, Vector2Int goal)
        {
            this.Parent = parent;
            this.X = x;
            this.Y = y;
            this.TotalMoveCost = mCost;
            this.HeuristicCost = new Vector2Int(x, y).GetDistance(goal);
        }

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
    public static bool TryGetSamePositionNode(this List<Node> nodes, Vector2Int pos, out Node old)
    {
        old = null;

        foreach (var node in nodes)
            if (node.Equals(pos) == true)
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
        var startNode = new Node(null, startPos.x, startPos.y, 0, goalPos);

        List<Node> openList = new List<Node>(); // 調査対象ノード
        List<Node> closeList = new List<Node>(); // 調査済みノード
        openList.Add(startNode);

        int calculationCount = 0;

        // OpenListが空になるまで続ける
        while (openList.Count > 0)
        {
            Node currentNode = openList[0];

            // openListの中で、スコアが最小のノードを探す
            for (int i = 0; i < openList.Count; i++)
            {
                if (openList[i].Score < currentNode.Score ||
                    openList[i].Score == currentNode.Score && openList[i].TotalMoveCost < currentNode.TotalMoveCost)
                {
                    currentNode = openList[i];
                }
            }

            // オープンリストからクローズリストに移動（調査済みとしてマーク）
            openList.Remove(currentNode);
            closeList.Add(currentNode);
            // goalと同じノードなら終了
            if (currentNode.Equals(goalPos) == true)
            {
#if DEBUG
                Debug.Log("計算量：" + calculationCount);
#endif
                return RetracePath(startNode, currentNode);
            }

            calculationCount++;
            // 隣接するノードを調査する
            OpenNeighborNode(currentNode, grid, goalPos, openList, closeList);
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

        while (currentNode != null)
        {
            path.Add(currentNode);
            currentNode = currentNode.Parent;
        }

        path.Reverse();
        return path;
    }

    /// <summary>
    /// 近傍のノードを調査する
    /// </summary>
    /// <param name="node"></param>
    /// <param name="grid"></param>
    /// <returns></returns>
    private static void OpenNeighborNode(Node node, int[,] grid, Vector2Int goalPos, List<Node> openList, List<Node> closeList)
    {
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

                    Vector2Int pos = new Vector2Int(checkX, checkY); // Node位置
                    float totalMoveCost = node.TotalMoveCost + moveCost; // トータル移動コスト計算

                    // 既に調査済みである
                    if (closeList.TryGetSamePositionNode(pos, out var close) == true)
                    {
                        // トータル移動コストが既存Node以上なら差し替え不要
                        if (totalMoveCost >= close.TotalMoveCost)
                            continue;
                        closeList.Remove(close);
                    }

                    // 現在調査中である
                    if (openList.TryGetSamePositionNode(pos, out var open) == true)
                    {
                        // トータル移動コストが既存Node以上なら差し替え不要
                        if (totalMoveCost >= open.TotalMoveCost)
                            continue;
                        openList.Remove(open);
                    }

                    Node neighbor = new Node(node, checkX, checkY, totalMoveCost, goalPos);
                    openList.Add(neighbor);
                }
            }
        }
    }

    /// <summary>
    /// 二点間の改良版チェビシェフ距離を計算する（斜め移動時に距離増）
    /// ヒューリスティックコストに使う
    /// </summary>
    /// <param name="distanceX"></param>
    /// <param name="distanceY"></param>
    /// <returns></returns>
    private static float GetDistance(int distanceX, int distanceY) => distanceX > distanceY ? distanceY * SQUARE2 + (distanceX - distanceY) : distanceX * SQUARE2 + (distanceY - distanceX);
    private static float GetDistance(this Node nodeA, Node nodeB) => GetDistance(Math.Abs(nodeA.X - nodeB.X), Math.Abs(nodeA.Y - nodeB.Y));
    private static float GetDistance(this Vector2Int a, Vector2Int b) => GetDistance(Math.Abs(a.x - b.x), Math.Abs(a.y - b.y));
}
