using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using Zenject;

public interface ICellMiniMapIconController : IActorInterface
{
    /// <summary>
    /// アイコン表示
    /// </summary>
    void AppearIcon();
}

[Obsolete]
public class CellMiniMapIconController : ActorComponentBase, ICellMiniMapIconController
{
    [Inject]
    private IDungeonHandler m_DungeonHandler;

    private ICellInfoHandler m_CellInfo;

    /// <summary>
    /// ミニマップ乗に表示されるアイコン
    /// </summary>
    [SerializeField]
    private GameObject m_Icon;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICellMiniMapIconController>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();

        m_CellInfo = Owner.GetInterface<ICellInfoHandler>();

        // ステート変化でミニマップ表示切り替え
        var state = Owner.GetEvent<ICellStateChangeEvent>();
        state.IsExploredChanged
            .Where(isExplored => isExplored == true)
            .Subscribe(_ => AppearAroundCellIcon()).AddTo(CompositeDisposable);
    }

    protected override void Dispose()
    {
        m_Icon.SetActive(false);
        base.Dispose();
    }

    void ICellMiniMapIconController.AppearIcon() => m_Icon.SetActive(true);

    /// <summary>
    /// ミニマップ用アイコンの表示
    /// </summary>
    private void AppearAroundCellIcon()
    {
        var pos = m_CellInfo.Position;
        var around = m_DungeonHandler.GetAroundCell(pos);
        foreach (var cell in around.Cells.Values)
        {
            if (cell.RequireInterface<ICellInfoHandler>(out var info) == false)
                continue;

            if (info.CellId != TERRAIN_ID.WALL)
                continue;

            if (cell.RequireInterface<ICellMiniMapIconController>(out var renderer) == false)
                continue;

            renderer.AppearIcon();
        }
    }
}
