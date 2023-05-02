using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FungusMethod : MonoBehaviour
{
    public void Fade() => FadeManager.Interface.StartFade(() => CheckPointController.Instance.ReadyToPlayable(), string.Empty, string.Empty);
}
