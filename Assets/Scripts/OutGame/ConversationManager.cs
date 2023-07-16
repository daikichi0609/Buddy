using System.Collections;
using System.Collections.Generic;
using Fungus;
using UnityEngine;

public interface IConversationManager
{
    /// <summary>
    /// 会話先登録
    /// </summary>
    /// <param name="chara"></param>
    void Register(ICollector chara, Flowchart flowchart, Vector3 pos);

    /// <summary>
    /// 話しかけられる相手がいるなら会話開始
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="player"></param>
    /// <returns></returns>
    bool TryTalk(Vector3 pos, ICollector player, out Vector3 dir);

    /// <summary>
    /// 会話終了
    /// </summary>
    void OnFinishTalking();
}

public class ConversationManager : MonoBehaviour, IConversationManager
{
    /// <summary>
    /// 会話先
    /// </summary>
    private HashSet<ICharaTalk> m_Talkable = new HashSet<ICharaTalk>();

    /// <summary>
    /// 会話中のプレイヤー
    /// </summary>
    private ICollector m_TalkingPlayer;

    /// <summary>
    /// 会話フローを持たせて登録
    /// </summary>
    /// <param name="chara"></param>
    /// <param name="flowchart"></param>
    /// <param name="pos"></param>
    /// <param name="conversationManager"></param>
    void IConversationManager.Register(ICollector chara, Flowchart flowchart, Vector3 pos)
    {
        // キャラを定位置に固定
        var friendConroller = chara.GetInterface<ICharaController>();
        friendConroller.Wrap(pos);
        friendConroller.Rigidbody.constraints = RigidbodyConstraints.FreezeAll; // 位置固定

        // 会話フローを持たせる
        var friendTalk = chara.GetInterface<ICharaTalk>();
        friendTalk.FlowChart = flowchart;

        m_Talkable.Add(friendTalk);
    }

    /// <summary>
    /// 話しかけられる相手がいるなら会話開始
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="player"></param>
    /// <param name="dest"></param>
    /// <returns></returns>
    bool IConversationManager.TryTalk(Vector3 pos, ICollector player, out Vector3 dest)
    {
        foreach (var talk in m_Talkable)
            if (talk.TryInteract(pos, out dest) == true)
            {
                m_TalkingPlayer = player;
                var input = m_TalkingPlayer.GetInterface<IOutGamePlayerInput>();
                input.CanOperate = false;
                return true;
            }

        dest = new Vector3();
        return false;
    }

    /// <summary>
    /// 会話終了時に操作可能にする
    /// </summary>
    void IConversationManager.OnFinishTalking()
    {
        if (m_TalkingPlayer == null)
        {
#if DEBUG
            Debug.LogAssertion("会話中のプレイヤーがいないのに会話終了メソッドが呼ばれました");
#endif
            return;
        }

        var input = m_TalkingPlayer.GetInterface<IOutGamePlayerInput>();
        input.CanOperate = true;
        m_TalkingPlayer = null;
    }
}
