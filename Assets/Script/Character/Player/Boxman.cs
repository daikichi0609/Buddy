using UnityEngine;
using System.Collections.Generic;

public class Boxman: PlayerBattle
{
    public override void Initialize()
    {
        Parameter = CharaDataManager.LoadPlayerScriptableObject(Define.CHARA_NAME.BOXMAN);
        base.Initialize();
    }

    protected override AttackInfo AttackInfo => new BoxmanNormalAttack();

    protected override void NormalAttack()
    {
        base.NormalAttack();
        Vector3 attackPos = CharaMove.Position + CharaMove.Direction;
        Attack(attackPos, InternalDefine.TARGET.ENEMY);
    }

    protected override void Skill()
    {
        base.Skill();
    }
}

public class BoxmanNormalAttack: AttackInfo
{
    public override string Name => "きりつける";
    public override float Mag => 1.0f;
    public override float ActFrame => 0.6f;
    public override float AnimFrame => 0.4f;
    public override bool IsPossibleToDiagonal => false;
}