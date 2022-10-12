using UnityEngine;

public abstract class BattleStatus: ScriptableObject //キャラ共通で必要なパラメタまとめ
{
	public class Parameter
	{
		[SerializeField, Label("名前")] private Define.CHARA_NAME m_GivenName;
		public Define.CHARA_NAME Name
		{
			get { return m_GivenName; }
			set { m_GivenName = value; }
		}

		// ヒットポイント
		[SerializeField, Label("体力")] private int m_Hp;
		public int Hp
		{
			get { return m_Hp; }
			set { m_Hp = value; }
		}
		// 攻撃力
		[SerializeField, Label("攻撃力")] private int m_Atk;
		public int Atk
		{
			get { return m_Atk; }
			set { m_Atk = value; }
		}
		// 防御力
		[SerializeField, Label("防御力")] private int m_Def;
		public int Def
		{
			get { return m_Def; }
			set { m_Def = value; }
		}
		// 速さ
		[SerializeField, Label("速さ")] private int m_Agi;
		public int Agi
		{
			get { return m_Agi; }
			set { m_Agi = value; }
		}
		// 命中率補正
		[SerializeField, Label("命中率補正")] private float m_Dex;
		public float Dex
		{
			get { return m_Dex; }
			set { m_Dex = value; }
		}
		// 回避率補正
		[SerializeField, Label("回避率補正")] private float m_Eva;
		public float Eva
		{
			get { return m_Eva; }
			set { m_Eva = value; }
		}
		// 会心率補正
		[SerializeField, Label("会心率補正")] private float m_CriticalRank;
		public float CriticalRate
		{
			get { return m_CriticalRank; }
			set { m_CriticalRank = value; }
		}
		// 抵抗率
		[SerializeField, Label("抵抗率")] private float m_Res;
		public float Res
		{
			get { return m_Res; }
			set { m_Res = value; }
		}
	}
}