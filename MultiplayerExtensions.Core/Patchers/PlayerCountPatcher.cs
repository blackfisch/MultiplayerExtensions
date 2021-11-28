using HarmonyLib;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace MultiplayerExtensions.Core.Patchers
{
    public class PlayerCountPatcher : IAffinity
    {
        public int MinPlayers { get; set; } = 2;
        public int MaxPlayers { get; set; } = 5;
        public bool AddEmptyPlayerSlotForEvenCount { get; set; } = false;

        private readonly SiraLog _logger;

        internal PlayerCountPatcher(
            SiraLog logger)
        {
            _logger = logger;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(CreateServerFormController), nameof(CreateServerFormController.Setup))]
        private void CreateServerFormSetup(ref int selectedNumberOfPlayers, FormattedFloatListSettingsController ____maxPlayersList)
        {
            _logger.Debug($"Creating server form with player clamp between '{MinPlayers}' and '{MaxPlayers}'");
            selectedNumberOfPlayers = Mathf.Clamp(selectedNumberOfPlayers, MinPlayers, MaxPlayers);
            ____maxPlayersList.values = Enumerable.Range(MinPlayers, MaxPlayers - MinPlayers + 1).Select(x => (float)x).ToArray();
        }

        [AffinityTranspiler]
        [AffinityPatch(typeof(CreateServerFormController), nameof(CreateServerFormController.formData), AffinityMethodType.Getter)]
        private IEnumerable<CodeInstruction> CreateServerFormData(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_R4 && codes[i + 1].opcode == OpCodes.Ldc_R4)
                {
                    codes[i + 2] = new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => ClampFloatAttacher(0f, 0f, 0f)));
                }
            }
            return codes.AsEnumerable();
        }

        [AffinityTranspiler]
        [AffinityPatch(typeof(MultiplayerPlayerPlacement), nameof(MultiplayerPlayerPlacement.GetAngleBetweenPlayersWithEvenAdjustment))]
        private IEnumerable<CodeInstruction> PlayerPlacementAngle(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            int divStartIndex = codes.FindIndex(code => code.opcode == OpCodes.Ldc_R4 && code.OperandIs(360));
            if (!AddEmptyPlayerSlotForEvenCount && divStartIndex != -1)
                codes.RemoveRange(0, divStartIndex);
            return codes.AsEnumerable();
        }

        [AffinityTranspiler]
        [AffinityPatch(typeof(MultiplayerLayoutProvider), nameof(MultiplayerLayoutProvider.CalculateLayout))]
        private IEnumerable<CodeInstruction> PlayerGameplayLayout(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                if (!AddEmptyPlayerSlotForEvenCount && codes[i].opcode == OpCodes.Ldc_I4_1 && codes[i + 1].opcode == OpCodes.Add)
                    codes.RemoveRange(i, 2);
            }
            return codes.AsEnumerable();
        }

        private static float ClampFloatAttacher(float value, float min, float max)
            => value;
    }
}
