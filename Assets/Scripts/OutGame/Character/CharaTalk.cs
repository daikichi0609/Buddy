using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

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
    bool TryInteract(Vector3 pos, out Vector3 dest);
}

public class CharaTalk : ActorComponentBase, ICharaTalk
{
    [Inject]
    private IConversationManager m_ConversationManager;

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
        if (m_ConversationManager.TryTalk(pos, Owner, out var dest) == true)
        {
            m_CharaController.Face(dest);
            m_CharaController.StopAnimation();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 話しかけられるか
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    bool ICharaTalk.TryInteract(Vector3 pos, out Vector3 dest)
    {
        dest = m_CharaController.Position;

        var dir = pos - dest;
        var distance = dir.magnitude;
        // 会話成功
        if (distance <= TALK_DISTANCE)
        {
            m_CharaController.Face(pos);
            m_CharaController.StopAnimation();
            m_FlowChart.SendFungusMessage(SPOKEN_MESSAGE);
            return true;
        }
        return false;
    }
}
