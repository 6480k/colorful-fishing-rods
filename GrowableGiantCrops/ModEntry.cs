﻿using AtraCore.Utilities;

using AtraShared.ConstantsAndEnums;
using AtraShared.Integrations;
using AtraShared.Integrations.Interfaces;
using AtraShared.MigrationManager;
using AtraShared.Utils.Extensions;

using GrowableGiantCrops.Framework;

using HarmonyLib;

using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;

using StardewModdingAPI.Events;

using AtraUtils = AtraShared.Utils.Utils;

namespace GrowableGiantCrops;

/// <inheritdoc />
internal sealed class ModEntry : Mod
{
    private MigrationManager? migrator;

    /// <summary>
    /// Gets the logger for this mod.
    /// </summary>
    internal static IMonitor ModMonitor { get; private set; } = null!;

    /// <summary>
    /// Gets the config instance for this mod.
    /// </summary>
    internal static ModConfig Config { get; private set; } = null!;

    #region APIs
    internal static IJsonAssetsAPI? JaAPI { get; private set; }

    internal static IMoreGiantCropsAPI? MoreGiantCropsAPI { get; private set; }

    internal static IGiantCropTweaks? GiantCropTweaksAPI { get; private set; }

    internal static IGrowableBushesAPI? GrowableBushesAPI { get; private set; }

