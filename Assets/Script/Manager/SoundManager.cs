using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager :  SingletonMonoBehaviour<SoundManager>
{
    [SerializeField] private AudioSource m_BlueCrossBGM;
    public AudioSource BlueCrossBGM
    {
        get { return m_BlueCrossBGM; }
    }

    [SerializeField] private AudioSource m_GrassBGM;
    public AudioSource GrassBGM
    {
        get { return m_GrassBGM; }
    }

    [SerializeField] private AudioSource m_RockBGM;
    public AudioSource RockBGM
    {
        get { return m_RockBGM; }
    }

    [SerializeField] private AudioSource m_CrystalBGM;
    public AudioSource CrystalBGM
    {
        get { return m_CrystalBGM; }
    }

    [SerializeField] private AudioSource m_WhiteBGM;
    public AudioSource WhiteBGM
    {
        get { return m_WhiteBGM; }
    }

    [SerializeField] private AudioSource m_BossBGM;
    public AudioSource BossBGM
    {
        get { return m_BossBGM; }
    }

    [SerializeField] private AudioSource m_KD;
    public AudioSource KD
    {
        get { return m_KD; }
    }

    [SerializeField] private AudioSource m_Attack_Sword;
    public AudioSource Attack_Sword
    {
        get { return m_Attack_Sword; }
    }

    [SerializeField] private AudioSource m_Damage_Small;
    public AudioSource Damage_Small
    {
        get { return m_Damage_Small; }
    }

    [SerializeField] private AudioSource m_Miss;
    public AudioSource Miss
    {
        get { return m_Miss; }
    }
}
