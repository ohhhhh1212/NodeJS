using socketionet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : MonoBehaviour
{
    public GameUI m_GameUI = null;
    public HudUI m_HudUI = null;

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        GameMgr.Inst.BattleFSM.SetReadyState();
    }

    public void Init()
    {
        GameMgr.Inst.m_gameScene = this;
        MoveNet.Inst.m_GameScene = this;
        GameMgr.Inst.BattleFSM.Initialize(OnReadyState, OnWaveState, OnGameState, OnResultState);
        GameMgr.Inst.GameInfo.Init();
        m_HudUI.Init();
        MoveNet.Inst.StartReceiver();
    }

    void OnReadyState()
    {
        GameMgr.Inst.GameInfo.Init();
        m_GameUI.OnReadyState();
        m_HudUI.OnReadyState();
    }
    void OnWaveState()
    {

    }
    void OnGameState()
    {
        m_HudUI.OnGameState();
        m_GameUI.OnGameState();
    }
    void OnResultState()
    {
        m_HudUI.OnResultState();
        m_GameUI.OnResultState();
    }

    private void Update()
    {
        GameMgr.Inst.BattleFSM.OnUpdate();

        if (GameMgr.Inst.BattleFSM.IsGameState())
        {
            GameMgr.Inst.GameInfo.CalTime();
        }
    }

    private void OnApplicationQuit()
    {
        CSocketIoMgr.Inst.SendReqDisconnect();
    }

}
