using UnityEngine;
using System.Collections.Generic;

public class BattleManager : SingletonMonoBehaviour<BattleManager>
{
    private delegate bool CheckFinishTurn();
    private CheckFinishTurn m_ChechFinishTurn;

    

    private int IsActingEnemyID;

    private bool m_PlayerTurn = true;
    public bool PlayerTurn
    {
        get { return m_PlayerTurn; }
        set { m_PlayerTurn = value; }
    }
}