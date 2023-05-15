using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class CellMiniMapRenderer : ActorComponentBase
{
    /// <summary>
    /// ミニマップ乗に表示されるアイコン
    /// </summary>
    [SerializeField]
    private GameObject m_Icon;

    protected override void Initialize()
    {
        base.Initialize();

        // ステート変化でミニマップ表示切り替え
        var state = Owner.GetEvent<ICellStateChangeEvent>();
        state.IsExploredChanged.Subscribe(isExplored => m_Icon.SetActive(isExplored)).AddTo(CompositeDisposable);
    }
}
