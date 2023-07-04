using System.Threading.Tasks;
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

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register(this);
    }

    protected override void Initialize()
    {
        base.Initialize();
        m_CharaObjectHolder = Owner.GetInterface<ICharaObjectHolder>();
    }

    async Task ICharaEffect.EffectFollow(string key)
    {
        var pos = Owner.GetInterface<ICharaObjectHolder>().MoveObject.transform.position;
        if (m_EffectHolder.TryGetEffect(key, out var effect) == true)
        {
            var stop = effect.Play(pos);
            var disposable = m_CharaObjectHolder.Follow(effect.gameObject);
            await Task.Delay(500);
            stop.Dispose();
            disposable.Dispose();
        }
    }
}