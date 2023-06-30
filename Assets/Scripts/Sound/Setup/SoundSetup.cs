using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

[CreateAssetMenu(menuName = "MyScriptable/Sound/SoundSetup")]
public class SoundSetup : ScriptableObject
{
    [Serializable]
    public class SoundPack
    {
        /// <summary>
        /// サウンドプレハブ
        /// </summary>
        [SerializeField, Header("プレハブ")]
        private GameObject m_Sound;
        public GameObject Sound => m_Sound;

        /// <summary>
        /// キー
        /// </summary>
        [SerializeField, ResizableTextArea, Header("キー")]
        private string m_Key;
        public string Key => m_Key;
    }

    /// <summary>
    /// サウンド設定
    /// </summary>
    [SerializeField, ReorderableList, Header("サウンド")]
    private SoundPack[] m_SoundPacks;
    public SoundPack[] SoundPacks => m_SoundPacks;
}