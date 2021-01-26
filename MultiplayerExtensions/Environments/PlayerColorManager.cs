﻿using MultiplayerExtensions.Packets;
using MultiplayerExtensions.Sessions;
using System;
using UnityEngine;
using Zenject;

namespace MultiplayerExtensions.Environments
{
    public class PlayerColorManager : IInitializable, IDisposable
    {
        protected readonly PacketManager _packetManager;
        protected readonly IMultiplayerSessionManager _sessionManager;
        protected readonly LobbyPlaceManager _placeManager;
        protected readonly ExtendedPlayerManager _playerManager;

        internal PlayerColorManager(PacketManager packetManager, IMultiplayerSessionManager sessionManager, LobbyPlaceManager placeManager, ExtendedPlayerManager playerManager)
        {
            _packetManager = packetManager;
            _sessionManager = sessionManager;
            _placeManager = placeManager;
            _playerManager = playerManager;
        }

        public void Initialize()
        {
            MPEvents.LobbyEnvironmentLoaded += HandleLobbyEnvironmentLoaded;
            _sessionManager.playerConnectedEvent += HandlePlayerConnected;
            _sessionManager.playerDisconnectedEvent += HandlePlayerDisconnected;
        }

        public void Dispose()
        {
            MPEvents.LobbyEnvironmentLoaded -= HandleLobbyEnvironmentLoaded;
            _sessionManager.playerConnectedEvent -= HandlePlayerConnected;
            _sessionManager.playerDisconnectedEvent -= HandlePlayerDisconnected;
        }

        private void HandleLobbyEnvironmentLoaded(object sender, System.EventArgs e)
        {
            ReloadEnvironment();
        }

        private void HandlePlayerConnected(IConnectedPlayer player)
        {
            ExtendedPlayer? exPlayer = _playerManager.GetExtendedPlayer(player);
            if (exPlayer != null)
                _placeManager.SetPlayerPlaceColor(player, exPlayer.playerColor);
        }

        private void HandlePlayerDisconnected(IConnectedPlayer player)
        {
            ReloadEnvironment();
        }

        private void ReloadEnvironment()
        {
            _placeManager.SetAllPlayerPlaceColor(Color.black);
            _placeManager.SetCenterScreenScale();
            _placeManager.SetPlayerPlaceColor(_sessionManager.localPlayer, _playerManager.localColor);
            foreach (IConnectedPlayer player in _sessionManager.connectedPlayers)
            {
                ExtendedPlayer? exPlayer = _playerManager.GetExtendedPlayer(player);
                if (exPlayer != null)
                    _placeManager.SetPlayerPlaceColor(player, exPlayer.playerColor);
                else
                    Plugin.Log.Info("Player's color not found.");
            }
        }
    }
}