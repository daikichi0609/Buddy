using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;

public interface ISkill
{
    /// <summary>
    /// スキル名
    /// </summary>
    string Name { get; }

    /// <summary>
    /// スキル説明
    /// </summary>
    string Description { get; }

    /// <summary>
    /// クールダウン
    /// </summary>
    int CoolTime { get; }

    /// <summary>
    /// スキル効果
    /// </summary>
    /// <returns></returns>
    Task Skill(SkillContext ctx);

    /// <summary>
    /// スキルが有効な状況か
    /// </summary>
    /// <returns></returns>
    bool ExistTarget(SkillTargetContext ctx, out DIRECTION[] dir);
}

public abstract class Skill : ScriptableObject, ISkill
{
    [ShowNativeProperty]
    protected abstract string Name { get; }
    string ISkill.Name => Name;

    protected string CT => "CT:" + CoolTime + "\n";
    [ShowNativeProperty]
    protected abstract string Description { get; }
    string ISkill.Description => CT + Description;

    [ShowNativeProperty]
    protected abstract int CoolTime { get; }
    int ISkill.CoolTime => CoolTime;

    protected abstract Task SkillEffect(SkillContext ctx);
    Task ISkill.Skill(SkillContext ctx) => SkillEffect(ctx);

    protected abstract bool ExistTarget(SkillTargetContext ctx, out DIRECTION[] dirs);
    bool ISkill.ExistTarget(SkillTargetContext ctx, out DIRECTION[] dirs) => ExistTarget(ctx, out dirs);
}

public readonly struct SkillContext
{
    /// <summary>
    /// スキル使用者
    /// </summary>
    public ICollector Owner { get; }

    /// <summary>
    /// ダンジョン
    /// </summary>
    public IDungeonHandler DungeonHandler { get; }

    /// <summary>
    /// ユニット取得
    /// </summary>
    public IUnitFinder UnitFinder { get; }

    /// <summary>
    /// バトルログ
    /// </summary>
    public IBattleLogManager BattleLogManager { get; }

    /// <summary>
    /// エフェクト
    /// </summary>
    public IEffectHolder EffectHolder { get; }

    /// <summary>
    /// エフェクト
    /// </summary>
    public ISoundHolder SoundHolder { get; }

    public SkillContext(ICollector owner, IDungeonHandler dungeonHandler, IUnitFinder unitFinder, IBattleLogManager battleLogManager, IEffectHolder effectHolder, ISoundHolder soundHolder)
    {
        Owner = owner;
        DungeonHandler = dungeonHandler;
        UnitFinder = unitFinder;
        BattleLogManager = battleLogManager;
        EffectHolder = effectHolder;
        SoundHolder = soundHolder;
    }
}

public readonly struct SkillTargetContext
{
    public Vector3Int Position { get; }

    public IUnitFinder UnitFinder { get; }

    public IDungeonHandler DungeonHandler { get; }

    public ICharaTypeHolder TypeHolder { get; }

    public SkillTargetContext(Vector3Int pos, IUnitFinder unitFinder, IDungeonHandler dungeonHandler, ICharaTypeHolder typeHolder)
    {
        Position = pos;
        UnitFinder = unitFinder;
        DungeonHandler = dungeonHandler;
        TypeHolder = typeHolder;
    }
}