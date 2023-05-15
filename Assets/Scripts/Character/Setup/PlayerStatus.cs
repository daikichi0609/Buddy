using UnityEngine;
using System;

[CreateAssetMenu(menuName = "MyScriptable/Character/Status/PlayerStatus")]
[System.Serializable] //定義したクラスをJSONデータに変換できるようにする
public class PlayerStatus : BattleStatus
{
    [SerializeField, Label("Playerパラメータ")]
    private PlayerParameter m_Param;
    public PlayerParameter Param => m_Param;

    [Serializable]
    public class PlayerParameter : Parameter
    {
        public PlayerParameter(PlayerParameter param) : base(param) { }
    }
}