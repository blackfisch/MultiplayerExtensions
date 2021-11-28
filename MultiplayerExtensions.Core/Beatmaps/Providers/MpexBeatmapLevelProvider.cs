using BeatSaverSharp;
using BeatSaverSharp.Models;
using SiraUtil.Zenject;
using System.Linq;
using System.Threading.Tasks;

namespace MultiplayerExtensions.Core.Beatmaps.Providers
{
    public class MpexBeatmapLevelProvider
    {
        private readonly BeatSaver _beatsaver;

        internal MpexBeatmapLevelProvider(
            UBinder<Plugin, BeatSaver> beatsaver)
        {
            _beatsaver = beatsaver.Value;
        }

        /// <summary>
        /// Gets an <see cref="IPreviewBeatmapLevel"/> for the specified level hash.
        /// </summary>
        /// <param name="levelHash">The hash of the level to get</param>
        /// <returns>An <see cref="IPreviewBeatmapLevel"/> with a matching level hash</returns>
        public async Task<IPreviewBeatmapLevel?> GetBeatmap(string levelHash)
            => GetBeatmapFromLocalBeatmaps(levelHash)
            ?? await GetBeatmapFromBeatSaver(levelHash);

        /// <summary>
        /// Gets an <see cref="IPreviewBeatmapLevel"/> for the specified level hash from local, already downloaded beatmaps.
        /// </summary>
        /// <param name="levelHash">The hash of the level to get</param>
        /// <returns>An <see cref="IPreviewBeatmapLevel"/> with a matching level hash, or null if none was found.</returns>
        public IPreviewBeatmapLevel? GetBeatmapFromLocalBeatmaps(string levelHash)
            => SongCore.Loader.GetLevelByHash(levelHash);

        /// <summary>
        /// Gets an <see cref="IPreviewBeatmapLevel"/> for the specified level hash from BeatSaver.
        /// </summary>
        /// <param name="levelHash">The hash of the level to get</param>
        /// <returns>An <see cref="IPreviewBeatmapLevel"/> with a matching level hash, or null if none was found.</returns>
        public async Task<IPreviewBeatmapLevel?> GetBeatmapFromBeatSaver(string levelHash)
        {
            Beatmap? beatmap = await _beatsaver.BeatmapByHash(levelHash);
            if (beatmap == null)
                return null;
            return new BeatSaverBeatmapLevel(levelHash, beatmap);
        }
    }
}
