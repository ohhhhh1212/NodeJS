using socketionet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyDlg : MonoBehaviour
{
    public Text txt_id = null;
    public Text txt_result = null;
    public InputField input_roomName = null;
    public Button btn_join = null;
    public Button btn_create = null;
    public Button btn_option = null;
    public RoomListMgr m_roomList = null;

    string roomName = "";

    private void Start()
    {
        Init();
    }

    void Init()
    {
        btn_join.onClick.AddListener(OnClicked_Join);
        btn_create.onClick.AddListener(OnClicked_Create);
        btn_option.onClick.AddListener(OnClicked_Option);
    }

    public void InitMyId()
    {
        txt_id.text = CSocketIoMgr.NickName();
    }

    void OnClicked_Join()
    {
        // 방이 이미 차있는지 확인 -> 차있으면 send X
        if(Check_RoomName())
        {
            txt_result.text = "방 이름을 다시 확인해주세요.";
            return;
        }

        CSocketIoMgr.Inst.SendReqJoinRoom(input_roomName.text);

        ClearInput();
    }

    void OnClicked_Create()
    {
        roomName = input_roomName.text;
        ClearInput();

        if (roomName == "")
        {
            txt_result.text = "방 이름을 똑바로 입력하세요.";
            return;
        }

        CSocketIoMgr.Inst.SendReqCreateRoom(roomName);
    }

    void OnClicked_Option()
    {

    }

    void OnAck_CreateRoom(object obj, SORoom room)
    {
        // 씬 이동 -> 룸 씬으로
        Debug.Log("성공적으로 룸이 생성 되었습니다");
        SceneManager.LoadScene(1);
    }

    void OnAck_JoinRoom(object obj, SORoom room)
    {
        if(room.PlayerCount() > 4)
        {
            txt_result.text = "방이 꽉 찼습니다";
            return;
        }

        // 씬 이동 -> 룸 씬으로
        Debug.Log("성공적으로 룸에 입장 하였습니다.");
        SceneManager.LoadScene(1);
    }

    bool Check_RoomName()
    {
        string name = input_roomName.text;

        if (name == "")
            return true;

        for (int i = 0; i < m_roomList.m_roomList.datas.Count; i++)
        {
            string id = m_roomList.m_roomList.datas[i].Name();
            if (name == id)
                return false;
        }

        return true;
    }

    void ClearInput()
    {
        input_roomName.text = "";
    }

    private void OnEnable()
    {
        CSocketIoMgr.Inst.onAck_CreateRoom += OnAck_CreateRoom;
        CSocketIoMgr.Inst.onAck_JoinRoom += OnAck_JoinRoom;
    }

    private void OnDisable()
    {
        CSocketIoMgr.Inst.onAck_CreateRoom -= OnAck_CreateRoom;
        CSocketIoMgr.Inst.onAck_JoinRoom -= OnAck_JoinRoom;
    }
}
