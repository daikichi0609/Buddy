using System.Collections.Generic;
using UnityEngine;
using Zenject;

public interface ISoundHolder
{
    bool TryGetSoundObject(string key, out GameObject sound);
    bool TryGetSound(string key, out AudioSource sound);
}

public class SoundHolder : ISoundHolder, IInitializable
{
    [Inject]
    private SoundSetup m_SoundSetup;

    private Dictionary<string, GameObject> m_KeySoundPairs = new Dictionary<string, GameObject>();

    /// <summary>
    /// インスタンス生成
    /// </summary>
    void IInitializable.Initialize()
    {
        foreach (var pack in m_SoundSetup.SoundPacks)
        {
            var sound = MonoBehaviour.Instantiate(pack.Sound);
            m_KeySoundPairs.Add(pack.Key, sound);
        }
    }

    bool ISoundHolder.TryGetSoundObject(string key, out GameObject sound) => m_KeySoundPairs.TryGetValue(key, out sound);
    bool ISoundHolder.TryGetSound(string key, out AudioSource sound)
    {
        if (m_KeySoundPairs.TryGetValue(key, out var gameObject) == true)
        {
            sound = gameObject.GetComponent<AudioSource>();
            return true;
        }
        sound = null;
        return false;
    }
}