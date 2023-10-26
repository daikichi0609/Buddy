using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using NaughtyAttributes;

public class TimelineCropAsset : PlayableAsset
{
    [SerializeField, ResizableTextArea]
    private string m_Text;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        //PlayableBehaviourを継承したTimelineCropクラスを元に、PlayableAsset(コマ)を作る
        var player = ScriptPlayable<TimelineCrop>.Create(graph);

        //TimelineCropクラスにあるプロパティを設定
        var behaviour = player.GetBehaviour();
        behaviour.Text = m_Text;

        return player;
    }
}