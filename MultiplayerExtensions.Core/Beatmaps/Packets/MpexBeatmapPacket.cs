using LiteNetLib.Utils;
using MultiplayerExtensions.Core.Beatmaps.Abstractions;
using MultiplayerExtensions.Core.Networking.Abstractions;

namespace MultiplayerExtensions.Core.Beatmaps.Packets
{
    public class MpexBeatmapPacket : MpexPacket<MpexBeatmapPacket>
    {
        public string levelHash = null!;
        public string songName = null!;
        public string songSubName = null!;
        public string songAuthorName = null!;
        public string levelAuthorName = null!;
        public float beatsPerMinute;
        public float songDuration;

        public string characteristic = null!;
        public BeatmapDifficulty difficulty;

        public MpexBeatmapPacket() { }

        public MpexBeatmapPacket(IPreviewBeatmapLevel preview, string characteristic, BeatmapDifficulty difficulty)
        {
            levelHash = SongCore.Collections.hashForLevelID(preview.levelID);
            songName = preview.songName;
            songSubName = preview.songSubName;
            songAuthorName = preview.songAuthorName;
            levelAuthorName = preview.levelAuthorName;
            beatsPerMinute = preview.beatsPerMinute;
            songDuration = preview.songDuration;

            this.characteristic = characteristic;
            this.difficulty = difficulty;
        }

        public override void Serialize(NetDataWriter writer)
        {
            writer.Put(levelHash);
            writer.Put(songName);
            writer.Put(songSubName);
            writer.Put(songAuthorName);
            writer.Put(levelAuthorName);
            writer.Put(beatsPerMinute);
            writer.Put(songDuration);

            writer.Put(characteristic);
            writer.Put((uint)difficulty);
        }

        public override void Deserialize(NetDataReader reader)
        {
            levelHash = reader.GetString();
            songName = reader.GetString();
            songSubName = reader.GetString();
            songAuthorName = reader.GetString();
            levelAuthorName = reader.GetString();
            beatsPerMinute = reader.GetFloat();
            songDuration = reader.GetFloat();

            characteristic = reader.GetString();
            difficulty = (BeatmapDifficulty)reader.GetUInt();
        }
    }
}
