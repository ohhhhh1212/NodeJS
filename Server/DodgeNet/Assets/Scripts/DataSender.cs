using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class DataSender
{
    delegate void SendDele(string ip, int port, byte[] packet);

    public void SendMsgAsync(string ip, int port, byte[] packet)
    {
        SendDele dele = SendMsg;
        dele.BeginInvoke(ip, port, packet, null, null);
    }

    private void SendMsg(string ip, int port, byte[] packet)
    {
        try
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint iepoint = new IPEndPoint(IPAddress.Parse(ip), port);
            socket.Connect(iepoint);

            socket.Send(packet);
            socket.Close();
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }
}
