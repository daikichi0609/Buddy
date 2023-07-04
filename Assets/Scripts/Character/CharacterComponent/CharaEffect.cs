using System.Threading.Tasks;
using UniRx;
using Zenject;

public interface ICharaEffect : IActorInterface
{
    Task EffectFollow(string key);
}

public class CharaEffect : ActorComponentBase, ICharaEffect
{
    [Inject]
    private IEffectHolder m_EffectHolder;

    private ICharaObjectHolder m_CharaObjectHolder;

    private static readonly string DAMAGE_EFFECT = "Damage";

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register(this);
    }

    protected override void Initialize()
    {
        base.Initialize();
        m_CharaObjectHolder = Owner.GetInterface<ICharaObjectHolder>();

        //if (Owner.RequireEvent<ICharaBattleEvent>(out var battle) == true)
        //{
        //    battle.OnDamageStart.SubscribeWithState(this, (result, self) =>
        //    {
        //        if (result.IsHit == true)
        //            if (self.m_EffectHolder.TryGetEffect(DAMAGE_EFFECT, out var effect) == true)
        //                effect.Play(self.Owner);
        //    }).AddTo(Owner.Disposables);
        //}
    }

    async Task ICharaEffect.EffectFollow(string key)
    {
        if (m_EffectHolder.TryGetEffect(key, out var effect) == true)
        {
            var stop = effect.Play(Owner);
            var disposable = m_CharaObjectHolder.Follow(effect.gameObject);
            await Task.Delay(500);
            stop.Dispose();
            disposable.Dispose();
        }
    }
}