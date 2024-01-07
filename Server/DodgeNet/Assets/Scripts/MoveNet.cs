using socketionet;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

//public class ChatData : DataReceiver.PacketHeader
//{
//    public string userName { get; set; } = "";
//    public string msg { get; set; } = "";

//    public int GetPacketSize()
//    {
//        int nSize = sizeof(short) * 2;
//        nSize += GetBytesSendSize(userName);
//        nSize += GetBytesSendSize(msg);
//        return nSize;
//    }

//    public ChatData(string sUserName, string sMsg)
//    {
//        userName = sUserName;
//        msg = sMsg;
//        packetId = (short)MyDataNet.EPacket.Chat;
//        size = (short)GetPacketSize();
//    }

//    public ChatData()
//    {
//        packetId = (short)MyDataNet.EPacket.Chat;
//    }

//    public byte[] SendMessage()
//    {
//        byte[] packet = new byte[1024];

//        this.size = (short)GetPacketSize();

//        MemoryStream ms = new MemoryStream(packet);
//        BinaryWriter bw = new BinaryWriter(ms);

//        bw.Write(packetId);
//        bw.Write(size);

//        WriteString(bw, userName);
//        WriteString(bw, msg);

//        ms.Close();
//        bw.Close();

//        return packet;
//    }

//    public void OnReceived_Data(byte[] data)
//    {
//        MemoryStream ms = new MemoryStream(data);
//        BinaryReader br = new BinaryReader(ms);

//        packetId = br.ReadInt16();
//        size = br.ReadInt16();

//        userName = ReadString(br);
//        msg = ReadString(br);

//        ms.Close();
//        br.Close();
//    }
//}

public class MoveNet : MonoBehaviour
{
    public enum EPacket
    {
        Chat = 1001,
    }

    static MoveNet _inst = null;

    public static MoveNet Inst
    {
        get
        {
            if (_inst == null)
            {
                GameObject go = new GameObject("MoveNet");
                _inst = go.AddComponent<MoveNet>();
                DontDestroyOnLoad(go);
            }

            return _inst;
        }
    }

    DataSender m_DataSender = new DataSender();
    DataReceiver m_DataReceiver = null;
    Coroutine m_Coroutine = null;

    Queue<DataReceiver.PacketData> m_EventQueue = new Queue<DataReceiver.PacketData>();

    public GameScene m_GameScene { get; set; } = null;
    public bool IsAction { get; set; } = true;

    public void StartReceiver()
    {
        string ip = SocketMgr.Inst.m_MyUserInfo.ip;
        int port = SocketMgr.Inst.m_MyUserInfo.dataPort;

        m_DataReceiver = new DataReceiver(ip, port);

        m_DataReceiver.OnReceiveEvent += OnReCeiveEvent;
        m_DataReceiver.Start();
        //m_Coroutine = StartCoroutine(Co_Recieve());
    }

    void OnReCeiveEvent(object sender, DataReceiver.PacketData e)
    {
        Debug.Log(e.packetId);
        UnityThread.executeInFixedUpdate(() => OnReceived_Data(e));
    }

    public void CloseServer()
    {
        if (m_DataReceiver != null)
        {
            m_DataReceiver.OnReceiveEvent -= OnReCeiveEvent;
            //m_DataReceiver.OnReceiveEvent -= StartReceiver;
            m_DataReceiver.CloseSocket();
        }

        IsAction = false;

        if (m_Coroutine != null)
            StopCoroutine(m_Coroutine);
    }

    //void GetPacket(object sender, DataReceiver.PacketData data)
    //{
    //    m_EventQueue.Enqueue(data);
    //}

    IEnumerator Co_Recieve()
    {
        while (IsAction)
        {
            yield return new WaitUntil(() => m_EventQueue.Count > 0);

            DataReceiver.PacketData packet = m_EventQueue.Dequeue();
            OnReceived_Data(packet);
        }
    }

    void OnReceived_Data(DataReceiver.PacketData kData)
    {
        if (kData.packetId == (int)SPlayerMove.EPacket.Move)
        {
            OnAck_MoveData(kData);
        }
    }

    void OnAck_MoveData(DataReceiver.PacketData e)
    {
        SPlayerMove kData = new SPlayerMove();
        kData.ReceiveData(e);
        Vector3 vPos = kData.MoveVector();
        // 플레이어 움직이는 함수 호출
        m_GameScene.m_GameUI.OnAck_PlayerMove(kData.userId, vPos);
    }

    public void SendPlayerMove(string sIp, int nPort, string sUserId, Vector3 vPos)
    {
        SPlayerMove kData = new SPlayerMove(sUserId, vPos);
        byte[] packet = kData.SendData();
        m_DataSender.SendMsgAsync(sIp, nPort, packet);
    }

    //public void SendMessage(string ip, int port, string name, string msg)
    //{
    //    byte[] packet = new byte[1024];

    //    short packetId = (short)EPacket.Chat;
    //    short size = (short)(DataReceiver2.HeaderSize + name.Length * 3 + msg.Length * 3);

    //    MemoryStream ms = new MemoryStream(packet);
    //    BinaryWriter bw = new BinaryWriter(ms);

    //    bw.Write(packetId);
    //    bw.Write(size);

    //    bw.Write(name);
    //    bw.Write(msg);

    //    ms.Close();
    //    bw.Close();

    //    m_ChatSender.SendMsgAsync(ip, port, packet);
    //}

    public void SendBrodcastPlayerMove(Vector3 vPos)
    {
        var listPlayer = CSocketIoMgr.MyRoom.players;
        var kMyUser = CSocketIoMgr.MyUserInfo;
        for (int i = 0; i < listPlayer.Count; i++)
        {
            var kPlayer = listPlayer[i];
            if(kPlayer.Name() != kMyUser.Name())
            {
                SendPlayerMove(kPlayer.IP(), kPlayer.MovePort(), kMyUser.id, vPos);
            }
        }
    }

    public void SendBroadcastChatMsg(string msg)
    {
        var list = SocketMgr.Inst.m_UserInfoList.datas;
        var myUser = SocketMgr.Inst.m_MyUserInfo;
        ChatData data = new ChatData(myUser.id, msg);
        byte[] packet = data.SendMessage();
        for (int i = 0; i < list.Count; i++)
        {
            var user = list[i];
            if (user.id != myUser.id)
            {
                Debug.LogFormat(" {0}:{1} - {2}", user.ip, user.dataPort, user.id);
                m_DataSender.SendMsgAsync(user.ip, user.dataPort, packet);
            }
        }
    }

    public bool IsSendMove { get; set; } = false;

    public async void StartSendMoveLoop()
    {
        IsSendMove = true;
        Debug.Log("Send Move Start");
        Player myPlayer = m_GameScene.m_GameUI.m_MyPlayer;

        while (IsSendMove)
        {
            if (myPlayer != null)
            {
                SendBrodcastPlayerMove(myPlayer.transform.position);
                Debug.Log("send");
            }

            await Task.Delay(100);
        }

        Debug.Log("Send Move End");
    }

    void SendChatMsg(string sIp, int nPort, string sName, string sMsg)
    {
        ChatData data = new ChatData(sName, sMsg);
        byte[] packet = data.SendMessage();
        m_DataSender.SendMsgAsync(sIp, nPort, packet);
    }

    private void OnDestroy()
    {
        CloseServer();
    }
}
