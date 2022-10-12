using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public abstract class EnemyBattle : CharaBattle
{
    protected InternalDefine.ENEMY_STATE CurrentState
    {
        get;
        set;
    }

    [SerializeField] protected GameObject m_TargetObject;
    protected GameObject TargetObject
    {
        get { return m_TargetObject; }
        set { m_TargetObject = value; }
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    /// <summary>
    /// 死亡
    /// </summary>
    protected override void Death()
    {
        DungeonContents.Instance.RemoveEnemyObject(gameObject);
    }

    /// <summary>
    /// 行動を決めて実行する
    /// </summary>
    public void DecideAndExecuteAction()
    {
        List<Vector3> attackPosList = AttackPosList(CharaMove.Position);
        EnemyActionAndTarget enemyActionAndTarget = Utility.DecideActionAndTarget(attackPosList, CharaMove.Position);
        switch (enemyActionAndTarget.NextState)
        {
            case InternalDefine.ENEMY_STATE.ATTACKING:
                //攻撃禁止なら
                if(TurnManager.Instance.IsCanAttack == false)
                {
                    StartCoroutine(TurnManager.Instance.WaitForIsCanAttack());
                }
                Face(enemyActionAndTarget.TargetList);
                NormalAttack();
                break;

            case InternalDefine.ENEMY_STATE.CHASING:
                Chase(enemyActionAndTarget.TargetList);
                CurrentState = InternalDefine.ENEMY_STATE.CHASING;
                break;

            case InternalDefine.ENEMY_STATE.SEARCHING:
                Search();
                CurrentState = InternalDefine.ENEMY_STATE.SEARCHING;
                break;
        }
    }

    /// <summary>
    /// 攻撃 ステート変更もする
    /// </summary>
    protected override void NormalAttack()
    {
        CurrentState = InternalDefine.ENEMY_STATE.ATTACKING;
        base.NormalAttack();
    }

    /// <summary>
    /// ターゲットの方を向く 主に攻撃前
    /// </summary>
    /// <param name="targetList"></param>
    protected void Face(List<GameObject> targetList)
    {
        //ターゲットをランダムに絞って向く
        Utility.Shuffle<GameObject>(targetList);
        if (AttackInfo.IsPossibleToDiagonal == true)
        {
            Vector3 direction = targetList[0].GetComponent<Chara>().Position - CharaMove.Position;
            CharaMove.Face(direction);
            return;
        }

        foreach (GameObject gameObject in targetList)
        {
            Vector3 direction = gameObject.GetComponent<Chara>().Position - CharaMove.Position;
            if(DungeonTerrain.Instance.IsPossibleToMoveDiagonal((int)CharaMove.Position.x, (int)CharaMove.Position.z, (int)direction.x, (int)direction.z) == true)
            {
                CharaMove.Face(direction);
                return;
            }
        }

        CompromiseMove(targetList[0].GetComponent<Chara>().Position);
    }

    /// <summary>
    /// スキル
    /// </summary>
    /// <param name="targetList"></param>
    protected virtual void Skill(List<GameObject> targetList)
    {
        CurrentState = InternalDefine.ENEMY_STATE.ATTACKING;
    }

    /// <summary>
    /// ターゲットに向かって進む
    /// </summary>
    /// <param name="targetList"></param>
    protected virtual void Chase(List<GameObject> targetList)
    {
        if (CurrentState != InternalDefine.ENEMY_STATE.CHASING)
        {
            int num = Random.Range(0, targetList.Count);
            TargetObject = targetList[num];
        }

        Chara player = TargetObject.GetComponent<Chara>();
        Vector3 direction = player.Position - CharaMove.Position;
        direction = Utility.Direction(direction);
        if (CharaMove.Move(direction) == false)
        {
            //移動できなかった場合の処理
            CompromiseMove(player.Position);
        }
    }

    /// <summary>
    /// プレイヤーを探して歩く
    /// </summary>
    protected virtual void Search()
    {
        int currentRoomId = Positional.IsOnRoomID(CharaMove.Position);
        //通路にいる場合
        if (currentRoomId == 0)
        {
            AroundGridID aroundGridID = DungeonTerrain.Instance.CreateAroundGrid((int)CharaMove.Position.x, (int)CharaMove.Position.z);
            Vector3 myDirection = CharaMove.Direction;
            Vector3 oppositeDirection = CharaMove.Direction * -1;
            List<Vector3> directionList = new List<Vector3>();
            if (aroundGridID.UpGrid > (int)DungeonTerrain.GRID_ID.WALL)
            {
                Vector3 upDirection = new Vector3(0f, 0f, 1f);
                if (myDirection != upDirection * -1)
                {
                    directionList.Add(upDirection);
                }
            }
            if (aroundGridID.UnderGrid > (int)DungeonTerrain.GRID_ID.WALL)
            {
                Vector3 downDirection = new Vector3(0f, 0f, -1f);
                if (myDirection != downDirection * -1)
                {
                    directionList.Add(downDirection);
                }
            }
            if (aroundGridID.LeftGrid > (int)DungeonTerrain.GRID_ID.WALL)
            {
                Vector3 leftDirection = new Vector3(-1f, 0f, 0f);
                if (myDirection != leftDirection * -1)
                {
                    directionList.Add(leftDirection);
                }
            }
            if (aroundGridID.RightGrid > (int)DungeonTerrain.GRID_ID.WALL)
            {
                Vector3 rightDirection = new Vector3(1f, 0f, 0f);
                if (myDirection != rightDirection * -1)
                {
                    directionList.Add(rightDirection);
                }
            }

            int num = Random.Range(0, directionList.Count);
            if(CharaMove.Move(directionList[num]) == false)
            {
                if (CharaMove.Move(oppositeDirection) == false)
                    CharaMove.Wait();
            }
            return;
        }

        //新しくSEARCHINGステートになった場合、目標となる部屋の入り口を設定する
        if (CurrentState != InternalDefine.ENEMY_STATE.SEARCHING) 
        {
            List<GameObject> gateWayObjectList = ObjectManager.Instance.GateWayObjectList(Positional.IsOnRoomID(CharaMove.Position));
            int num = Random.Range(0, gateWayObjectList.Count);
            TargetObject = gateWayObjectList[num];
        }

        //入り口についた場合、部屋を出る
        if(CharaMove.Position.x == TargetObject.transform.position.x && CharaMove.Position.z == TargetObject.transform.position.z)
        {
            AroundGridID aroundGridID = DungeonTerrain.Instance.CreateAroundGrid((int)CharaMove.Position.x, (int)CharaMove.Position.z);
            if(aroundGridID.UpGrid == (int)DungeonTerrain.GRID_ID.PATH_WAY)
            {
                if (CharaMove.Move(new Vector3(0f, 0f, 1f)) == true)
                return;
            }
            if (aroundGridID.UnderGrid == (int)DungeonTerrain.GRID_ID.PATH_WAY)
            {
                if (CharaMove.Move(new Vector3(0f, 0f, -1f)) == true)
                return;
            }
            if (aroundGridID.LeftGrid == (int)DungeonTerrain.GRID_ID.PATH_WAY)
            {
                if (CharaMove.Move(new Vector3(-1f, 0f, 0f)) == true)
                return;
            }
            if (aroundGridID.RightGrid == (int)DungeonTerrain.GRID_ID.PATH_WAY)
            {
                if (CharaMove.Move(new Vector3(1f, 0f, 0f)) == true)
                return;
            }

            CharaMove.Wait();
            return;
        }

        //通路出入り口へ向かう
        Vector3 direction = TargetObject.transform.position - CharaMove.Position;
        direction = Utility.Direction(direction);
        if (CharaMove.Move(direction) == false)
        {
            //移動できなかった場合の処理
            CompromiseMove(TargetObject.transform.position);
        }
    }

    /// <summary>
    /// 妥協した移動
    /// </summary>
    /// <param name="targetPos"></param>
    protected void CompromiseMove(Vector3 targetPos)
    {
        CharaMove.Wait();
        return;

        Vector3 direction = targetPos - CharaMove.Position;
        direction = Utility.Direction(direction);
        if (direction.x != 0 && direction.z != 0)
        {

        }

        if (direction.x == 0)
        {

        }
    }

    /// <summary>
    /// キャラ別通常攻撃範囲
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    protected virtual List<Vector3> AttackPosList(Vector3 pos)
    {
        return null;
    }
}

public struct EnemyActionAndTarget
{
    public InternalDefine.ENEMY_STATE NextState
    {
        get; set;
    }
    public List<GameObject> TargetList
    {
        get;
    }

    public EnemyActionAndTarget(InternalDefine.ENEMY_STATE state, List<GameObject> targetList)
    {
        NextState = state;
        TargetList = targetList;
    }
}