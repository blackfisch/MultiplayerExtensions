using SiraUtil.Affinity;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerExtensions.Core.Patchers
{
    public class CustomLevelsPatcher : IAffinity
    {
        public bool OverrideCustomLevels { get; set; } = true;

        private readonly SiraLog _logger;

        internal CustomLevelsPatcher(
            SiraLog logger)
        {
            _logger = logger;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MultiplayerLevelSelectionFlowCoordinator), "enableCustomLevels", AffinityMethodType.Getter)]
        private bool CustomLevelsEnabled(ref bool __result, SongPackMask ____songPackMask)
        {
            __result = ____songPackMask.Contains(new SongPackMask("custom_levelpack_CustomLevels"));
            return false;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(SongPackMask), nameof(SongPackMask.all), AffinityMethodType.Getter)]
        private bool GetSongPackMaskAll(ref SongPackMask __result)
        {
            // make default 'all' songpackmask not include custom levels - this is for official matchmaking
            SongPackMask.TryParse((BitMask128.maxValue ^ "custom_levelpack_CustomLevels".ToBloomFilter<BitMask128>(3, 8)).ToString(), out __result);
            return false;
        }
    }
}
