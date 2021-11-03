using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerExtensions.Core.Beatmaps
{
    class LocalPreviewBeatmapLevel : MpexPreviewBeatmapLevel
    {
        private IPreviewBeatmapLevel _preview;

        public override string levelID { get; protected set; }
        public override string levelHash { get; protected set; }

        public override string songName { get; protected set; }
        public override string songSubName { get; protected set; }
        public override string songAuthorName { get; protected set; }
        public override string levelAuthorName { get; protected set; }

        public LocalPreviewBeatmapLevel(string id, string hash, IPreviewBeatmapLevel preview)
        {
            _preview = preview;

            levelID = id;
            levelHash = hash;

            songName = preview.songName;
            songSubName = preview.songSubName;
            songAuthorName = preview.songAuthorName;
            levelAuthorName = preview.levelAuthorName;
            beatsPerMinute = preview.beatsPerMinute;
            songDuration = preview.songDuration;

            previewStartTime = preview.previewStartTime;
            previewDuration = preview.previewDuration;
            previewDifficultyBeatmapSets = preview.previewDifficultyBeatmapSets;
        }

        public override Task<Sprite> GetCoverImageAsync(CancellationToken cancellationToken)
            => _preview.GetCoverImageAsync(cancellationToken);
    }
}
