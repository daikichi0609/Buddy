using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public static class Positional
{
    public static bool IsPossibleToMoveGrid(Vector3 pos, Vector3 direction) //指定座標から指定方向へ１マス移動可能かどうか調べる
    {
        int[,] map = DungeonTerrain.Instance.Map;
        int pos_x = (int)pos.x;
        int pos_z = (int)pos.z;
        int direction_x = (int)direction.x;
        int direction_z = (int)direction.z;

        if (direction_x != 0 && direction_z != 0) //斜め移動の場合、壁が邪魔になっていないかどうかチェックする
        {
            if(DungeonTerrain.Instance.IsPossibleToMoveDiagonal(pos_x, pos_z, direction_x, direction_z) == false)
            {
                return false;
            }
        }

        int id = DungeonTerrain.Instance.DestinationGridID(pos_x, pos_z, direction_x, direction_z);

        if(id == (int)DungeonTerrain.GRID_ID.PATH_WAY || id == (int)DungeonTerrain.GRID_ID.ROOM || id == (int)DungeonTerrain.GRID_ID.GATE || id == (int)DungeonTerrain.GRID_ID.STAIRS)
        {
            return true;
        }
        return false;
    }

    public static bool IsCharacterOn(Vector3 pos)
    {
        if (IsPlayerOn(pos) == true)
        {
            return true;
        }

        if (IsEnemyOn(pos) == true)
        {
            return true;
        }

        return false;
    }

    public static bool IsPlayerOn(Vector3 pos)
    {
        int pos_x = (int)pos.x;
        int pos_z = (int)pos.z;

        foreach (GameObject player in ObjectManager.Instance.m_PlayerList)
        {
            Chara charaMove = player.GetComponent<Chara>();
            if (charaMove.Position.x == pos.x && charaMove.Position.z == pos.z)
            {
                return true;
            }
        }
        return false;
    }

    public static bool IsEnemyOn(Vector3 pos) //指定座標に敵がいるかどうかを調べる
    {
        int pos_x = (int)pos.x;
        int pos_z = (int)pos.z;

        foreach(GameObject enemy in ObjectManager.Instance.m_EnemyList)
        {
            Chara charaMove = enemy.GetComponent<Chara>();
            if (charaMove.Position.x == pos.x && charaMove.Position.z == pos.z)
            {
                return true;
            }
        }
        return false;
    }

    public static bool IsNoOneThere(Vector3 pos) //指定座標に誰もいないかどうかを返す
    {
        foreach(GameObject player in ObjectManager.Instance.m_PlayerList ?? new ReactiveCollection<GameObject>())
        {
            Chara charaMove = player.GetComponent<Chara>();
            if (charaMove.Position.x == pos.x && charaMove.Position.z == pos.z)
            {
                return false;
            }
        }
        foreach (GameObject enemy in ObjectManager.Instance.m_EnemyList ?? new ReactiveCollection<GameObject>())
        {
            Chara charaMove = enemy.GetComponent<Chara>();
            if (charaMove.Position.x == pos.x && charaMove.Position.z == pos.z)
            {
                return false;
            }
        }
        return true;
    }

    public static bool IsNothingThere(Vector3 pos)
    {
        if (IsNoOneThere(pos) == false)
        {
            return false;
        }

        foreach (GameObject itemObj in ObjectManager.Instance.ItemList ?? new List<GameObject>())
        {
            Item item = itemObj.GetComponent<Item>();
            if (item.Position.x == pos.x && item.Position.z == pos.z)
            {
                return false;
            }
        }

        return true;
    }

    public static int IsOnRoomID(Vector3 pos) //指定座標の部屋IDを返す
    {
        return DungeonTerrain.Instance.GetTerrainListObject((int)pos.x, (int)pos.z).GetComponent<Grid>().RoomID;
    }

    public static bool IsPlayerOnSpecifyRoom(int id) //指定IDの部屋にプレイヤーがいるかどうかを返す
    {
        foreach(GameObject player in ObjectManager.Instance.m_PlayerList)
        {
            CharaMove charaMove = player.GetComponent<CharaMove>();
            Vector3 playerPos = charaMove.Position;
            if(IsOnRoomID(playerPos) == id)
            {
                return true;
            }
        }
        return false;
    }
}