using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FungusMethod : MonoBehaviour
{
    public void OnEndDialog() => CheckPointController.Instance.OnEndDialog();
}
