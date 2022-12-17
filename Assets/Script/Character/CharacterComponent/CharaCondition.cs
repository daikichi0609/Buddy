using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharaCondition : ICharacterComponent
{

}

public class CharaCondition : CharaComponentBase, ICharaCondition
{
    public int BoxmanSkillBuffTime
    {
        private get; set;
    }
    public bool BoxmanAbilityBuff
    {
        private get; set;
    }

    public void TurnEnd()
    {

    }
}
