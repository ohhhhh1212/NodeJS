using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



namespace socketionet 
{
    public enum ERoomState
    {
        eReady = 0,     // 게임 시작 전 상태
        eGame = 1,      // 게임중 상태
    }
    public enum EUserState
    {
        eEmpty = 0,     // 빈슬롯인 경우
        eEnter = 1,     // 룸 입장상태
        eReady = 2,     // 게임 시작 대기 상태
    }

    [Serializable]
    public class SOUserInfo
    {
        public string id { get; set; } = "";             // id를 이름처럼 사용함
        public string socketId { get; set; } = "";       // socket id - 서버가 생성함.
        public string ip { get; set; } = "";            // ip
        public int dataPort { get; set; } = 0;          // data port ( Receiver에 사용 )
        public int movePort { get; set; } = 0;          // 이동 port - 유저 이동에 사용 (Receiver/Sender에 사용)

        public string Name() { return id; }
        public bool IsMine(string userId) { return id == userId; }
       
    }

    [Serializable]
    public class SOUserInfoList
    {
        public List<SOUserInfo> datas { get; set; } = new List<SOUserInfo>();
    }

    [Serializable]
    public class SOCreateId
    {
        public string id { get; set; } = string.Empty;  // 유저 id
        public int success { get; set; } = 0;           // 가입 성공 여부
        public string socketId { get; set; } = string.Empty; // 소켓 id
    }
    [Serializable]
    public class SOUserLogout
    {
        public string id { get; set; } = string.Empty;
    }

    /// <summary>
    /// Room에 들어온 유저 
    /// </summary>
    [Serializable]
    public class SOPlayer
    {
        public bool isMaster { get; set; } = false;        // 방장 여부
        public int userState { get; set; } = 0;            // 방에서 게임준비완료 되었는지 상태
        public int slotNo { get; set; } = 0;               // 룸의 슬롯 번호 , 1부터 시작한다. 
        public bool isAlive { get; set; } = true;          // 게임중 살아 있는지 여부
        public SOUserInfo userInfo { get; set; } = new SOUserInfo();
        public string Name() { return userInfo.id; }    // 닉네임 or 유저 ID
        public string IP() {  return userInfo.ip;  }
        public int DataPort() {  return userInfo.dataPort;  }
        public int MovePort() { return userInfo.movePort; }
    }

    [Serializable]
    public class SORoom
    {
        public string roomId { get; set; } = string.Empty;  // 방이름
        public string masterClientId { get; set; } = string.Empty;    // 마스터 클라이언트 id
        public byte maxPlayer { get; set; } = 4;                // 최대 플레이어 4명
        public bool isOpen { get; set; } = false;               // 방이 열려있으면 외부 공개됨 
        public int roomState { get; set; } = 0;                 // 0 : 대기중 , 1 : 게임중
        public bool removedFromList { get; set; } = false;      // 룸 리스트에서 삭제 되었는지 여부
        public List<SOPlayer> players { get; set; } = new List<SOPlayer>();        // 유저리스트
        public List<int> slots { get; set; } = new List<int>();     // 슬롯 리스트( maxPlayer 수만큼 생성된다.)

        public string Name() { return roomId; }
        public int PlayerCount() { return players.Count; }
        public SOPlayer GetPlayer(string sUserId)
        {
            return players.Find((e) => (e.Name() == sUserId) );
        }
        // 플레이어 추가
        public void AddPlayer(SOPlayer kPlayer){
            players.Add(kPlayer);
        }
        // 플레이어 삭제
        public void RemovePlayer(string sUserId){
            var kPlayer = GetPlayer(sUserId);
            if( kPlayer != null )
                players.Remove(kPlayer);
        }

        public SOPlayer GetMasterPlayer()
        {
            return GetPlayer(masterClientId);
        }
    }

    [Serializable]
    public class SORoomList
    {
        public List<SORoom> datas { get; set; } = new List<SORoom>();
    }

    [Serializable]
    public class SORoomPlayerList
    {
        public List<SOPlayer> datas { get; set; } = new List<SOPlayer>();
    }


    /// <summary>
    /// 룸에서 Ready버튼 클릭시 응답
    /// </summary>
    [Serializable]
    public class SORoomUserState
    {
        public string id { get; set; } = "";        // 유저 id or Name
        public int userState { get; set; } = 0;     // 유저 상태 EUserState.eReady ?
    }

    /// <summary>
    /// 룸에서 마스터 변경
    /// </summary>
    [Serializable]
    public class SORoomChangeMaster
    {
        public string masterId { get; set; } = "";     // 마스터 유저 
    }


    /// <summary>
    /// 채팅 하기
    /// </summary>
    [Serializable]
    public class SOChat
    {
        public string userId { get; set; } = "";        // 유저 id (or 이름)
        public string msg { get; set; } = "";           // 메세지
    }

    public class CSokcetIoInfo 
    {
   
    }



}
