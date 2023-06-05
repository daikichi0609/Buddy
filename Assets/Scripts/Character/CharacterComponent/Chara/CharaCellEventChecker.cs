using UnityEngine;
using System.Threading.Tasks;
using UniRx;
using System;
using Zenject;

public interface ICharaCellEventChecker : IActorInterface
{
    /// <summary>
    /// セルイベントチェック
    /// </summary>
    /// <returns></returns>
    bool CheckCurrentCell();

    /// <summary>
    /// 階段マスチェック
    /// </summary>
    /// <returns></returns>
    bool CheckStairsCell();
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
    private ITurnManager m_TurnManager;
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
    bool ICharaCellEventChecker.CheckCurrentCell()
    {
        // アイテムチェック
        if (CheckItem() == true)
            return true;

        // 罠チェック
        if (CheckTrap() == true)
            return true;

        return false;
    }

    /// <summary>
    /// 階段チェック
    /// </summary>
    /// <returns></returns>
    private bool CheckStairsCell()
    {
        //階段チェック
        if (m_DungeonHandler.GetCellId(m_CharaMove.Position) == TERRAIN_ID.STAIRS)
        {
            m_QuestionUiManager.SetQuestion(QUESTION_TYPE.STAIRS);
            m_CharaTurn.WaitFinishActing(() => m_QuestionUiManager.Activate());
            return true;
        }

        return false;
    }
    bool ICharaCellEventChecker.CheckStairsCell() => CheckStairsCell();

    /// <summary>
    /// アイテムチェック
    /// </summary>
    /// <returns></returns>
    private bool CheckItem()
    {
        //アイテムチェック
        foreach (IItemHandler item in m_ItemManager.ItemList)
        {
            Vector3Int itemPos = item.Position;
            if (m_CharaMove.Position == itemPos)
            {
                var disposable = m_TurnManager.RequestProhibitAction(Owner);
                m_CharaTurn.WaitFinishActing(() => m_CharaInventory.Put(item, disposable));
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 罠チェック
    /// </summary>
    /// <returns></returns>
    private bool CheckTrap()
    {
        if (m_TypeHolder.Type != CHARA_TYPE.PLAYER)
            return false;

        var cell = m_DungeonHandler.GetCell(m_CharaMove.Position);
        if (cell.RequireInterface<ITrapHandler>(out var handler) == true)
            if (handler.HasTrap == true)
            {
                var disposable = m_TurnManager.RequestProhibitAction(Owner);
                m_CharaTurn.WaitFinishActing(() => handler.ActivateTrap(Owner, m_UnitFinder, m_DungeonHandler, m_BattleLogManager, disposable));
                return true;
            }

        return false;
    }
}