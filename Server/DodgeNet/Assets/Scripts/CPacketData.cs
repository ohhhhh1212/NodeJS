using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CPacketData : MonoBehaviour
{



}

public class SPlayerMove : DataReceiver.PacketHeader
{
    const int DHEADER_SIZE = 4;

    public enum EPacket : short
    {
        Move = 1101,
        TurretFire = 1102,
    }

    public string userId = "";
    public float x = 0;
    public float y = 0;
    public float z = 0;
    public SPlayerMove(string sUserId, Vector3 vPos) : base((short)EPacket.Move, 0)
    {
        userId = sUserId;
        x = vPos.x;
        y = vPos.y;
        z = vPos.z;
        size = (short)GetPacketSize();
    }

    public SPlayerMove() : base((short)EPacket.Move, 0) { }

    public short GetPacketSize()
    {
        int idSize = GetBytesSendSize(userId);
        return (short)(DHEADER_SIZE + idSize + sizeof(float) * 3);
    }

    public byte[] SendData()
    {
        byte[] packet = new byte[1024];

        size = (short)GetPacketSize();

        MemoryStream ms = new MemoryStream(packet);
        BinaryWriter bw = new BinaryWriter(ms);

        bw.Write(packetId);
        bw.Write(size);
        WriteString(bw, userId);

        bw.Write(x);
        bw.Write(y);
        bw.Write(z);

        bw.Close();
        ms.Close();

        return packet;
    }

    public void ReceiveData(DataReceiver.PacketData kData)
    {
        MemoryStream ms = new MemoryStream(kData.packet);
        BinaryReader br = new BinaryReader(ms);
        ReadHeaderData(br);
        userId = ReadString(br);
        x = br.ReadSingle();
        y = br.ReadSingle();
        z = br.ReadSingle();
        br.Close();
        ms.Close();
    }

    public Vector3 MoveVector()
    {
        return new Vector3(x, y, z);
    }
}
