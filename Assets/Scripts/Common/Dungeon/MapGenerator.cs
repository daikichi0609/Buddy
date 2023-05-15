using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//https://www.kurage.net/game-dev/187

public class Position
{
    public int X { get; set; }
    public int Y { get; set; }

    public Position(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Position() : this(0, 0) { }

    public override string ToString()
    {
        return string.Format("({0}, {1})", X, Y);
    }
}

public class Range
{
    public Position Start { get; set; }
    public Position End { get; set; }

    public int GetWidthX() => End.X - Start.X + 1;

    public int GetWidthY() => End.Y - Start.Y + 1;

    public Range(Position start, Position end)
    {
        Start = start;
        End = end;
    }

    public Range(int startX, int startY, int endX, int endY) : this(new Position(startX, startY), new Position(endX, endY)) { }

    public Range() : this(0, 0, 0, 0) { }

    public override string ToString()
    {
        return string.Format("{0} => {1}", Start, End);
    }

}

public class RogueUtils
{
    public static int GetRandomInt(int min, int max)
    {
        return min + Mathf.FloorToInt(Random.value * (max - min + 1));
    }

    public static bool RandomJadge(float rate)
    {
        return Random.value < rate;
    }
}

public class MapInfo
{
    public MapInfo(TERRAIN_ID[,] map, List<Range> rangeList)
    {
        Map = map;
        RangeList = rangeList;
    }

    public TERRAIN_ID[,] Map { get; }
    public List<Range> RangeList { get; }
}

public static class MapGenerator
{

    private const int MINIMUM_RANGE_WIDTH = 6;

    private static int m_MapSizeX;
    private static int m_MapSizeY;
    private static int m_MaxRoom;

    private static List<Range> m_RoomList = new List<Range>();
    private static List<Range> m_RangeList = new List<Range>();
    private static List<Range> m_PassList = new List<Range>();
    private static List<Range> m_RoomPassList = new List<Range>();

    public static MapInfo GenerateMap(int mapSizeX, int mapSizeY, int maxRoom)
    {
        m_MapSizeX = mapSizeX;
        m_MapSizeY = mapSizeY;

        TERRAIN_ID[,] map = new TERRAIN_ID[mapSizeX, mapSizeY];

        Initialize();

        CreateRange(maxRoom);
        CreateRoom();

        // ここまでの結果を一度配列に反映する
        foreach (Range pass in m_PassList)
        {
            for (int x = pass.Start.X; x <= pass.End.X; x++)
            {
                for (int y = pass.Start.Y; y <= pass.End.Y; y++)
                {
                    map[x, y] = TERRAIN_ID.PATH_WAY;
                }
            }
        }
        foreach (Range roomPass in m_RoomPassList)
        {
            for (int x = roomPass.Start.X; x <= roomPass.End.X; x++)
            {
                for (int y = roomPass.Start.Y; y <= roomPass.End.Y; y++)
                {
                    map[x, y] = TERRAIN_ID.PATH_WAY;
                }
            }
        }
        foreach (Range room in m_RoomList)
        {
            for (int x = room.Start.X; x <= room.End.X; x++)
            {
                for (int y = room.Start.Y; y <= room.End.Y; y++)
                {
                    map[x, y] = TERRAIN_ID.ROOM;
                }
            }
        }

        TrimPassList(ref map);

        return new MapInfo(map, m_RoomList);
    }

    private static void Initialize()
    {
        m_RoomList = new List<Range>();
        m_RangeList = new List<Range>();
        m_PassList = new List<Range>();
        m_RoomPassList = new List<Range>();
    }

    public static void CreateRange(int maxRoom)
    {
        // 区画のリストの初期値としてマップ全体を入れる
        m_RangeList.Add(new Range(0, 0, m_MapSizeX - 1, m_MapSizeY - 1));

        bool isDevided;
        do
        {
            // 縦 → 横 の順番で部屋を区切っていく。一つも区切らなかったら終了
            isDevided = DevideRange(false);
            isDevided = DevideRange(true) || isDevided;

            // もしくは最大区画数を超えたら終了
            if (m_RangeList.Count >= maxRoom)
            {
                break;
            }
        } while (isDevided);

    }

