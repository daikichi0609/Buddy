using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Zenject;

public interface ISoundHolder
{
    bool TryGetSound(string key, out AudioSource sound);
}

public class SoundHolder : ISoundHolder, IInitializable
{
    [Inject]
    private SoundSetup m_SoundSetup;

    private Dictionary<string, AudioSource> m_KeySoundPairs = new Dictionary<string, AudioSource>();

    /// <summary>
    /// インスタンス生成
    /// </summary>
    void IInitializable.Initialize()
    {
        foreach (var pack in m_SoundSetup.SoundPacks)
        {
            var sound = MonoBehaviour.Instantiate(pack.Sound).GetComponent<AudioSource>();
            m_KeySoundPairs.Add(pack.Key, sound);
        }
    }

    bool ISoundHolder.TryGetSound(string key, out AudioSource sound) => m_KeySoundPairs.TryGetValue(key, out sound);
}
