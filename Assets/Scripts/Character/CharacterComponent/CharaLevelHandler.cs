using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

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
    [Inject]
    private CharacterMasterSetup m_CharacterMasterSetup;

    /// <summary>
    /// 累計経験値
    /// </summary>
    private int m_TotalExperience;

    /// <summary>
    /// 経験値テーブル
    /// 動的に生成する
    /// </summary>
    private int[] m_LevelUpBorder;

    /// <summary>
    /// レベル
    /// </summary>
    private int Level
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
    int ICharaLevelHandler.Level => Level;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        Owner.Register<ICharaLevelHandler>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();

        // 経験値テーブルの作成
        m_LevelUpBorder = new int[m_CharacterMasterSetup.MaxLevel];
        int border = m_CharacterMasterSetup.LevelUpFirstBorder;
        for (int i = 0; i < m_LevelUpBorder.Length; i++)
        {
            m_LevelUpBorder[i] = border;
            border = (int)(border * m_CharacterMasterSetup.NextExMag);
        }
    }

    /// <summary>
    /// 経験値入手
    /// </summary>
    /// <param name="add"></param>
    /// <returns>レベルアップしたかどうか</returns>
    bool ICharaLevelHandler.AddExperience(int add)
    {
        int level = Level;
        m_TotalExperience += level;
        return Level > level;
    }
}