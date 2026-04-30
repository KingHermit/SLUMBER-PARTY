using UnityEngine;
using Unity.Netcode;
using System;

public struct CharacterSelectState : INetworkSerializable, IEquatable<CharacterSelectState>
{
    public ulong ClientId;
    public int CharacterId;
    public bool isReady;


    public CharacterSelectState(ulong clientId, bool? ready=null, int charId = -1)
    {
        ClientId = clientId;
        CharacterId = charId;
        isReady = ready ?? false;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref CharacterId);
    }

    public bool Equals(CharacterSelectState other)
    {
        return ClientId == other.ClientId && CharacterId == other.CharacterId;
    }
}
