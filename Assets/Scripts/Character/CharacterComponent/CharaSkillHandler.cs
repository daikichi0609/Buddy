using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using Zenject;

public interface ICharaSkillHandler : IActorInterface
{
    bool TryGetSkill(int index, out ISkill skill);

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
}

public class CharaSkillHandler : ActorComponentBase, ICharaSkillHandler
{
    private sealed class SkillHolder
    {
        /// <summary>
        /// スキル
        /// </summary>
        private ISkill Skill { get; }
        public ISkill GetSkill => Skill;

        /// <summary>
        /// クールタイム
        /// </summary>
        private int m_CoolTime;
        public int CoolTime => m_CoolTime;

        public SkillHolder(ISkill skill) => Skill = skill;

        /// <summary>
        /// クールタイム短縮
        /// </summary>
        public void CoolDown()
        {
            if (m_CoolTime > 0)
                m_CoolTime--;
        }

        /// <summary>
        /// スキル効果
        /// </summary>
        /// <returns></returns>
        public async Task Activate(SkillContext ctx)
        {
            m_CoolTime = Skill.CoolTime;
            await Skill.Skill(ctx);
        }
    }

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

    bool ICharaSkillHandler.TryGetSkill(int index, out ISkill skill)
    {
        skill = null;
        if (index < 0 || index >= m_Skills.Count)
            return false;

        skill = m_Skills[index].GetSkill;
        return skill != null;
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
        if (skillHolder.CoolTime > 0)
        {
            m_BattleLogManager.Log("クールダウン中。残り" + skillHolder.CoolTime + "ターンで使用可能。");
            return false;
        }

        m_LastAction.RegisterAction(CHARA_ACTION.SKILL);

        SkillContext ctx = new SkillContext(Owner, m_DungeonHandler, m_UnitFinder, m_BattleLogManager, m_EffectHolder, m_SoundHolder);
        await skillHolder.Activate(ctx);
        return true;
    }
}