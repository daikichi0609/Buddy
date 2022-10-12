using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField] private int m_RoomID;
    public int RoomID
    {
        get { return m_RoomID; }
        set { m_RoomID = value; }
    }

    [SerializeField] private DungeonTerrain.GRID_ID m_GridID;
    public DungeonTerrain.GRID_ID GridID
    {
        get { return m_GridID; }
        set { m_GridID = value; }
    }
}
