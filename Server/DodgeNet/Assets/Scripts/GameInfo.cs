using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInfo
{
    public void Init()
    {
        hp = 100;
        time = 0f;
    }

    public void AttackPlayer()
    {
        hp -= 10;
        if(hp <= 0)
        {
            MoveNet.Inst.IsSendMove = false;
            GameMgr.Inst.BattleFSM.SetResultState();
        }
    }

    int hp = 100;

    public int PlayerHP
    {
        get { return hp; }
    }

    float time = 0f;

    public void CalTime()
    {
        time += Time.deltaTime;
    }

    public int Sec
    {
        get { return (int)time; }
    }
}
