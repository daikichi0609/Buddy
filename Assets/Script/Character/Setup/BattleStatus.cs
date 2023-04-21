using UnityEngine;
using static PlayerStatus;

public abstract class BattleStatus : ScriptableObject //キャラ共通で必要なパラメタまとめ
{
    public class Parameter
    {
        public Parameter(Parameter param)
        {
            m_GivenName = param.GivenName;
            m_MaxHp = param.MaxHp;
            m_Atk = param.Atk;
            m_Def = param.Def;
            m_Agi = param.Agi;
            m_Res = param.Res;
        }

        // 名前
        [SerializeField, Label("名前")]
        private string m_GivenName;
        public string GivenName => m_GivenName;

        // ヒットポイント
        [SerializeField, Label("体力")]
        private int m_MaxHp = 100;
        public int MaxHp => m_MaxHp;

        // 攻撃力
        [SerializeField, Label("攻撃力")]
        private int m_Atk = 10;
        public int Atk => m_Atk;

        // 防御力
        [SerializeField, Label("防御力")]
        private int m_Def = 10;
        public int Def => m_Def;

        // 速さ
        [SerializeField, Label("速さ")]
        private int m_Agi = 1;
        public int Agi => m_Agi;

        // 抵抗率
        [SerializeField, Label("抵抗率")]
        private float m_Res = 0.1f;
        public float Res => m_Res;
    }
}