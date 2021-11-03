using BeatSaverSharp;
using BeatSaverSharp.Models;
using SiraUtil.Logging;
using SiraUtil.Zenject;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace MultiplayerExtensions.Core.Objects
{
    public class MpexEntitlementChecker : NetworkPlayerEntitlementChecker
	{
		public event Action<string, string, EntitlementsStatus>? receivedEntitlementEvent;

		private ConcurrentDictionary<string, ConcurrentDictionary<string, EntitlementsStatus>> _entitlementsDictionary = new();
		private ConcurrentDictionary<string, ConcurrentDictionary<string, TaskCompletionSource<EntitlementsStatus>>> _tcsDictionary = new();

		private readonly IMultiplayerSessionManager _sessionManager;
		private readonly BeatSaver _beatsaver;
		private readonly SiraLog _logger;

		internal MpexEntitlementChecker(
			IMultiplayerSessionManager sessionManager, 
			UBinder<Plugin, BeatSaver> beatsaver, 
			SiraLog logger)
		{
			_sessionManager = sessionManager;
			_beatsaver = beatsaver.Value;
			_logger = logger;
		}

		public override void Start()
		{
			base.Start();

			_rpcManager.getIsEntitledToLevelEvent -= base.HandleGetIsEntitledToLevel;
			_rpcManager.getIsEntitledToLevelEvent += HandleGetIsEntitledToLevel;
			_rpcManager.setIsEntitledToLevelEvent += HandleSetIsEntitledToLevel;
		}

		public override void OnDestroy()
		{
			_rpcManager.getIsEntitledToLevelEvent -= HandleGetIsEntitledToLevel;
			_rpcManager.getIsEntitledToLevelEvent += base.HandleGetIsEntitledToLevel;
			_rpcManager.setIsEntitledToLevelEvent -= HandleSetIsEntitledToLevel;

			base.OnDestroy();
		}

		private new async void HandleGetIsEntitledToLevel(string userId, string levelId)
		{
			EntitlementsStatus entitlementStatus = await GetEntitlementStatus(levelId);
			_rpcManager.SetIsEntitledToLevel(levelId, entitlementStatus);
		}

		private void HandleSetIsEntitledToLevel(string userId, string levelId, EntitlementsStatus entitlement)
		{
			_logger.Debug($"Entitlement from '{userId}' for '{levelId}' is {entitlement}");

			if (!_entitlementsDictionary.ContainsKey(userId))
				_entitlementsDictionary[userId] = new ConcurrentDictionary<string, EntitlementsStatus>();
			_entitlementsDictionary[userId][levelId] = entitlement;

			if (_tcsDictionary.TryGetValue(userId, out ConcurrentDictionary<string, TaskCompletionSource<EntitlementsStatus>> userTcsDictionary))
				if (userTcsDictionary.TryGetValue(levelId, out TaskCompletionSource<EntitlementsStatus> entitlementTcs) && !entitlementTcs.Task.IsCompleted)
					entitlementTcs.SetResult(entitlement);

			receivedEntitlementEvent?.Invoke(userId, levelId, entitlement);
		}

		/// <summary>
		/// Gets the local user's entitlement for a level.
		/// </summary>
		/// <param name="levelId">Level to check entitlement</param>
		/// <returns>Level entitlement status</returns>
		public override Task<EntitlementsStatus> GetEntitlementStatus(string levelId)
		{
			_logger.Debug($"Checking level entitlement for '{levelId}'");

			string? levelHash = SongCore.Collections.hashForLevelID(levelId);
			if (string.IsNullOrEmpty(levelHash))
				return base.GetEntitlementStatus(levelId);

			if (SongCore.Collections.songWithHashPresent(levelHash))
				return Task.FromResult(EntitlementsStatus.Ok);
			return _beatsaver.BeatmapByHash(levelHash).ContinueWith<EntitlementsStatus>(r =>
			{
				Beatmap? beatmap = r.Result;
				if (beatmap == null)
					return EntitlementsStatus.NotOwned;
				return EntitlementsStatus.NotDownloaded;
			});
		}

		/// <summary>
		/// Gets a remote user's entitlement for a level.
		/// </summary>
		/// <param name="userId">Remote user</param>
		/// <param name="levelId">Level to check entitlement</param>
		/// <returns>Level entitlement status</returns>
		public Task<EntitlementsStatus> GetUserEntitlementStatus(string userId, string levelId)
		{
			if (!string.IsNullOrEmpty(SongCore.Collections.hashForLevelID(levelId)) && !_sessionManager.GetPlayerByUserId(userId).HasState("modded"))
				return Task.FromResult(EntitlementsStatus.NotOwned);

			if (userId == _sessionManager.localPlayer.userId)
				return GetEntitlementStatus(levelId);

			if (_entitlementsDictionary.TryGetValue(userId, out ConcurrentDictionary<string, EntitlementsStatus> userDictionary))
				if (userDictionary.TryGetValue(levelId, out EntitlementsStatus entitlement))
					return Task.FromResult(entitlement);

			if (!_tcsDictionary.ContainsKey(userId))
				_tcsDictionary[userId] = new ConcurrentDictionary<string, TaskCompletionSource<EntitlementsStatus>>();
			if (!_tcsDictionary[userId].ContainsKey(levelId))
				_tcsDictionary[userId][levelId] = new TaskCompletionSource<EntitlementsStatus>();
			_rpcManager.GetIsEntitledToLevel(levelId);
			return _tcsDictionary[userId][levelId].Task;
		}

		/// <summary>
		/// Gets a remote user's entitlement for a level without sending a packet requesting it.
		/// </summary>
		/// <param name="userId">Remote user</param>
		/// <param name="levelId">Level to check entitlement</param>
		/// <returns>Level entitlement status</returns>
		public EntitlementsStatus GetUserEntitlementStatusWithoutRequest(string userId, string levelId)
		{
			if (_entitlementsDictionary.TryGetValue(userId, out ConcurrentDictionary<string, EntitlementsStatus> userDictionary))
				if (userDictionary.TryGetValue(levelId, out EntitlementsStatus entitlement))
					return entitlement;

			return EntitlementsStatus.Unknown;
		}

		/// <summary>
		/// Returns a task that will be completed once a remote user's entitlement for a level is 'Ok'.
		/// </summary>
		/// <param name="userId">Remote user</param>
		/// <param name="levelId">Level to check entitlement</param>
		/// <param name="cancellationToken">Token to cancel task</param>
		/// <returns>Task that is completed on 'Ok' entitlement</returns>
		public async Task WaitForOkEntitlement(string userId, string levelId, CancellationToken cancellationToken)
		{
			if (_entitlementsDictionary.TryGetValue(userId, out ConcurrentDictionary<string, EntitlementsStatus> userDictionary))
				if (userDictionary.TryGetValue(levelId, out EntitlementsStatus entitlement) && entitlement == EntitlementsStatus.Ok)
					return;

			if (!_tcsDictionary.ContainsKey(userId))
				_tcsDictionary[userId] = new ConcurrentDictionary<string, TaskCompletionSource<EntitlementsStatus>>();
			if (!_tcsDictionary[userId].ContainsKey(levelId))
				_tcsDictionary[userId][levelId] = new TaskCompletionSource<EntitlementsStatus>();

			cancellationToken.Register(() => _tcsDictionary[userId][levelId].TrySetCanceled());

			EntitlementsStatus result = EntitlementsStatus.Unknown;
			while (result != EntitlementsStatus.Ok && !cancellationToken.IsCancellationRequested)
			{
				result = await _tcsDictionary[userId][levelId].Task.ContinueWith(t => t.IsCompleted ? t.Result : EntitlementsStatus.Unknown);
				_tcsDictionary[userId][levelId] = new TaskCompletionSource<EntitlementsStatus>();
			}
		}
	}
}
