﻿using AtraBase.Toolkit.Extensions;

using AtraShared.Utils.Extensions;

namespace CatGiftsRedux.Framework;

/// <summary>
/// Tries to pick a random animal product.
/// </summary>
internal static class AnimalProductChooser
{
    internal static SObject? Pick(Random random)
    {
        ModEntry.ModMonitor.DebugOnlyLog("Picked Animal Products");

        var content = Game1.content.Load<Dictionary<string, string>>("Data\\FarmAnimals");
        if (content.Count == 0)
        {
            return null;
        }
        var randomAnimal = content.ElementAt(random.Next(content.Count));

        if (int.TryParse(randomAnimal.Value.GetNthChunk('/', 2), out var id) && id > 0)
        {
            return new SObject(id, 1);
        }
        return null;
    }
}
