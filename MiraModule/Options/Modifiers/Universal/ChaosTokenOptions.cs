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

    [ModdedToggleOption("Allow Repeat Effects")]
    public bool AllowRepeatEffects { get; set; } = true;

    [ModdedToggleOption("Effects Stack")]
    public bool EffectsStack { get; set; } = false;

    // ── Effect Durations ──────────────────────────────────────────────────
    [ModdedNumberOption("Timed Effect Duration", 5f, 60f, 5f, MiraNumberSuffixes.Seconds)]
    public float TimedEffectDuration { get; set; } = 20f;

    // ── Rarity Weights ────────────────────────────────────────────────────
    [ModdedNumberOption("Common Effect Weight", 1f, 10f, 1f)]
    public float CommonWeight { get; set; } = 6f;

    [ModdedNumberOption("Rare Effect Weight", 1f, 10f, 1f)]
    public float RareWeight { get; set; } = 3f;

    [ModdedNumberOption("Legendary Effect Weight", 1f, 10f, 1f)]
    public float LegendaryWeight { get; set; } = 1f;
}
