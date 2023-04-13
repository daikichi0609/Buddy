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
