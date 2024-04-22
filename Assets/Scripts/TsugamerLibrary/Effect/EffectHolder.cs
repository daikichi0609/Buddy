using System.Collections.Generic;
using UnityEngine;
using Zenject;

public interface IEffectHolder
{
    bool TryGetEffectObject(string key, out GameObject gameObject);
    bool TryGetEffect(string key, out ParticleSystemHolder holder);
}

public class EffectHolder : IEffectHolder, IInitializable
{
    [Inject]
    private EffectSetup m_EffectSetup;

    private Dictionary<string, List<GameObject>> m_KeySoundPairs = new Dictionary<string, List<GameObject>>();

    /// <summary>
    /// インスタンス生成
    /// </summary>
    void IInitializable.Initialize()
    {
        foreach (var pack in m_EffectSetup.EffectPacks)
        {
            var effect = MonoBehaviour.Instantiate(pack.Effect);
            var list = new List<GameObject>();
            list.Add(effect);
            m_KeySoundPairs.Add(pack.Key, list);
        }
    }

    private bool TryGetEffect(string key, out GameObject gameObject)
    {
        gameObject = null;
        if (m_KeySoundPairs.TryGetValue(key, out var list) == true)
        {
            foreach (var effect in list)
            {
                if (gameObject != null)
                    break;

                var holderSystems = effect.GetComponent<ParticleSystemHolder>();
                if (holderSystems.IsPlaying == true)
                    break;

                gameObject = effect;
            }

            if (gameObject != null)
                return true;
            else
            {
                var newEffect = MonoBehaviour.Instantiate(list[0]);
                list.Add(newEffect);
                gameObject = newEffect;
                return true;
            }
        }
        else
            return false;
    }
    bool IEffectHolder.TryGetEffectObject(string key, out GameObject gameObject) => TryGetEffect(key, out gameObject);
    bool IEffectHolder.TryGetEffect(string key, out ParticleSystemHolder holder)
    {
        if (TryGetEffect(key, out var gameObject) == true)
        {
            holder = gameObject.GetComponent<ParticleSystemHolder>();
            return holder != null;
        }
        holder = null;
        return false;
    }
}