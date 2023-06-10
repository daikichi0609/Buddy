using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UniRx;
using static UnityEngine.UI.GridLayoutGroup;
using System;

public interface ITeamLevelHandler
{
    /// <summary>
    /// レベル
    /// </summary>
    int Level { get; }

    /// <summary>
    /// Exp何割貯まっているか
    /// </summary>
    float ExpFillAmount { get; }

    /// <summary>
    /// 経験値入手
    /// </summary>
    /// <returns></returns>
    void AddExperience(float add);

    /// <summary>
    /// レベル変動時
    /// </summary>
    IObservable<int> OnLevelChanged { get; }
}

public class LevelInfo
{
    public int Level { get; set; }

    public float Exp { get; set; }
}

public class TeamLevelHandler : ITeamLevelHandler, IInitializable
{
    [Inject]
    private CharacterMasterSetup m_CharacterMasterSetup;
    [Inject]
    private ITurnManager m_TurnManager;
    [Inject]
    private IUnitHolder m_UnitHolder;
    [Inject]
    private IDungeonHandler m_DungeonHandler;

    /// <summary>
    /// 経験値テーブル
    /// 動的に生成する
    /// </summary>
    private int[] m_LevelUpBorder;

    /// <summary>
    /// 累計経験値
    /// </summary>
    private ReactiveProperty<float> m_TotalExp = new ReactiveProperty<float>();

    /// <summary>
    /// レベル情報
    /// ReactivePropertyで更新
    /// </summary>
    private LevelInfo m_LevelInfo = new LevelInfo();
    int ITeamLevelHandler.Level => m_LevelInfo.Level;

    /// <summary>
    /// レベル変動時
    /// </summary>
    private Subject<int> m_LevelChanged = new Subject<int>();
    IObservable<int> ITeamLevelHandler.OnLevelChanged => m_LevelChanged;

    /// <summary>
    /// 経験値何割蓄積済みか
    /// </summary>
    float ITeamLevelHandler.ExpFillAmount
    {
        get
        {
            float border = m_LevelUpBorder[m_LevelInfo.Level];
            float rate = (float)m_LevelInfo.Exp / border;
            return rate;
        }
    }

    /// <summary>
    /// 初期化
    /// </summary>
    void IInitializable.Initialize()
    {
        // 経験値テーブルの作成
        m_LevelUpBorder = new int[m_CharacterMasterSetup.MaxLevel];
        int border = m_CharacterMasterSetup.LevelUpFirstBorder;
        for (int i = 0; i < m_LevelUpBorder.Length; i++)
        {
            m_LevelUpBorder[i] = border;
            border = (int)(border * m_CharacterMasterSetup.NextExMag);
        }

        // レベル情報の更新
        m_TotalExp.SubscribeWithState(this, (_, self) =>
        {
            int diff = self.UpdateLevelInfo();
            if (diff != 0)
                self.m_LevelChanged.OnNext(diff); // レベル変動時
        });

        SubscribeGetExp(); // 経験値入手購読

        void SubscribeGetExp()
        {
            // 同じ部屋にいる || 近傍マスにいるならBuddyゲージ増加
            m_TurnManager.OnTurnEnd.SubscribeWithState(this, (_, self) =>
            {
                var playerPos = self.m_UnitHolder.Player.GetInterface<ICharaMove>().Position;
                var buddyPos = self.m_UnitHolder.Buddy.GetInterface<ICharaMove>().Position;
                // 同じ部屋にいるなら
                if (self.m_DungeonHandler.TryGetRoomId(playerPos, out var playerId) == true &&
                    self.m_DungeonHandler.TryGetRoomId(buddyPos, out var buddyId) == true &&
                    playerId == buddyId)
                {
                    self.AddExperience(0.1f);
                    return;
                }

                foreach (var dir in Positional.Directions)
                {
                    var pos = playerPos + dir;
                    if (buddyPos == pos)
                    {
                        self.AddExperience(0.1f);
                        return;
                    }
                }

                self.AddExperience(-1f);
            });
        }
    }

    /// <summary>
    /// レベル情報更新
    /// </summary>
    private int UpdateLevelInfo()
    {
        int level = 0;
        var ex = m_TotalExp.Value;
        for (int i = 0; i < m_LevelUpBorder.Length; i++)
        {
            if (ex < m_LevelUpBorder[i])
                break;

            ex -= m_LevelUpBorder[i];
            level++;
        }
        m_LevelInfo.Exp = ex;
        int diff = level - m_LevelInfo.Level;
        m_LevelInfo.Level = level;
        return diff;
    }

    /// <summary>
    /// 経験値入手
    /// </summary>
    /// <param name="add"></param>
    /// <returns>レベルアップしたかどうか</returns>
    private void AddExperience(float add) => m_TotalExp.Value += add;
    void ITeamLevelHandler.AddExperience(float add) => AddExperience(add);
}