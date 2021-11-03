using BeatSaverSharp;
using BeatSaverSharp.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerExtensions.Core.Beatmaps
{
    class BeatSaverPreviewBeatmapLevel : MpexPreviewBeatmapLevel
    {
        public override string songName => _beatmap.Result?.Metadata.SongName ?? "Loading...";
        public override string songSubName => _beatmap.Result?.Metadata.SongSubName ?? "";
        public override string songAuthorName => _beatmap.Result?.Metadata.SongAuthorName ?? "";
        public override string levelAuthorName => _beatmap.Result?.Metadata.LevelAuthorName ?? "";
        public override float beatsPerMinute => _beatmap.Result?.Metadata.BPM ?? 0f;
        public override float songDuration => _beatmap.Result?.Metadata.Duration ?? 0f;

        public BeatSaverPreviewBeatmapLevel(string hash, BeatSaver beatsaver) 
            : base(hash, beatsaver)
        {

        }
    }
}
