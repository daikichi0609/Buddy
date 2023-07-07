using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using Zenject;

public interface ICharaClevernessHandler : IActorInterface
{
    bool TryGetCleverness(int index, out ICleverness skill);

    /// <summary>
    /// スキル登録
    /// </summary>
    /// <param name="skill"></param>
    void RegisterCleverness(ICleverness skill);

    /// <summary>
    /// 賢さ有効化
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    void Activate(int index);
}

public class CharaClevernessHandler : ActorComponentBase, ICharaClevernessHandler
{
    private sealed class ClevernessHolder
    {
        /// <summary>
        /// スキル
        /// </summary>
        private ICleverness Cleverness { get; }
        public ICleverness GetCleverness => Cleverness;

        /// <summary>
        /// クールタイム
        /// </summary>
        private IDisposable m_Disposable;
        public bool IsActive => m_Disposable != null;

        public ClevernessHolder(ICleverness cleverness) => Cleverness = cleverness;

        /// <summary>
        /// 賢さ有効化
        /// </summary>
        /// <returns></returns>
        public void Activate(ClevernessContext ctx) => m_Disposable = Cleverness.Activate(ctx);

        /// <summary>
        /// 賢さ無効化
        /// </summary>
        public void Deactivate() => m_Disposable?.Dispose();
    }

    [Inject]
    private IDungeonHandler m_DungeonHandler;
    [Inject]
    private IUnitFinder m_UnitFinder;

    /// <summary>
    /// 登録されたスキル
    /// </summary>
    private List<ClevernessHolder> m_Clevernesses = new List<ClevernessHolder>();

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register(this);
    }

    bool ICharaClevernessHandler.TryGetCleverness(int index, out ICleverness cleverness)
    {
        cleverness = null;
        if (index < 0 || index >= m_Clevernesses.Count)
            return false;

        cleverness = m_Clevernesses[index].GetCleverness;
        return cleverness != null;
    }

    /// <summary>
    /// スキル登録
    /// </summary>
    /// <param name="cleverness"></param>
    void ICharaClevernessHandler.RegisterCleverness(ICleverness cleverness)
    {
        var holder = new ClevernessHolder(cleverness);
        m_Clevernesses.Add(holder);
    }

    /// <summary>
    /// かしこさ有効化
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    void ICharaClevernessHandler.Activate(int key)
    {
        int index = key - 1;
        if (index < 0 || index >= m_Clevernesses.Count)
            return;

        var clevernessHolder = m_Clevernesses[index];

        ClevernessContext ctx = new ClevernessContext(Owner, m_DungeonHandler, m_UnitFinder);
        clevernessHolder.Activate(ctx);
    }
}