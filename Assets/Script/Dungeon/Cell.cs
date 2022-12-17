using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICell
{
    GameObject GameObject { get; set; }

    Vector3 Position { get; }
    int X { get; }
    int Z { get; }

    int RoomId { get; set; }

    GRID_ID GridID { get; set; }
}

public class Cell : MonoBehaviour, ICell
{
    private GameObject m_GameObject;
    GameObject ICell.GameObject { get; set; }

    private Vector3 Position => gameObject.transform.position;
    Vector3 ICell.Position => Position;
    int ICell.X => (int)Position.x;
    int ICell.Z => (int)Position.z;

    int ICell.RoomId { get; set; }

    GRID_ID ICell.GridID { get; set; }
}

public static class CellExtension
{

}