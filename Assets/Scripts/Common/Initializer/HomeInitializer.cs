using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Threading.Tasks;

public class HomeInitializer : SceneInitializer<HomeInitializer>
{
    protected override string FungusMessage => "Home";

    protected override Vector3 LeaderStartPos => new Vector3(-1f, OFFSET_Y, -5f);
    protected override Vector3 LeaderEndPos => new Vector3(-1f, OFFSET_Y, -1.5f);

    protected override Vector3 FriendStartPos => new Vector3(1f, OFFSET_Y, -5f);
    protected override Vector3 FriendEndPos => new Vector3(1f, OFFSET_Y, 0f);

    /// <summary>
    /// スタート処理
    /// </summary>
    protected override void OnStart()
    {
        // 仮
        FadeManager.Interface.TurnBright(async () => await OnTurnBright(), "", "");
    }

    /// <summary>
    /// 会話開始前
    /// </summary>
    async protected override Task OnTurnBright()
    {
        await FadeManager.Interface.LoadScene(SceneName.SCENE_DUNGEON);
    }

    /// <summary>
    /// 操作可能にする
    /// </summary>
    public override void ReadyToOperatable()
    {

    }
}
