using System.Collections.Generic;
using UnityEngine;

public class Mashroom : EnemyBattle
{
    public override void Initialize()
    {
        Parameter = CharaDataManager.LoadEnemyScriptableObject(Define.CHARA_NAME.MASHROOM);
        base.Initialize();
    }

    protected override AttackInfo AttackInfo => new MashroomNormalAttack();

    protected override void NormalAttack()
    {
        base.NormalAttack();

        Vector3 attackPos = CharaMove.Position + CharaMove.Direction;
        Attack(attackPos, InternalDefine.TARGET.PLAYER);
    }

    protected override void Skill(List<GameObject> targetList)
    {
        base.Skill(targetList);
    }

    protected override List<Vector3> AttackPosList(Vector3 pos)
    {
        List<Vector3> list = new List<Vector3>();

        list.Add(pos + new Vector3(0f, 0f, 1f));
        list.Add(pos + new Vector3(1f, 0f, 1f));
        list.Add(pos + new Vector3(1f, 0f, 0f));
        list.Add(pos + new Vector3(1f, 0f, -1f));
        list.Add(pos + new Vector3(0f, 0f, -1f));
        list.Add(pos + new Vector3(-1f, 0f, -1f));
        list.Add(pos + new Vector3(-1f, 0f, 0f));
        list.Add(pos + new Vector3(-1f, 0f, 1f));

        return list;
    }
}

public class MashroomNormalAttack : AttackInfo
{
    public override float Mag => 1.0f;
    public override float ActFrame => 0.6f;
    public override float AnimFrame => 0.4f;
    public override bool IsPossibleToDiagonal => false;
}