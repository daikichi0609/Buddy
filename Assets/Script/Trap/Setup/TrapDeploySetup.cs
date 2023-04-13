using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

[CreateAssetMenu(menuName = "MyScriptable/Trap/DeploySetup")]
public class TrapDeploySetup : ScriptableObject
{
    [Serializable]
    public class TrapPack
    {
        /// <summary>
        /// 罠セットアップ
        /// </summary>
        [SerializeField, Expandable]
        private TrapSetup m_Setup;
        public TrapSetup Setup => m_Setup;

        /// <summary>
        /// 抽選用重み
        /// </summary>
        [SerializeField, Range(0, 100)]
        private int m_Weight;
        public int Weight => m_Weight;
    }

    /// <summary>
    /// 罠設定コレクション
    /// </summary>
    [SerializeField, ReorderableList]
    private TrapPack[] m_TrapPacks;
    public TrapPack[] TrapPacks => m_TrapPacks;
}
