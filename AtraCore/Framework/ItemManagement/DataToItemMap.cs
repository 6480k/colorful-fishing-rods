﻿using System.Runtime.InteropServices;

using AtraBase.Toolkit.Extensions;

using AtraShared.ConstantsAndEnums;
using AtraShared.Utils.Extensions;
using AtraShared.Wrappers;

using CommunityToolkit.Diagnostics;

namespace AtraCore.Framework.ItemManagement;

/// <summary>
/// Handles looking up the id of an item by its name and type.
/// </summary>
public static class DataToItemMap
{
    private static readonly SortedList<ItemTypeEnum, IAssetName> enumToAssetMap = new(7);

    private static readonly SortedList<ItemTypeEnum, Lazy<Dictionary<string, (string id, bool repeat)>>> nameToIDMap = new(8);

    /// <summary>
    /// Given an ItemType and a name, gets the id.
    /// </summary>
    /// <param name="type">type of the item.</param>
    /// <param name="name">name of the item.</param>
    /// <param name="resolveRecipesSeperately">Whether or not to ignore the recipe bit.</param>
    /// <returns>string ID, or null if not found.</returns>
    public static string? GetID(ItemTypeEnum type, string name, bool resolveRecipesSeperately = false)
    {
        Guard.IsNotNullOrWhiteSpace(name, nameof(name));

        if (!resolveRecipesSeperately)
        {
            type &= ~ItemTypeEnum.Recipe;
        }
        if (type == ItemTypeEnum.ColoredSObject)
        {
            type = ItemTypeEnum.SObject;
        }
        if (nameToIDMap.TryGetValue(type, out Lazy<Dictionary<string, (string, bool)>>? asset)
            && asset.Value.TryGetValue(name, out (string id, bool repeat) pair))
        {
            return pair.id;
        }
        return null;
    }

    /// <summary>
    /// Sets up various maps.
    /// </summary>
    /// <param name="helper">GameContentHelper.</param>
    internal static void Init(IGameContentHelper helper)
    {
        // Populate item-to-asset-enumToAssetMap.
        // Note: Rings are in ObjectInformation, because
        // nothing is nice. So are boots, but they have their own data asset as well.
        enumToAssetMap.Add(ItemTypeEnum.BigCraftable, helper.ParseAssetName(@"Data\BigCraftablesInformation"));
        enumToAssetMap.Add(ItemTypeEnum.Boots, helper.ParseAssetName(@"Data\Boots"));
        enumToAssetMap.Add(ItemTypeEnum.Clothing, helper.ParseAssetName(@"Data\ClothingInformation"));
        enumToAssetMap.Add(ItemTypeEnum.Furniture, helper.ParseAssetName(@"Data\Furniture"));
        enumToAssetMap.Add(ItemTypeEnum.Hat, helper.ParseAssetName(@"Data\hats"));
        enumToAssetMap.Add(ItemTypeEnum.SObject, helper.ParseAssetName(@"Data\ObjectInformation"));
        enumToAssetMap.Add(ItemTypeEnum.Weapon, helper.ParseAssetName(@"Data\weapons"));

        // load the lazies.
        Reset();
    }

