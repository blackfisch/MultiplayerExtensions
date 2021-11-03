using BeatSaverSharp;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerExtensions.Core.Beatmaps
{
    class LocalPreviewBeatmapLevel : MpexPreviewBeatmapLevel
    {
        public override string levelID => _preview.levelID;
        public override string songName => _preview.songName;
        public override string songSubName => _preview.songSubName;
        public override string songAuthorName => _preview.songAuthorName;
        public override string levelAuthorName => _preview.levelAuthorName;
        public override float beatsPerMinute => _preview.beatsPerMinute;
        public override float songDuration => _preview.songDuration;
        public override float previewStartTime => _preview.previewStartTime;
        public override float previewDuration => _preview.previewDuration;
        public override PreviewDifficultyBeatmapSet[] previewDifficultyBeatmapSets => _preview.previewDifficultyBeatmapSets;

        private IPreviewBeatmapLevel _preview;

        public LocalPreviewBeatmapLevel(IPreviewBeatmapLevel preview, BeatSaver beatsaver) : base(SongCore.Collections.hashForLevelID(preview.levelID), beatsaver)
        {
            _preview = preview;
        }

        public override Task<Sprite> GetCoverImageAsync(CancellationToken cancellationToken)
            => _preview.GetCoverImageAsync(cancellationToken);

        public override Task<AudioClip> GetPreviewAudioClipAsync(CancellationToken cancellationToken)
            => _preview.GetPreviewAudioClipAsync(cancellationToken);
    }
}
