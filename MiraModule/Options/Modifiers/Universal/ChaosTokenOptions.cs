using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using MiraModule.Modifiers.Universal;
using MiraModule.Options.Modifiers;
using UnityEngine;

namespace MiraModule.Options.Modifiers.Universal;

public sealed class ChaosTokenOptions : AbstractOptionGroup<ChaosTokenModifier>
{
    public override string GroupName => "Chaos Tokens";
    public override uint GroupPriority => 51;
    public override Color GroupColor => new Color32(255, 200, 50, 255);

    public override Func<bool> GroupVisible =>
        () => OptionGroupSingleton<ChaosTokenModifierOptions>.Instance.ChaosTokenAmount > 0;

    // ── Token Economy ─────────────────────────────────────────────────────
    [ModdedNumberOption("Starting Tokens", 0f, 10f, 1f)]
    public float StartingTokens { get; set; } = 2f;

    [ModdedNumberOption("Max Token Cap", 1f, 20f, 1f)]
    public float MaxTokenCap { get; set; } = 10f;

    // ── Token Gain Triggers ───────────────────────────────────────────────
    [ModdedToggleOption("Gain Token on Task")]
    public bool GainOnTask { get; set; } = true;

    [ModdedNumberOption("Tokens Per Task", 1f, 3f, 1f)]
    public float TokensPerTask { get; set; } = 1f;

    [ModdedToggleOption("Gain Token After Meeting")]
    public bool GainOnMeeting { get; set; } = true;

    [ModdedNumberOption("Tokens Per Meeting", 1f, 3f, 1f)]
    public float TokensPerMeeting { get; set; } = 1f;

    [ModdedToggleOption("Gain Token on Kill")]
    public bool GainOnKill { get; set; } = true;

    [ModdedNumberOption("Tokens Per Kill", 1f, 3f, 1f)]
    public float TokensPerKill { get; set; } = 1f;

    // ── Roll Behaviour ────────────────────────────────────────────────────
    [ModdedToggleOption("Allow Negative Effects")]
    public bool AllowNegativeEffects { get; set; } = true;

    [ModdedToggleOption("Allow Neutral Effects")]
    public bool AllowNeutralEffects { get; set; } = true;

    [ModdedToggleOption("Allow Repeat Effects")]
    public bool AllowRepeatEffects { get; set; } = true;

    // ── Effect-specific settings ──────────────────────────────────────────
    [ModdedNumberOption("Speed Multiplier Min", 1.2f, 3f, 0.1f)]
    public float SpeedMultiplierMin { get; set; } = 1.5f;

    [ModdedNumberOption("Speed Multiplier Max", 1.5f, 5f, 0.1f)]
    public float SpeedMultiplierMax { get; set; } = 3f;

    [ModdedNumberOption("Vision Multiplier", 1.2f, 5f, 0.1f)]
    public float VisionMultiplier { get; set; } = 2f;

    [ModdedNumberOption("Vision Duration (s)", 10f, 60f, 5f, MiraNumberSuffixes.Seconds)]
    public float VisionDuration { get; set; } = 30f;

    [ModdedNumberOption("Shield Duration (s)", 10f, 60f, 5f, MiraNumberSuffixes.Seconds)]
    public float ShieldDuration { get; set; } = 30f;

    [ModdedNumberOption("Invisible Duration (s)", 5f, 30f, 5f, MiraNumberSuffixes.Seconds)]
    public float InvisibleDuration { get; set; } = 15f;

    [ModdedNumberOption("Bonus Tokens (Min)", 1f, 5f, 1f)]
    public float BonusTokensMin { get; set; } = 1f;

    [ModdedNumberOption("Bonus Tokens (Max)", 2f, 10f, 1f)]
    public float BonusTokensMax { get; set; } = 4f;

    [ModdedNumberOption("Tasks Completed (Min)", 1f, 3f, 1f)]
    public float TasksMin { get; set; } = 1f;

    [ModdedNumberOption("Tasks Completed (Max)", 2f, 8f, 1f)]
    public float TasksMax { get; set; } = 3f;

    [ModdedNumberOption("Bonus Votes (Min)", 1f, 3f, 1f)]
    public float BonusVotesMin { get; set; } = 1f;

    [ModdedNumberOption("Bonus Votes (Max)", 2f, 5f, 1f)]
    public float BonusVotesMax { get; set; } = 3f;

    // ── Rarity Weights ────────────────────────────────────────────────────
    [ModdedNumberOption("Common Effect Weight", 1f, 10f, 1f)]
    public float CommonWeight { get; set; } = 6f;

    [ModdedNumberOption("Rare Effect Weight", 1f, 10f, 1f)]
    public float RareWeight { get; set; } = 3f;

    [ModdedNumberOption("Legendary Effect Weight", 1f, 10f, 1f)]
    public float LegendaryWeight { get; set; } = 1f;
}
