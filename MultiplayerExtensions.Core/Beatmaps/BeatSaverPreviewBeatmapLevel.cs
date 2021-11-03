using BeatSaverSharp.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerExtensions.Core.Beatmaps
{
    class BeatSaverPreviewBeatmapLevel : MpexPreviewBeatmapLevel
    {
        public override string songName => _beatmap.Metadata.SongName;
        public override string songSubName => _beatmap.Metadata.SongSubName;
        public override string songAuthorName => _beatmap.Metadata.SongAuthorName;
        public override string levelAuthorName => _beatmap.Metadata.LevelAuthorName;
        public override float beatsPerMinute => _beatmap.Metadata.BPM;
        public override float songDuration => _beatmap.Metadata.Duration;

        private Beatmap _beatmap;
        private BeatmapVersion _beatmapVersion;

        public BeatSaverPreviewBeatmapLevel(string hash, Beatmap beatmap) : base(hash)
        {
            _beatmap = beatmap;
            _beatmapVersion = beatmap.Versions.FirstOrDefault(v => v.Hash == hash);
        }

        public override async Task<Sprite> GetCoverImageAsync(CancellationToken cancellationToken)
        {
            byte[]? coverBytes = await _beatmapVersion.DownloadCoverImage(cancellationToken);
            if (coverBytes != null && coverBytes.Length != 0)
            {
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(coverBytes);
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), 100.0f);
            }

            return Sprite.Create(Texture2D.blackTexture, new Rect(0, 0, 2, 2), new Vector2(0, 0), 100.0f);
        }
    }
}