    /// <summary>
    /// Resets the requested name-to-id maps.
    /// </summary>
    /// <param name="assets">Assets to reset, or null for all.</param>
    internal static void Reset(IReadOnlySet<IAssetName>? assets = null)
    {
        bool ShouldReset(IAssetName name) => assets is null || assets.Contains(name);

        if (ShouldReset(enumToAssetMap[ItemTypeEnum.SObject]))
        {
            if (!nameToIDMap.TryGetValue(ItemTypeEnum.SObject, out var sobj) || sobj.IsValueCreated)
            {
                nameToIDMap[ItemTypeEnum.SObject] = new(() =>
                {
                    ModEntry.ModMonitor.DebugOnlyLog("Building map to resolve normal objects.", LogLevel.Info);

                    Dictionary<string, (string id, bool duplicate)> mapping = new(Game1Wrappers.ObjectInfo.Count)
                    {
                        // Special cases
                        ["Brown Egg"] = ("180", false),
                        ["Large Brown Egg"] = ("182", false),
                        ["Strange Doll 2"] = ("127", false),
                    };

                    HashSet<string> preAdded = mapping.Values.Select(pair => pair.id).ToHashSet();

                    // Processing from the data.
                    foreach ((string id, string data) in Game1Wrappers.ObjectInfo)
                    {
                        if (ItemHelperUtils.ObjectFilter(id, data) || preAdded.Contains(id))
                        {
                            continue;
                        }

                        string name = data.GetNthChunk('/', SObject.objectInfoNameIndex).ToString();
                        var val = CollectionsMarshal.GetValueRefOrAddDefault(mapping, name, out bool exists);
                        if (exists)
                        {
                            val.duplicate = true;
                        }
                        else
                        {
                            val = new(id, false);
                        }
                    }
                    return mapping;
                });
            }
            if (!nameToIDMap.TryGetValue(ItemTypeEnum.Ring, out var rings) || rings.IsValueCreated)
            {
                nameToIDMap[ItemTypeEnum.Ring] = new(() =>
                {
                    ModEntry.ModMonitor.DebugOnlyLog("Building map to resolve rings.", LogLevel.Info);

                    Dictionary<string, (string id, bool duplicate)> mapping = new(10);
                    foreach ((string id, string data) in Game1Wrappers.ObjectInfo)
                    {
                        ReadOnlySpan<char> cat = data.GetNthChunk('/', 3);

                        // wedding ring (801) isn't a real ring.
                        // JA rings are registered as "Basic -96"
                        if (id == "801" || (!cat.Equals("Ring", StringComparison.Ordinal) && !cat.Equals("Basic -96", StringComparison.Ordinal)))
                        {
                            continue;
                        }

                        string name = data.GetNthChunk('/', SObject.objectInfoNameIndex).ToString();
                        var val = CollectionsMarshal.GetValueRefOrAddDefault(mapping, name, out bool exists);
                        if (exists)
                        {
                            val.duplicate = true;
                        }
                        else
                        {
                            val = new(id, false);
                        }
                    }
                    return mapping;
                });
            }
        }

        if (ShouldReset(enumToAssetMap[ItemTypeEnum.Boots])
            && (!nameToIDMap.TryGetValue(ItemTypeEnum.Boots, out var boots) || boots.IsValueCreated))
        {
            nameToIDMap[ItemTypeEnum.Boots] = new(() =>
            {
                ModEntry.ModMonitor.DebugOnlyLog("Building map to resolve Boots", LogLevel.Info);

                Dictionary<string, (string id, bool duplicate)> mapping = new(20);
                foreach ((string id, string data) in Game1.content.Load<Dictionary<string, string>>(enumToAssetMap[ItemTypeEnum.Boots].BaseName))
                {
                    string name = data.GetNthChunk('/', SObject.objectInfoNameIndex).ToString();
                    var val = CollectionsMarshal.GetValueRefOrAddDefault(mapping, name, out bool exists);
                    if (exists)
                    {
                        val.duplicate = true;
                    }
                    else
                    {
                        val = new(id, false);
                    }
                }
                return mapping;
            });
        }
        if (ShouldReset(enumToAssetMap[ItemTypeEnum.BigCraftable])
            && (!nameToIDMap.TryGetValue(ItemTypeEnum.BigCraftable, out var bc) || bc.IsValueCreated))
        {
            nameToIDMap[ItemTypeEnum.BigCraftable] = new(() =>
            {
                ModEntry.ModMonitor.DebugOnlyLog("Building map to resolve BigCraftables", LogLevel.Info);

                Dictionary<string, (string id, bool duplicate)> mapping = new(Game1.bigCraftablesInformation.Count)
                {
                    // special cases
                    ["Tub o' Flowers"] = Game1.season is Season.Fall or Season.Winter ? ("109", false) : ("108", false),
                    ["Rarecrow 1"] = ("110", false),
                    ["Rarecrow 2"] = ("113", false),
                    ["Rarecrow 3"] = 126,
                    ["Rarecrow 4"] = 136,
                    ["Rarecrow 5"] = 137,
                    ["Rarecrow 6"] = 138,
                    ["Rarecrow 7"] = 139,
                    ["Rarecrow 8"] = 140,
                    ["Seasonal Plant 1"] = 188,
                    ["Seasonal Plant 2"] = 192,
                    ["Seasonal Plant 3"] = 196,
                    ["Seasonal Plant 4"] = 200,
                    ["Seasonal Plant 5"] = 204,
                };

                // House plants :P
                for (int i = 1; i <= 7; i++)
                {
                    mapping["House Plant " + i.ToString()] = (i.ToString(), false);
                }
                foreach ((string id, string data) in Game1.bigCraftablesInformation)
                {
                    ReadOnlySpan<char> nameSpan = data.GetNthChunk('/', SObject.objectInfoNameIndex);
                    if (nameSpan.Equals("House Plant", StringComparison.OrdinalIgnoreCase)
                        || nameSpan.Equals("Wood Chair", StringComparison.OrdinalIgnoreCase)
                        || nameSpan.Equals("Door", StringComparison.OrdinalIgnoreCase)
                        || nameSpan.Equals("Locked Door", StringComparison.OrdinalIgnoreCase)
                        || nameSpan.Equals("Tub o' Flowers", StringComparison.OrdinalIgnoreCase)
                        || nameSpan.Equals("Seasonal Plant", StringComparison.OrdinalIgnoreCase)
                        || nameSpan.Equals("Rarecrow", StringComparison.OrdinalIgnoreCase)
                        || nameSpan.Equals("Crate", StringComparison.OrdinalIgnoreCase)
                        || nameSpan.Equals("Barrel", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string name = nameSpan.ToString();
                    if (!mapping.TryAdd(name, id))
                    {
                        ModEntry.ModMonitor.Log($"{name} with {id} seems to be a duplicate BigCraftable and may not be resolved correctly.", LogLevel.Warn);
                    }
                }
                return mapping;
            });
        }
        if (ShouldReset(enumToAssetMap[ItemTypeEnum.Clothing])
            && (!nameToIDMap.TryGetValue(ItemTypeEnum.Clothing, out Lazy<Dictionary<string, int>>? clothing) || clothing.IsValueCreated))
        {
            nameToIDMap[ItemTypeEnum.Clothing] = new(() =>
            {
                ModEntry.ModMonitor.DebugOnlyLog("Building map to resolve Clothing", LogLevel.Info);

                Dictionary<string, int> mapping = new(Game1.clothingInformation.Count)
                {
                    ["Prismatic Shirt"] = 1999,
                    ["Dark Prismatic Shirt"] = 1998,
                };

                foreach ((int id, string data) in Game1.clothingInformation)
                {
                    ReadOnlySpan<char> nameSpan = data.GetNthChunk('/', SObject.objectInfoNameIndex);
                    if (nameSpan.Equals("Prismatic Shirt", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string name = nameSpan.ToString();
                    if (!mapping.TryAdd(name, id))
                    {
                        ModEntry.ModMonitor.Log($"{name} with {id} seems to be a duplicate ClothingItem and may not be resolved correctly.", LogLevel.Warn);
                    }
                }
                return mapping;
            });
        }
        if (ShouldReset(enumToAssetMap[ItemTypeEnum.Furniture])
            && (!nameToIDMap.TryGetValue(ItemTypeEnum.Furniture, out Lazy<Dictionary<string, int>>? furniture) || furniture.IsValueCreated))
        {
            nameToIDMap[ItemTypeEnum.Furniture] = new(() =>
            {
                ModEntry.ModMonitor.DebugOnlyLog("Building map to resolve Furniture", LogLevel.Info);

                Dictionary<string, int> mapping = new(300);
                foreach ((int id, string data) in Game1.content.Load<Dictionary<int, string>>(enumToAssetMap[ItemTypeEnum.Furniture].BaseName))
                {
                    string name = data.GetNthChunk('/', SObject.objectInfoNameIndex).ToString();
                    if (!mapping.TryAdd(name, id))
                    {
                        ModEntry.ModMonitor.Log($"{name} with {id} seems to be a duplicate Furniture Item and may not be resolved correctly.", LogLevel.Warn);
                    }
                }
                return mapping;
            });
        }
        if (ShouldReset(enumToAssetMap[ItemTypeEnum.Hat])
            && (!nameToIDMap.TryGetValue(ItemTypeEnum.Hat, out Lazy<Dictionary<string, int>>? hats) || hats.IsValueCreated))
        {
            nameToIDMap[ItemTypeEnum.Hat] = new(() =>
            {
                ModEntry.ModMonitor.DebugOnlyLog("Building map to resolve Hats", LogLevel.Info);

                Dictionary<string, int> mapping = new(100);

                foreach ((int id, string data) in Game1.content.Load<Dictionary<int, string>>(enumToAssetMap[ItemTypeEnum.Hat].BaseName))
                {
                    ReadOnlySpan<char> nameSpan = data.GetNthChunk('/', SObject.objectInfoNameIndex);

                    string name = nameSpan.ToString();
                    if (!mapping.TryAdd(name, id))
                    {
                        ModEntry.ModMonitor.Log($"{name} with {id} seems to be a duplicate Hat and may not be resolved correctly.", LogLevel.Warn);
                    }
                }
                return mapping;
            });
        }
    }
}
