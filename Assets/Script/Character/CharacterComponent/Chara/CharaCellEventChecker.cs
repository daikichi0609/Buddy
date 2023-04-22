using UnityEngine;
using System.Threading.Tasks;
using UniRx;
using static UnityEditor.Progress;

public interface ICharaCellEventChecker : IActorInterface
{
    bool CheckCurrentCell();

    bool CheckStairsCell();
}

/// <summary>
/// セルイベント実行
/// </summary>
public class CharaCellEventChecker : ActorComponentBase, ICharaCellEventChecker
{
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
        if (DungeonHandler.Interface.GetCellId(m_CharaMove.Position) == CELL_ID.STAIRS)
        {
            YesorNoQuestionUiManager.Interface.SetQuestion(QUESTION_TYPE.STAIRS);
            m_CharaTurn.WaitFinishActing(() => YesorNoQuestionUiManager.Interface.Activate());
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
        foreach (IItem item in ItemManager.Interface.ItemList)
        {
            Vector3Int itemPos = item.Position;
            if (m_CharaMove.Position == itemPos)
            {
                m_CharaTurn.WaitFinishActing(() => m_CharaInventory.Put(item));
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

        var cell = DungeonHandler.Interface.GetCell(m_CharaMove.Position);
        if (cell.RequireInterface<ITrapHandler>(out var handler) == true)
            if (handler.HasTrap == true)
            {
                m_CharaTurn.WaitFinishActing(() => handler.ActivateTrap(Owner, UnitFinder.Interface, cell.GetAroundCell()));
                return true;
            }

        return false;
    }
}