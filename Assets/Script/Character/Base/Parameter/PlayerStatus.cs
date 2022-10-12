using UnityEngine;
using System;

[CreateAssetMenu(menuName = "MyScriptable/Create PlayerStatus")]
[System.Serializable] //定義したクラスをJSONデータに変換できるようにする
public class PlayerStatus : BattleStatus
{
	[SerializeField, Label("Playerパラメータ")] private PlayerParameter m_Param;
	public PlayerParameter Param
	{
		get { return m_Param; }
		set { m_Param = value; }
	}

	[Serializable]
	public class PlayerParameter : Parameter
	{
		//満腹度
		[SerializeField, Label("満腹度")] public int m_Satiety;
		public int Satiety
		{
			get { return m_Satiety; }
			set { m_Satiety = value; }
		}

		//運
		[SerializeField, Label("運")] private float m_Luk;
		public float Luk
		{
			get { return m_Luk; }
			set { m_Luk = value; }
		}
	}
}