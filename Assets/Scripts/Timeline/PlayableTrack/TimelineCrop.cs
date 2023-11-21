using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineCrop : PlayableBehaviour
{
    public string Text { get; set; }

    // クリップに入った時に走らせる処理
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        double duration = playable.GetDuration();
        MessageBroker.Default.Publish(new CropTextMessage(duration, Text));
    }
}