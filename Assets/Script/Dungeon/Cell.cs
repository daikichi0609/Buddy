using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICell
{
    GameObject CellObject { get; set; }

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
    private GameObject m_CellObject;
    GameObject ICell.CellObject { get => m_CellObject; set => m_CellObject = value; }

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