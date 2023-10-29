using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

[CreateAssetMenu(menuName = "MyScriptable/Character/EnemyTableSetup")]
[Serializable]
public class EnemyTableSetup : ScriptableObject
{
    [SerializeField, ReorderableList]
    private EnemyPack[] m_EnemyPack = new EnemyPack[0];
    public EnemyPack[] EnemyPacks => m_EnemyPack;

    [Serializable]
    public class EnemyPack
    {
        /// <summary>
        /// キャラ設定
        /// </summary>
        [SerializeField]
        private CharacterSetup m_Setup;
        public CharacterSetup Setup => m_Setup;

        /// <summary>
        /// 抽選用重み
        /// </summary>
        [SerializeField, Range(0, 100)]
        private int m_Weight;
        public int Weight => m_Weight;
    }
}

public static class EnemyTableSetupExtension
{
    /// <summary>
    /// ランダムな敵キャラセットアップを重み抽選
    /// </summary>
    /// <returns></returns>
    public static CharacterSetup GetRandomEnemySetup(this EnemyTableSetup t)
    {
        int length = t.EnemyPacks.Length;
        int[] weights = new int[length];

        for (int i = 0; i < length; i++)
            weights[i] = t.EnemyPacks[i].Weight;

        var index = WeightedRandomSelector.SelectIndex(weights);
        return t.EnemyPacks[index].Setup;
    }
}