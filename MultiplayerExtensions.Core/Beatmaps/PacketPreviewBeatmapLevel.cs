using BeatSaverSharp;

namespace MultiplayerExtensions.Core.Beatmaps
{
    class PacketPreviewBeatmapLevel : MpexPreviewBeatmapLevel
    {
        public override string songName => _packet.songName;
        public override string songSubName => _packet.songSubName;
        public override string songAuthorName => _packet.songAuthorName;
        public override string levelAuthorName => _packet.levelAuthorName;
        public override float beatsPerMinute => _packet.beatsPerMinute;
        public override float songDuration => _packet.songDuration;

        private readonly MpexBeatmapPacket _packet;

        public PacketPreviewBeatmapLevel(MpexBeatmapPacket packet, BeatSaver beatsaver) 
            : base(packet.levelHash, beatsaver)
        {
            _packet = packet;
        }
    }
}
