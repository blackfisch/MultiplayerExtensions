using MultiplayerExtensions.Core.Beatmaps;
using MultiplayerExtensions.Core.Packets;
using SiraUtil.Logging;
using System;
using System.Linq;

namespace MultiplayerExtensions.Core.Objects
{
    public class MpexPlayersDataModel : LobbyPlayersDataModel, ILobbyPlayersDataModel, IDisposable
    {
        private readonly MpexPacketSerializer _packetSerializer;
        private readonly SiraLog _logger;

        internal MpexPlayersDataModel(
            MpexPacketSerializer packetSerializer,
            SiraLog logger)
        {
            _packetSerializer = packetSerializer;
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
            MpexPreviewBeatmapLevel preview = new PacketPreviewBeatmapLevel(packet);
            base.SetPlayerBeatmapLevel(player.userId, preview, packet.difficulty, characteristic);
        }

        public override void HandleMenuRpcManagerGetRecommendedBeatmap(string userId)
        {
            ILobbyPlayerData localPlayerData = playersData[localUserId];

            if (localPlayerData.beatmapLevel is MpexPreviewBeatmapLevel mpexBeatmapLevel)
                _multiplayerSessionManager.Send(new MpexBeatmapPacket(mpexBeatmapLevel, localPlayerData.beatmapCharacteristic.serializedName, localPlayerData.beatmapDifficulty));

            base.HandleMenuRpcManagerGetRecommendedBeatmap(userId);
        }

        public override void HandleMenuRpcManagerRecommendBeatmap(string userId, BeatmapIdentifierNetSerializable beatmapId)
        {
            if (string.IsNullOrEmpty(SongCore.Collections.hashForLevelID(beatmapId.levelID)))
                return;
            base.HandleMenuRpcManagerRecommendBeatmap(userId, beatmapId);
        }

        public override void SetLocalPlayerBeatmapLevel(string levelId, BeatmapDifficulty beatmapDifficulty, BeatmapCharacteristicSO characteristic)
        {
            string? levelHash = SongCore.Collections.hashForLevelID(levelId);
            if (string.IsNullOrEmpty(levelHash))
            {
                MpexPreviewBeatmapLevel? mpexBeatmapLevel = null;
                IPreviewBeatmapLevel? localBeatmapLevel = SongCore.Loader.GetLevelById(levelId);
                if (localBeatmapLevel != null)
                    mpexBeatmapLevel = new LocalPreviewBeatmapLevel(levelId, levelHash, localBeatmapLevel);
                if (mpexBeatmapLevel == null)
                    mpexBeatmapLevel = GetExistingPreviewBeatmap(levelId);
                if (mpexBeatmapLevel == null)
                    return;

                _multiplayerSessionManager.Send(new MpexBeatmapPacket(mpexBeatmapLevel, characteristic.serializedName, beatmapDifficulty));
                _menuRpcManager.RecommendBeatmap(new BeatmapIdentifierNetSerializable(levelId, characteristic.serializedName, beatmapDifficulty));
                SetPlayerBeatmapLevel(localUserId, mpexBeatmapLevel, beatmapDifficulty, characteristic);
                return;
            }
            base.SetLocalPlayerBeatmapLevel(levelId, beatmapDifficulty, characteristic);
        }

        private MpexPreviewBeatmapLevel? GetExistingPreviewBeatmap(string levelId)
        {
            IPreviewBeatmapLevel? preview = _playersData.Values.ToList().Find(playerData => playerData.beatmapLevel?.levelID == levelId)?.beatmapLevel;
            if (preview is MpexPreviewBeatmapLevel previewBeatmap)
                return previewBeatmap;
            return null;
        }
    }
}
