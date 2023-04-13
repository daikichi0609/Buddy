using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

[CreateAssetMenu(menuName = "MyScriptable/Item/DeploySetup")]
public class ItemDeploySetup : ScriptableObject
{
    [Serializable]
    public class ItemPack
    {
        /// <summary>
        /// 罠セットアップ
        /// </summary>
        [SerializeField, Expandable]
        private ItemSetup m_Setup;
        public ItemSetup Setup => m_Setup;

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
    private ItemPack[] m_ItemPacks;
    public ItemPack[] ItemPacks => m_ItemPacks;
}