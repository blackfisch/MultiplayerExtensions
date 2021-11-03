using BeatSaverSharp.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerExtensions.Core.Beatmaps
{
    class BeatSaverPreviewBeatmapLevel : MpexPreviewBeatmapLevel
    {
        private Beatmap _beatmap;
        private BeatmapVersion _beatmapVersion;

        public override string levelID { get; protected set; }
        public override string levelHash { get; protected set; }

        public override string songName { get; protected set; }
        public override string songSubName { get; protected set; }
        public override string songAuthorName { get; protected set; }
        public override string levelAuthorName { get; protected set; }

        public BeatSaverPreviewBeatmapLevel(string id, string hash, Beatmap beatmap)
        {
            _beatmap = beatmap;
            _beatmapVersion = beatmap.Versions.FirstOrDefault(v => v.Hash == hash);

            levelID = id;
            levelHash = hash;

            songName = beatmap.Metadata.SongName;
            songSubName = beatmap.Metadata.SongSubName;
            songAuthorName = beatmap.Metadata.SongAuthorName;
            levelAuthorName = beatmap.Metadata.LevelAuthorName;
            beatsPerMinute = beatmap.Metadata.BPM;
            songDuration = beatmap.Metadata.Duration;

            // TODO: Populate previewDifficultyBeatmapSets
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
