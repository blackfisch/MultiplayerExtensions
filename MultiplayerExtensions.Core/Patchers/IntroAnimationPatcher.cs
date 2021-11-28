using HarmonyLib;
using SiraUtil.Affinity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace MultiplayerExtensions.Core.Patchers
{
    public class IntroAnimationPatcher : IAffinity
    {
        private PlayableDirector? _originalDirector;
        private int iteration = 0;

        [AffinityPrefix]
        [AffinityPatch(typeof(MultiplayerIntroAnimationController), nameof(MultiplayerIntroAnimationController.PlayIntroAnimation))]
        private void BeginPlayIntroAnimation(ref Action onCompleted, Action ____onCompleted, ref bool ____bindingFinished, ref PlayableDirector ____introPlayableDirector)
        {
            _originalDirector = ____introPlayableDirector;

            // Create new gameobject to play the animation after first
            if (iteration != 0)
            {
                GameObject newPlayableGameObject = new GameObject();
                ____introPlayableDirector = newPlayableGameObject.AddComponent<PlayableDirector>();
                ____introPlayableDirector.playableAsset = _originalDirector.playableAsset;

                // Cleanup gameobject
                onCompleted = () => {
                    GameObject.Destroy(newPlayableGameObject);

                    // Make sure old action happens by calling it
                    ____onCompleted.Invoke();
                };
            }

            // Mute audio if animation is not first animation, so audio only plays once
            foreach (TrackAsset track in ((TimelineAsset)____introPlayableDirector.playableAsset).GetOutputTracks())
            {
                track.muted = track is AudioTrack && iteration != 0;
            }

            // Makes animator rebind to new playable
            ____bindingFinished = false;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(MultiplayerIntroAnimationController), nameof(MultiplayerIntroAnimationController.PlayIntroAnimation))]
        private void EndPlayIntroAnimation(ref MultiplayerIntroAnimationController __instance, float maxDesiredIntroAnimationDuration, Action onCompleted, ref PlayableDirector ____introPlayableDirector, MultiplayerPlayersManager ____multiplayerPlayersManager)
        {
            iteration++;
            ____introPlayableDirector = _originalDirector!;
            IEnumerable<IConnectedPlayer> players = ____multiplayerPlayersManager.allActiveAtGameStartPlayers.Where(p => !p.isMe);
            if (iteration < ((players.Count() + 3) / 4))
                __instance.PlayIntroAnimation(maxDesiredIntroAnimationDuration, onCompleted);
            else
                iteration = 0; // Reset
        }

        private readonly MethodInfo _getActivePlayersMethod = AccessTools.PropertyGetter(typeof(MultiplayerPlayersManager), nameof(MultiplayerPlayersManager.allActiveAtGameStartPlayers));
        private readonly MethodInfo _getActivePlayersAttacher = SymbolExtensions.GetMethodInfo(() => GetActivePlayersAttacher());

        [AffinityTranspiler]
        [AffinityPatch(typeof(MultiplayerIntroAnimationController), nameof(MultiplayerIntroAnimationController.BindTimeline))]
        private IEnumerable<CodeInstruction> PlayIntroPlayerCount(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_R4 && codes[i + 1].opcode == OpCodes.Ldc_R4)
                {
                    codes[i] = new CodeInstruction(OpCodes.Ldc_R4, (float)MinPlayers);
                    codes[i + 1] = new CodeInstruction(OpCodes.Ldc_R4, (float)MaxPlayers);
                }
            }
            return codes.AsEnumerable();
        }

        private IReadOnlyList<IConnectedPlayer> GetActivePlayersAttacher()
        {

        }


        private static readonly MethodInfo _rootMethod = typeof(ConcreteBinderNonGeneric).GetMethod(nameof(ConcreteBinderNonGeneric.To), Array.Empty<Type>());

        private static readonly MethodInfo _playersDataModelAttacher = SymbolExtensions.GetMethodInfo(() => PlayersDataModelAttacher(null!));
        private static readonly MethodInfo _playersDataModelMethod = _rootMethod.MakeGenericMethod(new Type[] { typeof(LobbyPlayersDataModel) });

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt)
                {
                    if (codes[i].Calls(_playersDataModelMethod))
                    {
                        CodeInstruction newCode = new CodeInstruction(OpCodes.Callvirt, _playersDataModelAttacher);
                        codes[i] = newCode;
                    }
                }
            }

            return codes.AsEnumerable();
        }

        private static FromBinderNonGeneric PlayersDataModelAttacher(ConcreteBinderNonGeneric contract)
        {
            return contract.To<MpexPlayersDataModel>();
        }
    }
}
