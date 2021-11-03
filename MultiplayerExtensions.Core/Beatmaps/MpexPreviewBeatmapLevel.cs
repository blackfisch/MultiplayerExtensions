using BeatSaverSharp.Models;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerExtensions.Core.Beatmaps
{
    abstract class MpexPreviewBeatmapLevel : IPreviewBeatmapLevel
    {
        /// <summary>
        /// The local ID of the level. Can vary between clients.
        /// </summary>
        public virtual string levelID => $"custom_level_{levelHash}";

        /// <summary>
        /// The hash of the level. Should be the same on all clients.
        /// </summary>
        public virtual string levelHash { get; protected set; }

        public abstract string songName { get; }
        public abstract string songSubName { get; }
        public abstract string songAuthorName { get; }
        public abstract string levelAuthorName { get; }

        public virtual float beatsPerMinute { get; protected set; }
        public virtual float songDuration { get; protected set; }
        public virtual float previewStartTime { get; protected set; }
        public virtual float previewDuration { get; protected set; }
        public virtual PreviewDifficultyBeatmapSet[]? previewDifficultyBeatmapSets { get; protected set; }

        public float songTimeOffset { get; private set; } // Not needed
        public float shuffle { get; private set; } // Not needed
        public float shufflePeriod { get; private set; } // Not needed
        public EnvironmentInfoSO? environmentInfo => null; // Not needed, used for level load
        public EnvironmentInfoSO? allDirectionsEnvironmentInfo => null; // Not needed, used for level load

        public MpexPreviewBeatmapLevel(string hash)
        {
            levelHash = hash;
        }

        public virtual Task<Sprite> GetCoverImageAsync(CancellationToken cancellationToken)
            => Task.FromResult<Sprite>(null!);

        public virtual Task<AudioClip> GetPreviewAudioClipAsync(CancellationToken cancellationToken) 
            => Task.FromResult<AudioClip>(null!);
    }
}
