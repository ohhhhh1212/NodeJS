using socketionet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RoomDlg : MonoBehaviour
{
    public Text txt_RoomName = null;
    public Text txt_PlayerCount = null;
    public Button btn_Start = null;
    public Button btn_Ready = null;
    public Button btn_Exit = null;
    public List<RoomUser> m_RoomUsers = null;

    int myIdx { get; set; } = -1;

    private void Start()
    {
        Init();
    }

    void Init()
    {
        InitUsers();
        txt_RoomName.text = $"Room : {CSocketIoMgr.MyRoom.roomId}";
        txt_PlayerCount.text = $"Player : {CSocketIoMgr.MyRoom.PlayerCount()}/{CSocketIoMgr.MyRoom.maxPlayer}";
        txt_RoomName.text = CSocketIoMgr.MyRoom.roomId;

        btn_Start.onClick.AddListener(OnClicked_Start);
        btn_Ready.onClick.AddListener(OnClicked_Ready);
        btn_Exit.onClick.AddListener(OnClicked_Exit);
    }

    void InitUsers()
    {
        myIdx = GetMyIdx(CSocketIoMgr.MyUserInfo.id);
        int count = CSocketIoMgr.MyRoom.PlayerCount();
        for (int i = 0; i < count; i++)
        {
            SOPlayer player = CSocketIoMgr.MyRoom.players[i];
            m_RoomUsers[i].PlayerEnter(player.Name(), player.userState);
            if (player.isMaster)
                m_RoomUsers[i].OnMyMasterIkon(true);
        }

        m_RoomUsers[myIdx].OnMyUserIkon(true);
    }

    void EnterPlayer(string name, int state)
    {
        int idx = CSocketIoMgr.MyRoom.PlayerCount() - 1;

        if(idx < 0)
        {
            Debug.Log("인덱스가 0이하임");
            return;
        }

        m_RoomUsers[idx].PlayerEnter(name, state);

        txt_PlayerCount.text = $"Player : {CSocketIoMgr.MyRoom.PlayerCount()}/{CSocketIoMgr.MyRoom.maxPlayer}";
    }

    void OnClicked_Start()
    {
        if (!CSocketIoMgr.IsMasterClient)
            return;

        // 모두 준비 상태면 시작
        int cnt = CSocketIoMgr.MyRoom.PlayerCount();
        for (int i = 0; i < cnt; i++)
        {
            int state = m_RoomUsers[i].m_state;
            if(state != (int)EUserState.eReady)
            {
                Debug.Log("준비되지 않은 플레이어가 있습니다.");
                return;
            }
        }

        // 게임 시작
        CSocketIoMgr.Inst.SendReqGameStart(CSocketIoMgr.NickName());
        SceneManager.LoadScene("GameScene");
    }

    void OnClicked_Ready()
    {
        // 레디 이미지 켜기
        if(myIdx == -1)
        {
            Debug.Log("인덱스 계산 잘못됨");
            return;
        }

        m_RoomUsers[myIdx].SetMyReadyState();
    }

    void OnClicked_Exit()
    {
        // 로비로 이동
        CSocketIoMgr.Inst.SendReqLeaveRoom(m_RoomUsers[myIdx].m_name);
        SceneManager.LoadScene(0);
    }

    void OnNotify_EnterRoom(object obj, SOPlayer player)
    {
        EnterPlayer(player.Name(), player.userState);
    }

    void OnNotify_LeaveRoom(object obj, string id)
    {
        Debug.Log("나간 사람 이름 : " + id);
        RefreshRoomUsers();
        txt_PlayerCount.text = $"Player : {CSocketIoMgr.MyRoom.PlayerCount()}/{CSocketIoMgr.MyRoom.maxPlayer}";
    }

    void OnNotify_RoomUserReady(object obj, SORoomUserState user)// id , state
    {
        int idx = GetMyIdx(user.id);

        Debug.Log(idx + "번째 유저");

        m_RoomUsers[idx].SetReadyState(user.userState);
    }

    void OnNotify_RoomChangeMaster(object obj, SORoomChangeMaster masterid) // masterid
    {
        RefreshRoomUsers();

        int idx = GetMyIdx(masterid.masterId);

        m_RoomUsers[idx].OnMyMasterIkon(true);
    }

    void OnNotify_GameStart(object obj, bool isStart)
    {
        SceneManager.LoadScene("GameScene");
    }

    void RefreshRoomUsers()
    {
        for (int i = 0; i < m_RoomUsers.Count; i++)
            m_RoomUsers[i].Init();

        InitUsers();
    }

    int GetMyIdx(string name)
    {
        int cnt = CSocketIoMgr.MyRoom.PlayerCount();
        for (int i = 0; i < cnt; i++)
        {
            string kName = CSocketIoMgr.MyRoom.players[i].Name();
            if (kName == name)
                return i;
        }

        return -1;
    }

    private void OnEnable()
    {
        CSocketIoMgr.Inst.onNotify_EnterRoom += OnNotify_EnterRoom;
        CSocketIoMgr.Inst.onNotify_LeaveRoom += OnNotify_LeaveRoom;
        CSocketIoMgr.Inst.onNotify_RoomUserReady += OnNotify_RoomUserReady;
        CSocketIoMgr.Inst.onNotify_RoomChangeMaster += OnNotify_RoomChangeMaster;
        CSocketIoMgr.Inst.onNotify_GameStart += OnNotify_GameStart;
    }

    private void OnDisable()
    {
        CSocketIoMgr.Inst.onNotify_EnterRoom -= OnNotify_EnterRoom;
        CSocketIoMgr.Inst.onNotify_LeaveRoom -= OnNotify_LeaveRoom;
        CSocketIoMgr.Inst.onNotify_RoomUserReady -= OnNotify_RoomUserReady;
        CSocketIoMgr.Inst.onNotify_RoomChangeMaster -= OnNotify_RoomChangeMaster;
        CSocketIoMgr.Inst.onNotify_GameStart -= OnNotify_GameStart;
    }

    private void OnApplicationQuit()
    {
        CSocketIoMgr.Inst.SendReqDisconnect();
    }
}
