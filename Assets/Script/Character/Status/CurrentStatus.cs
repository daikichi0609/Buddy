using System;
using NaughtyAttributes;

[Serializable]
public class CurrentStatus
{
    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="param"></param>
    public CurrentStatus(BattleStatus.Parameter param)
    {
        Name = param.GivenName;
        Hp = param.MaxHp;
        Atk = param.Atk;
        Def = param.Def;
        Agi = param.Agi;
        Dex = param.Dex;
        Eva = param.Eva;
        CriticalRate = param.CriticalRate;
        Res = param.Res;
    }

    public CHARA_NAME Name { get; }

    // レベル
    [ShowNativeProperty]
    public int Lv { get; set; } = 1;

    // ヒットポイント
    [ShowNativeProperty]
    public int Hp { get; set; }

    // 攻撃力
    [ShowNativeProperty]
    public int Atk { get; set; }

    // 防御力
    [ShowNativeProperty]
    public int Def { get; set; }

    // 速さ
    [ShowNativeProperty]
    public int Agi { get; set; }

    // 命中率補正
    [ShowNativeProperty]
    public float Dex { get; set; }

    // 回避率補正
    [ShowNativeProperty]
    public float Eva { get; set; }

    // 会心率補正
    [ShowNativeProperty]
    public float CriticalRate { get; set; }

    // 抵抗率
    [ShowNativeProperty]
    public float Res { get; set; }
}