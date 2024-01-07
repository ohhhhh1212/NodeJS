using socketionet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomItem : MonoBehaviour
{
    [SerializeField] Text txt_roomName = null;
    [SerializeField] Text txt_roomState = null;
    [SerializeField] Button btn_room = null;
    public string roomName = "";
    string roomState = "";

    public void Init(SORoom room)
    {
        roomName = room.Name();

        if (room.roomState == (int)ERoomState.eReady)
            roomState = "대기";
        else
            roomState = "게임 중";

        txt_roomName.text = $"{roomName} ( {room.PlayerCount()}/{room.maxPlayer})";
        txt_roomState.text = roomState;
    }


}
