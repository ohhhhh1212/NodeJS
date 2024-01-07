using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMgr
{
    static GameMgr _inst = null;
    public static GameMgr Inst
    {
        get
        {
            if (_inst == null)
                _inst = new GameMgr();

            return _inst;
        }
    }

    private GameMgr() { }

    public BattleFSM BattleFSM = new BattleFSM();
    public GameInfo GameInfo = new GameInfo();
    public GameScene m_gameScene = null;
}
