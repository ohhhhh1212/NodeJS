using socketionet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginDlg : MonoBehaviour
{
    public InputField input_Id = null;
    public InputField input_Pw = null;
    public Button btn_Login = null;
    public Button btn_Join = null;
    public Button btn_Exit = null;
    public LobbyDlg lobbyDlg = null;

    string id = "";
    string pw = "";

    private void Start()
    {
        Init();
    }

    void Init()
    {
        btn_Login.onClick.AddListener(OnClicked_Login);
        btn_Join.onClick.AddListener(OnClicked_Join);
        btn_Exit.onClick.AddListener(OnClicked_Exit);
        CSocketIoMgr.Inst.onAck_Login += OnAck_Login;
        CSocketIoMgr.Inst.onAck_CreateAccount += OnAck_Join;
    }

    void OnClicked_Login()
    {
        id = input_Id.text;
        pw = input_Pw.text;

        ClearInput();

        if (id == "" || pw == "")
        {
            Debug.Log("제대로 입력해주세요.");
            return;
        }

        CSocketIoMgr.Inst.SendReqLogin(id, pw);
    }

    void OnClicked_Join()
    {
        id = input_Id.text;
        pw = input_Pw.text;

        ClearInput();

        if (id == "" || pw == "")
        {
            Debug.Log("제대로 입력해주세요.");
            return;
        }

        CSocketIoMgr.Inst.SendReqCreateId(id, pw);
    }

    void OnClicked_Exit()
    {

    }

    void OnAck_Login(object obj, int result)
    {
        if(result == (int)CSocketIoMgr.ELoginResult.Success)
        {
            // 로그인 성공
            Debug.Log("로그인에 성공하였습니다.");

            // 룸 리스트
            CSocketIoMgr.Inst.SendReqInitRoomList();

            lobbyDlg.InitMyId();

            CSocketIoMgr.Inst.isLogin = true;

            gameObject.SetActive(false);
        }
    }

    void OnAck_Join(object obj, int result)
    {
        if (result == (int)CSocketIoMgr.ELoginResult.Success)
        {
            // 회원가입 성공

            Debug.Log("회원가입에 성공하였습니다.");
            CSocketIoMgr.Inst.SendReqLogin(input_Id.text, input_Pw.text);
        }
    }

    void ClearInput()
    {
        input_Id.text = "";
        input_Pw.text = "";
    }

}
