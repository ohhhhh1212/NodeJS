using socketionet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : MonoBehaviour
{
    public LoginDlg loginDlg = null;
    public LobbyDlg lobbyDlg = null;

    private void Awake()
    {
        CSocketIoMgr.Inst.m_LobbyScene = this;

        if (!CSocketIoMgr.Inst.IsConnected)
            CSocketIoMgr.Inst.InitSocketIO("192.168.0.61", 19000);
            //CSocketIoMgr.Inst.InitSocketIO("192.168.0.72", 19000);
    }

    private void Start()
    {
        if (CSocketIoMgr.Inst.isLogin)
        {
            loginDlg.gameObject.SetActive(false);
            CSocketIoMgr.Inst.SendReqInitRoomList();
            lobbyDlg.InitMyId();
        }
        else
            loginDlg.gameObject.SetActive(true);
    }

    private void OnApplicationQuit()
    {
        CSocketIoMgr.Inst.SendReqDisconnect();
    }
}
