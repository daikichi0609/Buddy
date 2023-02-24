public class CurrentStatus
{
    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="param"></param>
    public CurrentStatus(BattleStatus.Parameter param)
    {
        Name = param.Name;
        Hp = param.MaxHp;
        Atk = param.Atk;
        Def = param.Def;
        Agi = param.Agi;
        Dex = param.Dex;
        Eva = param.Eva;
        CriticalRate = param.CriticalRate;
        Res = param.Res;
    }

    public Define.CHARA_NAME Name { get; }

    // レベル
    public int Lv { get; set; } = 1;

    // ヒットポイント
    public int Hp { get; set; }

    // 攻撃力
    public int Atk { get; set; }

    // 防御力
    public int Def { get; set; }

    // 速さ
    public int Agi { get; set; }

    // 命中率補正
    public float Dex { get; set; }

    // 回避率補正
    public float Eva { get; set; }

    // 会心率補正
    public float CriticalRate { get; set; }

    // 抵抗率
    public float Res { get; set; }
}