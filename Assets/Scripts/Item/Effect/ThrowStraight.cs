using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(menuName = "MyScriptable/Item/Effect/空腹値回復")]
public class ThrowStraight : ItemEffectBase
{
    [SerializeField, Header("ダメージ")]
    private int m_Damage;

    [SerializeField, Header("飛距離")]
    private int m_Distance;

    protected override void EffectInternal(ICollector owner, IItemHandler item)
    {
        // Log
        var status = owner.GetInterface<ICharaStatus>();
        BattleLogManager.Interface.Log(status.CurrentStatus.Name + "は" + item.Setup.ItemName + "を投げた！");

        // 飛ばす方向
        var move = owner.GetInterface<ICharaMove>();
        var dir = move.Direction;
        var dirV3 = dir.ToV3Int();
        var currentPos = move.Position;

        // ターゲットタイプ
        var targetType = owner.GetInterface<ICharaTypeHolder>().TargetType;
        // ヒットしたキャラ
        ICollector hit = null;
        // 飛距離
        int distance;

        for (distance = 1; distance <= m_Distance; distance++)
        {
            // 攻撃マス
            var targetPos = currentPos + dirV3 * distance;

            // 攻撃対象ユニットが存在するか調べる
            if (UnitFinder.Interface.TryGetSpecifiedPositionUnit(targetPos, out hit, targetType) == true)
                break;

            // 地形チェック
            var terrain = DungeonHandler.Interface.GetCellId(targetPos);
            // 壁だったら走査終了
            if (terrain == TERRAIN_ID.WALL)
            {
                distance--; // 手前に落ちる
                break;
            }
        }

        // ヒット対象がいるならダメージを与える
        if (hit != null)
        {
            var battle = hit.GetInterface<ICharaBattle>();
            var result = battle.Damage(new AttackInfo(owner, status.CurrentStatus.Name, m_Damage, 100f, true, dir));
            var log = CharaLog.CreateAttackResultLog(result);
            BattleLogManager.Interface.Log(log);
        }
        // ヒットしていないならアイテムを落とす
        else
        {

        }
    }
}