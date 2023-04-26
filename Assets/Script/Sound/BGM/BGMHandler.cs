using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public interface IBGMHandler : ISingleton
{
    /// <summary>
    /// BGMスタート
    /// </summary>
    void Start();

    /// <summary>
    /// BGMストップ
    /// </summary>
    void Stop();
}

public class BGMHandler : Singleton<BGMHandler, IBGMHandler>, IBGMHandler
{
    [SerializeField, ReadOnly]
    private GameObject m_BGM;
    private AudioSource m_Audio;

    protected override void Awake()
    {
        base.Awake();
        var bgm = Instantiate(DungeonProgressManager.Interface.CurrentDungeonSetup.BGM);
        SetBGM(bgm);
    }

    /// <summary>
    /// BGMセット
    /// </summary>
    /// <param name="bgm"></param>
    private void SetBGM(GameObject bgm, bool play = true)
    {
        if (m_BGM != null)
            Destroy(m_BGM);

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
