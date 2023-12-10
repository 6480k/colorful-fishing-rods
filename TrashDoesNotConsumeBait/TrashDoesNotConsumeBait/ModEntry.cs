﻿namespace TrashDoesNotConsumeBait;

using System.Reflection;

using AtraCore.Framework.Internal;

using AtraShared.ConstantsAndEnums;
using AtraShared.Integrations;
using AtraShared.MigrationManager;
using AtraShared.Utils.Extensions;
using HarmonyLib;
using StardewModdingAPI.Events;

using StardewValley.Menus;
using StardewValley.Tools;

using AtraUtils = AtraShared.Utils.Utils;

/// <inheritdoc/>
internal sealed class ModEntry : BaseMod<ModEntry>
{
    private MigrationManager? migrator;

    /// <summary>
    /// Gets the config instance for this mod.
    /// </summary>
    internal static ModConfig Config { get; private set; } = null!;

    /// <summary>
    /// Gets the game content helper for this mod.
    /// </summary>
    internal static IGameContentHelper GameContentHelper { get; private set; } = null!;

    /// <inheritdoc/>
    public override void Entry(IModHelper helper)
    {
        base.Entry(helper);
        GameContentHelper = helper.GameContent;
        I18n.Init(helper.Translation);
        AssetEditor.Initialize(helper.GameContent);
        Config = AtraUtils.GetConfigOrDefault<ModConfig>(helper, this.Monitor);

        helper.Events.GameLoop.GameLaunched += this.SetUpConfig;
        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;

        helper.Events.Content.AssetRequested += static (_, e) => AssetEditor.EditAssets(e);

        this.ApplyPatches(new Harmony(this.ModManifest.UniqueID));
    }

    /// <summary>
    /// Sets up the GMCM for this mod.
    /// </summary>
    /// <param name="sender">SMAPI.</param>
    /// <param name="e">event args.</param>
    private void SetUpConfig(object? sender, GameLaunchedEventArgs e)
    {
        GMCMHelper helper = new(this.Monitor, this.Helper.Translation, this.Helper.ModRegistry, this.ModManifest);
        if (!helper.TryGetAPI())
        {
            return;
        }
        helper.Register(
            reset: static () => Config = new(),
            save: () =>
            {
                this.Helper.AsyncWriteConfig(this.Monitor, Config);
                AssetEditor.Invalidate();
            });
        foreach (PropertyInfo property in typeof(ModConfig).GetProperties())
        {
            if (property.PropertyType == typeof(bool))
            {
                helper.AddBoolOption(
                    property: property,
                    getConfig: static () => Config);
            }
        }
        helper.AddPageHere("CheatyStuff", I18n.CheatyStuffTitle);
        foreach (PropertyInfo property in typeof(ModConfig).GetProperties())
        {
            if (property.PropertyType == typeof(float))
            {
                helper.AddFloatOption(
                    property: property,
                    getConfig: static () => Config);
            }
        }
    }

    /// <summary>
    /// Applies the patches for this mod.
    /// </summary>
    /// <param name="harmony">This mod's harmony instance.</param>
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
        harmony.Snitch(this.Monitor, this.ModManifest.UniqueID, transpilersOnly: true);
    }

    /// <summary>
    /// Sets up the migrator on save loaded.
    /// </summary>
    /// <param name="sender">SMAPI.</param>
    /// <param name="e">Save loaded event arguments.</param>
    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
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

    /// <summary>
    /// Writes migration data then detaches the migrator.
    /// </summary>
    /// <param name="sender">Smapi thing.</param>
    /// <param name="e">Arguments for just-before-saving.</param>
    private void WriteMigrationData(object? sender, SavedEventArgs e)
    {
        if (this.migrator is not null)
        {
            this.migrator.SaveVersionInfo();
            this.migrator = null;
        }
        this.Helper.Events.GameLoop.Saved -= this.WriteMigrationData;
    }
}