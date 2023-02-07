﻿using AtraShared.Integrations.GMCMAttributes;

using Microsoft.Xna.Framework;

namespace GrowableBushes.Framework;

/// <summary>
/// The config class for this mod.
/// </summary>
internal sealed class ModConfig
{
    public bool CanAxeAllBushes { get; set; } = false;

    [GMCMDefaultVector(1,7)]
    public Vector2 ShopLocation { get; set; } = new(1, 7);

    public bool ShouldNPCsTrampleBushes { get; set; } = true;

    public bool RelaxedPlacement { get; set; } = false;
}
