﻿using UnityEngine;
using UniRx;
using System;
using System.Collections.Generic;
using NaughtyAttributes;
using Zenject;
using Fungus;

public interface ICharaInventory : IActorInterface
{
    /// <summary>
    /// アイテムをしまう
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    void Put(IItemHandler item);

    /// <summary>
    /// アイテムをしまう
    /// </summary>
    void Put(ItemSetup item);
}

public interface ICharaInventoryEvent : IActorInterface
{
    /// <summary>
    /// アイテムしまうとき
    /// </summary>
    IObservable<ItemPutInfo> OnPutItem { get; }

    /// <summary>
    /// アイテムしまうとき
    /// </summary>
    IObservable<ItemPutInfo> OnPutItemFail { get; }
}

public readonly struct ItemPutInfo
{
    public ICollector Owner { get; }
    public ItemSetup Item { get; }

    public ItemPutInfo(ICollector owner, ItemSetup item)
    {
        Owner = owner;
        Item = item;
    }
}

public class CharaInventory : ActorComponentBase, ICharaInventory, ICharaInventoryEvent
{
    [Inject]
    private ITeamInventory m_TeamInventory;
    [Inject]
    private IDungeonItemSpawner m_ItemSpawner;
    [Inject]
    private ISoundHolder m_SoundHolder;

    private static readonly string ITEM_PICKUP = "ItemPickUp";
    private static readonly string ITEM_PICKUP_ENEMY = "ItemPickUpEnemy";

    private ICharaTypeHolder m_Type;
    private ICharaMove m_CharaMove;

    /// <summary>
    /// アイテム入手
    /// </summary>
    Subject<ItemPutInfo> m_OnPutItem = new Subject<ItemPutInfo>();
    IObservable<ItemPutInfo> ICharaInventoryEvent.OnPutItem => m_OnPutItem;

    /// <summary>
    /// アイテム入手失敗
    /// </summary>
    Subject<ItemPutInfo> m_OnPutItemFail = new Subject<ItemPutInfo>();
    IObservable<ItemPutInfo> ICharaInventoryEvent.OnPutItemFail => m_OnPutItemFail;

    /// <summary>
    /// 所持アイテム
    /// </summary>
    private ItemSetup m_PocketItem;

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        owner.Register<ICharaInventory>(this);
        owner.Register<ICharaInventoryEvent>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();
        m_Type = Owner.GetInterface<ICharaTypeHolder>();
        m_CharaMove = Owner.GetInterface<ICharaMove>();

        if (Owner.RequireEvent<ICharaBattleEvent>(out var battle) == true)
        {
            battle.OnDead.SubscribeWithState(this, (_, self) => self.DropItem()).AddTo(Owner.Disposables);
        }
    }

    /// <summary>
    /// アイテムをしまう
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    void ICharaInventory.Put(IItemHandler item)
    {
        // 味方なら共有バッグに入れる
        if (m_Type.Type == CHARA_TYPE.FRIEND)
        {
            if (m_TeamInventory.TryPut(item.Setup) == true)
            {
                OnPutItem(item);
                if (m_SoundHolder.TryGetSound(ITEM_PICKUP, out var sound) == true)
                    sound.Play();
            }
            else
                m_OnPutItemFail.OnNext(new ItemPutInfo(Owner, item.Setup));
        }
        // 敵なら個人インベントリに入れる
        else if (m_Type.Type == CHARA_TYPE.ENEMY)
        {
            if (m_PocketItem == null)
            {
                m_PocketItem = item.Setup;
                OnPutItem(item);
                if (m_SoundHolder.TryGetSound(ITEM_PICKUP_ENEMY, out var sound) == true)
                    sound.Play();
            }
            else
                m_OnPutItemFail.OnNext(new ItemPutInfo(Owner, item.Setup));
        }

    }

    /// <summary>
    /// アイテム収納後共通処理
    /// </summary>
    /// <param name="item"></param>
    private void OnPutItem(IItemHandler item)
    {
        m_OnPutItem.OnNext(new ItemPutInfo(Owner, item.Setup));
        item.OnPut();
    }

    /// <summary>
    /// アイテムを落とす
    /// </summary>
    private void DropItem()
    {
        if (m_PocketItem != null)
        {
            m_ItemSpawner.SpawnItem(m_PocketItem, m_CharaMove.Position);
            m_PocketItem = null;
        }
    }

    protected override void Dispose()
    {
        m_PocketItem = null;
        base.Dispose();
    }

    /// <summary>
    /// セットアップからアイテム生成
    /// </summary>
    void ICharaInventory.Put(ItemSetup item)
    {
        if (m_TeamInventory.TryPut(item) == true)
        {
            if (m_SoundHolder.TryGetSound(ITEM_PICKUP, out var sound) == true)
                sound.Play();

            m_OnPutItem.OnNext(new ItemPutInfo(Owner, item));
        }
    }
}