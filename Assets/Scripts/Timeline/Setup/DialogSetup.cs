using NaughtyAttributes;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptable/Dialog")]
public class DialogSetup : ScriptableObject
{
    [Serializable]
    public sealed class DialogPack
    {
        /// <summary>
        /// 表示テキスト
        /// </summary>
        [SerializeField, ResizableTextArea]
        public string m_Text;

        /// <summary>
        /// 表示時間
        /// </summary>
        [SerializeField]
        public float m_Time;
    }

    /// <summary>
    /// ダイアログ設定
    /// </summary>
    [SerializeField, ReorderableList, Header("ダイアログ")]
    private DialogPack[] m_DialogPacks;
    public DialogPack[] DialogPacks => m_DialogPacks;
}