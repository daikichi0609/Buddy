using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public interface ICharaObjectHolder : ICharacterComponent
{
    GameObject MoveObject { get; }

    GameObject CharaObject { get; }
}

public class CharaObjectHolder : CharaComponentBase, ICharaObjectHolder
{
    public CharaObjectHolder(GameObject chara, GameObject move)
    {
        m_CharaObject = chara;
        m_MoveObject = move;
    }

    /// <summary>
    /// キャラのオブジェクト
    /// </summary>
    [SerializeField]
    private GameObject m_CharaObject;
    GameObject ICharaObjectHolder.CharaObject => m_CharaObject;

    /// <summary>
    /// 移動用オブジェクト
    /// </summary>
    [SerializeField]
    private GameObject m_MoveObject;
    GameObject ICharaObjectHolder.MoveObject => m_MoveObject;
}