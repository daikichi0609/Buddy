

public class LogueRegister : ActorComponentBase
{
    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register(this);
    }

    protected override void Initialize()
    {
        base.Initialize();

        var skill = Owner.GetInterface<ICharaSkillHandler>();
        skill.RegisterSkill(new ContinuousSlash());
        skill.RegisterSkill(new SpinningSlash());
        skill.RegisterSkill(new VaccumSlash());

        var cleverness = Owner.GetInterface<ICharaClevernessHandler>();
        cleverness.RegisterCleverness(new CriticalRatioUp());
    }
}