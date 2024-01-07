using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Newtonsoft.Json;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Text;


/*
 *  Socket.io : https://github.com/itisnajim/SocketIOUnity.git
 *  
 *  [ Socket.io 관리자 ]
 *   - 서버와 연결 (send, ack )
 *   - 가입 (send, ack )
 *   - 로그인 ( send, ack )
 *   - 로그아웃 ( send )
 *   - KeepAlive( ping )
 *   - 유저정보( ID, IP, dataPort, backPort ) 
 *   - 연결 해제
 *   
 *  [Newtonsoft json 설치]
 *   - [windows]->[package manager]로 이동 후 [add pakaage by name...] 선택
 *   - com.unity.nuget.newtonsoft-json  입력
 *   
 */
namespace socketionet
{

    public partial class CSocketIoMgr : MonoBehaviour
    {
        public enum ELoginResult
        {
            Success = 0,    // 로그인 성공    
            NotID,          // 없는 id
            WrongPass,      // 패스워드 다름
            AlreadyLogin,   // 이미 로그인중
        }

        static CSocketIoMgr _inst = null;
        public static CSocketIoMgr Inst
        {
            get
            {
                if (_inst == null) {
                    GameObject go = new GameObject("Socket IO Manager");
                    _inst = go.AddComponent<CSocketIoMgr>();
                    DontDestroyOnLoad(go);
                }
                return _inst;
            }
        }

        //변수  ===========================================================================
        public LobbyScene m_LobbyScene = null;
        public bool isLogin = false;

        SocketIOUnity m_Socket = null;
        static SOUserInfo m_MyUserInfo = new SOUserInfo();

        public SOUserInfoList m_UserInfoList = new SOUserInfoList();
        public static SOUserInfo MyUserInfo { get => m_MyUserInfo; }
        public static SORoom MyRoom { get; private set; } = null;
        public static bool InRoom { get=> (MyRoom != null); }

        //로비
        public event EventHandler<int> onAck_CreateAccount = null;         // 계정생성 응답
        public event EventHandler<int> onAck_Login = null;                 // 로그인 응답
        public event EventHandler<string> onNotify_Logout = null;          // 다른 유저가 로그아웃 통지

        public event EventHandler<SOUserInfo> onNotify_UserInfo = null;          // 다른 유저가 로그인 할때 통지
        public event EventHandler<SOUserInfoList> onNotify_UserInfoList = null;  // 처음 로그인 후 모든 유저정보 받기
        public event EventHandler<SORoomList> onAck_InitRoomList = null;        // 룸리스트 응답
        public event EventHandler<SORoomList> onNotify_UpdateRoomList = null;   // 룸리스트 통지


        //룸( 그룹 )
        public event EventHandler<SORoom> onAck_CreateRoom = null;                  // 룸생성 응답
        public event EventHandler<SORoom> onAck_JoinRoom = null;                    // 룸조인 응답
        public event EventHandler<SORoomUserState> onNotify_RoomUserReady = null;          // 룸에서 준비상태값 응답
        public event EventHandler<SORoomChangeMaster> onNotify_RoomChangeMaster = null;    // 방장 변경

        public event EventHandler<SOPlayer> onNotify_EnterRoom = null;          // 룸입장 응답
        public event EventHandler<string> onNotify_LeaveRoom = null;            // 룸나가기 응답

        public event EventHandler<bool> onNotify_GameStart = null;            // 게임 시작
        public event EventHandler<bool> onNotify_GameEnd = null;              // 게임 종료

        public event EventHandler<SOChat> onAck_ChatMsg = null;
        public event EventHandler<SOChat> onNotify_ChatMsg = null;


        public static bool IsMasterClient {
            get {
                if( MyUserInfo == null || MyRoom == null) return false;
                return (MyUserInfo.id == MyRoom.masterClientId);
            }
        }

        public static bool IsMine(SOPlayer kPlayer){
            return (kPlayer.Name() == MyUserInfo.Name());
        }
        public static bool IsMine(string sUserName){
            return (MyUserInfo.Name() == sUserName);
        }