    public static bool DevideRange(bool isVertical)
    {
        bool isDevided = false;

        // 区画ごとに切るかどうか判定する
        List<Range> newRangeList = new List<Range>();
        foreach (Range range in m_RangeList)
        {
            // これ以上分割できない場合はスキップ
            if (isVertical && range.GetWidthY() < MINIMUM_RANGE_WIDTH * 2 + 1)
            {
                continue;
            }
            else if (!isVertical && range.GetWidthX() < MINIMUM_RANGE_WIDTH * 2 + 1)
            {
                continue;
            }

            System.Threading.Thread.Sleep(1);

            // 40％の確率で分割しない
            // ただし、区画の数が1つの時は必ず分割する
            if (m_RangeList.Count > 2 && RogueUtils.RandomJadge(0.4f))
            {
                continue;
            }

            // 長さから最少の区画サイズ2つ分を引き、残りからランダムで分割位置を決める
            int length = isVertical ? range.GetWidthY() : range.GetWidthX();
            int margin = length - MINIMUM_RANGE_WIDTH * 2;
            int baseIndex = isVertical ? range.Start.Y : range.Start.X;
            int devideIndex = baseIndex + MINIMUM_RANGE_WIDTH + RogueUtils.GetRandomInt(1, margin) - 1;

            // 分割された区画の大きさを変更し、新しい区画を追加リストに追加する
            // 同時に、分割した境界を通路として保存しておく
            Range newRange = new Range();
            if (isVertical)
            {
                m_PassList.Add(new Range(range.Start.X, devideIndex, range.End.X, devideIndex));
                newRange = new Range(range.Start.X, devideIndex + 1, range.End.X, range.End.Y);
                range.End.Y = devideIndex - 1;
            }
            else
            {
                m_PassList.Add(new Range(devideIndex, range.Start.Y, devideIndex, range.End.Y));
                newRange = new Range(devideIndex + 1, range.Start.Y, range.End.X, range.End.Y);
                range.End.X = devideIndex - 1;
            }

            // 追加リストに新しい区画を退避する。
            newRangeList.Add(newRange);

            isDevided = true;
        }

        // 追加リストに退避しておいた新しい区画を追加する。
        m_RangeList.AddRange(newRangeList);

        return isDevided;
    }

    private static void CreateRoom()
    {
        // 部屋のない区画が偏らないようにリストをシャッフルする
        m_RangeList.Sort((a, b) => RogueUtils.GetRandomInt(0, 1) - 1);

        // 1区画あたり1部屋を作っていく。作らない区画もあり。
        foreach (Range range in m_RangeList)
        {
            System.Threading.Thread.Sleep(1);
            // 30％の確率で部屋を作らない
            // ただし、最大部屋数の半分に満たない場合は作る
            if (m_RoomList.Count > m_MaxRoom / 2 && RogueUtils.RandomJadge(0.3f))
            {
                continue;
            }

            // 猶予を計算
            int marginX = range.GetWidthX() - MINIMUM_RANGE_WIDTH + 1;
            int marginY = range.GetWidthY() - MINIMUM_RANGE_WIDTH + 1;

            // 開始位置を決定
            int randomX = RogueUtils.GetRandomInt(1, marginX);
            int randomY = RogueUtils.GetRandomInt(1, marginY);

            // 座標を算出
            int startX = range.Start.X + randomX;
            int endX = range.End.X - RogueUtils.GetRandomInt(0, (marginX - randomX)) - 1;
            int startY = range.Start.Y + randomY;
            int endY = range.End.Y - RogueUtils.GetRandomInt(0, (marginY - randomY)) - 1;

            // 部屋リストへ追加
            Range room = new Range(startX, startY, endX, endY);
            m_RoomList.Add(room);

            // 通路を作る
            CreatePass(range, room);
        }
    }

    private static void CreatePass(Range range, Range room)
    {
        List<int> directionList = new List<int>();
        if (range.Start.X != 0)
        {
            // Xマイナス方向
            directionList.Add(0);
        }
        if (range.End.X != m_MapSizeX - 1)
        {
            // Xプラス方向
            directionList.Add(1);
        }
        if (range.Start.Y != 0)
        {
            // Yマイナス方向
            directionList.Add(2);
        }
        if (range.End.Y != m_MapSizeY - 1)
        {
            // Yプラス方向
            directionList.Add(3);
        }

        // 通路の有無が偏らないよう、リストをシャッフルする
        directionList.Sort((a, b) => RogueUtils.GetRandomInt(0, 1) - 1);

        bool isFirst = true;
        foreach (int direction in directionList)
        {
            System.Threading.Thread.Sleep(1);
            // 80%の確率で通路を作らない
            // ただし、まだ通路がない場合は必ず作る
            if (!isFirst && RogueUtils.RandomJadge(0.8f))
            {
                continue;
            }
            else
            {
                isFirst = false;
            }

            // 向きの判定
            int random;
            switch (direction)
            {
                case 0: // Xマイナス方向
                    random = room.Start.Y + RogueUtils.GetRandomInt(1, room.GetWidthY()) - 1;
                    m_RoomPassList.Add(new Range(range.Start.X, random, room.Start.X - 1, random));
                    break;

                case 1: // Xプラス方向
                    random = room.Start.Y + RogueUtils.GetRandomInt(1, room.GetWidthY()) - 1;
                    m_RoomPassList.Add(new Range(room.End.X + 1, random, range.End.X, random));
                    break;

                case 2: // Yマイナス方向
                    random = room.Start.X + RogueUtils.GetRandomInt(1, room.GetWidthX()) - 1;
                    m_RoomPassList.Add(new Range(random, range.Start.Y, random, room.Start.Y - 1));
                    break;

                case 3: // Yプラス方向
                    random = room.Start.X + RogueUtils.GetRandomInt(1, room.GetWidthX()) - 1;
                    m_RoomPassList.Add(new Range(random, room.End.Y + 1, random, range.End.Y));
                    break;
            }
        }

    }

