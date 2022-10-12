using UnityEngine;

public class CharaHolder : SingletonMonoBehaviour<CharaHolder>
{
    public GameObject CharaObject(Define.CHARA_NAME name)
    {
        switch (name)
        {
            case Define.CHARA_NAME.BOXMAN:
                return Boxman;

            case Define.CHARA_NAME.MASHROOM:
                return Mashroom;
        }
        return null;
    }

    [SerializeField] private GameObject m_Boxman;
    public GameObject Boxman
    {
        get { return m_Boxman; }
    }

    [SerializeField] private GameObject m_Mashroom;
    public GameObject Mashroom
    {
        get { return m_Mashroom; }
    }
}
