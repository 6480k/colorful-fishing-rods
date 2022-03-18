﻿using System.Reflection;
using System.Reflection.Emit;
using AtraShared.Utils.HarmonyHelper;
using HarmonyLib;
using StardewValley.Buildings;

namespace DrawFishPondsOverGrass.HarmonyPatches;

[HarmonyPatch(typeof(JumpingFish))]
internal static class JumpingFishLayerDepthFix
{
    private const float Offset = 280f;

    [HarmonyPatch(nameof(JumpingFish.Draw))]
    private static IEnumerable<CodeInstruction>? Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator gen, MethodBase original)
    {
        try
        {
            ILHelper helper = new(original, instructions, ModEntry.ModMonitor, gen);
            helper.ForEachMatch(
                new CodeInstructionWrapper[]
                {
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldflda),
                    new(OpCodes.Ldfld),
                    new(OpCodes.Ldc_R4, 10000f),
                    new(OpCodes.Div),
                },
                transformer: (helper) =>
                {
                    helper.Advance(3)
                        .Insert(new CodeInstruction[]
                        {
                            new (OpCodes.Ldc_R4, Offset),
                            new (OpCodes.Add),
                        });
                    return true;
                });
            return helper.Render();
        }
        catch (Exception ex)
        {
            ModEntry.ModMonitor.Log($"Failed while trying to transpile jumping fish layer depth\n{ex}", LogLevel.Error);
        }
        return null;
    }
}

[HarmonyPatch(typeof(PondFishSilhouette))]
internal static class PondFishLayerDepthFix
{
    private const float Offset = 280f;

    [HarmonyPatch(nameof(PondFishSilhouette.Draw))]
    private static IEnumerable<CodeInstruction>? Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator gen, MethodBase original)
    {
        try
        {
            ILHelper helper = new(original, instructions, ModEntry.ModMonitor, gen);
            helper.ForEachMatch(
                new CodeInstructionWrapper[]
                {
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldflda),
                    new(OpCodes.Ldfld),
                    new(OpCodes.Ldc_R4, 10000f),
                    new(OpCodes.Div),
                },
                transformer: (helper) =>
                {
                    helper.Advance(3)
                        .Insert(new CodeInstruction[]
                        {
                            new (OpCodes.Ldc_R4, Offset),
                            new (OpCodes.Add),
                        });
                    return true;
                });
            return helper.Render();
        }
        catch (Exception ex)
        {
            ModEntry.ModMonitor.Log($"Failed while trying to transpile shadow fish layer depth\n{ex}", LogLevel.Error);
        }

        return null;
    }
}