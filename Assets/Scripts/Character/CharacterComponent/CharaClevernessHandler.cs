using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using Zenject;

public interface ICharaClevernessHandler : IActorInterface
{
    /// <summary>
    /// スキル登録
    /// </summary>
    /// <param name="cleverness"></param>
    IDisposable RegisterCleverness(ICleverness cleverness);

    /// <summary>
    /// 賢さ有効化
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    bool SwitchActivate(int index);

    /// <summary>
    /// 賢さ取得
    /// </summary>
    /// <param name="index"></param>
    /// <param name="cleverness"></param>
    /// <returns></returns>
    bool TryGetCleverness(int index, out ClevernessHolder cleverness);
}

public sealed class ClevernessHolder
{
    /// <summary>
    /// スキル
    /// </summary>
    private ICleverness Cleverness { get; }

    /// <summary>
    /// クールタイム
    /// </summary>
    private IDisposable m_Disposable;
    public bool IsActive => m_Disposable != null;

    public string Name => Cleverness.Name;
    public string Description => Cleverness.Description;

    public ClevernessHolder(ICleverness cleverness) => Cleverness = cleverness;

    /// <summary>
    /// 賢さ有効化
    /// </summary>
    /// <returns></returns>
    public void Activate(ClevernessContext ctx) => m_Disposable = Cleverness.Activate(ctx);

    /// <summary>
    /// 賢さ無効化
    /// </summary>
    public void Deactivate()
    {
        m_Disposable?.Dispose();
        m_Disposable = null;
    }
}

public class CharaClevernessHandler : ActorComponentBase, ICharaClevernessHandler
{
    [Inject]
    private ITurnManager m_TurnManager;

    /// <summary>
    /// 登録されたスキル
    /// </summary>
    private List<ClevernessHolder> m_Clevernesses = new List<ClevernessHolder>();

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register(this);
    }

    /// <summary>
    /// スキル登録
    /// </summary>
    /// <param name="cleverness"></param>
    IDisposable ICharaClevernessHandler.RegisterCleverness(ICleverness cleverness)
    {
        var holder = new ClevernessHolder(cleverness);
        ClevernessContext ctx = new ClevernessContext(Owner, m_TurnManager);
        holder.Activate(ctx);

        m_Clevernesses.Add(holder);
        return Disposable.CreateWithState((this, holder), tuple => tuple.Item1.m_Clevernesses.Remove(tuple.holder));
    }

    /// <summary>
    /// かしこさ切り替え
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    bool ICharaClevernessHandler.SwitchActivate(int index)
    {
        if (TryGetCleverness(index, out var cleverness) == false)
            return false;

        bool isActivate = cleverness.IsActive; // 現在のステータス
        if (isActivate == false)
        {
            ClevernessContext ctx = new ClevernessContext(Owner, m_TurnManager);
            cleverness.Activate(ctx);
        }
        else
            cleverness.Deactivate();

        return cleverness.IsActive;
    }

    /// <summary>
    /// 賢さ取得（あるなら）
    /// </summary>
    /// <param name="index"></param>
    /// <param name="cleverness"></param>
    /// <returns></returns>
    private bool TryGetCleverness(int index, out ClevernessHolder cleverness)
    {
        cleverness = null;
        if (index < 0 || index >= m_Clevernesses.Count)
            return false;

        cleverness = m_Clevernesses[index];
        return cleverness != null;
    }
    bool ICharaClevernessHandler.TryGetCleverness(int index, out ClevernessHolder cleverness) => TryGetCleverness(index, out cleverness);
}