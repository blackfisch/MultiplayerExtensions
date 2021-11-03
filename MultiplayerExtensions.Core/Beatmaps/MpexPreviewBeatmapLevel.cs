using BeatSaverSharp;
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

        protected Task<Beatmap?> _beatmap;

        public MpexPreviewBeatmapLevel(string hash, BeatSaver beatsaver)
        {
            levelHash = hash;
            _beatmap = beatsaver.BeatmapByHash(hash);
        }

        public virtual async Task<Sprite> GetCoverImageAsync(CancellationToken cancellationToken)
        {
            Beatmap? beatmap = await _beatmap;
            if (beatmap == null)
                return null!;
            byte[]? coverBytes = await beatmap.LatestVersion.DownloadCoverImage(cancellationToken);
            if (coverBytes == null || coverBytes.Length == 0)
                return null!;
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(coverBytes);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), 100.0f);
        }

        public virtual Task<AudioClip> GetPreviewAudioClipAsync(CancellationToken cancellationToken) 
            => Task.FromResult<AudioClip>(null!);

        public virtual Task<Beatmap?> GetBeatmapAsync(CancellationToken cancellationToken)
            => _beatmap;
    }
}
