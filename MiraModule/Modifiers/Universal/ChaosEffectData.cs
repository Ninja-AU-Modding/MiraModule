using UnityEngine;

namespace MiraModule.Modifiers.Universal;

// ── Rarity ────────────────────────────────────────────────────────────────────

public enum ChaosEffectRarity
{
    Common,
    Rare,
    Legendary,
}

// ── Category ──────────────────────────────────────────────────────────────────

public enum ChaosEffectCategory
{
    Positive,
    Neutral,
    Negative,
}

// ── Effect IDs ────────────────────────────────────────────────────────────────

public enum ChaosEffect
{
    // Positive
    Speed,
    MoreTokens,
    OneTimeKill,
    Vision,

    // Neutral
    PositionSwap,
    Revive,

    // Negative
    Death,
}

// ── Per-effect metadata ───────────────────────────────────────────────────────

public sealed class ChaosEffectInfo
{
    public ChaosEffect         Effect      { get; init; }
    public string              Name        { get; init; } = "";
    public string              Description { get; init; } = "";
    public ChaosEffectCategory Category    { get; init; }
    public ChaosEffectRarity   Rarity      { get; init; } = ChaosEffectRarity.Common;
    public Color               Color       { get; init; } = Color.white;
}

// ── Static metadata for each effect ──────────────────────────────────────────

public static class ChaosEffectData
{
    public static readonly List<ChaosEffectInfo> All =
    [
        // ── Positive ──────────────────────────────────────────────────────────
        new() { Effect = ChaosEffect.Speed,       Name = "Speed",         Description = "Your speed is multiplied by a random value.",                                                                       Category = ChaosEffectCategory.Positive, Rarity = ChaosEffectRarity.Common,    Color = new Color32(100, 220, 100, 255) },
        new() { Effect = ChaosEffect.MoreTokens,  Name = "More Tokens",   Description = "You gain more tokens to gamble more.",                                                                              Category = ChaosEffectCategory.Positive, Rarity = ChaosEffectRarity.Common,    Color = new Color32(255, 200, 50,  255) },
        new() { Effect = ChaosEffect.OneTimeKill, Name = "One Time Kill", Description = "You gain a one-time-use kill button. There is no penalty for killing a crewmate.",                                 Category = ChaosEffectCategory.Positive, Rarity = ChaosEffectRarity.Legendary, Color = new Color32(200, 50,  50,  255) },
        new() { Effect = ChaosEffect.Vision,      Name = "Vision",        Description = "Your vision is increased.",                                                                                        Category = ChaosEffectCategory.Positive, Rarity = ChaosEffectRarity.Common,    Color = new Color32(255, 255, 150, 255) },

        // ── Neutral ───────────────────────────────────────────────────────────
        new() { Effect = ChaosEffect.PositionSwap, Name = "Position Swap", Description = "You swap places with a random person.",                                                                           Category = ChaosEffectCategory.Neutral,  Rarity = ChaosEffectRarity.Common,    Color = new Color32(100, 200, 200, 255) },
        new() { Effect = ChaosEffect.Revive,       Name = "Revive",        Description = "You revive a random person that died this round.",                                                                 Category = ChaosEffectCategory.Neutral,  Rarity = ChaosEffectRarity.Legendary, Color = new Color32(100, 255, 100, 255) },

        // ── Negative ──────────────────────────────────────────────────────────
        new() { Effect = ChaosEffect.Death, Name = "Death", Description = "You die at the end of the next meeting. No meeting ability can be used on you. Any votes on you won't count.", Category = ChaosEffectCategory.Negative, Rarity = ChaosEffectRarity.Rare, Color = new Color32(60, 60, 60, 255) },
    ];

    public static ChaosEffectInfo Get(ChaosEffect effect) =>
        All.First(e => e.Effect == effect);
}
