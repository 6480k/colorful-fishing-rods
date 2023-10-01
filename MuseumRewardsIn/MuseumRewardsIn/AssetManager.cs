﻿using StardewModdingAPI.Events;

namespace MuseumRewardsIn;

/// <summary>
/// Manages assets for this mod.
/// </summary>
internal static class AssetManager
{
    private static Lazy<HashSet<string>> mailflags = new(GetMailFlagsForStore);

    private static IAssetName letters = null!;
    private static IAssetName shops = null!;

    /// <summary>
    /// Gets a hashset of mailflags to process for gifts.
    /// </summary>
    internal static HashSet<string> MailFlags => mailflags.Value;

    /// <summary>
    /// Initializes the AssetManager.
    /// </summary>
    /// <param name="parser">Game Content Helper.</param>
    internal static void Initialize(IGameContentHelper parser)
    {
        letters = parser.ParseAssetName("Mods/atravita/MuseumStore/Letters");
        shops = parser.ParseAssetName("Data/Shops");
    }

    /// <summary>
    /// Listens for invalidations to drop the cache if needed.
    /// </summary>
    /// <param name="names">Hashset of assetnames invalidated.</param>
    internal static void Invalidate(IReadOnlySet<IAssetName>? names = null)
    {
        if (mailflags.IsValueCreated && (names is null || names.Contains(letters)))
        {
            mailflags = new(GetMailFlagsForStore);
        }
    }

    /// <summary>
    /// Applies the asset loads.
    /// </summary>
    /// <param name="e">Event args.</param>
    internal static void Apply(AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo(letters))
        {
            e.LoadFromModFile<Dictionary<string, string>>("assets/vanilla_mail.json", AssetLoadPriority.Exclusive);
        }
    }

    private static HashSet<string> GetMailFlagsForStore()
        => Game1.temporaryContent.Load<Dictionary<string, string>>(letters.BaseName).Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
}
