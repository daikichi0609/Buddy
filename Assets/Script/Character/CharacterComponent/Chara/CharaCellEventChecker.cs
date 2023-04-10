using UnityEngine;
using System.Threading.Tasks;

public interface ICharaCellEventChecker : IActorInterface
{
    Task<bool> CheckCurrentCell();

    Task<bool> CheckStairsCell();
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
    async Task<bool> ICharaCellEventChecker.CheckStairsCell()
    {
        //階段チェック
        if (DungeonHandler.Interface.GetCellId(m_CharaMove.Position) == CELL_ID.STAIRS)
        {
            YesorNoQuestionUiManager.Interface.SetQuestion(QUESTION_TYPE.STAIRS);
            await m_CharaTurn.WaitFinishActing(() => YesorNoQuestionUiManager.Interface.Activate());
            return true;
        }

        return false;
    }

    /// <summary>
    /// アイテムチェック
    /// </summary>
    /// <returns></returns>
    async private Task<bool> CheckItem()
    {
        //アイテムチェック
        foreach (IItem item in ItemManager.Interface.ItemList)
        {
            Vector3Int itemPos = item.Position;
            if (m_CharaMove.Position == itemPos)
            {
                await m_CharaTurn.WaitFinishActing(() => m_CharaInventory.Put(item));
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// アイテムチェック
    /// </summary>
    /// <returns></returns>
    async private Task<bool> CheckTrap()
    {
        if (m_TypeHolder.Type != CHARA_TYPE.PLAYER)
            return false;

        var cell = DungeonHandler.Interface.GetCell(m_CharaMove.Position);
        if (cell.RequireInterface<ITrapHolder>(out var holder) == true)
            if (holder.TryGetTrap(out var trap) == true)
            {
                await trap.Effect(Owner, UnitFinder.Interface, cell.GetAroundCell());
                return true;
            }

        return false;
    }
}