using System.Collections.Generic;
using UnityEngine;
using Zenject;

public interface IEffectHolder
{
    bool TryGetEffect(string key, out GameObject gameObject);
}

public class EffectHolder : IEffectHolder, IInitializable
{
    [Inject]
    private EffectSetup m_EffectSetup;

    private Dictionary<string, GameObject> m_KeySoundPairs = new Dictionary<string, GameObject>();

    /// <summary>
    /// インスタンス生成
    /// </summary>
    void IInitializable.Initialize()
    {
        foreach (var pack in m_EffectSetup.EffectPacks)
        {
            var sound = MonoBehaviour.Instantiate(pack.Effect);
            m_KeySoundPairs.Add(pack.Key, sound);
        }
    }

    bool IEffectHolder.TryGetEffect(string key, out GameObject gameObject) => m_KeySoundPairs.TryGetValue(key, out gameObject);
}