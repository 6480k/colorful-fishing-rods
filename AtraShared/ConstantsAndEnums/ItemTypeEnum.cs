﻿using Microsoft.Xna.Framework;
using StardewValley.Objects;
using StardewValley.Tools;

namespace AtraShared.ConstantsAndEnums;

/// <summary>
/// An enum that represents the various types of objects in stardew.
/// </summary>
[Flags]
public enum ItemTypeEnum : uint
{
    /// <summary>
    /// A big craftable - <see cref="Game1.bigCraftablesInformation"/>
    /// Use the Vector2 constructor.
    /// </summary>
    BigCraftable = 0b1 << 1,

    /// <summary>
    /// Boots - <see cref="StardewValley.Objects.Boots"/>
    /// </summary>
    Boots = 0b1 << 4 | SObject,

    /// <summary>
    /// Clothing - <see cref="StardewValley.Objects.Clothing" />
    /// </summary>
    Clothing = 0b1 << 5,

    /// <summary>
    /// A colored SObject - <see cref="StardewValley.Objects.ColoredObject" />
    /// </summary>
    ColoredSObject = 0b1 << 3 | SObject,

    /// <summary>
    /// An item managed by DGA.
    /// </summary>
    DGAItem = 0b1 << 15,

    /// <summary>
    /// A furniture item. <see cref="StardewValley.Objects.Furniture"/>
    /// </summary>
    /// <remarks>NOTICE: Don't try to use the usual constructor here.
    /// <see cref="Furniture.GetFurnitureInstance(int, Vector2?) "/>
    /// will create the correct item.</remarks>
    Furniture = 0b1 << 7,

    /// <summary>
    /// A hat item. <see cref="StardewValley.Objects.Hat"/>
    /// </summary>
    Hat = 0b1 << 6,

    /// <summary>
    /// A ring item. <see cref="StardewValley.Objects.Ring" />
    /// </summary>
    /// <remarks>NOTICE: Rings must be passed through the ring constructor, else they won't work.</remarks>
    Ring = 0b1 << 2 | SObject,

    /// <summary>
    /// Any normal object. <see cref="Game1.objectInformation" />
    /// </summary>
    /// <remarks>NOTICE: this includes <see cref="Ring"/>, must handle rings carefully!</remarks>
    SObject = 0b1,

    /// <summary>
    /// Any tool. <see cref="StardewValley.Tool"/>, excluding MeleeWeapons.
    /// </summary>
    Tool = 0b1 << 9,

    /// <summary>
    /// Any wallpaper or flooring item. <see cref="StardewValley.Objects.Wallpaper"/>
    /// </summary>
    WallpaperAndFlooring = 0b1 << 8,

    /// <summary>
    /// Any member of the class <see cref="StardewValley.Tools.MeleeWeapon"/>
    /// </summary>
    Weapon = 0b1 << 10,

    /// <summary>
    /// Any item that should actually be the recipe form.
    /// See <see cref="SObject.IsRecipe"/>
    /// </summary>
    Recipe = 0b1 << 14,
}

/// <summary>
/// Extensions for the ItemType Enum.
/// </summary>
public static class ItemExtensions
{
#warning - TODO: generate all the isinst methods for DGA objects.

    /// <summary>
    /// Tries to get the ItemTypeEnum for a specific item.
    /// Returns null if not possible.
    /// </summary>
    /// <param name="item">Item to check.</param>
    /// <returns>The ItemTypeEnum.</returns>
    public static ItemTypeEnum? GetItemType(this Item item)
    {
        ItemTypeEnum ret;
        switch (item)
        {
            case Boots:
                ret = ItemTypeEnum.Boots;
                break;
            case Clothing:
                ret = ItemTypeEnum.Clothing;
                break;
            case Hat:
                ret = ItemTypeEnum.Hat;
                break;
            case Ring:
                ret = ItemTypeEnum.Ring;
                break;
            case ColoredObject:
                ret = ItemTypeEnum.ColoredSObject;
                break;
            case MeleeWeapon:
                ret = ItemTypeEnum.Weapon;
                break;
            case Tool:
                ret = ItemTypeEnum.Tool;
                break;
            case Wallpaper:
                ret = ItemTypeEnum.WallpaperAndFlooring;
                break;
            case Furniture:
                ret = ItemTypeEnum.Furniture;
                break;
            case SObject obj:
            {
                ret = obj.bigCraftable.Value ? ItemTypeEnum.BigCraftable : ItemTypeEnum.SObject;
                if (obj.IsRecipe)
                {
                    ret |= ItemTypeEnum.Recipe;
                }
                break;
            }
            default:
                return null;
        }
        return ret;
    }
}