using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public interface ICell
{
    /// <summary>
    /// GameObject
    /// </summary>
    GameObject CellObject { get; set; }

    /// <summary>
    /// 座標
    /// </summary>
    Vector3Int Position { get; }
    int X { get; }
    int Z { get; }

    /// <summary>
    /// 部屋Id
    /// </summary>
    int RoomId { get; set; }

    /// <summary>
    /// CellId
    /// </summary>
    CELL_ID CellId { get; set; }

    /// <summary>
    /// 罠取得、あるなら
    /// </summary>
    /// <param name="trap"></param>
    /// <returns></returns>
    bool TryGetTrap(out TrapBase trap);

    /// <summary>
    /// 罠設置
    /// </summary>
    /// <param name="trap"></param>
    void SetTrap(TrapBase trap);
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
    [SerializeField]
    private Vector3Int Position => gameObject.transform.position.ToV3Int();
    Vector3Int ICell.Position => Position;
    int ICell.X => Position.x;
    int ICell.Z => Position.z;

    /// <summary>
    /// CellId
    /// </summary>
    [SerializeField, ReadOnly]
    private CELL_ID m_CellId;
    CELL_ID ICell.CellId { get => m_CellId; set => m_CellId = value; }

    /// <summary>
    /// 部屋Id
    /// </summary>
    [SerializeField, ReadOnly]
    private int m_RoomId;
    int ICell.RoomId { get => m_RoomId; set => m_RoomId = value; }

    /// <summary>
    /// 罠
    /// </summary>
    [SerializeField, ReadOnly, Expandable]
    private TrapBase m_Trap;

    /// <summary>
    /// 罠取得
    /// </summary>
    /// <param name="trap"></param>
    /// <returns></returns>
    bool ICell.TryGetTrap(out TrapBase trap)
    {
        trap = m_Trap;
        return trap != null;
    }

    /// <summary>
    /// 罠取得
    /// </summary>
    /// <param name="trap"></param>
    void ICell.SetTrap(TrapBase trap) => m_Trap = trap;
}

public static class CellExtension
{

}