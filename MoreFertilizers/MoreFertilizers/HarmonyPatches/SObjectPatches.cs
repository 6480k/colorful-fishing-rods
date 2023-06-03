﻿using AtraBase.Toolkit;
using AtraBase.Toolkit.Reflection;

using AtraCore.Framework.ReflectionManager;

using AtraShared.ConstantsAndEnums;
using AtraShared.Utils.Extensions;

using HarmonyLib;

using MoreFertilizers.Framework;

namespace MoreFertilizers.HarmonyPatches;

/// <summary>
/// Holds patches against SObject.
/// </summary>
[HarmonyPatch(typeof(SObject))]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = StyleCopConstants.NamedForHarmony)]
internal static class SObjectPatches
{
    /// <summary>
    /// Applies patches for objects against DGA as well.
    /// </summary>
    /// <param name="harmony">Harmony instance.</param>
    internal static void ApplyDGAPatch(Harmony harmony)
    {
        try
        {
            Type dgaObject = AccessTools.TypeByName("DynamicGameAssets.Game.CustomObject")
                ?? ReflectionThrowHelper.ThrowMethodNotFoundException<Type>("DGA SObject");
            harmony.Patch(
                original: dgaObject.GetCachedMethod("loadDisplayName", ReflectionCache.FlagTypes.InstanceFlags),
                postfix: new HarmonyMethod(typeof(SObjectPatches), nameof(PostfixLoadDisplayName)));
        }
        catch (Exception ex)
        {
            ModEntry.ModMonitor.LogError("transpiling DGA", ex);
        }
    }

    [HarmonyPostfix]
    [HarmonyPriority(Priority.VeryLow)]
    [HarmonyPatch(nameof(SObject.performObjectDropInAction))]
    private static void PostfixDropInAction(SObject __instance, Item dropInItem, bool probe, bool __result)
    {
        if (!probe && __result)
        {
            try
            {
                if (__instance.heldObject?.Value is not null && dropInItem.modData?.GetBool(CanPlaceHandler.Organic) == true
                    && !__instance.heldObject.Value.modData?.GetBool(CanPlaceHandler.Organic) != true)
                {
                    __instance.heldObject.Value.modData?.SetBool(CanPlaceHandler.Organic, true);
                    if (!__instance.heldObject.Value.Name.Contains(" (Organic)"))
                    {
                        __instance.heldObject.Value.Name += " (Organic)";

                        if (__instance.ParentSheetIndex is 346 or 303 or 614 or 395 or 459)
                        {
                            __instance.Price = (int)(1.1 * __instance.Price);
                        }
                    }
                    __instance.heldObject.Value.MarkContextTagsDirty();
                }
            }
            catch (Exception ex)
            {
                ModEntry.ModMonitor.LogError($"Failed in setting organic for {dropInItem.Name} for machine {__instance.Name}", ex);
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SObject.HighlightFertilizers))]
    private static bool PrefixHighlightFertilizers(Item i, ref bool __result)
    {
        try
        {
            if (__result && TypeUtils.IsExactlyOfType(i, out SObject? obj) && !obj.bigCraftable.Value
                && obj.ParentSheetIndex != -1 && ModEntry.SpecialFertilizerIDs.Contains(obj.ParentSheetIndex))
            {
                __result = false;
                return false;
            }
        }
        catch (Exception ex)
        {
            ModEntry.ModMonitor.LogError("adjusting highlighted fertilizers for enricher", ex);
        }
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch("loadDisplayName")]
    private static void PostfixLoadDisplayName(SObject __instance, ref string __result)
    {
        if (__instance.modData?.GetBool(CanPlaceHandler.Organic) == true)
        {
            __result += ' ' + I18n.Organic();
        }
    }
}