    #endregion

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);

        ModMonitor = this.Monitor;
        Config = AtraUtils.GetConfigOrDefault<ModConfig>(helper, this.Monitor);

        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        ConsoleCommands.RegisterCommands(helper.ConsoleCommands);

        AssetManager.Initialize(helper.GameContent);

        //ShopManager.Initialize(helper.GameContent);

        this.Monitor.Log($"Starting up: {this.ModManifest.UniqueID} - {typeof(ModEntry).Assembly.FullName}");

        // assets
        this.Helper.Events.Content.AssetRequested += static (_, e) => AssetManager.OnAssetRequested(e);
    }

    // /// <inheritdoc />
    // public override object? GetApi() => new GrowableBushesAPI();

    /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        IntegrationHelper helper = new(this.Monitor, this.Helper.Translation, this.Helper.ModRegistry, LogLevel.Error);

        if (helper.TryGetAPI("spacechase0.SpaceCore", "1.9.3", out ICompleteSpaceCoreAPI? api))
        {
            api.RegisterSerializerType(typeof(InventoryGiantCrop));
            api.RegisterSerializerType(typeof(ShovelTool));

            this.Helper.Events.GameLoop.SaveLoaded += this.SaveLoaded;

            // this.Helper.Events.Player.Warped += this.OnWarped;
            // this.Helper.Events.Player.InventoryChanged += this.OnInventoryChanged;

            // shop
            // this.Helper.Events.Content.AssetRequested += static (_, e) => ShopManager.OnAssetRequested(e);
            // this.Helper.Events.GameLoop.DayEnding += static (_, _) => ShopManager.OnDayEnd();
            // this.Helper.Events.Input.ButtonPressed += (_, e) => ShopManager.OnButtonPressed(e, this.Helper.Input);

            this.ApplyPatches(new Harmony(this.ModManifest.UniqueID));

            GMCMHelper gmcmHelper = new(this.Monitor, this.Helper.Translation, this.Helper.ModRegistry, this.ModManifest);

            if (gmcmHelper.TryGetAPI())
            {
                gmcmHelper.Register(
                    reset: static () => Config = new(),
                    save: () => this.Helper.AsyncWriteConfig(this.Monitor, Config))
                .AddParagraph(I18n.ModDescription)
                .GenerateDefaultGMCM(static () => Config)
                .AddTextOption(
                    name: I18n.ShopLocation,
                    getValue: static () => Config.ShopLocation.X + ", " + Config.ShopLocation.Y,
                    setValue: static (str) => Config.ShopLocation = str.TryParseVector2(out Vector2 vec) ? vec : new Vector2(1, 7),
                    tooltip: I18n.ShopLocation_Description);
            }

            // optional APIs
            IntegrationHelper optional = new(this.Monitor, this.Helper.Translation, this.Helper.ModRegistry, LogLevel.Trace);
            if (optional.TryGetAPI("spacechase0.JsonAssets", "1.10.10", out IJsonAssetsAPI? jaAPI))
            {
                JaAPI = jaAPI;
            }
            if (optional.TryGetAPI("spacechase0.MoreGiantCrops", "1.2.0", out IMoreGiantCropsAPI? mgAPI))
            {
                MoreGiantCropsAPI = mgAPI;
            }
            if (optional.TryGetAPI("leclair.giantcroptweaks", "0.1.0", out IGiantCropTweaks? gcAPI))
            {
                GiantCropTweaksAPI = gcAPI;
            }
            if (optional.TryGetAPI("atravita.GrowableBushes", "0.0.1", out IGrowableBushesAPI? growable))
            {
                GrowableBushesAPI = growable;
            }
        }
        else
        {
            this.Monitor.Log($"Could not load spacecore's API. This is a fatal error.", LogLevel.Error);
        }
    }

    /*
    /// <inheritdoc cref="IPlayerEvents.InventoryChanged"/>
    private void OnInventoryChanged(object? sender, InventoryChangedEventArgs e)
    {
        if (!e.IsLocalPlayer || Game1.currentLocation is not GameLocation loc)
        {
            return;
        }

        foreach (Item item in e.Added)
        {
            if (item is InventoryBush bush)
            {
                bush.UpdateForNewLocation(loc);
            }
        }

        foreach (Item item in e.Removed)
        {
            if (item is InventoryBush bush)
            {
                bush.UpdateForNewLocation(loc);
            }
        }
    }
    */

    /*
    /// <inheritdoc cref="IPlayerEvents.Warped"/>
    private void OnWarped(object? sender, WarpedEventArgs e)
    {
        if (e.IsLocalPlayer && e.NewLocation is GameLocation loc)
        {
            foreach (Item? item in e.Player.Items)
            {
                if (item is InventoryBush bush)
                {
                    bush.UpdateForNewLocation(loc);
                }
            }
        }
    }
    */

    /// <summary>
    /// Applies the patches for this mod.
    /// </summary>
    /// <param name="harmony">This mod's harmony instance.</param>
    /// <remarks>Delay until GameLaunched in order to patch other mods....</remarks>
    private void ApplyPatches(Harmony harmony)
    {
        try
        {
            harmony.PatchAll(typeof(ModEntry).Assembly);
        }
        catch (Exception ex)
        {
            ModMonitor.Log(string.Format(ErrorMessageConsts.HARMONYCRASH, ex), LogLevel.Error);
        }
        harmony.Snitch(this.Monitor, harmony.Id, transpilersOnly: true);
    }

    #region migration

    /// <inheritdoc cref="IGameLoopEvents.SaveLoaded"/>
    /// <remarks>Used to load in this mod's data models.</remarks>
    private void SaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        MultiplayerHelpers.AssertMultiplayerVersions(this.Helper.Multiplayer, this.ModManifest, this.Monitor, this.Helper.Translation);

        if (Context.IsSplitScreen && Context.ScreenId != 0)
        {
            return;
        }

        this.migrator = new(this.ModManifest, this.Helper, this.Monitor);
        if (!this.migrator.CheckVersionInfo())
        {
            this.Helper.Events.GameLoop.Saved += this.WriteMigrationData;
        }
        else
        {
            this.migrator = null;
        }
    }

    /// <inheritdoc cref="IGameLoopEvents.Saved"/>
    /// <remarks>
    /// Writes migration data then detaches the migrator.
    /// </remarks>
    private void WriteMigrationData(object? sender, SavedEventArgs e)
    {
        if (this.migrator is not null)
        {
            this.migrator.SaveVersionInfo();
            this.migrator = null;
        }

        this.Helper.Events.GameLoop.Saved -= this.WriteMigrationData;
    }
    #endregion
}