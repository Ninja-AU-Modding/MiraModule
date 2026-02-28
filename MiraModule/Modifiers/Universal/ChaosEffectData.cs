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
    Defense,
    Speed,
    Votes,
    MoreTokens,
    OneTimeKill,
    Tasks,
    Vision,
    Invisible,

    // Neutral
    RevealRandomPlayer,
    PositionSwap,
    RoleSwap,
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
        new() { Effect = ChaosEffect.Defense,           Name = "Defense",             Description = "You gain a temporary shield. You are protected from attacks and guesses.",                                    Category = ChaosEffectCategory.Positive, Rarity = ChaosEffectRarity.Rare,      Color = new Color32(80,  180, 255, 255) },
        new() { Effect = ChaosEffect.Speed,             Name = "Speed",               Description = "Your speed is multiplied by a random value.",                                                                Category = ChaosEffectCategory.Positive, Rarity = ChaosEffectRarity.Common,    Color = new Color32(100, 220, 100, 255) },
        new() { Effect = ChaosEffect.Votes,             Name = "Votes",               Description = "You gain a random amount of bonus votes during the next meeting.",                                           Category = ChaosEffectCategory.Positive, Rarity = ChaosEffectRarity.Rare,      Color = new Color32(255, 215, 0,   255) },
        new() { Effect = ChaosEffect.MoreTokens,        Name = "More Tokens",         Description = "You gain more tokens to gamble more.",                                                                       Category = ChaosEffectCategory.Positive, Rarity = ChaosEffectRarity.Common,    Color = new Color32(255, 200, 50,  255) },
        new() { Effect = ChaosEffect.OneTimeKill,       Name = "One Time Kill",       Description = "You gain a one-time-use kill button. There is no penalty for killing a crewmate.",                          Category = ChaosEffectCategory.Positive, Rarity = ChaosEffectRarity.Legendary, Color = new Color32(200, 50,  50,  255) },
        new() { Effect = ChaosEffect.Tasks,             Name = "Tasks",               Description = "A random amount of tasks are completed for you.",                                                            Category = ChaosEffectCategory.Positive, Rarity = ChaosEffectRarity.Common,    Color = new Color32(130, 255, 180, 255) },
        new() { Effect = ChaosEffect.Vision,            Name = "Vision",              Description = "Your vision is increased.",                                                                                  Category = ChaosEffectCategory.Positive, Rarity = ChaosEffectRarity.Common,    Color = new Color32(255, 255, 150, 255) },
        new() { Effect = ChaosEffect.Invisible,         Name = "Invisible",           Description = "You become invisible after standing still.",                                                                 Category = ChaosEffectCategory.Positive, Rarity = ChaosEffectRarity.Legendary, Color = new Color32(200, 200, 255, 255) },

        // ── Neutral ───────────────────────────────────────────────────────────
        new() { Effect = ChaosEffect.RevealRandomPlayer, Name = "Reveal Random Player", Description = "A random person gets their role revealed to everyone.",                                                   Category = ChaosEffectCategory.Neutral,  Rarity = ChaosEffectRarity.Rare,      Color = new Color32(180, 100, 255, 255) },
        new() { Effect = ChaosEffect.PositionSwap,       Name = "Position Swap",        Description = "You swap places with a random person.",                                                                   Category = ChaosEffectCategory.Neutral,  Rarity = ChaosEffectRarity.Common,    Color = new Color32(100, 200, 200, 255) },
        new() { Effect = ChaosEffect.RoleSwap,           Name = "Role Swap",            Description = "You swap roles with a random person of your alignment.",                                                  Category = ChaosEffectCategory.Neutral,  Rarity = ChaosEffectRarity.Legendary, Color = new Color32(255, 150, 50,  255) },
        new() { Effect = ChaosEffect.Revive,             Name = "Revive",               Description = "You revive a random person that died this round.",                                                        Category = ChaosEffectCategory.Neutral,  Rarity = ChaosEffectRarity.Legendary, Color = new Color32(100, 255, 100, 255) },

        // ── Negative ──────────────────────────────────────────────────────────
        new() { Effect = ChaosEffect.Death,             Name = "Death",               Description = "You die at the end of the next meeting. No meeting ability can be used on you. Any votes on you won't count.", Category = ChaosEffectCategory.Negative, Rarity = ChaosEffectRarity.Rare,   Color = new Color32(60,  60,  60,  255) },
    ];

    public static ChaosEffectInfo Get(ChaosEffect effect) =>
        All.First(e => e.Effect == effect);
}
