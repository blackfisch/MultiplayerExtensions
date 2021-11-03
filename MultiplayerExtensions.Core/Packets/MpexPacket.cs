using LiteNetLib.Utils;

namespace MultiplayerExtensions.Core.Packets
{
    abstract class MpexPacket<TPacket> : INetSerializable, IPoolablePacket where TPacket : MpexPacket<TPacket>, new()
    {
        public abstract void Serialize(NetDataWriter writer);
        public abstract void Deserialize(NetDataReader reader);

        public void Release()
        {
            ThreadStaticPacketPool<TPacket>.pool.Release((this as TPacket)!);
        }
    }
}
