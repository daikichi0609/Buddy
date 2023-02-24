using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public interface ICharaTurn : ICharacterComponent
{
    bool CanAct { get; set; }
    bool IsActing { get; set; }
}

public class CharaTurn : CharaComponentBase, ICharaTurn
{
    /// <summary>
    /// 行動済みステータス
    /// </summary>
    private bool CanAct { get; set; } = true;
    bool ICharaTurn.CanAct
    {
        get => CanAct;
        set => CanAct = value;
    }

    /// <summary>
    /// 行動中ステータス
    /// </summary>
    private bool IsActing { get; set; } = false;
    bool ICharaTurn.IsActing
    {
        get => IsActing;
        set => IsActing = value;
    }

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaTurn>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();

        if (Owner.RequireComponent<ICharaBattleEvent>(out var battle) == true)
        {
            // 攻撃
            battle.OnAttackStart.Subscribe(_ => CanAct = false).AddTo(this);

            // ダメージ前
            battle.OnDamageStart.Subscribe(_ => CanAct = false).AddTo(this);
        }

        if (Owner.RequireComponent<ICharaMoveEvent>(out var move) == true)
        {
            // 移動前
            move.OnMoveStart.Subscribe(_ => CanAct = false).AddTo(this);
        }
    }
}