    private static void TrimPassList(ref TERRAIN_ID[,] map)
    {
        // どの部屋通路からも接続されなかった通路を削除する
        for (int i = m_PassList.Count - 1; i >= 0; i--)
        {
            Range pass = m_PassList[i];

            bool isVertical = pass.GetWidthY() > 1;

            // 通路が部屋通路から接続されているかチェック
            bool isTrimTarget = true;
            if (isVertical)
            {
                int x = pass.Start.X;
                for (int y = pass.Start.Y; y <= pass.End.Y; y++)
                {
                    if (map[x - 1, y] == TERRAIN_ID.PATH_WAY || map[x + 1, y] == TERRAIN_ID.PATH_WAY)
                    {
                        isTrimTarget = false;
                        break;
                    }
                }
            }
            else
            {
                int y = pass.Start.Y;
                for (int x = pass.Start.X; x <= pass.End.X; x++)
                {
                    if (map[x, y - 1] == TERRAIN_ID.PATH_WAY || map[x, y + 1] == TERRAIN_ID.PATH_WAY)
                    {
                        isTrimTarget = false;
                        break;
                    }
                }
            }

            // 削除対象となった通路を削除する
            if (isTrimTarget)
            {
                m_PassList.Remove(pass);

                // マップ配列からも削除
                if (isVertical)
                {
                    int x = pass.Start.X;
                    for (int y = pass.Start.Y; y <= pass.End.Y; y++)
                    {
                        map[x, y] = 0;
                    }
                }
                else
                {
                    int y = pass.Start.Y;
                    for (int x = pass.Start.X; x <= pass.End.X; x++)
                    {
                        map[x, y] = 0;
                    }
                }
            }
        }

        // 外周に接している通路を別の通路との接続点まで削除する
        // 上下基準
        for (int x = 0; x < m_MapSizeX - 1; x++)
        {
            if (map[x, 0] == TERRAIN_ID.PATH_WAY)
            {
                for (int y = 0; y < m_MapSizeY; y++)
                {
                    if (map[x - 1, y] == TERRAIN_ID.PATH_WAY || map[x + 1, y] == TERRAIN_ID.PATH_WAY)
                    {
                        break;
                    }
                    map[x, y] = 0;
                }
            }
            if (map[x, m_MapSizeY - 1] == TERRAIN_ID.PATH_WAY)
            {
                for (int y = m_MapSizeY - 1; y >= 0; y--)
                {
                    if (map[x - 1, y] == TERRAIN_ID.PATH_WAY || map[x + 1, y] == TERRAIN_ID.PATH_WAY)
                    {
                        break;
                    }
                    map[x, y] = 0;
                }
            }
        }
        // 左右基準
        for (int y = 0; y < m_MapSizeY - 1; y++)
        {
            if (map[0, y] == TERRAIN_ID.PATH_WAY)
            {
                for (int x = 0; x < m_MapSizeY; x++)
                {
                    if (map[x, y - 1] == TERRAIN_ID.PATH_WAY || map[x, y + 1] == TERRAIN_ID.PATH_WAY)
                    {
                        break;
                    }
                    map[x, y] = 0;
                }
            }
            if (map[m_MapSizeX - 1, y] == TERRAIN_ID.PATH_WAY)
            {
                for (int x = m_MapSizeX - 1; x >= 0; x--)
                {
                    if (map[x, y - 1] == TERRAIN_ID.PATH_WAY || map[x, y + 1] == TERRAIN_ID.PATH_WAY)
                    {
                        break;
                    }
                    map[x, y] = 0;
                }
            }
        }
    }

}
