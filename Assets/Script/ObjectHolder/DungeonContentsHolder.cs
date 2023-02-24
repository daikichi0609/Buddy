using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonContentsHolder : Singleton<DungeonContentsHolder>
{
    [SerializeField] private GameObject m_Wall;
    public GameObject Wall
    {
        get { return m_Wall; }
    }
    [SerializeField] private GameObject m_Stairs;
    public GameObject Stairs
    {
        get { return m_Stairs; }
    }

    [SerializeField] private GameObject m_CrystalRock_A;
    public GameObject CrystalRock_A
    {
        get { return m_CrystalRock_A; }
    }
    [SerializeField] private GameObject m_CrystalRock_B;
    public GameObject CrystalRock_B
    {
        get { return m_CrystalRock_B; }
    }
    [SerializeField] private GameObject m_CrystalRock_C;
    public GameObject CrystalRock_C
    {
        get { return m_CrystalRock_C; }
    }

    [SerializeField] private GameObject m_Grass_A;
    public GameObject Grass_A
    {
        get { return m_Grass_A; }
    }
    [SerializeField] private GameObject m_Grass_B;
    public GameObject Grass_B
    {
        get { return m_Grass_B; }
    }
    [SerializeField] private GameObject m_Grass_C;
    public GameObject Grass_C
    {
        get { return m_Grass_C; }
    }

    [SerializeField] private GameObject m_White_A;
    public GameObject White_A
    {
        get { return m_White_A; }
    }
    [SerializeField] private GameObject m_White_B;
    public GameObject White_B
    {
        get { return m_White_B; }
    }
    [SerializeField] private GameObject m_White_C;
    public GameObject White_C
    {
        get { return m_White_C; }
    }

    [SerializeField] private GameObject m_Rock_A;
    public GameObject Rock_A
    {
        get { return m_Rock_A; }
    }
    [SerializeField] private GameObject m_Rock_B;
    public GameObject Rock_B
    {
        get { return m_Rock_B; }
    }
    [SerializeField] private GameObject m_Rock_C;
    public GameObject Rock_C
    {
        get { return m_Rock_C; }
    }

    public GameObject PathWayGrid
    {
        get
        {
            switch (GameManager.Interface.DungeonTheme)
            {
                case Define.DUNGEON_THEME.GRASS:
                    return DungeonContentsHolder.Instance.Grass_C;

                case Define.DUNGEON_THEME.ROCK:
                    return DungeonContentsHolder.Instance.Rock_C;

                case Define.DUNGEON_THEME.WHITE:
                    return DungeonContentsHolder.Instance.White_C;

                case Define.DUNGEON_THEME.CRYSTAL:
                    return DungeonContentsHolder.Instance.CrystalRock_C;
            }
            return null;
        }
    }

    public GameObject RoomGrid
    {
        get
        {
            switch (GameManager.Interface.DungeonTheme)
            {
                case Define.DUNGEON_THEME.GRASS:
                    return DungeonContentsHolder.Instance.Grass_A;

                case Define.DUNGEON_THEME.ROCK:
                    return DungeonContentsHolder.Instance.Rock_A;

                case Define.DUNGEON_THEME.WHITE:
                    return DungeonContentsHolder.Instance.White_A;

                case Define.DUNGEON_THEME.CRYSTAL:
                    return DungeonContentsHolder.Instance.CrystalRock_A;
            }
            return null;
        }
    }
}
