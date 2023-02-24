using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// コレクターインターフェイス
/// </summary>
public interface ICollector
{
    /// <summary>
    /// コンポーネント登録
    /// </summary>
    /// <param name="comp"></param>
    void Register<TComp>(TComp comp) where TComp : class, ICharacterComponent;

    /// <summary>
    /// コンポーネント取得
    /// </summary>
    /// <typeparam name="TComp"></typeparam>
    /// <returns></returns>
    TComp GetComponent<TComp>() where TComp : class, ICharacterComponent;

    /// <summary>
    /// コンポーネント要求
    /// </summary>
    /// <param name="comp"></param>
    /// <returns></returns>
    bool RequireComponent<TComp>(out TComp comp) where TComp : class, ICharacterComponent;
}

/// <summary>
/// コンポーネント集約クラス
/// </summary>
public class CharacterComponentCollector : MonoBehaviour, ICollector
{
    private List<ICharacterComponent> m_Components = new List<ICharacterComponent>();

    /// <summary>
    /// Initialize実行
    /// </summary>
    private void Start()
    {
        foreach(var comp in m_Components)
            comp.Initialize();
    }

    void ICollector.Register<TComp>(TComp comp)
    {
        m_Components.Add(comp);
    }

    TComp ICollector.GetComponent<TComp>()
    {
        foreach (var val in m_Components)
            if (val is TComp)
                return val as TComp;

        Debug.LogError("コンポーネントの取得に失敗しました");
        return null;
    }

    bool ICollector.RequireComponent<TComp>(out TComp comp)
    {
        foreach (var val in m_Components)
            if (val is TComp)
            {
                comp = val as TComp;
                return true;
            }

        comp = null;
        return false;
    }
}
