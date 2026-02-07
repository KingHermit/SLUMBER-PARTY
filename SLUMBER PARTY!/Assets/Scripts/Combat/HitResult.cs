using UnityEngine;
using Unity.Netcode;

public struct HitResult : INetworkSerializable
{
    public float damage;
    public float hitstun;
    public float knockbackForce;
    public Vector2 direction;
    public int attackerFacing;
    public float hitstop;
    public AudioClip attackerClip;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref damage);
        serializer.SerializeValue(ref hitstun);
        serializer.SerializeValue(ref knockbackForce);
        serializer.SerializeValue(ref direction);
        serializer.SerializeValue(ref attackerFacing);
        serializer.SerializeValue(ref hitstop);
    }
}
