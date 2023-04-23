using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Utility
{
    public static T RandomLottery<T>(this IList<T> list)
    {
        var index = UnityEngine.Random.Range(0, list.Count);
        return list[index];
    }
}

public static class Calculator
{
    public static int CalculatePower(int atk, float mag)
    {
        return (int)(atk * mag);
    }

    public static int CalculateDamage(int power, int def)
    {
        int damage = power - def;

        if (damage < 1)
            damage = 1;

        return damage;
    }

    public static int CalculateRemainingHp(int hp, int damage)
    {
        hp -= damage;

        if (hp < 0)
            hp = 0;

        return hp;
    }

    public static bool JudgeHit(float dex)
    {
        if (UnityEngine.Random.Range(0, 1f) >= dex)
            return false;

        return true;
    }
}

public static class Coroutine
{
    public static IEnumerator DelayCoroutine(float seconds, System.Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }
}