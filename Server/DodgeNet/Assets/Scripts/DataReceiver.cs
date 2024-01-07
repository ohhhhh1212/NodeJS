using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class DataReceiver
{
    [Serializable]
    public class PacketHeader
    {
        public short packetId { get; set; }
        public short size { get; set; }
        public PacketHeader() { }
        public PacketHeader(short nPacketId, short nSize)
        {
            packetId = nPacketId;
            size = nSize;
        }

        public static int GetHeaderSize() { return 4; }

        public void ReadHeaderData(BinaryReader br)
        {
            packetId = br.ReadInt16();
            size = br.ReadInt16();
        }

        public void WriteString(BinaryWriter bw, string str)
        {
            byte[] buf = Encoding.UTF8.GetBytes(str);
            short nSize = (short)buf.Length;
            bw.Write(nSize);
            bw.Write(buf);
        }

        public string ReadString(BinaryReader br)
        {
            short nSize = br.ReadInt16();
            byte[] buf = br.ReadBytes(nSize);
            return Encoding.UTF8.GetString(buf);
        }

        public int GetBytesSendSize(string str)
        {
            byte[] buf = Encoding.UTF8.GetBytes(str);
            return buf.Length + sizeof(short);
        }
    }
    [Serializable]
    public class PacketData : PacketHeader
    {
        public byte[] packet { get; set; }
        public PacketData(short nPacketId, short nSize, byte[] kData)
        {
            packetId = nPacketId;
            size = nSize;
            packet = kData;
        }
        public PacketData() { }
    }

    public Action<Socket> OnAcceptEvent = null;
    public Action<Socket> OnCloseEvent = null;

    public event EventHandler<PacketData> OnReceiveEvent = null;
    public const int HeaderSize = 4;

    public string ipStr { get; private set; }
    public int port { get; private set; }

    Socket m_Socket = null;
    public bool isActive { get; set; } = true;
    public DataReceiver(string sIp, int nPort)
    {
        ipStr = sIp;
        port = nPort;
    }

    public bool Start()
    {
        try
        {
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint iepoint = new IPEndPoint(IPAddress.Parse(ipStr), port);
            m_Socket.Bind(iepoint);
            m_Socket.Listen(5);

            AcceptLoopAsync();
            Debug.Log("서버 열림");
            return true;
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            return false;
        }
        finally
        {
            if (OnCloseEvent != null)
                OnCloseEvent(m_Socket);
        }
    }

    public void CloseSocket()
    {
        m_Socket?.Close();
    }

    delegate void AcceptDele();

    private void AcceptLoopAsync()
    {
        AcceptDele dele = AssetLoop;
        dele.BeginInvoke(null, null);
    }
    
    private void AssetLoop()
    {
        Socket doSocket = null;
        while (true)
        {
            doSocket = m_Socket.Accept();
            if (OnAcceptEvent != null)
                OnAcceptEvent(doSocket);

            DoItAsync(doSocket);
        }
    }

    delegate void DoItDele(Socket doSocket);
    
    private void DoItAsync(Socket doSocket)
    {
        DoItDele dele = DoIt;
        dele.BeginInvoke(doSocket, null, null);
    }

    void DoIt(Socket doSocket)
    {
        //IPEndPoint remote = doSocket.RemoteEndPoint as IPEndPoint;
        byte[] packet = new byte[1024];

        doSocket.Receive(packet);
        doSocket.Close();

        MemoryStream ms = new MemoryStream(packet);
        BinaryReader br = new BinaryReader(ms);

        short nPacketId = br.ReadInt16();
        short nSize = br.ReadInt16();

        //int bodySize = nSize - HeaderSize;
        //byte[] body = br.ReadBytes(bodySize);

        br.Close();
        ms.Close();

        Debug.Log("test - packetID = " + nPacketId);

        if (OnReceiveEvent != null)
        {
            OnReceiveEvent(this, new PacketData(nPacketId, nSize, packet));
        }
    }
}
