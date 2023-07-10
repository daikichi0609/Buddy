

public class RagonRegister : ActorComponentBase
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
        skill.RegisterSkill(new RagonSpear());

        var cleverness = Owner.GetInterface<ICharaClevernessHandler>();
        cleverness.RegisterCleverness(new AttackUp());
        cleverness.RegisterCleverness(new AttackWithPoison());
        cleverness.RegisterCleverness(new AttackUpIfPoison());
    }
}