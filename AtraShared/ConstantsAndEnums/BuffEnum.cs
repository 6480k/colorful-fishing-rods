﻿using AtraBase.Toolkit.Extensions;

using NetEscapades.EnumGenerators;

namespace AtraShared.ConstantsAndEnums;

[Flags]
[EnumExtensions]
public enum BuffEnum
{
    Farming = 1 << Buff.farming,
    Fishing = 1 << Buff.fishing,
    Mining = 1 << Buff.mining,
    Luck = 1 << Buff.luck,
    Foraging = 1 << Buff.foraging,
    MaxStamina = 1 << Buff.maxStamina,
    MagneticRadius = 1 << Buff.magneticRadius,
    Speed = 1 << Buff.speed,
    Defense = 1 << Buff.defense,
    Attack = 1 << Buff.attack,

    // not sure where this comes from.
    Crafting = 1 << Buff.crafting,
}

/// <summary>
/// Extensions for <see cref="BuffEnum"/>.
/// </summary>
public static partial class BuffEnumExtensions
{
    private static readonly Random _random = new Random().PreWarm();
    private static readonly BuffEnum[] _all = BuffEnumExtensions.GetValues();

    private static readonly int CraftingIndex = Array.IndexOf(_all, BuffEnum.Crafting);

    public static BuffEnum GetRandomBuff(Random? random, bool includeCrafting)
    {
        random ??= _random;
        if (includeCrafting)
        {
            return _all[random.Next(Length)];
        }

        int value = random.Next(Length - 1);
        if (value >= CraftingIndex)
        {
            value++;
        }
        return _all[value];
    }

    public static Buff GetBuffOf(this BuffEnum buffEnum, int amount, int minutesDuration, string source, string displaySource)
    {
        int farming = 0;
        if (buffEnum.HasFlagFast(BuffEnum.Farming))
        {
            farming = amount;
        }

        int fishing = 0;
        if (buffEnum.HasFlagFast(BuffEnum.Fishing))
        {
            fishing = amount;
        }

        int mining = 0;
        if (buffEnum.HasFlagFast(BuffEnum.Mining))
        {
            mining = amount;
        }

        int luck = 0;
        if (buffEnum.HasFlagFast(BuffEnum.Luck))
        {
            luck = amount;
        }

        int foraging = 0;
        if (buffEnum.HasFlagFast(BuffEnum.Foraging))
        {
            foraging = amount;
        }

        int maxStamina = 0;
        if (buffEnum.HasFlagFast(BuffEnum.MaxStamina))
        {
            maxStamina = amount * 10;
        }

        int magneticRadius = 0;
        if (buffEnum.HasFlagFast(BuffEnum.MagneticRadius))
        {
            magneticRadius = amount * 10;
        }

        int speed = 0;
        if (buffEnum.HasFlagFast(BuffEnum.Speed))
        {
            speed = amount;
        }

        int defense = 0;
        if (buffEnum.HasFlagFast(BuffEnum.Defense))
        {
            defense = amount;
        }

        int attack = 0;
        if (buffEnum.HasFlagFast(BuffEnum.Attack))
        {
            attack = amount;
        }

        int crafting = 0;
        if (buffEnum.HasFlagFast(BuffEnum.Crafting))
        {
            crafting = amount;
        }

        return new Buff(farming, fishing, mining, 0, luck, foraging, crafting, maxStamina, magneticRadius, speed, defense, attack, minutesDuration, source, displaySource);
    }
}
