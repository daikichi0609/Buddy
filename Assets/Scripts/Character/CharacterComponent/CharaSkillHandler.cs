using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using Zenject;

public interface ICharaSkillHandler : IActorInterface
{
    /// <summary>
    /// スキル登録
    /// </summary>
    /// <param name="skill"></param>
    void RegisterSkill(ISkill skill);

    /// <summary>
    /// スキル発動
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    Task<bool> Skill(int index);

    /// <summary>
    /// スキル取得（あるなら）
    /// </summary>
    /// <param name="index"></param>
    /// <param name="skill"></param>
    /// <returns></returns>
    bool TryGetSkill(int index, out SkillHolder skill);

    /// <summary>
    /// 有効化切り替え
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    bool SwitchActivate(int index);
}

public sealed class SkillHolder
{
    /// <summary>
    /// スキル
    /// </summary>
    private ISkill Skill { get; }

    /// <summary>
    /// クールタイム
    /// </summary>
    private int m_CurrentCoolTime;
    public int CurrentCoolTime => m_CurrentCoolTime;
    public int MaxCoolTime => Skill.CoolTime;

    /// <summary>
    /// アクティブであるかどうか
    /// </summary>
    private bool m_IsActive;
    public bool IsActive { get => m_IsActive; set => m_IsActive = value; }

    public string Name => Skill.Name;
    public string Description => Skill.Description;

    public SkillHolder(ISkill skill) => Skill = skill;

    /// <summary>
    /// クールタイム短縮
    /// </summary>
    public void CoolDown()
    {
        if (m_CurrentCoolTime > 0)
            m_CurrentCoolTime--;
    }

    /// <summary>
    /// スキル効果
    /// </summary>
    /// <returns></returns>
    public async Task SkillInternal(SkillContext ctx)
    {
        m_CurrentCoolTime = Skill.CoolTime;
        await Skill.Skill(ctx);
    }
}

public class CharaSkillHandler : ActorComponentBase, ICharaSkillHandler
{
    [Inject]
    private IDungeonHandler m_DungeonHandler;
    [Inject]
    private IUnitFinder m_UnitFinder;
    [Inject]
    private IBattleLogManager m_BattleLogManager;
    [Inject]
    private IEffectHolder m_EffectHolder;
    [Inject]
    private ISoundHolder m_SoundHolder;
    [Inject]
    private ITurnManager m_TurnManager;

    private ICharaLastActionHolder m_LastAction;

    /// <summary>
    /// 登録されたスキル
    /// </summary>
    private List<SkillHolder> m_Skills = new List<SkillHolder>();

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register(this);
    }

    protected override void Initialize()
    {
        base.Initialize();
        m_LastAction = Owner.GetInterface<ICharaLastActionHolder>();

        m_TurnManager.OnTurnEnd.SubscribeWithState(this, (_, self) => self.CoolDown()).AddTo(Owner.Disposables);
    }

    /// <summary>
    /// クールタイム進める
    /// </summary>
    private void CoolDown()
    {
        foreach (var skill in m_Skills)
            skill.CoolDown();
    }

    /// <summary>
    /// スキル登録
    /// </summary>
    /// <param name="skill"></param>
    void ICharaSkillHandler.RegisterSkill(ISkill skill)
    {
        var holder = new SkillHolder(skill);
        holder.IsActive = true;
        m_Skills.Add(holder);
    }

    /// <summary>
    /// スキル発動
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    async Task<bool> ICharaSkillHandler.Skill(int key)
    {
        int index = key - 1;
        if (index < 0 || index >= m_Skills.Count)
            return false;

        var skillHolder = m_Skills[index];
        if (skillHolder.CurrentCoolTime > 0)
        {
            m_BattleLogManager.Log("クールダウン中。残り" + skillHolder.CurrentCoolTime + "ターンで使用可能。");
            return false;
        }

        m_LastAction.RegisterAction(CHARA_ACTION.SKILL);

        SkillContext ctx = new SkillContext(Owner, m_DungeonHandler, m_UnitFinder, m_BattleLogManager, m_EffectHolder, m_SoundHolder);
        await skillHolder.SkillInternal(ctx);
        return true;
    }

    bool ICharaSkillHandler.SwitchActivate(int index)
    {
        if (TryGetSkill(index, out var skill) == false)
            return false;

        skill.IsActive = !skill.IsActive;
        return skill.IsActive;
    }

    /// <summary>
    /// スキル取得（あるなら）
    /// </summary>
    /// <param name="index"></param>
    /// <param name="skill"></param>
    /// <returns></returns>
    private bool TryGetSkill(int index, out SkillHolder skill)
    {
        skill = null;
        if (index < 0 || index >= m_Skills.Count)
            return false;

        skill = m_Skills[index];
        return skill != null;
    }
    bool ICharaSkillHandler.TryGetSkill(int index, out SkillHolder skill) => TryGetSkill(index, out skill);
}