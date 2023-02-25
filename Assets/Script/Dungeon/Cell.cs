using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICell
{
    GameObject GameObject { get; set; }

    Vector3Int Position { get; }
    int X { get; }
    int Z { get; }

    int RoomId { get; set; }

    CELL_ID CellId { get; set; }
}

public class Cell : MonoBehaviour, ICell
{
    /// <summary>
    /// GameObject
    /// </summary>
    [SerializeField]
    private GameObject m_GameObject;
    GameObject ICell.GameObject { get => m_GameObject; set => m_GameObject = value; }

    /// <summary>
    /// Position
    /// </summary>
    private Vector3Int Position => gameObject.transform.position.ToV3Int();
    Vector3Int ICell.Position => Position;
    int ICell.X => Position.x;
    int ICell.Z => Position.z;

    /// <summary>
    /// 部屋Id
    /// </summary>
    private int m_RoomId;
    int ICell.RoomId { get => m_RoomId; set => m_RoomId = value; }

    /// <summary>
    /// CellId
    /// </summary>
    private CELL_ID m_CellId;
    CELL_ID ICell.CellId { get => m_CellId; set => m_CellId = value; }
}

public static class CellExtension
{

}