﻿using AtraBase.Toolkit.Extensions;
using AtraBase.Toolkit.StringHandler;

using AtraShared.Utils.Extensions;

using Microsoft.Xna.Framework;

using StardewValley.Objects;

namespace CatGiftsRedux.Framework;

/// <summary>
/// Picks a random seasonal crop item.
/// </summary>
internal class SeasonalCropChooser
{
    internal static SObject? Pick(Random random)
    {
        ModEntry.ModMonitor.DebugOnlyLog("Picked Seasonal Crops");

        var content = Game1.content.Load<Dictionary<int, string>>("Data/Crops")
                                   .Where((kvp) => kvp.Value.GetNthChunk('/', 1).Contains(Game1.currentSeason, StringComparison.OrdinalIgnoreCase))
                                   .ToList();

        if (content.Count == 0)
        {
            return null;
        }

        var entry = content[random.Next(content.Count)];

        if (int.TryParse(entry.Value.GetNthChunk('/', 3), out var id) && id > 0)
        {
            var colored = entry.Value.GetNthChunk('/', 8);
            if (colored.StartsWith("true", StringComparison.Ordinal))
            {
                var stream = colored.StreamSplit();
                _ = stream.MoveNext(); // the original "true"

                byte[] colorarray = new byte[3];
                int index = 0;
                foreach (var c in stream)
                {
                    if (byte.TryParse(c, out var colorbit))
                    {
                        colorarray[index++] = colorbit;
                        if (index >= 3)
                        {
                            break;
                        }
                    }
                    else
                    {
                        goto NoColor;
                    }
                }
                return new ColoredObject(id, 1, new Color(colorarray[0], colorarray[1], colorarray[2]));
            }

NoColor:
            return new SObject(id, 1);
        }
        return null;
    }
}
