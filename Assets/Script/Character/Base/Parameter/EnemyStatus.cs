using UnityEngine;
using System;

[CreateAssetMenu(menuName = "MyScriptable/Create EnemyData")]
public class EnemyStatus : BattleStatus
{
	[SerializeField, Label("Enemyパラメータ")] private EnemyParameter m_Param;
	public EnemyParameter Param
	{
		get { return m_Param; }
		set { m_Param = value; }
	}

	[Serializable]
	public class EnemyParameter : Parameter
	{
		//倒されるともらえる経験値
		[SerializeField, Label("獲得経験値")] private int m_Ex;
		public int Ex
		{
			get { return m_Ex; }
			set { m_Ex = value; }
		}
	}
}