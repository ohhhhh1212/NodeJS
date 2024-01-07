using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainDlg : MonoBehaviour
{
    public ScrollRect m_scrChat = null;
    public ScrollRect m_scrUsers = null;
    public InputField m_inputText = null;
    public Button m_btnSend = null;
    public Button m_btnLogout = null;
    public Button m_btnExit = null;
    public Text m_txtName = null;
    public GameObject m_preTextItem = null;
    public GameObject m_preUserItem = null;
    public LoginDlg m_LoginDlg = null;

    public string m_id = "";

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        SocketMgr.Inst.onAck_UserInfo += OnCallback_UserInfo;
        SocketMgr.Inst.onAck_UserInfoList += OnCallback_UserInfoList;
        SocketMgr.Inst.onAck_Logout += OnCallback_Logout;
        m_btnSend.onClick.AddListener(OnClicked_Send);
        m_btnLogout.onClick.AddListener(OnClicked_Logout);
        m_btnExit.onClick.AddListener(OnClicked_Exit);
    }

    public void SetMyId()
    {
        m_id = SocketMgr.Inst.m_MyUserInfo.id;
        m_txtName.text = $"[User] : {m_id}";
    }

    public void CreateUserItem(string id)
    {
        //if (!SocketMgr.Inst.isLogin)
        //    return;

        //GameObject go = Instantiate(m_preUserItem, m_scrUsers.content);
        //go.GetComponent<UserItem>().Init(id);
    }

    public void CreateTextItem(string text)
    {
        //if (!SocketMgr.Inst.isLogin)
        //    return;

        //GameObject go = Instantiate(m_preTextItem, m_scrChat.content);
        //go.GetComponent<TextItem>().Init(text);
    }

    void OnClicked_Logout()
    {
        SocketMgr.Inst.SendLogout(m_id);

        ClearPrefab();
        m_txtName.text = "My ID";
        m_inputText.text = string.Empty;
        m_LoginDlg.gameObject.SetActive(true);

        SocketMgr.Inst.isLogin = false;
    }

    void OnClicked_Send()
    {
        string str = m_inputText.text;

        if (str == "")
        {
            Debug.Log("메시지를 입력해주세요.");
            return;
        }

        string text = $"{m_id} : {str}";
        CreateTextItem(text);
        MyDataNet.Inst.SendBroadcastChatMsg(str);

        m_inputText.text = "";
    }

    void OnClicked_Exit()
    {
        SocketMgr.Inst.SendWithdraw(m_id);
        OnClicked_Logout();
    }

    void OnCallback_UserInfo(object obj, SocketMgr.UserInfo newUser)
    {
        CreateUserItem(newUser.id);
    }

    void OnCallback_UserInfoList(object obj, SocketMgr.UserInfoList infolist)
    {
        for (int i = 0; i < infolist.datas.Count; i++)
        {
            CreateUserItem(infolist.datas[i].id);
        }

        MyDataNet.Inst.StartReceiver();
    }

    void OnCallback_Logout(object obj, string id)
    {
        //for (int i = 0; i < m_scrUsers.content.childCount; i++)
        //{
        //    UserItem user = m_scrUsers.content.GetComponentsInChildren<UserItem>()[i];
        //    if (user.m_id == id)
        //    {
        //        Destroy(user.gameObject);
        //        break;
        //    }
        //}
    }

    void ClearPrefab()
    {
        for (int i = 0; i < m_scrChat.content.transform.childCount; i++)
        {
            Destroy(m_scrChat.content.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < m_scrUsers.content.transform.childCount; i++)
        {
            Destroy(m_scrUsers.content.transform.GetChild(i).gameObject);
        }
    }

    private void OnDestroy()
    {
        ClearPrefab();
    }

    private void OnApplicationQuit()
    {
        OnClicked_Logout();
    }
}
