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
        Res = param.Res;
    }

    [ShowNativeProperty]
    public string Name { get; }

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

    // 抵抗率
    [ShowNativeProperty]
    public float Res { get; set; }
}