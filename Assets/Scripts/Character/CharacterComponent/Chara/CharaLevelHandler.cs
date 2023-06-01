using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharaLevelHandler : IActorInterface
{
    /// <summary>
    /// レベル
    /// </summary>
    int Level { get; }

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
    private int m_TotalExperience;

    /// <summary>
    /// レベル
    /// </summary>
    int ICharaLevelHandler.Level
    {
        get
        {
            int level = 1;
            int ex = m_TotalExperience;
            for (int i = 0; i < m_LevelUpBorder.Length; i++)
            {
                ex -= m_LevelUpBorder[i];
                if (ex <= 0)
                    level++;
            }
            return level;
        }
    }

    /// <summary>
    /// 経験値テーブル
    /// 動的に生成する
    /// </summary>
    private int[] m_LevelUpBorder;

    /// <summary>
    /// 経験値テーブル設定
    /// </summary>
    private static readonly int ms_LevelUpBorderBase = 5;
    private static readonly int ms_MaxLevel = 100;
    private static readonly float ms_NextExMag = 1.1f;

    protected override void Initialize()
    {
        base.Initialize();

        // 経験値テーブルの作成
        m_LevelUpBorder = new int[ms_LevelUpBorderBase];
        int border = ms_LevelUpBorderBase;
        for (int i = 0; i < m_LevelUpBorder.Length; i++)
        {
            m_LevelUpBorder[i] = border;
            border = (int)(border * ms_NextExMag);
        }
    }

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