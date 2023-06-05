using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public interface IBGMHandler : ISingleton
{
    /// <summary>
    /// BGMセット
    /// </summary>
    void SetBGM(GameObject bgm, bool play = true);

    /// <summary>
    /// BGMスタート
    /// </summary>
    void Start();

    /// <summary>
    /// BGMストップ
    /// </summary>
    void Stop();
}

public class BGMHandler : MonoBehaviour, IBGMHandler
{
    [SerializeField, ReadOnly]
    private GameObject m_BGM;
    private AudioSource m_Audio;

    /// <summary>
    /// BGMセット
    /// </summary>
    /// <param name="bgm"></param>
    void IBGMHandler.SetBGM(GameObject bgm, bool play)
    {
        if (m_BGM != null)
            MonoBehaviour.Destroy(m_BGM);

        m_BGM = bgm;
        m_Audio = m_BGM.GetComponent<AudioSource>();

        if (play == true)
            m_Audio.Play();
    }

    /// <summary>
    /// BGMスタート
    /// </summary>
    void IBGMHandler.Start()
    {
        m_Audio.Play();
    }

    /// <summary>
    /// BGMストップ
    /// </summary>
    void IBGMHandler.Stop()
    {
        m_Audio.Stop();
    }
}
