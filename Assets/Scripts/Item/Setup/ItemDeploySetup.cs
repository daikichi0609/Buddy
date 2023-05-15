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
        [SerializeField, Expandable, Header("アイテムセットアップ")]
        private ItemSetup m_Setup;
        public ItemSetup Setup => m_Setup;

        /// <summary>
        /// 抽選用重み
        /// </summary>
        [SerializeField, Range(0, 100), Header("重み")]
        private int m_Weight;
        public int Weight => m_Weight;
    }

    /// <summary>
    /// 罠設定コレクション
    /// </summary>
    [SerializeField, ReorderableList, Header("アイテム")]
    private ItemPack[] m_ItemPacks;
    public ItemPack[] ItemPacks => m_ItemPacks;
}