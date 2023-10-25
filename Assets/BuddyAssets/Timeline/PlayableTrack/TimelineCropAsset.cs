using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineCropAsset : PlayableAsset
{
    [SerializeField]
    private float m_Duration;

    [SerializeField]
    private string m_Text;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        //PlayableBehaviourを継承したTimelineCropクラスを元に、PlayableAsset(コマ)を作る
        var player = ScriptPlayable<TimelineCrop>.Create(graph);

        //TimelineCropクラスにあるプロパティを設定
        var behaviour = player.GetBehaviour();
        behaviour.Duration = m_Duration;
        behaviour.Text = m_Text;

        return player;
    }
}