using UnityEngine;
using Unity.Netcode;
using System.Runtime.Serialization;

public struct MovePacketNet : INetworkSerializable
{
    public ulong attackerID;
    public int MoveIndex;
    public int HitboxIndex;
    public int facing;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref attackerID);
        serializer.SerializeValue(ref MoveIndex);
        serializer.SerializeValue(ref HitboxIndex);
        serializer.SerializeValue(ref facing);
    }
}
