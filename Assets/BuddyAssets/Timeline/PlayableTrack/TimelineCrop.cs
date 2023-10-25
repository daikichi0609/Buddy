using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineCrop : PlayableBehaviour
{
    public float Duration { get; set; }

    public string Text { get; set; }

    //PlayableAsset(コマ)再生時実行される
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        // MessageBroker.Default.Publish(new DialogMessage(new DialogSetup.DialogPack(Duration, Text)));
    }
}