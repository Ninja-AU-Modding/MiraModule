using UnityEngine;

namespace MiraModule.Modifiers.Universal;

// ── Rarity & Duration Categories ─────────────────────────────────────────────

public enum ChaosEffectRarity
{
    Common,
    Rare,
    Legendary,
}

public enum ChaosEffectDuration
{
    /// <summary>Lasts for a fixed number of seconds (ChaosTokenOptions.TimedEffectDuration).</summary>
    Timed,
    /// <summary>Lasts until the next meeting starts.</summary>
    UntilMeeting,
    /// <summary>Permanent for the rest of the game.</summary>
    Permanent,
}

// ── Effect IDs ────────────────────────────────────────────────────────────────

public enum ChaosEffect
{
    // Common Buffs
    SpeedBoost,
    VisionBoost,
    CooldownReduction,

    // Common Debuffs
    SpeedSlow,
    VisionReduced,
    CooldownIncrease,

    // Rare Buffs
    DoubleSpeedBurst,
    PerfectVision,

    // Rare Debuffs
    BlindVision,
    RandomTeleport,

    // Legendary
    Invincibility,
    SuperSpeed,
}

// ── Static metadata for each effect ──────────────────────────────────────────

public static class ChaosEffectData
{
    public record EffectInfo(
        ChaosEffect Effect,
        string Name,
        string Description,
        ChaosEffectRarity Rarity,
        ChaosEffectDuration Duration,
        bool IsNegative,
        Color Color);

    public static readonly EffectInfo[] All =
    [
        // ── Common Buffs ──────────────────────────────────────────────────
        new(ChaosEffect.SpeedBoost,
            "Speed Boost",
            "Your movement speed is increased.",
            ChaosEffectRarity.Common, ChaosEffectDuration.Timed,
            IsNegative: false,
            new Color32(100, 220, 100, 255)),

        new(ChaosEffect.VisionBoost,
            "Eagle Eye",
            "Your vision range is expanded.",
            ChaosEffectRarity.Common, ChaosEffectDuration.Timed,
            IsNegative: false,
            new Color32(180, 220, 255, 255)),

        new(ChaosEffect.CooldownReduction,
            "Quick Hands",
            "Your ability cooldowns are reduced by 10 seconds.",
            ChaosEffectRarity.Common, ChaosEffectDuration.UntilMeeting,
            IsNegative: false,
            new Color32(130, 255, 200, 255)),

        // ── Common Debuffs ────────────────────────────────────────────────
        new(ChaosEffect.SpeedSlow,
            "Lead Feet",
            "Your movement speed is reduced.",
            ChaosEffectRarity.Common, ChaosEffectDuration.Timed,
            IsNegative: true,
            new Color32(180, 100, 100, 255)),

        new(ChaosEffect.VisionReduced,
            "Blurry Eyes",
            "Your vision range is reduced.",
            ChaosEffectRarity.Common, ChaosEffectDuration.Timed,
            IsNegative: true,
            new Color32(150, 80, 80, 255)),

        new(ChaosEffect.CooldownIncrease,
            "Slow Hands",
            "Your ability cooldowns are increased by 10 seconds.",
            ChaosEffectRarity.Common, ChaosEffectDuration.UntilMeeting,
            IsNegative: true,
            new Color32(220, 80, 80, 255)),

        // ── Rare Buffs ────────────────────────────────────────────────────
        new(ChaosEffect.DoubleSpeedBurst,
            "Hyperdrive",
            "You briefly move at double speed.",
            ChaosEffectRarity.Rare, ChaosEffectDuration.Timed,
            IsNegative: false,
            new Color32(255, 200, 50, 255)),

        new(ChaosEffect.PerfectVision,
            "All-Seeing",
            "You can see the entire map until next meeting.",
            ChaosEffectRarity.Rare, ChaosEffectDuration.UntilMeeting,
            IsNegative: false,
            new Color32(255, 255, 150, 255)),

        // ── Rare Debuffs ──────────────────────────────────────────────────
        new(ChaosEffect.BlindVision,
            "Tunnel Vision",
            "Your vision is nearly zero.",
            ChaosEffectRarity.Rare, ChaosEffectDuration.Timed,
            IsNegative: true,
            new Color32(60, 60, 60, 255)),

        new(ChaosEffect.RandomTeleport,
            "Chaos Warp",
            "You are teleported to a random location on the map.",
            ChaosEffectRarity.Rare, ChaosEffectDuration.Permanent,
            IsNegative: false,   // Chaotic — neutral framing
            new Color32(180, 80, 255, 255)),

        // ── Legendary ─────────────────────────────────────────────────────
        new(ChaosEffect.Invincibility,
            "Invincible",
            "You cannot be killed until the next meeting.",
            ChaosEffectRarity.Legendary, ChaosEffectDuration.UntilMeeting,
            IsNegative: false,
            new Color32(255, 215, 0, 255)),

        new(ChaosEffect.SuperSpeed,
            "Sonic",
            "You move at extreme speed until the next meeting.",
            ChaosEffectRarity.Legendary, ChaosEffectDuration.UntilMeeting,
            IsNegative: false,
            new Color32(100, 255, 255, 255)),
    ];

    public static EffectInfo Get(ChaosEffect effect) =>
        System.Array.Find(All, e => e.Effect == effect)!;
}
