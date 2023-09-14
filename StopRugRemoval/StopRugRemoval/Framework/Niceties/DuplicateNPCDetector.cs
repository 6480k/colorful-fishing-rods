﻿using AtraBase.Toolkit.Extensions;
using AtraBase.Toolkit.StringHandler;

using AtraCore.Framework.Caches;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.GameData.Characters;

namespace StopRugRemoval.Framework.Niceties;

/// <summary>
/// Detects and tries to fix up duplicate NPCs.
/// </summary>
internal static class DuplicateNPCDetector
{
    /// <inheritdoc cref="IGameLoopEvents.DayStarted"/>
    internal static void DayStart()
    {
        if (Context.IsMainPlayer)
        {
            DetectDuplicateNPCs();
        }
    }

    private static Dictionary<string, NPC> DetectDuplicateNPCs()
    {
        Dictionary<string, NPC> found = new();
        foreach (GameLocation loc in Game1.locations)
        {
            for (int i = loc.characters.Count - 1; i >= 0; i--)
            {
                NPC? character = loc.characters[i];

                // no clue who, but someone's managing to stick nulls into the characters list.
                if (character is null)
                {
                    loc.characters.RemoveAt(i);
                    continue;
                }

                if (!character.isVillager() || character.GetType() != typeof(NPC))
                {
                    continue;
                }

                // let's populate AtraCore's cache while we're at it.
                _ = NPCCache.TryInsert(character);

                if (!found.TryAdd(character.Name, character) && character.Name != "Mister Qi")
                {
                    ModEntry.ModMonitor.Log($"Found duplicate NPC {character.Name}", LogLevel.Info);
                    if (ReferenceEquals(character, found[character.Name]))
                    {
                        ModEntry.ModMonitor.Log("    These appear to be the same instance.", LogLevel.Info);
                    }
                    if (character.id != found[character.Name].id)
                    {
                        ModEntry.ModMonitor.Log("    These appear to have different internal IDs", LogLevel.Warn);
                    }

                    if (ModEntry.Config.RemoveDuplicateNPCs)
                    {
                        loc.characters.RemoveAt(i);
                        ModEntry.ModMonitor.Log("    Removing duplicate.", LogLevel.Info);
                    }
                }
            }
        }

        return found;
    }
}
