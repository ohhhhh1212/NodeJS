using socketionet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomListMgr : MonoBehaviour
{
    public ScrollRect scr_roomList;
    public GameObject pre_roomItem = null;
    public SORoomList m_roomList = null;
    public LobbyDlg m_lobbyDlg = null;

    void OnAck_InitRoomList(object obj, SORoomList roomList)
    {
        m_roomList = roomList;

        Debug.Log("·ë °¹¼ö : " + roomList.datas.Count);

        for (int i = 0; i < m_roomList.datas.Count; i++)
        {
            SORoom room = m_roomList.datas[i];
            CreateRoomItem(room);
        }
    }

    void OnAck_UpdateRoomList(object obj, SORoomList roomList)
    {
        ClearRoomList();
        m_roomList.datas.Clear();

        m_roomList = roomList;

        for (int i = 0; i < m_roomList.datas.Count; i++)
        {
            SORoom room = m_roomList.datas[i];
            CreateRoomItem(room);
        }
    }

    public void CreateRoomItem(SORoom room)
    {
        if (room.PlayerCount() == 0)
            return;

        GameObject go = Instantiate(pre_roomItem, scr_roomList.content);
        RoomItem roomItem = go.GetComponent<RoomItem>();
        roomItem.Init(room);
        go.GetComponent<Button>().onClick.AddListener(() => OnClicked_Room(roomItem));
    }

    void OnClicked_Room(RoomItem room)
    {
        m_lobbyDlg.input_roomName.text = room.roomName;
    }

    private void OnEnable()
    {
        CSocketIoMgr.Inst.onAck_InitRoomList += OnAck_InitRoomList;
        CSocketIoMgr.Inst.onNotify_UpdateRoomList += OnAck_UpdateRoomList;
    }

    private void OnDisable()
    {
        CSocketIoMgr.Inst.onAck_InitRoomList -= OnAck_InitRoomList;
        CSocketIoMgr.Inst.onNotify_UpdateRoomList -= OnAck_UpdateRoomList;
    }

    void ClearRoomList()
    {
        for (int i = 0; i < scr_roomList.content.childCount; i++)
        {
            Destroy(scr_roomList.content.GetChild(i).gameObject);
        }
    }

    private void OnDestroy()
    {
        ClearRoomList();
    }
}
