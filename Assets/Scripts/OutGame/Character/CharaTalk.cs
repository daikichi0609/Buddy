using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharaTalk : IActorInterface
{
    /// <summary>
    /// 会話アセット
    /// </summary>
    Fungus.Flowchart FlowChart { get; set; }

    /// <summary>
    /// 話しかける
    /// </summary>
    /// <returns></returns>
    bool TryTalk();

    /// <summary>
    /// 話しかけられる
    /// </summary>
    /// <returns></returns>
    bool TryInteract(Vector3 pos, out Vector3 dir);
}

public class CharaTalk : ActorComponentBase, ICharaTalk
{
    private static readonly float TALK_DISTANCE = 2.0f;
    private static readonly string SPOKEN_MESSAGE = "Spoken";

    private ICharaController m_CharaController;

    /// <summary>
    /// 会話フローチャート
    /// </summary>
    private Fungus.Flowchart m_FlowChart;
    Fungus.Flowchart ICharaTalk.FlowChart { get => m_FlowChart; set => m_FlowChart = value; }

    protected override void Register(ICollector owner)
    {
        base.Register(owner);
        Owner.Register<ICharaTalk>(this);
    }

    protected override void Initialize()
    {
        base.Initialize();
        m_CharaController = Owner.GetInterface<ICharaController>();
    }

    /// <summary>
    /// 話しかける（対象がいるなら）
    /// </summary>
    /// <returns></returns>
    bool ICharaTalk.TryTalk()
    {
        var pos = m_CharaController.Position;
        if (ConversationManager.Interface.TryTalk(pos, Owner, out var dir) == true)
        {
            m_CharaController.Face(dir);
            m_CharaController.StopAnimation(ANIMATION_TYPE.MOVE);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 話しかけられるか
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    bool ICharaTalk.TryInteract(Vector3 pos, out Vector3 dir)
    {
        dir = m_CharaController.Position - pos;
        var distance = dir.magnitude;
        if (distance <= TALK_DISTANCE)
        {
            m_CharaController.Face(dir.ToOppositeDir());
            m_CharaController.StopAnimation(ANIMATION_TYPE.IDLE);
            m_FlowChart.SendFungusMessage(SPOKEN_MESSAGE);
            return true;
        }
        return false;
    }
}
