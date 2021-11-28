using LiteNetLib.Utils;

namespace MultiplayerExtensions.Core.Networking.Abstractions
{
    public abstract class MpexPacket<TPacket> : INetSerializable, IPoolablePacket where TPacket : MpexPacket<TPacket>, new()
    {
        /// <summary>
        /// Serializes the packet and puts data into a <see cref="NetDataWriter"/>.
        /// </summary>
        /// <param name="writer">Writer to put data into</param>
        public abstract void Serialize(NetDataWriter writer);

        /// <summary>
        /// Deserializes packet data from a <see cref="NetDataReader"/>.
        /// </summary>
        /// <param name="reader">Reader to get data from</param>
        public abstract void Deserialize(NetDataReader reader);

        /// <summary>
        /// GC's the packet (? this method is called by the game)
        /// </summary>
        public void Release()
        {
            ThreadStaticPacketPool<TPacket>.pool.Release((this as TPacket)!);
        }
    }
}
