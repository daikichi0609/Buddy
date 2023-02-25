using UnityEngine;

public class CharaHolder : Singleton<CharaHolder>
{
    public GameObject CharaObject(CHARA_NAME name)
    {
        switch (name)
        {
            case CHARA_NAME.BOXMAN:
                return m_Boxman;

            case CHARA_NAME.MASHROOM:
                return m_Mashroom;
        }
        return null;
    }

    [SerializeField]
    private GameObject m_Boxman;

    [SerializeField]
    private GameObject m_Mashroom;
}
