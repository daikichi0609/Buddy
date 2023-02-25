using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHolder : Singleton<ItemHolder>
{
    public GameObject ItemObject(ITEM_NAME name)
    {
        switch (name)
        {
            case ITEM_NAME.APPLE:
                return m_Apple;
        }
        return null;
    }

    [SerializeField]
    private GameObject m_Apple;
}