        public static bool IsMasterPlayer (SOPlayer kPlayer)
        {
            return (kPlayer.Name() == MyRoom.masterClientId) ;
        }
        public static string NickName()
        {
            return MyUserInfo.Name();
        }


        public string ipStr { get; set; } = "192.168.0.195";
        public int dataPort { get; set; } = 15001;
        public int movePort { get; set; } = 16001;
        //public int backPort { get; set; } = 17001;

        public bool IsConnected { get; private set; } = false;



        /// <summary>
        ///  socket.On => data 처리만 할때 ( scrollveiw 또는 transform 사용시 동작 안함. )
        ///  socket.OnUnityThread => Unity Resource 처리할때 사용
        ///  
        /// 소켓 io 초기화
        ///  - 처음 시작시 반드시 외부에서 한번은 호출해야 됨
        /// </summary>
        public async void InitSocketIO(string sIP, int nPort )
        {
            //var uri = new Uri("http://192.168.0.195:19001");

            string sUri = $"http://{sIP}:{nPort}";
            var uri = new Uri(sUri);
            var socket = new SocketIOUnity(uri, new SocketIOOptions
            {
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

            IsConnected = socket.Connected;
            Debug.Log($"[ScocktIO] socket.Connected = " + IsConnected);

            #region  // 서버기본 연결관련
            // 서버 연결 기본 응답 ---------------------------------------
            socket.OnConnected += (sender, e) => {
                Debug.Log($"[ScocktIO] socket.OnConnected...2");
            };

            // 서버에서 설정한 (pingInterval) 15초 마다 호출
            socket.OnPing += (sender, e) =>{
                IsConnected = true;
                //Debug.Log("Ping");
                //Debug.Log($"Ping : {Time.time}" );  // 동작안함 ???...
            };
            // 서버에서 설정한 (pingInterval) 15초 마다 호출
            socket.OnPong += (sender, e) =>
            {
                //Debug.Log("Pong: " + e.TotalMilliseconds);
            };
            socket.OnDisconnected += (sender, e) =>{
                IsConnected = false;
                Debug.Log("disconnect: " + e);
            };
            #endregion
            
            // 로그인 Ack -------------------------------------------
            socket.OnUnityThread("ack_login", (res) =>
            {
                IsConnected = true;
                Debug.Log("[SocketIO] login -> " + res + $"  {Time.time} ");

                var jsonArr = JArray.Parse(res.ToString());
                var json = jsonArr[0];

                var sID = (string)json["id"]; // res.GetValue<string>(0);
                var nSuccess = (int)json["success"];


                if (nSuccess == (int)ELoginResult.Success) // 로그인 성공
                {
                    //Init_Timer();
                    Debug.LogFormat("로그인 성공 !!!, ( id={0} )", sID);
                    SOUserInfo kInfo = new SOUserInfo();

                    kInfo.id = sID;
                    kInfo.ip = ipStr;
                    kInfo.dataPort = dataPort;  // 룸에서 변경된 포트번호를 받아야 한다.
                    //kInfo.backPort = backPort;
                    kInfo.movePort = movePort;

                    m_MyUserInfo = kInfo;

                    string data = JsonConvert.SerializeObject(kInfo);
                    socket.Emit("req_user_info", data);
                }
                else // 로그인 실패 처리
                {
                    Debug.Log($"로그인 실패!, 코드 : {nSuccess}");
                }

                if (onAck_Login != null)
                    onAck_Login(this, nSuccess);

            });

            // 계정생성 Ack ------------------------------------------------
            socket.OnUnityThread("ack_create_id", (res) =>
            {
                Debug.Log("[SocketIO] create_id -> " + res);
                //var jsonArr = JArray.Parse(res.ToString());
                //var json = jsonArr[0];

                SOCreateId packet = res.GetValue<SOCreateId>();

            //Debug.LogFormat("[SocketIO] id = {0}, success={1}", packet.id, packet.success);
            if (packet.success == 0)
                {
                    SetMyUserInfo(packet.id, packet.socketId);
                    Debug.Log($"[SocketIO] 게정 생성 성공!!, id : {packet.id}");
                }

                if (onAck_CreateAccount != null)
                    onAck_CreateAccount(this, packet.success);

            });

            // 로그인된 유저정보 받기 ---------------------------------
            socket.On("notify_user_info", (res) =>
            {
                UnityThread.executeInUpdate(() =>    // UI, transform 사용가능함.
                {
                    SOUserInfo user = res.GetValue<SOUserInfo>();
                    m_UserInfoList.datas.Add(user);
                    Debug.Log(res);

                    if (onNotify_UserInfo != null)
                        onNotify_UserInfo(this, user);
                });
            });

            // 로그인된 유저정보 받기 ---------------------------------
            socket.OnUnityThread("ack_user_info", (res) =>
            {
                Debug.Log(res);
                m_UserInfoList.datas.Clear();
                m_UserInfoList = res.GetValue<SOUserInfoList>();

                if (m_UserInfoList != null)
                {
                    Debug.Log("[SocketIO] user count = " + m_UserInfoList.datas.Count);

                    var kUserInfo = m_UserInfoList.datas.Find((info) => (info.id == m_MyUserInfo.id));

                    if (kUserInfo != null)
                        m_MyUserInfo = kUserInfo;
                }

                if (onNotify_UserInfoList != null)
                    onNotify_UserInfoList(this, m_UserInfoList);
            });

            // 다른유저가 로그아웃 되었을때 처리 ------------------------
            socket.OnUnityThread("notify_logout", (res) =>
            {
                SOUserLogout kLogout = res.GetValue<SOUserLogout>();

                // 유저목록에서 삭제하기
                if (kLogout != null)
                {
                    SOUserInfo kUserInfo = m_UserInfoList.datas.Find((info) => (info.id == kLogout.id));

                    string sUserId = "";
                    if (kUserInfo != null) {
                        m_UserInfoList.datas.Remove(kUserInfo);
                        sUserId = kLogout.id;
                    }
                    else // 없는 id 일때 empty를 보낸다.
                        sUserId = "";

                    if (onNotify_Logout != null)
                        onNotify_Logout(this, sUserId);
                }
            });

            // 룸리스트 받기 --------------------------------------------
            socket.OnUnityThread("ack_init_roomlist", (res) =>
            {
                SORoomList kRoomList = res.GetValue<SORoomList>();
                Debug.LogFormat(" room count = {0}", kRoomList.datas.Count);

                if (onAck_InitRoomList != null)
                    onAck_InitRoomList(this, kRoomList);
            });
            
            // 업데이트 된 룸리스트 받기 --------------------------------------------
            socket.OnUnityThread("notify_update_roomlist", (res) =>
            {
                //Debug.LogFormat("called update_roomlist : {0}", res);
                SORoomList kRoomList = res.GetValue<SORoomList>();
                Debug.LogFormat(" room count = {0}", kRoomList.datas.Count);

                if (onNotify_UpdateRoomList != null)
                    onNotify_UpdateRoomList(this, kRoomList);
            });

            //내가 룸을 생성 했을때  ------------------------
            socket.OnUnityThread("ack_create_room", (res) =>
            {
                var jsonArr = JArray.Parse(res.ToString());
                var json = jsonArr[0];

                var nSuccess = (int)json["success"];

                Debug.Log("Ack_CreateRoom : " + json["room"] );

                SORoom kRoom = null;
                if (nSuccess == 0)
                {
                    kRoom = JsonConvert.DeserializeObject<SORoom>(json["room"].ToString());

                    MyRoom = kRoom;
                    ResetMyPort(kRoom.GetPlayer(MyUserInfo.id));

                }
                if (onAck_CreateRoom != null)
                    onAck_CreateRoom(nSuccess, kRoom);
            });

            // 내가 룸에 조인 했을때  ------------------------
            socket.OnUnityThread("ack_join_room", (res) =>
            {
                var jsonArr = JArray.Parse(res.ToString());
                var data = jsonArr[0];
                var nSuccess = (int)data["success"];

                Debug.Log("Ack_JoinRoom : " + data["room"]);

                SORoom kRoom = JsonConvert.DeserializeObject<SORoom>(data["room"].ToString());
                if (nSuccess == 0)
                {
                    MyRoom = kRoom;
                    ResetMyPort(kRoom.GetPlayer(MyUserInfo.id));
                }
                else
                {
                    Debug.Log("룸 입장 실패!!!");
                }

                if (onAck_JoinRoom != null)
                    onAck_JoinRoom(nSuccess, kRoom);

            });

            // 다른 유저가 룸에 입장 했을때
            socket.OnUnityThread("notify_enter_room", (res) =>
            {
                SOPlayer kPlayer = res.GetValue<SOPlayer>();
                MyRoom.AddPlayer(kPlayer);

                if( onNotify_EnterRoom != null)
                    onNotify_EnterRoom(this, kPlayer);
            });

            // 유저가 룸에서 퇴장 했을때
            socket.OnUnityThread("notify_leave_room", (res) =>
            {
                var jsonArr = JArray.Parse(res.ToString());
                var data = jsonArr[0];
                string sUserId = (string)data["id"];

                if(!MyUserInfo.IsMine( sUserId)) {
                    MyRoom.RemovePlayer(sUserId);
                }

                if(onNotify_LeaveRoom != null)
                    onNotify_LeaveRoom(this, sUserId);
            });

            // 유저가 룸에서 Ready버튼 클릭시
            socket.OnUnityThread("notify_room_ready", (res) =>
            {
                SORoomUserState kState = res.GetValue<SORoomUserState>();

                if (onNotify_RoomUserReady != null)
                    onNotify_RoomUserReady(this, kState);
            });
            // 방장(마스터) 변경    
            socket.OnUnityThread("notify_room_change_master", (res) =>
            {
                SORoomChangeMaster kMaster = res.GetValue<SORoomChangeMaster>();
                MyRoom.masterClientId = kMaster.masterId;
                for (int i = 0; i < MyRoom.players.Count; i++)
                {
                    SOPlayer kPlayer = MyRoom.players[i];
                    kPlayer.isMaster = (kPlayer.Name() == kMaster.masterId);
                }

                if (onNotify_RoomChangeMaster != null)
                    onNotify_RoomChangeMaster(this, kMaster);

            });

            // 방장 게임 시작버튼 클릭했을때
            socket.OnUnityThread("notify_game_start", (res) =>
            {
                if (onNotify_GameStart != null)
                    onNotify_GameStart(this, true);
            });

            // 방장 게임이 완전히 종료됐때
            socket.OnUnityThread("notify_game_end", (res) =>
            {
                if (onNotify_GameEnd != null)
                    onNotify_GameEnd(this, true);
            });
            // 룸 채팅 응답
            socket.OnUnityThread("ack_room_chat", (res) =>
            {
                SOChat kChat = res.GetValue<SOChat>();

                if (onAck_ChatMsg != null)
                    onAck_ChatMsg(this, kChat);
            });
            // 룽 채팅 통지
            socket.OnUnityThread("notify_room_chat", (res) =>
            {
                SOChat kChat = res.GetValue<SOChat>();

                if (onNotify_ChatMsg != null)
                    onNotify_ChatMsg(this, kChat);
            });
        }

        // 내 Port를 재설정하기
        private void ResetMyPort(SOPlayer kPlayer)
        {
            m_MyUserInfo.dataPort = kPlayer.DataPort();
            //m_MyUserInfo.backPort = kPlayer.BackPort();
            m_MyUserInfo.movePort = kPlayer.MovePort();
        }


        // 내 정보 셋팅하기
        public void SetMyUserInfo(string id, string socketId)
        {
            m_MyUserInfo.id = id;
            m_MyUserInfo.ip = ipStr;
            m_MyUserInfo.dataPort = dataPort;
            m_MyUserInfo.socketId = socketId;
        }

        #region  // 타이머 사용하기
        //// 핑 체크를 위한 타이머 ( socket.io 서버에서 보내주므로 필요없음 )
        //public void Init_Timer()
        //{
        //    m_Timer = new Timer(CheckKeepAlive);
        //    m_Timer.Change(0, 10000);
        //}

        //// 핑 체크 콜백함수
        //private async void CheckKeepAlive(object state)
        //{
        //    if (m_Socket != null)
        //    {
        //        var time = DateTime.Now;
        //        await m_Socket.EmitAsync("ping", time);
        //    }
        //}
        #endregion
        // Send 함수 ==========================================================

        // 로그인 요청
        public async void SendReqLogin(string id, string pass)
        {
            await m_Socket?.EmitAsync("req_login", id, pass);
        }

        // 계정생성 요청
        public async void SendReqCreateId(string id, string pass)
        {
            await m_Socket?.EmitAsync("req_create_id", id, pass);
        }

        // 로그아웃 요청
        public async void SendReqLogout(string id)
        {
            await m_Socket?.EmitAsync("req_logout", id);
        }

        // 탈퇴 요청
        public async void SendReqWithdraw(string id)
        {
            await m_Socket?.EmitAsync("req_withdraw", id);
        }

        // 유저정보 요청
        public async void SendReqUserInfo(string id, string ip, int dataPort, int movePort)
        {
            await m_Socket?.EmitAsync("req_user_info", id, ip, dataPort, movePort);
        }

        public void SendReqDisconnect()
        {
            IsConnected = false;
            m_Socket?.Emit("disconnect");
        }

        public void SendReqInitRoomList()
        {
            m_Socket?.Emit("req_init_roomlist", m_MyUserInfo.id);
        }


        //룸관련 정보 ======================================================
        // 룸 생성 요청 
        public async void SendReqCreateRoom(string roomName) //, string userId)
        {
            await m_Socket?.EmitAsync("req_create_room", roomName, MyUserInfo.id);
        }

        // 룸에 조인하기 요청
        public async void SendReqJoinRoom(string roomName)
        {
            await m_Socket?.EmitAsync("req_join_room", roomName, MyUserInfo.id);
        }

        // 게임 준비완료 요청
        public async void SendReqRoomReady(string userId, int userState)
        {
            await m_Socket?.EmitAsync("req_room_ready", MyRoom.Name(), userId, userState);
        }
        public async void SendReqLeaveRoom(string userId)
        {
            await m_Socket?.EmitAsync("req_leave_room", MyRoom.Name(), userId);
            MyRoom = null;
        }

        public async void SendReqGameStart(string userId)
        {
            await m_Socket?.EmitAsync("req_game_start", MyRoom.Name(), userId);
        }

        public async void SendReqGameEnd(string userId)
        {
            await m_Socket?.EmitAsync("req_game_end", MyRoom.Name(), userId);
        }

        public async void SendReqRoomChat(string msg)
        {
            await m_Socket?.EmitAsync("req_room_chat", MyRoom.Name(), MyUserInfo.Name(), msg);
        }



        //-------------------------------------------------------------------------------------------
        // 소켓 파괴
        public void DestroySocket()
        {
            if (m_Socket != null)
                m_Socket.Dispose();
        }

        private void OnApplicationQuit()
        {
            DestroySocket();
        }

        // UserInfo 이름으로 얻기
        public SOUserInfo GetUserInfo(string name)
        {
            var kUserInfo = m_UserInfoList.datas.Find((user) =>
            {
                return (user.id == name);
            });

            return kUserInfo;
        }

        // 유니티 쓰레드 사용
        // 사용법 : ExecuteInUnityThread(()=>{ Received_Data(); });
        public static void ExecuteInUnityThread(Action action)
        {
            UnityThread.executeInUpdate(action);
        }


    }
}