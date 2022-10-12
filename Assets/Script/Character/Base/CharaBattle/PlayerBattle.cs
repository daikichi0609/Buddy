using UnityEngine;

public abstract class PlayerBattle : CharaBattle
{
    public int Ex
    {
        private get; set;
    }

    public int NormalAttackLv
    {
        protected get; set;
    }
    public int SkillLv
    {
        protected get; set;
    }
    public int AbilityLv
    {
        protected get; set;
    }

    public override void Initialize()
    {
        NormalAttackLv = 1;
        SkillLv = 1;
        AbilityLv = 1;

        base.Initialize();
    }

    public void Act(InternalDefine.ACTION action)
    {
        switch (action)
        {
            case InternalDefine.ACTION.ATTACK:
                if (TurnManager.Instance.IsCanAttack == false)
                {
                    return;
                }
                NormalAttack();
                break;

            case InternalDefine.ACTION.SKILL:
                if (TurnManager.Instance.IsCanAttack == false)
                {
                    return;
                }
                Skill();
                break;
        }
    }


}