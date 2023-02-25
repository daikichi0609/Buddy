using UnityEngine;

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
            m_Dex = param.Dex;
            m_Eva = param.Eva;
            m_CriticalRate = param.CriticalRate;
            m_Res = param.Res;
        }

        // 名前
        [SerializeField, Label("名前")]
        private CHARA_NAME m_GivenName;
        public CHARA_NAME GivenName => m_GivenName;

        // ヒットポイント
        [SerializeField, Label("体力")]
        private int m_MaxHp;
        public int MaxHp => m_MaxHp;

        // 攻撃力
        [SerializeField, Label("攻撃力")]
        private int m_Atk;
        public int Atk => m_Atk;

        // 防御力
        [SerializeField, Label("防御力")]
        private int m_Def;
        public int Def => m_Def;

        // 速さ
        [SerializeField, Label("速さ")]
        private int m_Agi;
        public int Agi => m_Agi;

        // 命中率補正
        [SerializeField, Label("命中率補正")]
        private float m_Dex;
        public float Dex => m_Dex;

        // 回避率補正
        [SerializeField, Label("回避率補正")]
        private float m_Eva;
        public float Eva => m_Eva;

        // 会心率補正
        [SerializeField, Label("会心率補正")]
        private float m_CriticalRate;
        public float CriticalRate => m_CriticalRate;

        // 抵抗率
        [SerializeField, Label("抵抗率")]
        private float m_Res;
        public float Res => m_Res;
    }
}