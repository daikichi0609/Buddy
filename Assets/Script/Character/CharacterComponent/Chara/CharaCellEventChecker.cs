using UnityEngine;
using UniRx;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static UnityEditor.PlayerSettings;
using static UnityEditor.Progress;
using static UnityEditor.VersionControl.Asset;

public interface ICharaCellEventChecker : ICharacterInterface
{
    Task<bool> CheckCurrentCell();
}

/// <summary>
/// セルイベント実行
/// </summary>
public class CharaCellEventChecker : CharaComponentBase, ICharaCellEventChecker
{
    private ICharaMove m_CharaMove;
    private ICharaInventory m_CharaInventory;
    private ICharaTurn m_CharaTurn;

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
    }

    /// <summary>
    /// 現在地セルのイベントチェック
    /// </summary>
    /// <returns></returns>
    async Task<bool> ICharaCellEventChecker.CheckCurrentCell()
    {
        // 階段チェック
        if (await CheckStairsCell() == true)
        {
            Debug.Log("階段マス");
            return true;
        }


        // 階段チェック
        if (await CheckItem() == true)
        {
            Debug.Log("アイテムマス");
            return true;
        }

        // 罠チェック



        return false;
    }

    /// <summary>
    /// 階段チェック
    /// </summary>
    /// <returns></returns>
    async private Task<bool> CheckStairsCell()
    {
        //メインプレイヤーなら
        if (Owner == UnitHolder.Interface.PlayerList[0])
        {
            //階段チェック
            if (DungeonHandler.Interface.GetCellId(m_CharaMove.Position) == CELL_ID.STAIRS)
            {
                YesorNoQuestionUiManager.Interface.SetQuestion(QUESTION_TYPE.STAIRS);
                await m_CharaTurn.WaitFinishActing(() => YesorNoQuestionUiManager.Interface.Activate());
                return true;
            }
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
}