using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Utility
{
    public static void Shuffle<T>(this IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            var tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }

    public static Vector3 Direction(Vector3 direction) //移動方向を返す
    {
        int x = (int)direction.x;
        int z = (int)direction.z;

        if (x < 0)
        {
            x = -1;
        }
        else if (x > 0)
        {
            x = 1;
        }

        if (z < 0)
        {
            z = -1;
        }
        else if (z > 0)
        {
            z = 1;
        }

        return new Vector3(x, 0, z);
    }

    //敵AI

    public static EnemyActionAndTarget CreateActionAndTarget(InternalDefine.ENEMY_STATE action, List<GameObject> targetList)
    {
        return new EnemyActionAndTarget(action, targetList);
    }

    public static EnemyActionAndTarget DecideActionAndTarget(List<Vector3> attackPosList, Vector3 pos)
    {
        List<GameObject> targetList = CreateTargetList_Attack(attackPosList);
        if (targetList.Count >= 1)
        {
            return CreateActionAndTarget(InternalDefine.ENEMY_STATE.ATTACKING, targetList);
        }

        targetList = CreateTargetList_Chase(pos);
        if (targetList.Count >= 1)
        {
            return CreateActionAndTarget(InternalDefine.ENEMY_STATE.CHASING, targetList);
        }

        targetList = CreateTargetList_Search();
        return CreateActionAndTarget(InternalDefine.ENEMY_STATE.SEARCHING, targetList);
    }

    private static List<GameObject> CreateTargetList_Attack(List<Vector3> attackPosList)
    {
        List<GameObject> targetList = new List<GameObject>();
        foreach (Vector3 attackPos in attackPosList)
        {
            GameObject player = ObjectManager.Instance.SpecifiedPositionPlayerObject(attackPos);
            if (player != null)
            {
                targetList.Add(player);
            }
        }
        return targetList;
    }

    private static List<GameObject> CreateTargetList_Chase(Vector3 pos)
    {
        List<GameObject> targetList = new List<GameObject>();
        int roomId = Positional.IsOnRoomID(pos);
        if (roomId == 0)
        {
            return targetList;
        }
        targetList = ObjectManager.Instance.SpecifiedRoomPlayerObjectList(roomId);

        return targetList;
    }

    private static List<GameObject> CreateTargetList_Search()
    {
        return new List<GameObject>();
    }

    public static Define.CHARA_NAME RandomEnemyName()
    {
        return Define.CHARA_NAME.MASHROOM;
    }

    public static Define.ITEM_NAME RandomItemName()
    {
        return Define.ITEM_NAME.APPLE;
    }
}

public static class Calculator
{
    public static float CalculateNormalAttackMag(int lv, float mag)
    {
        switch (lv)
        {
            case 1:
                return mag * 1.0f;

            case 2:
                return mag * 1.5f;

            case 3:
                return mag * 2.0f;

            case 4:
                return mag * 2.5f;

            case 5:
                return mag * 3.5f;
        }
        return 0f;
    }

    public static int CalculatePower(int atk, float mag)
    {
        return (int)(atk * mag);
    }

    public static int CalculateDamage(int power, int def)
    {
        int damage = power - def;
        if (damage < 1)
        {
            damage = 1;
        }
        return damage;
    }

    public static int CalculateRemainingHp(int hp, int damage)
    {
        int remainingHp = hp - damage;
        if (remainingHp < 0)
        {
            remainingHp = 0;
        }
        return remainingHp;
    }

    public static bool JudgeHit(float dex, float eva)
    {
        if(UnityEngine.Random.Range(1,101) >= dex * 100)
        {
            return false;
        }
        if(UnityEngine.Random.Range(1, 101) <= eva * 100)
        {
            return false;
        }
        return true;
    }
}

public static class Coroutine
{
    public static IEnumerator DelayCoroutine(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }
}