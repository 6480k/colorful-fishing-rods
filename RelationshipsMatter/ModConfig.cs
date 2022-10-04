﻿using AtraShared.Integrations.GMCMAttributes;

namespace RelationshipsMatter;

/// <summary>
/// The config class for this mod.
/// </summary>
[SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "Fields kept near accessors.")]
internal sealed class ModConfig
{
    private float friendshipGainFactor = 0.5f;

    [GMCMRange(0.01, 20)]
    public float FriendshipGainFactor
    {
        get => this.friendshipGainFactor;
        set => this.friendshipGainFactor = Math.Clamp(value, 0.01f, 20f);
    }

    private float friendshipLossFactor = 1.0f;

    [GMCMRange(0.01, 20)]
    public float FriendshipLossFactor
    {
        get => this.friendshipLossFactor;
        set => this.friendshipLossFactor = Math.Clamp(value, 0.01f, 20f);
    }

    private int minRelativeHeartLevel = 5;

    [GMCMRange(0, 8)]
    public int MinRelativeHeartLevel
    {
        get => this.minRelativeHeartLevel;
        set => this.minRelativeHeartLevel = Math.Clamp(value, 0, 8);
    }
}
