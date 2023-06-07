using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

//[CreateAssetMenu(menuName = "MyScriptable/Item/Effect/空腹値回復")]
public class ThrowStraight : ItemEffectBase
{
    [SerializeField, Header("ダメージ")]
    private int m_Damage;

    [SerializeField, Header("飛距離")]
    private int m_Distance;

    protected override async Task EffectInternal(ItemEffectContext ctx)
    {
        // Log
        var status = ctx.Owner.GetInterface<ICharaStatus>();
        ctx.BattleLogManager.Log(status.CurrentStatus.OriginParam.GivenName + "は" + ctx.ItemSetup.ItemName + "を投げた！");

        // 飛ばす方向
        var move = ctx.Owner.GetInterface<ICharaMove>();
        var dir = move.Direction;
        var dirV3 = dir.ToV3Int();
        var currentPos = move.Position;

        // ターゲットタイプ
        var targetType = ctx.Owner.GetInterface<ICharaTypeHolder>().TargetType;
        // ヒットしたキャラ
        ICollector target = null;
        bool isHit = false;
        // 飛距離
        int distance;

        for (distance = 1; distance <= m_Distance; distance++)
        {
            // 攻撃マス
            var targetPos = currentPos + dirV3 * distance;

            // 攻撃対象ユニットが存在するか調べる
            if (ctx.UnitFinder.TryGetSpecifiedPositionUnit(targetPos, out target, targetType) == true)
            {
                isHit = true;
                break;
            }

            // 地形チェック
            var terrain = ctx.DungeonHandler.GetCellId(targetPos);
            // 壁だったら走査終了
            if (terrain == TERRAIN_ID.WALL)
            {
                distance--; // 手前に落ちる
                break;
            }
        }

        await ctx.ItemManager.FlyItem(ctx.ItemSetup, currentPos, dirV3 * distance, !isHit);

        // ヒット対象がいるならダメージを与える
        if (isHit == true)
        {
            var battle = target.GetInterface<ICharaBattle>();
            var result = battle.Damage(new AttackInfo(ctx.Owner, status.CurrentStatus.OriginParam.GivenName, m_Damage, 100f, 0f, true, dir));
            var log = CharaLog.CreateAttackResultLog(result);
            ctx.BattleLogManager.Log(log);
        }
    }
}