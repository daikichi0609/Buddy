using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

[CreateAssetMenu(menuName = "MyScriptable/Effect/EffectSetup")]
public class EffectSetup : ScriptableObject
{
    [Serializable]
    public class EffectPack
    {
        /// <summary>
        /// エフェクトプレハブ
        /// </summary>
        [SerializeField, Header("プレハブ")]
        private GameObject m_Effect;
        public GameObject Effect => m_Effect;

        /// <summary>
        /// キー
        /// </summary>
        [SerializeField, Header("キー")]
        private string m_Key;
        public string Key => m_Key;
    }

    /// <summary>
    /// エフェクト設定
    /// </summary>
    [SerializeField, ReorderableList, Header("エフェクト")]
    private EffectPack[] m_EffectPacks;
    public EffectPack[] EffectPacks => m_EffectPacks;
}