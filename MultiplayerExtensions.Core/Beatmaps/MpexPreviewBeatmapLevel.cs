using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerExtensions.Core.Beatmaps
{
    abstract class MpexPreviewBeatmapLevel : IPreviewBeatmapLevel
    {
        public abstract string levelID { get; protected set; }
        public abstract string levelHash { get; protected set; }

        public abstract string songName { get; protected set; }
        public abstract string songSubName { get; protected set; }
        public abstract string songAuthorName { get; protected set; }
        public abstract string levelAuthorName { get; protected set; }
        public float beatsPerMinute { get; protected set; }
        public float songDuration { get; protected set; }

        public float songTimeOffset { get; protected set; }
        public float shuffle { get; protected set; }
        public float shufflePeriod { get; protected set; }
        public float previewStartTime { get; protected set; }
        public float previewDuration { get; protected set; }
        public EnvironmentInfoSO? environmentInfo { get; protected set; }
        public EnvironmentInfoSO? allDirectionsEnvironmentInfo { get; protected set; }
        public PreviewDifficultyBeatmapSet[]? previewDifficultyBeatmapSets { get; protected set; }

        public virtual Task<Sprite> GetCoverImageAsync(CancellationToken cancellationToken)
            => Task.FromResult<Sprite>(null!);

        public virtual Task<AudioClip> GetPreviewAudioClipAsync(CancellationToken cancellationToken) 
            => Task.FromResult<AudioClip>(null!);
    }
}
