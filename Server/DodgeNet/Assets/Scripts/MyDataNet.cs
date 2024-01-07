using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class ChatData : DataReceiver.PacketHeader
{
    public string userName { get; set; } = "";
    public string msg { get; set; } = "";

    public int GetPacketSize()
    {
        int nSize = sizeof(short) * 2;
        nSize += GetBytesSendSize(userName);
        nSize += GetBytesSendSize(msg);
        return nSize;
    }

    public ChatData(string sUserName, string sMsg)
    {
        userName = sUserName;
        msg = sMsg;
        packetId = (short)MyDataNet.EPacket.Chat;
        size = (short)GetPacketSize();
    }

    public ChatData()
    {
        packetId = (short)MyDataNet.EPacket.Chat;
    }

    public byte[] SendMessage()
    {
        byte[] packet = new byte[1024];

        this.size = (short)GetPacketSize();

        MemoryStream ms = new MemoryStream(packet);
        BinaryWriter bw = new BinaryWriter(ms);

        bw.Write(packetId);
        bw.Write(size);

        WriteString(bw, userName);
        WriteString(bw, msg);

        ms.Close();
        bw.Close();

        return packet;
    }

    public void OnReceived_Data(byte[] data)
    {
        MemoryStream ms = new MemoryStream(data);
        BinaryReader br = new BinaryReader(ms);

        packetId = br.ReadInt16();
        size = br.ReadInt16();

        userName = ReadString(br);
        msg = ReadString(br);

        ms.Close();
        br.Close();
    }
}

public class MyDataNet : MonoBehaviour
{
    public enum EPacket
    {
        Chat = 1001,
    }

    static MyDataNet _inst = null;

    public static MyDataNet Inst
    {
        get
        {
            if (_inst == null)
            {
                GameObject go = new GameObject("MyDataNet");
                _inst = go.AddComponent<MyDataNet>();
                DontDestroyOnLoad(go);
            }

            return _inst;
        }
    }

    DataSender m_DataSender = new DataSender();
    DataReceiver m_DataReceiver = null;
    Coroutine m_Coroutine = null;

    Queue<DataReceiver.PacketData> m_EventQueue = new Queue<DataReceiver.PacketData>();

    //public MultiChatScene m_ChatScene { get; set; } = null;
    public bool IsAction { get; set; } = true;

    public void StartReceiver()
    {
        string ip = SocketMgr.Inst.m_MyUserInfo.ip;
        int port = SocketMgr.Inst.m_MyUserInfo.dataPort;

        m_DataReceiver = new DataReceiver(ip, port);

        m_DataReceiver.OnReceiveEvent += GetPacket;
        m_DataReceiver.Start();
        m_Coroutine = StartCoroutine(Co_Recieve());
    }

    public void CloseServer()
    {
        if (m_DataReceiver != null)
        {
            m_DataReceiver.OnReceiveEvent -= GetPacket;
            m_DataReceiver.CloseSocket();
        }

        IsAction = false;

        if (m_Coroutine != null)
            StopCoroutine(m_Coroutine);
    }

    void GetPacket(object sender, DataReceiver.PacketData data)
    {
        m_EventQueue.Enqueue(data);
    }

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
        if (kData.packetId == (int)EPacket.Chat)
        {
            ChatData data = new ChatData();
            data.OnReceived_Data(kData.packet);

            // 메시지 생성
            string text = $"{data.userName} : {data.msg}";
            //m_ChatScene.MainDlg.CreateTextItem(text);
        }
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
