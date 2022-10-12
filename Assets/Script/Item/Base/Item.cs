using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Item : MonoBehaviour
{
    /// <summary>
    /// アイテム名
    /// </summary>
    public virtual Define.ITEM_NAME Name
    {
        get;
    }

    /// <summary>
    /// アイテムの位置
    /// </summary>
    [SerializeField] private Vector3 m_Position;
    public Vector3 Position
    {
        get => m_Position;
        set => m_Position = value;
    }

    /// <summary>
    /// アイテム効果
    /// </summary>
    public virtual Action Method
    {
        get;
    }
}