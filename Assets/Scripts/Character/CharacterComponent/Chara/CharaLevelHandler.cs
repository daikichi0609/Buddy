using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharaLevelHandler : IActorInterface
{
    /// <summary>
    /// 経験値入手
    /// </summary>
    /// <returns></returns>
    bool AddExperience(int add);


}

public class CharaLevelHandler : ActorComponentBase, ICharaLevelHandler
{
    /// <summary>
    /// 累計経験値
    /// </summary>
    private int m_Experience;

    /// <summary>
    /// 経験値テーブル
    /// 動的に生成する
    /// </summary>
    private int[] m_LevelUpBorder;

    /// <summary>
    /// 初めのレベルアップボーダー
    /// これを元に経験値テーブルを生成
    /// </summary>
    private static readonly int ms_LevelUpBorderBase = 5;

    /// <summary>
    /// 経験値入手
    /// </summary>
    /// <param name="add"></param>
    /// <returns></returns>
    bool ICharaLevelHandler.AddExperience(int add)
    {
        return true;
    }
}