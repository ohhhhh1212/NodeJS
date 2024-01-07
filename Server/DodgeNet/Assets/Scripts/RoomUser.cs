using socketionet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomUser : MonoBehaviour
{
    public int m_state { get; set; } = -1;
    public string m_name = "";
    public Text txt_State = null;
    public Text txt_Name = null;
    public Image img_ReadyState = null;
    public Image img_MyUser = null;
    public Image img_RoomMaster = null;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        m_name = "";
        txt_State.text = "Empty";
        txt_Name.text = "None";
        m_state = -1;
        img_ReadyState.gameObject.SetActive(false);
        img_MyUser.gameObject.SetActive(false);
        img_RoomMaster.gameObject.SetActive(false);
    }

    public void PlayerEnter(string name, int state)
    {
        Debug.Log(state);
        m_state = state;
        m_name = name;

        SetReadyState2();

        txt_Name.text = m_name;
    }

    public void SetReadyState2()
    {
        string str;
        bool isActive = false;

        if (m_state == (int)EUserState.eReady)
        {
            str = "Ready";
            m_state = (int)EUserState.eReady;
            isActive = true;
        }
        else if (m_state == (int)EUserState.eEnter)
        {
            str = "Enter";
            m_state = (int)EUserState.eEnter;
            isActive = false;
        }
        else
        {
            str = "Empty";
            m_state = (int)EUserState.eEmpty;
        }

        img_ReadyState.gameObject.SetActive(isActive);

        txt_State.text = str;
    }

    public void SetReadyState(int state)
    {
        string str;
        bool isActive = false;

        if(state == (int)EUserState.eReady)
        {
            str = "Ready";
            m_state = (int)EUserState.eReady;
            isActive = true;
        }
        else if (state == (int)EUserState.eEnter)
        {
            str = "Enter";
            m_state = (int)EUserState.eEnter;
            isActive = false;
        }
        else
        {
            str = "다른숫자가 나옴";
            m_state = (int)EUserState.eEmpty;
        }

        img_ReadyState.gameObject.SetActive(isActive);

        txt_State.text = str;
    }

    public string SetReadyState()
    {
        string str;
        bool isActive = false;

        if (m_state == (int)EUserState.eEnter)
        {
            str = "Ready";
            m_state = (int)EUserState.eReady;
            isActive = true;
        }
        else if (m_state == (int)EUserState.eReady)
        {
            str = "Enter";
            m_state = (int)EUserState.eEnter;
            isActive = false;
        }
        else
        {
            str = "다른숫자가 나옴";
            m_state = (int)EUserState.eEmpty;
        }

        img_ReadyState.gameObject.SetActive(isActive);

        return str;
    }

    public void SetMyReadyState()
    {
        bool isActive = !img_ReadyState.IsActive();
        string str = SetReadyState();
        txt_State.text = str;

        if (isActive)
            CSocketIoMgr.Inst.SendReqRoomReady(m_name, (int)EUserState.eReady);
        else
            CSocketIoMgr.Inst.SendReqRoomReady(m_name, (int)EUserState.eEnter);
    }

    public void OnMyUserIkon(bool active)
    {
        img_MyUser.gameObject.SetActive(active);
    }

    public void OnMyMasterIkon(bool active)
    {
        img_RoomMaster.gameObject.SetActive(active);
    }
}
