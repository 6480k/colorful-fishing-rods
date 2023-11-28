﻿namespace AtraCore.Framework.GameStateQueries;

using static StardewValley.GameStateQuery;

/// <summary>
/// Handles adding a GSQ that checks for a wallet item. wallet item.
/// </summary>
internal static class MoneyEarned
{
    /// <inheritdoc cref="T:StardewValley.Delegates.GameStateQueryDelegate"/>
    /// <remarks>Checks if the given player has the specific wallet item.</remarks>
    internal static bool CheckMoneyEarned(string[] query, GameLocation location, Farmer player, Item targetItem, Item inputItem, Random random)
    {
        uint max = uint.MaxValue;
        if (!ArgUtility.TryGet(query, 1, out string? playerKey, out string? error)
            || !ArgUtility.TryGet(query, 2, out var minS, out error) || !TryParseUInt(minS, out uint min, out error)
            || !ArgUtility.TryGetOptional(query, 3, out var maxS, out error, null)
            || (maxS is not null && !TryParseUInt(maxS, out max, out error)))
        {
            return Helpers.ErrorResult(query, error);
        }

        return Helpers.WithPlayer(player, playerKey, (Farmer target) => target.totalMoneyEarned >= min && target.totalMoneyEarned <= max);
    }

    private static bool TryParseUInt(string str, out uint value, out string error)
    {
        if (uint.TryParse(str, out value))
        {
            error = string.Empty;
            return true;
        }

        value = 0;
        error = $"value '{str}', which can't be parsed as uint";
        return false;
    }
}