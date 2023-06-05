using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UniRx;
using System;
using Zenject;

public interface ICellInfoHandler : IActorInterface
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
    TERRAIN_ID CellId { get; set; }
}

public class CellInfoHandler : ActorComponentBase, ICellInfoHandler
{
    [Inject]
    private IObjectPoolController m_ObjectPoolController;

    /// <summary>
    /// GameObject
    /// </summary>
    [SerializeField, ReadOnly]
    private GameObject m_CellObject;
    GameObject ICellInfoHandler.CellObject { get => m_CellObject; set => m_CellObject = value; }

    /// <summary>
    /// Position
    /// </summary>
    [SerializeField]
    private Vector3 ObjectPosition => gameObject.transform.position;
    public Vector3Int Position => new Vector3Int((int)ObjectPosition.x, 0, (int)ObjectPosition.z);
    int ICellInfoHandler.X => Position.x;
    int ICellInfoHandler.Z => Position.z;

    /// <summary>
    /// CellId
    /// </summary>
    [SerializeField, ReadOnly]
    private TERRAIN_ID m_CellId;
    TERRAIN_ID ICellInfoHandler.CellId { get => m_CellId; set => m_CellId = value; }

    /// <summary>
    /// 部屋Id
    /// </summary>
    [SerializeField, ReadOnly]
    private int m_RoomId = -1;
    int ICellInfoHandler.RoomId { get => m_RoomId; set => m_RoomId = value; }

    /// <summary>
    /// 探索済みかどうか
    /// </summary>
    private ReactiveProperty<bool> m_IsExplored = new ReactiveProperty<bool>();
    public IObservable<bool> m_IsExploredObservable => m_IsExploredObservable;
    [ShowNativeProperty]
    private bool IsExplored { get => m_IsExplored.Value; set => m_IsExplored.Value = value; }

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICellInfoHandler>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();
        m_RoomId = -1;
    }

    protected override void Dispose()
    {
        var key = m_CellId.ToString();
        m_ObjectPoolController.SetObject(key, m_CellObject);

        base.Dispose();
    }
}