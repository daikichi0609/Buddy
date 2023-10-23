using UnityEngine;
using System.Threading.Tasks;
using UniRx;
using System;
using Zenject;
using System.Threading;

public interface ICharaCellEventChecker : IActorInterface
{
    /// <summary>
    /// セルイベントチェック
    /// </summary>
    /// <returns></returns>
    Task<bool> CheckCurrentCell();

    /// <summary>
    /// 階段マスチェック
    /// </summary>
    /// <returns></returns>
    Task<bool> CheckStairsCell();
}

/// <summary>
/// セルイベント実行
/// </summary>
public class CharaCellEventChecker : ActorComponentBase, ICharaCellEventChecker
{
    [Inject]
    private IDungeonHandler m_DungeonHandler;
    [Inject]
    private IItemManager m_ItemManager;
    [Inject]
    private IUnitFinder m_UnitFinder;
    [Inject]
    private IYesorNoQuestionUiManager m_QuestionUiManager;
    [Inject]
    private IBattleLogManager m_BattleLogManager;

    private ICharaMove m_CharaMove;
    private ICharaInventory m_CharaInventory;
    private ICharaTurn m_CharaTurn;
    private ICharaTypeHolder m_TypeHolder;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaCellEventChecker>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();
        m_CharaMove = Owner.GetInterface<ICharaMove>();
        m_CharaInventory = Owner.GetInterface<ICharaInventory>();
        m_CharaTurn = Owner.GetInterface<ICharaTurn>();
        m_TypeHolder = Owner.GetInterface<ICharaTypeHolder>();
    }

    /// <summary>
    /// 現在地セルのイベントチェック
    /// </summary>
    /// <returns></returns>
    async Task<bool> ICharaCellEventChecker.CheckCurrentCell()
    {
        // アイテムチェック
        if (await CheckItem() == true)
            return true;

        // 罠チェック
        if (await CheckTrap() == true)
            return true;

        return false;
    }

    /// <summary>
    /// 階段チェック
    /// </summary>
    /// <returns></returns>
    private async Task<bool> CheckStairsCell()
    {
        //階段チェック
        if (m_DungeonHandler.GetCellId(m_CharaMove.Position) == TERRAIN_ID.STAIRS)
        {
            m_QuestionUiManager.SetQuestion(QUESTION_TYPE.STAIRS);
            await m_CharaTurn.WaitFinishActing();
            m_QuestionUiManager.Activate();
            return true;
        }

        return false;
    }
    Task<bool> ICharaCellEventChecker.CheckStairsCell() => CheckStairsCell();

    /// <summary>
    /// アイテムチェック
    /// </summary>
    /// <returns></returns>
    private async Task<bool> CheckItem()
    {
        if (TryGetCurrentCellItem(out var cellItem) == false)
            return false;

        await m_CharaTurn.WaitFinishActing();
        m_CharaInventory.Put(cellItem);
        return true;
    }
    private bool TryGetCurrentCellItem(out IItemHandler cellItem)
    {
        //アイテムチェック
        foreach (var item in m_ItemManager.ItemList)
        {
            var handler = item.GetInterface<IItemHandler>();
            if (m_CharaMove.Position == handler.Position)
            {
                cellItem = handler;
                return true;
            }
        }
        cellItem = null;
        return false;
    }

    /// <summary>
    /// 罠チェック
    /// </summary>
    /// <returns></returns>
    private async Task<bool> CheckTrap()
    {
        // プレイヤーサイドなら罠を踏む
        if (m_TypeHolder.Type != CHARA_TYPE.FRIEND)
            return false;

        if (TryGetCurrentCellTrap(out var trap) == false)
            return false;

        await m_CharaTurn.WaitFinishActing();
        await trap.ActivateTrap(Owner, m_UnitFinder, m_DungeonHandler, m_BattleLogManager);
        return true;
    }
    private bool TryGetCurrentCellTrap(out ITrapHandler trap)
    {
        var cell = m_DungeonHandler.GetCell(m_CharaMove.Position);
        if (cell.RequireInterface<ITrapHandler>(out var handler) == true)
            if (handler.HasTrap == true)
            {
                trap = handler;
                return true;
            }

        trap = null;
        return false;
    }
}