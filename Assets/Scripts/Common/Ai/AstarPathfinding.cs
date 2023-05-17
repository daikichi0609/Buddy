using System;
using System.Collections.Generic;
using UnityEngine;

public static class AStarSearch
{
    private static readonly float SQRT_2 = 1.4f;

    // ノードクラス
    public class Node : IEquatable<Node>
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
        /// 推定コスト（ゴールまでの距離）
        /// </summary>
        public float HeuristicCost { get; }

        /// <summary>
        /// 実コスト（今まで何歩歩いたか）
        /// </summary>
        public float Cost { get; private set; }

        /// <summary>
        /// 合計コスト（総コスト＝実コスト＋推定コスト）
        /// </summary>
        public float SumCost => Cost + HeuristicCost;

        /// <summary>
        /// 親ノード
        /// </summary>
        public Node Parent { get; private set; }

        /// <summary>
        /// コンストラクタは座標情報とヒューリスティックコスト
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Node(int x, int y, float hCost)
        {
            this.X = x;
            this.Y = y;
            this.HeuristicCost = hCost;
        }
        public Node(int x, int y, Vector2Int goal)
        {
            this.X = x;
            this.Y = y;
            this.HeuristicCost = new Vector2Int(x, y).GetDistance(goal);
        }

        /// <summary>
        /// Node情報をセット
        /// </summary>
        /// <param name="cost"></param>
        /// <param name="hCost"></param>
        /// <param name="parent"></param>
        public void SetInfo(float cost, Node parent)
        {
            Cost = cost;
            Parent = parent;
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
    /// リストに同じ座標ノードが含まれるか
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool ContainsSamePositionNode(this List<Node> nodes, Node other)
    {
        foreach (var node in nodes)
        {
            if (node.Equals(other) == true)
                return true;
        }
        return false;
    }

    /// <summary>
    /// リストから任意の座標のノードを取得
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static void AddNodeInstead(this List<Node> nodes, Node other)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            if (node.Equals(other) == true)
            {
                nodes.Remove(node);
                nodes.Add(other);
                return;
            }
        }
        Debug.LogError("ノードの入れ替えに失敗");
    }

    /// <summary>
    /// リストから任意のノードを削除
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="other"></param>
    public static void RemoveNode(this List<Node> nodes, Node other)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            if (node.Equals(other) == true)
            {
                nodes.Remove(node);
                return;
            }
        }
        Debug.LogError("ノードの削除に失敗");
    }

    // パス検索のメソッド
    public static List<Node> FindPath(Vector2Int startPos, Vector2Int goalPos, int[,] grid)
    {
        // ノード集合
        var nodeList = new List<Node>();

        // スタートとゴールのノード作成
        var startNode = new Node(startPos.x, startPos.y, startPos.GetDistance(goalPos));

        // startNodeの設定
        startNode.SetInfo(0, null);

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
                // コスト計算
                float newHCost = GetDistance(currentNode, neighbor);
                float newCostToNeighbor = currentNode.Cost + newHCost;
                // コストと親ノードをセット
                neighbor.SetInfo(newCostToNeighbor, currentNode);

                bool existInOpen = openList.ContainsSamePositionNode(neighbor); // オープンリストに存在するか
                bool existInClose = closedList.ContainsSamePositionNode(neighbor); // クローズリストに存在するか

                // オープンとクローズリストに追加するノードがない
                if (existInOpen == false && existInClose == false)
                    openList.Add(neighbor);

                // 新しいノードの方がトータルコストが低い
                else if (newCostToNeighbor < neighbor.Cost)
                {
                    // オープンリストにノードがあるなら情報を上書き
                    if (existInOpen == true)
                        openList.AddNodeInstead(neighbor);

                    // クローズリストにノードがあるならクローズのノードを削除してオープンに追加
                    else if (existInClose == true)
                    {
                        closedList.RemoveNode(neighbor);
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

        while (currentNode.Equals(startNode) == false)
        {
            path.Add(currentNode);
            currentNode = currentNode.Parent;
        }

        path.Reverse();
        return path;
    }

    /// <summary>
    /// 近傍のノードを取得するメソッド
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
                    // 壁は除外
                    if (grid[checkX, checkY] == 0)
                        continue;

                    Node neighbor = new Node(checkX, checkY, goalPos);
                    neighbors.Add(neighbor);
                }
            }
        }

        return neighbors;
    }

    /// <summary>
    /// 二点間の距離を計算するメソッド
    /// 斜め移動可能
    /// ヒューリスティックコストに使う
    /// </summary>
    /// <param name="nodeA"></param>
    /// <param name="nodeB"></param>
    /// <returns></returns>
    private static float GetDistance(this Node nodeA, Node nodeB)
    {
        int distanceX = Math.Abs(nodeA.X - nodeB.X);
        int distanceY = Math.Abs(nodeA.Y - nodeB.Y);

        if (distanceX > distanceY)
            return SQRT_2 * distanceY + (distanceX - distanceY);
        else
            return SQRT_2 * distanceX + (distanceY - distanceX);
    }

    private static float GetDistance(this Vector2Int a, Vector2Int b)
    {
        int distanceX = Math.Abs(a.x - b.x);
        int distanceY = Math.Abs(a.y - b.y);

        // 斜め移動の場合は、コストを1.4倍にする（sqrt(2)≒1.4）
        if (distanceX > distanceY)
            return SQRT_2 * distanceY + (distanceX - distanceY);
        else
            return SQRT_2 * distanceX + (distanceY - distanceX);
    }
}
