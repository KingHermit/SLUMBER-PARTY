using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct PlayerLobbyState : INetworkSerializable, IEquatable<PlayerLobbyState>
{
    public ulong ClientId;
    public FixedString64Bytes playerName;
    public bool isReady;


    public PlayerLobbyState(ulong clientId, bool ready = false, string name = "")
    {
        ClientId = clientId;
        playerName = name ?? "";
        isReady = ready;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref isReady);
    }

    public bool Equals(PlayerLobbyState other)
    {
        return ClientId == other.ClientId && playerName == other.playerName;
    }
}
