namespace MultiplayerExtensions.Core.Beatmaps
{
    class PacketPreviewBeatmapLevel : MpexPreviewBeatmapLevel
    {
        public override string levelID { get; protected set; }
        public override string levelHash { get; protected set; }

        public override string songName { get; protected set; }
        public override string songSubName { get; protected set; }
        public override string songAuthorName { get; protected set; }
        public override string levelAuthorName { get; protected set; }

        public PacketPreviewBeatmapLevel(MpexBeatmapPacket packet)
        {
            levelID = packet.levelId;
            levelHash = packet.levelHash;

            songName = packet.songName;
            songSubName = packet.songSubName;
            songAuthorName = packet.songAuthorName;
            levelAuthorName = packet.levelAuthorName;

            beatsPerMinute = packet.beatsPerMinute;
            songDuration = packet.songDuration;
        }
    }
}
