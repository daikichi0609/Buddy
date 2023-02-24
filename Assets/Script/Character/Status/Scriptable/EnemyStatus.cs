using UnityEngine;
using System;

[CreateAssetMenu(menuName = "MyScriptable/Create EnemyData")]
public class EnemyStatus : BattleStatus
{
	[SerializeField, Label("Enemyパラメータ")] private EnemyParameter m_Param;
	public EnemyParameter Param => m_Param;

	[Serializable]
	public class EnemyParameter : Parameter
	{
		public EnemyParameter(EnemyParameter param) : base(param)
		{
			m_Ex = param.Ex;
		}

		//倒されるともらえる経験値
		[SerializeField, Label("獲得経験値")]
		private int m_Ex;
		public int Ex => m_Ex;
	}
}