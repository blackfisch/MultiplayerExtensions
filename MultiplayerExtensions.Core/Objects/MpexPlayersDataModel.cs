using MultiplayerExtensions.Core.Beatmaps;
using MultiplayerExtensions.Core.Beatmaps.Abstractions;
using MultiplayerExtensions.Core.Beatmaps.Packets;
using MultiplayerExtensions.Core.Beatmaps.Providers;
using MultiplayerExtensions.Core.Networking;
using SiraUtil.Logging;
using System;
using System.Linq;

namespace MultiplayerExtensions.Core.Objects
{
    public class MpexPlayersDataModel : LobbyPlayersDataModel, ILobbyPlayersDataModel, IDisposable
    {
        private readonly MpexPacketSerializer _packetSerializer;
        private readonly MpexBeatmapLevelProvider _beatmapLevelProvider;
        private readonly SiraLog _logger;

        internal MpexPlayersDataModel(
            MpexPacketSerializer packetSerializer,
            MpexBeatmapLevelProvider beatmapLevelProvider,
            SiraLog logger)
        {
            _packetSerializer = packetSerializer;
            _beatmapLevelProvider = beatmapLevelProvider;
            _logger = logger;
        }

        public override void Activate()
        {
            _packetSerializer.RegisterCallback<MpexBeatmapPacket>(HandleMpexBeatmapPacket);
            base.Activate();
        }

        public override void Deactivate()
        {
            _packetSerializer.UnregisterCallback<MpexBeatmapPacket>();
            base.Deactivate();
        }

        public new void Dispose()
            => Deactivate();

        private void HandleMpexBeatmapPacket(MpexBeatmapPacket packet, IConnectedPlayer player)
        {
            _logger.Debug($"'{player.userId}' selected song '{packet.levelHash}'.");
            BeatmapCharacteristicSO characteristic = _beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(packet.characteristic);
            MpexBeatmapLevel preview = new NetworkBeatmapLevel(packet);
            base.SetPlayerBeatmapLevel(player.userId, preview, packet.difficulty, characteristic);
        }

        public override void HandleMenuRpcManagerGetRecommendedBeatmap(string userId)
        {
            ILobbyPlayerData localPlayerData = playersData[localUserId];

            if (localPlayerData.beatmapLevel is MpexBeatmapLevel mpexBeatmapLevel)
                _multiplayerSessionManager.Send(new MpexBeatmapPacket(mpexBeatmapLevel, localPlayerData.beatmapCharacteristic.serializedName, localPlayerData.beatmapDifficulty));

            base.HandleMenuRpcManagerGetRecommendedBeatmap(userId);
        }

        public override void HandleMenuRpcManagerRecommendBeatmap(string userId, BeatmapIdentifierNetSerializable beatmapId)
        {
            if (!string.IsNullOrEmpty(SongCore.Collections.hashForLevelID(beatmapId.levelID)))
                return;
            base.HandleMenuRpcManagerRecommendBeatmap(userId, beatmapId);
        }

        public async override void SetLocalPlayerBeatmapLevel(string levelId, BeatmapDifficulty beatmapDifficulty, BeatmapCharacteristicSO characteristic)
        {
            _logger.Debug($"Local player selected song '{levelId}'");
            string? levelHash = SongCore.Collections.hashForLevelID(levelId);
            if (!string.IsNullOrEmpty(levelHash))
            {
                IPreviewBeatmapLevel? beatmapLevel = await _beatmapLevelProvider.GetBeatmap(levelHash);
                if (beatmapLevel == null)
                    return;

                _multiplayerSessionManager.Send(new MpexBeatmapPacket(beatmapLevel, characteristic.serializedName, beatmapDifficulty));
                _menuRpcManager.RecommendBeatmap(new BeatmapIdentifierNetSerializable(levelId, characteristic.serializedName, beatmapDifficulty));
                SetPlayerBeatmapLevel(localUserId, beatmapLevel, beatmapDifficulty, characteristic);
                return;
            }
            base.SetLocalPlayerBeatmapLevel(levelId, beatmapDifficulty, characteristic);
        }
    }
}
