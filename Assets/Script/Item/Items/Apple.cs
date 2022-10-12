using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Apple : Item
{ 
    public override Define.ITEM_NAME Name => Define.ITEM_NAME.APPLE;

    public override Action Method => () =>
    {
        Debug.Log("りんごおいしい");
    };
}
