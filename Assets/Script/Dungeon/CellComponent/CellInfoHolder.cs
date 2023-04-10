using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public interface ICellInfoHolder : IActorInterface
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
}

public class CellInfoHolder : ActorComponentBase, ICellInfoHolder
{
    /// <summary>
    /// GameObject
    /// </summary>
    [SerializeField]
    private GameObject m_CellObject;
    GameObject ICellInfoHolder.CellObject { get => m_CellObject; set => m_CellObject = value; }

    /// <summary>
    /// Position
    /// </summary>
    [SerializeField]
    private Vector3Int Position => gameObject.transform.position.ToV3Int();
    Vector3Int ICellInfoHolder.Position => Position;
    int ICellInfoHolder.X => Position.x;
    int ICellInfoHolder.Z => Position.z;

    /// <summary>
    /// CellId
    /// </summary>
    [SerializeField, ReadOnly]
    private CELL_ID m_CellId;
    CELL_ID ICellInfoHolder.CellId { get => m_CellId; set => m_CellId = value; }

    /// <summary>
    /// 部屋Id
    /// </summary>
    [SerializeField, ReadOnly]
    private int m_RoomId;
    int ICellInfoHolder.RoomId { get => m_RoomId; set => m_RoomId = value; }

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICellInfoHolder>(this);
    }
}