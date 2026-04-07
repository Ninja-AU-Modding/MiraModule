using System;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraModule.Modifiers.ChaosTokens;
using UnityEngine;

namespace MiraModule.Options.Modifiers;

public sealed class ChaosTokensOptions : AbstractOptionGroup<ChaosTokenModifier>
{
    public override string GroupName => "Chaos Tokens";
    public override Color GroupColor => MiraModuleColors.ChaosTokens;
    public override uint GroupPriority => 100;

    public override Func<bool> GroupVisible =>
        () => OptionGroupSingleton<ChaosTokensModifierOptions>.Instance.ChaosTokenAmount > 0;

    // Basic Settings
    [ModdedNumberOption("Start Roll Cooldown", 0f, 60f, 2.5f, MiraNumberSuffixes.Seconds, "0.0")]
    public float InitialRollCooldown { get; set; } = 10f;

    [ModdedNumberOption("Roll Cooldown", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds, "0.0")]
    public float RollCooldown { get; set; } = 5f;

    [ModdedToggleOption("Hand Tokens First Round")]
    public bool TokensEnabledFirstRound { get; set; } = true;

    [ModdedToggleOption("Enable Token Handing Priority")]
    public bool TokenHandingPriority { get; set; } = true;

    [ModdedNumberOption("Handed Tokens Min", 1f, 30f, 1f)]
    public float TokensMin { get; set; } = 5f;

    [ModdedNumberOption("Handed Tokens Max", 0f, 30f, 1f, MiraNumberSuffixes.None, null, true)]
    public float TokensMax { get; set; } = 10f;

    [ModdedNumberOption("Initial Double Token Chance", 0f, 100f, 10f, MiraNumberSuffixes.Percent)]
    public float InitialDoubleTokenChance { get; set; } = 10f;

    [ModdedNumberOption("Double Token Chance Increase Per Round", 5f, 50f, 5f, MiraNumberSuffixes.Percent)]
    public float DoubleTokenIncrease { get; set; } = 10f;

    // Balance Settings
    [ModdedToggleOption("Disable Death Token")]
    public bool DeathDisabled { get; set; } = false;

    [ModdedToggleOption("Disable Revival")]
    public bool ReviveDisabled { get; set; } = false;

    [ModdedNumberOption("Max Role Reveals", 0f, 15f, 1f, MiraNumberSuffixes.None, null, true)]
    public float MaxRoleReveals { get; set; } = 0f;

    // Advanced - Effect Chances Toggle
    [ModdedToggleOption("Show Advanced Effect Chances")]
    public bool ShowAdvancedChances { get; set; } = false;

    // Positive Effects - Visible when ShowAdvancedChances is true
    public ModdedNumberOption DefenseChance { get; } =
        new("Defense Chance", 100f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensOptions>.Instance.ShowAdvancedChances
        };

    public ModdedNumberOption SpeedChance { get; } =
        new("Speed Chance", 100f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensOptions>.Instance.ShowAdvancedChances
        };

    public ModdedNumberOption VotesChance { get; } =
        new("Votes Chance", 100f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensOptions>.Instance.ShowAdvancedChances
        };

    public ModdedNumberOption MoreTokensChance { get; } =
        new("More Tokens Chance", 100f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensOptions>.Instance.ShowAdvancedChances
        };

    public ModdedNumberOption TransparentChance { get; } =
        new("Transparent Chance", 100f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensOptions>.Instance.ShowAdvancedChances
        };

    public ModdedNumberOption LowerCooldownChance { get; } =
        new("Lower Cooldown Chance", 100f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensOptions>.Instance.ShowAdvancedChances
        };

    public ModdedNumberOption MediumChance { get; } =
        new("Medium Chance", 100f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensOptions>.Instance.ShowAdvancedChances
        };

    public ModdedNumberOption KillButtonChance { get; } =
        new("Kill Button Chance", 100f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensOptions>.Instance.ShowAdvancedChances
        };

    public ModdedNumberOption TasksChance { get; } =
        new("Tasks Chance", 100f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensOptions>.Instance.ShowAdvancedChances
        };

    public ModdedNumberOption VisionChance { get; } =
        new("Vision Chance", 100f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensOptions>.Instance.ShowAdvancedChances
        };

    public ModdedNumberOption InvisibleChance { get; } =
        new("Invisible Chance", 100f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensOptions>.Instance.ShowAdvancedChances
        };

    public ModdedNumberOption AssassinChance { get; } =
        new("Assassin Chance", 100f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensOptions>.Instance.ShowAdvancedChances
        };

    // Negative Effects
    public ModdedNumberOption RevealSelfChance { get; } =
        new("Reveal Self Chance", 100f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensOptions>.Instance.ShowAdvancedChances
        };

    public ModdedNumberOption DeathChance { get; } =
        new("Death Chance", 100f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensOptions>.Instance.ShowAdvancedChances
        };

    public ModdedNumberOption BlackmailedChance { get; } =
        new("Blackmailed Chance", 100f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensOptions>.Instance.ShowAdvancedChances
        };

    public ModdedNumberOption DrunkChance { get; } =
        new("Drunk Chance", 100f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensOptions>.Instance.ShowAdvancedChances
        };

    public ModdedNumberOption FakeRevealChance { get; } =
        new("Fake Reveal Chance", 100f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensOptions>.Instance.ShowAdvancedChances
        };

    public ModdedNumberOption HyperactiveChance { get; } =
        new("Hyperactive Chance", 100f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensOptions>.Instance.ShowAdvancedChances
        };

    public ModdedNumberOption ColorblindChance { get; } =
        new("Colorblind Chance", 100f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensOptions>.Instance.ShowAdvancedChances
        };

    // Neutral Effects
    public ModdedNumberOption RevealRandomChance { get; } =
        new("Reveal Random Chance", 100f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensOptions>.Instance.ShowAdvancedChances
        };

    public ModdedNumberOption PositionSwapChance { get; } =
        new("Position Swap Chance", 100f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensOptions>.Instance.ShowAdvancedChances
        };

    public ModdedNumberOption RoleSwapChance { get; } =
        new("Role Swap Chance", 100f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensOptions>.Instance.ShowAdvancedChances
        };

    public ModdedNumberOption ReviveChance { get; } =
        new("Revive Chance", 100f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensOptions>.Instance.ShowAdvancedChances
        };

    public ModdedNumberOption RandomModifierChance { get; } =
        new("Random Modifier Chance", 100f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensOptions>.Instance.ShowAdvancedChances
        };

    public ModdedNumberOption NoSkipChance { get; } =
        new("No Skip Chance", 100f, 0f, 100f, 5f, MiraNumberSuffixes.Percent)
        {
            Visible = () => OptionGroupSingleton<ChaosTokensOptions>.Instance.ShowAdvancedChances
        };
}
