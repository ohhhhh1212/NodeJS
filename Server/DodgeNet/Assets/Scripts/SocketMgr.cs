using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

public class SocketMgr : MonoBehaviour
{
    static SocketMgr _inst = null;
    public  static SocketMgr Inst
    {
        get
        {
            if(_inst == null)
            {
                GameObject go = new GameObject("Socket io manager");
                _inst = go.AddComponent<SocketMgr>();
                DontDestroyOnLoad(go);
            }

            return _inst;
        }
    }

    public class UserJoin
    {
        public string id { get; set; } = string.Empty; // 유저 id
        public int success { get; set; } = 0; // 가입 성공 여부
        public string socketId { get; set; } = string.Empty; // 소켓 id
    }
    public class UserLogin
    {
        public string id { get; set; } = string.Empty; // 유저 id
        public int success { get; set; } = 0; // 가입 성공 여부
    }
    public class UserInfo
    {
        public string id { get; set; } = string.Empty; // 유저 id
        public string socketId { get; set; } = string.Empty; // 소켓 id
        public string ip { get; set; } = string.Empty; // 유저 ip
        public int dataPort { get; set; } = 0;
        public int backPort { get; set; } = 0;
    }
    public class UserInfoList
    {
        public List<UserInfo> datas { get; set; } = new List<UserInfo>();
    }


    public event EventHandler<int> onAck_Join = null;
    public event EventHandler<int> onAck_Login = null;
    public event EventHandler<UserInfo> onAck_UserInfo = null;
    public event EventHandler<UserInfoList> onAck_UserInfoList = null;
    public event EventHandler<string> onAck_Logout = null;


    public UserInfo m_MyUserInfo = new UserInfo();
    public UserInfoList m_UserInfoList = new UserInfoList();
    SocketIOUnity m_Socket = null;

    public string ipStr { get; set; } = "192.168.0.72";
    public int dataPort { get; set; } = 15001;
    public int backPort { get; set; } = 16001;
    public bool isLogin { get; set; } = false;

    public async void InitSocketIO()
    {
        var uri = new Uri("http://192.168.0.72:19000");
        var socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            // 접속 보안 토큰 
            Query = new Dictionary<string, string>
            {
                {"token", "UNITY" }
            },
            EIO = 4,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        socket.JsonSerializer = new NewtonsoftJsonSerializer();
        m_Socket = socket;

        await socket.ConnectAsync();

        socket.OnUnityThread("login", (res) =>
        {
            Debug.Log("[SocketIO] login -> " + res + $" {Time.time}");

            var jsonArr = JArray.Parse(res.ToString());
            var json = jsonArr[0];

            var sId = (string)json["id"];
            var nSuccess = (int)json["success"];

            // 위와 똑같이 파싱
            //UserLogin packet = res.GetValue<UserLogin>();

            if (nSuccess == 0)
            {
                Debug.Log("로그인 성공!!");

                UserInfo info = new UserInfo();

                info.id = sId;
                info.ip = ipStr;
                info.dataPort = dataPort;
                info.backPort = backPort;

                m_MyUserInfo = info;

                string data = JsonConvert.SerializeObject(info);
                socket.Emit("user_info", data);
            }
            else
                Debug.Log("아이디 또는 비밀번호가 잘못 되었습니다.");

            if (onAck_Login != null)
                onAck_Login(this, nSuccess);
        });

        socket.OnUnityThread("join", (res) =>
        {
            Debug.Log("[SocketIO] join -> " + res);

            UserJoin packet = res.GetValue<UserJoin>();
            if (packet.success == 0)
            {
                Debug.Log("가입 성공!!");
            }
            else
                Debug.Log("가입 실패");

            if (onAck_Join != null)
                onAck_Join(this, packet.success);
        });

        socket.OnUnityThread("user_info", (res) =>
        {
            UserInfo newUser = res.GetValue<UserInfo>();

            m_UserInfoList.datas.Add(newUser);

            if (onAck_UserInfo != null)
                onAck_UserInfo(this, newUser);
        });

        socket.OnUnityThread("user_info_list", (res) =>
        {
            UserInfoList infolist = res.GetValue<UserInfoList>();

            m_UserInfoList.datas.Clear();
            m_UserInfoList = infolist;

            if (onAck_UserInfoList != null)
                onAck_UserInfoList(this, m_UserInfoList);
        });

        socket.OnUnityThread("logout", (res) =>
        {
            var jsonArr = JArray.Parse(res.ToString());
            var json = jsonArr[0];

            var id = (string)json["id"];

            for (int i = 0; i < m_UserInfoList.datas.Count; i++)
            {
                if (m_UserInfoList.datas[i].id == id)
                {
                    m_UserInfoList.datas.RemoveAt(i);
                    break;
                }
            }

            if (onAck_Logout != null)
                onAck_Logout(this, id);
        });
    }

    public async void SendJoin(string id, string pass)
    {
        await m_Socket.EmitAsync("join", id, pass);
    }

    public async void SendLogin(string id, string pass)
    {
        await m_Socket.EmitAsync("login", id, pass);
    }

    public async void SendLogout(string id)
    {
        await m_Socket.EmitAsync("logout", id);
    }

    public async void SendWithdraw(string id)
    {
        await m_Socket.EmitAsync("withdraw", id);
    }
}
