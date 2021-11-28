using BeatSaverSharp;
using MultiplayerExtensions.Core.Beatmaps.Abstractions;
using MultiplayerExtensions.Core.Beatmaps.Packets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerExtensions.Core.Beatmaps
{
    class NetworkBeatmapLevel : MpexBeatmapLevel
    {
        public override string levelHash { get; protected set; }

        public override string songName => _packet.songName;
        public override string songSubName => _packet.songSubName;
        public override string songAuthorName => _packet.songAuthorName;
        public override string levelAuthorName => _packet.levelAuthorName;
        public override float beatsPerMinute => _packet.beatsPerMinute;
        public override float songDuration => _packet.songDuration;

        private readonly MpexBeatmapPacket _packet;

        public NetworkBeatmapLevel(MpexBeatmapPacket packet)
        {
            levelHash = packet.levelHash;
            _packet = packet;
        }
    }
}
