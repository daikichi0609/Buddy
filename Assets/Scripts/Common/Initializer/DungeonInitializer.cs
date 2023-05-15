using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class DungeonInitializer : MonoBehaviour
{
    private void Awake() => PlayerLoopManager.Interface.GetInitEvent.Subscribe(_ => DungeonProgressManager.Interface.InitializeDungeon()).AddTo(this);
}
