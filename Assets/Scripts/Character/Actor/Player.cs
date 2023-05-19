using UnityEngine;

[RequireComponent(typeof(ActorComponentCollector))]
[RequireComponent(typeof(CharaAnimator))]
[RequireComponent(typeof(CharaBattle))]
[RequireComponent(typeof(CharaLog))]
[RequireComponent(typeof(CharaMove))]
[RequireComponent(typeof(CharaObjectHolder))]
[RequireComponent(typeof(CharaSound))]
[RequireComponent(typeof(CharaStatus))]
[RequireComponent(typeof(CharaTurn))]
[RequireComponent(typeof(CharaTypeHolder))]
[RequireComponent(typeof(CharaLastActionHolder))]

[RequireComponent(typeof(CharaCellEventChecker))]
[RequireComponent(typeof(CharaInventory))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(CharaAutoRecovery))]
[RequireComponent(typeof(CharaStarvation))]

public class Player : MonoBehaviour { }