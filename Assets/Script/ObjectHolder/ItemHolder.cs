using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHolder : SingletonMonoBehaviour<ItemHolder>
{
    public GameObject ItemObject(Define.ITEM_NAME name)
    {
        switch (name)
        {
            case Define.ITEM_NAME.APPLE:
                return Apple;
        }
        return null;
    }

    [SerializeField] private GameObject m_Apple;
    public GameObject Apple
    {
        get { return m_Apple; }
    }
}
