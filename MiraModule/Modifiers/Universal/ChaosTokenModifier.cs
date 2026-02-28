using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using MiraModule.Assets;
using MiraModule.Buttons.Universal;
using MiraModule.Options.Modifiers;
using MiraModule.Options.Modifiers.Universal;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Modifiers.Game;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace MiraModule.Modifiers.Universal;

// ── Effect enums & data ───────────────────────────────────────────────────────

public enum ChaosEffect
{
    // Speed
    SpeedBoost,
    SpeedSlow,
    DoubleSpeedBurst,
    SuperSpeed,

    // Vision
    VisionBoost,
    VisionReduced,
    PerfectVision,
    BlindVision,

    // Cooldown
    CooldownReduction,
    CooldownIncrease,

    // Misc
    RandomTeleport,
    Invincibility,
}

public enum ChaosEffectRarity  { Common, Rare, Legendary }
public enum ChaosEffectDuration { Timed, UntilMeeting, Permanent }

public sealed class ChaosEffectInfo
{
    public ChaosEffect Effect     { get; init; }
    public string      Name       { get; init; } = "";
    public string      Description{ get; init; } = "";
    public bool        IsNegative { get; init; }
    public ChaosEffectRarity   Rarity   { get; init; } = ChaosEffectRarity.Common;
    public ChaosEffectDuration Duration { get; init; } = ChaosEffectDuration.Timed;
    public Color               Color    { get; init; } = Color.white;
}

public static class ChaosEffectData
{
    public static readonly List<ChaosEffectInfo> All =
    [
        new() { Effect = ChaosEffect.SpeedBoost,       Name = "Speed Boost",       Description = "Move 50% faster for a while.",             IsNegative = false, Rarity = ChaosEffectRarity.Common,    Duration = ChaosEffectDuration.Timed,       Color = new Color32(100, 220, 100, 255) },
        new() { Effect = ChaosEffect.SpeedSlow,        Name = "Lead Boots",         Description = "Move 40% slower for a while.",             IsNegative = true,  Rarity = ChaosEffectRarity.Common,    Duration = ChaosEffectDuration.Timed,       Color = new Color32(150, 100, 60,  255) },
        new() { Effect = ChaosEffect.DoubleSpeedBurst, Name = "Overdrive",          Description = "Double speed until next meeting!",         IsNegative = false, Rarity = ChaosEffectRarity.Rare,      Duration = ChaosEffectDuration.UntilMeeting,Color = new Color32(80,  200, 255, 255) },
        new() { Effect = ChaosEffect.SuperSpeed,       Name = "Chaos Sprint",       Description = "3× speed — good luck controlling it!",    IsNegative = false, Rarity = ChaosEffectRarity.Legendary, Duration = ChaosEffectDuration.Timed,       Color = new Color32(255, 200, 50,  255) },
        new() { Effect = ChaosEffect.VisionBoost,      Name = "Eagle Eyes",         Description = "Vision boosted by 75% for a while.",       IsNegative = false, Rarity = ChaosEffectRarity.Common,    Duration = ChaosEffectDuration.Timed,       Color = new Color32(255, 255, 120, 255) },
        new() { Effect = ChaosEffect.VisionReduced,    Name = "Tunnel Vision",      Description = "Vision cut to 40% for a while.",           IsNegative = true,  Rarity = ChaosEffectRarity.Common,    Duration = ChaosEffectDuration.Timed,       Color = new Color32(80,  80,  80,  255) },
        new() { Effect = ChaosEffect.PerfectVision,    Name = "Omniscient",         Description = "Full map vision until next meeting!",      IsNegative = false, Rarity = ChaosEffectRarity.Legendary, Duration = ChaosEffectDuration.UntilMeeting,Color = new Color32(255, 240, 80,  255) },
        new() { Effect = ChaosEffect.BlindVision,      Name = "Blindfolded",        Description = "Almost no vision for a while.",            IsNegative = true,  Rarity = ChaosEffectRarity.Rare,      Duration = ChaosEffectDuration.Timed,       Color = new Color32(30,  30,  30,  255) },
        new() { Effect = ChaosEffect.CooldownReduction,Name = "Quick Hands",        Description = "All ability cooldowns -10s permanently.",  IsNegative = false, Rarity = ChaosEffectRarity.Rare,      Duration = ChaosEffectDuration.Permanent,   Color = new Color32(100, 255, 180, 255) },
        new() { Effect = ChaosEffect.CooldownIncrease, Name = "Fumbling",           Description = "All ability cooldowns +10s permanently.",  IsNegative = true,  Rarity = ChaosEffectRarity.Common,    Duration = ChaosEffectDuration.Permanent,   Color = new Color32(200, 80,  80,  255) },
        new() { Effect = ChaosEffect.RandomTeleport,   Name = "Warp",               Description = "Teleported to a random location!",         IsNegative = false, Rarity = ChaosEffectRarity.Rare,      Duration = ChaosEffectDuration.Permanent,   Color = new Color32(180, 100, 255, 255) },
        new() { Effect = ChaosEffect.Invincibility,    Name = "Invincible",         Description = "Cannot be killed until next meeting!",     IsNegative = false, Rarity = ChaosEffectRarity.Legendary, Duration = ChaosEffectDuration.UntilMeeting,Color = new Color32(255, 215, 0,   255) },
    ];

    public static ChaosEffectInfo Get(ChaosEffect effect) =>
        All.First(e => e.Effect == effect);
}

// ── The modifier itself ───────────────────────────────────────────────────────

/// <summary>
/// Chaos Tokens modifier. Grants a dice button that rolls a random effect on use.
/// Tokens are earned via tasks, meetings, and kills (all configurable).
/// </summary>
public sealed class ChaosTokenModifier : UniversalGameModifier, IVisualAppearance
{
    // ── Static colour ────────────────────────────────────────────────────────
    public static readonly Color ModColor = new Color32(255, 200, 50, 255);

    // ── Identity ─────────────────────────────────────────────────────────────
    public override string ModifierName  => "Chaos Tokens";
    public override string LocaleKey     => "ChaosTokens";
    public override ModifierFaction FactionType => ModifierFaction.UniversalUtility;
    public override Color FreeplayFileColor     => ModColor;
    public override LoadableAsset<Sprite>? ModifierIcon => ModifierIcons.ChaosToken;

    public override string GetDescription() =>
        $"You have {Tokens} Chaos Token{(Tokens == 1 ? "" : "s")}. Press the dice button to roll a random effect!";

    // ── Spawn config ──────────────────────────────────────────────────────────
    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<ChaosTokenModifierOptions>.Instance.ChaosTokenChance;
    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<ChaosTokenModifierOptions>.Instance.ChaosTokenAmount;

    // ── Options shortcut ─────────────────────────────────────────────────────
    private static ChaosTokenOptions Opts =>
        OptionGroupSingleton<ChaosTokenOptions>.Instance;

    // ── Token state ───────────────────────────────────────────────────────────
    public int Tokens { get; private set; }

    // ── Active effect tracking ────────────────────────────────────────────────
    public ChaosEffect? ActiveEffect    { get; private set; }
    public float EffectTimeRemaining    { get; private set; } = -1f;

    private readonly List<ChaosEffect>   _permanentEffects = [];
    private readonly List<ChaosEffect>   _meetingEffects   = [];
    private readonly HashSet<ChaosEffect> _usedEffects     = [];

    // ── Stat multipliers (local to this player) ───────────────────────────────
    public float CooldownAdjustment { get; private set; } = 0f;
    public float VisionMultiplier   { get; private set; } = 1f;
    public float SpeedMultiplier    { get; private set; } = 1f;
    public bool  IsInvincible       { get; private set; }

    // ── IVisualAppearance (speed sync via TOU's appearance system) ────────────
    private bool _appearanceDirty;

    public VisualAppearance GetVisualAppearance()
    {
        var appearance = Player.GetDefaultAppearance();
        appearance.Speed = SpeedMultiplier;
        return appearance;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private ChaosTokenButton? FindButton() =>
        CustomButtonManager.Buttons.OfType<ChaosTokenButton>().FirstOrDefault();

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    public override void OnActivate()
    {
        Tokens = (int)Opts.StartingTokens;
        _permanentEffects.Clear();
        _meetingEffects.Clear();
        _usedEffects.Clear();
        ActiveEffect        = null;
        EffectTimeRemaining = -1f;
        CooldownAdjustment  = 0f;
        VisionMultiplier    = 1f;
        SpeedMultiplier     = 1f;
        IsInvincible        = false;
        _appearanceDirty    = false;
    }

    public override void OnDeactivate()
    {
        if (_appearanceDirty && Player != null)
            Player.ResetAppearance(fullReset: true);

        // Restore base vision if we changed it
        RestoreVision();
    }

    public override void OnMeetingStart()
    {
        // Expire until-meeting effects
        foreach (var effect in _meetingEffects)
            UnapplyEffect(effect);
        _meetingEffects.Clear();

        // Expire active timed effect
        if (ActiveEffect.HasValue &&
            ChaosEffectData.Get(ActiveEffect.Value).Duration == ChaosEffectDuration.Timed)
        {
            UnapplyEffect(ActiveEffect.Value);
            ActiveEffect        = null;
            EffectTimeRemaining = -1f;
        }

        // Meeting token gain (local only — every client independently manages their own tokens)
        if (Player.AmOwner && Opts.GainOnMeeting)
            GainTokens((int)Opts.TokensPerMeeting);
    }

    public override void FixedUpdate()
    {
        // Tick down timed effect
        if (ActiveEffect.HasValue && EffectTimeRemaining > 0f)
        {
            EffectTimeRemaining -= Time.fixedDeltaTime;
            if (EffectTimeRemaining <= 0f)
            {
                UnapplyEffect(ActiveEffect.Value);
                ActiveEffect        = null;
                EffectTimeRemaining = -1f;
            }
        }

        // Apply vision multiplier each frame by patching the game options value
        // (Among Us reads CrewLightMod every frame to set the light radius)
        if (Player.AmOwner && VisionMultiplier != 1f && !MeetingHud.Instance)
        {
            var opts = GameOptionsManager.Instance.currentNormalGameOptions;
            opts.CrewLightMod = _baseCrewVision * VisionMultiplier;
        }
    }

    // Cache the map's base vision so we can restore it later
    private float _baseCrewVision = -1f;

    private void CaptureBaseVision()
    {
        if (_baseCrewVision < 0f)
            _baseCrewVision = GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod;
    }

    private void RestoreVision()
    {
        if (_baseCrewVision >= 0f && Player.AmOwner)
        {
            GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod = _baseCrewVision;
            _baseCrewVision = -1f;
        }
    }

    // ── Token management ──────────────────────────────────────────────────────
    public void GainTokens(int amount)
    {
        var cap = (int)Opts.MaxTokenCap;
        Tokens = Math.Min(Tokens + amount, cap);
        FindButton()?.SetUses(Tokens);
    }

    public bool TrySpendToken()
    {
        if (Tokens <= 0) return false;
        Tokens--;
        FindButton()?.SetUses(Tokens);
        return true;
    }

    // ── Effect rolling ─────────────────────────────────────────────────────────
    public void RollAndApply()
    {
        var effect = PickEffect();
        if (effect == null) return;
        RpcRollEffect(Player, (int)effect.Value);
    }

    private ChaosEffect? PickEffect()
    {
        var candidates = ChaosEffectData.All.AsEnumerable();

        if (!Opts.AllowNegativeEffects)
            candidates = candidates.Where(e => !e.IsNegative);

        if (!Opts.AllowRepeatEffects)
            candidates = candidates.Where(e => !_usedEffects.Contains(e.Effect));

        if (!Opts.EffectsStack && ActiveEffect.HasValue)
            candidates = candidates.Where(e => e.Effect != ActiveEffect.Value);

        var pool = candidates.ToList();
        if (pool.Count == 0) return null;

        float totalWeight = pool.Sum(e => GetWeight(e.Rarity));
        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var e in pool)
        {
            cumulative += GetWeight(e.Rarity);
            if (roll <= cumulative)
                return e.Effect;
        }

        return pool[^1].Effect;
    }

    private float GetWeight(ChaosEffectRarity rarity) => rarity switch
    {
        ChaosEffectRarity.Common    => Opts.CommonWeight,
        ChaosEffectRarity.Rare      => Opts.RareWeight,
        ChaosEffectRarity.Legendary => Opts.LegendaryWeight,
        _                           => 1f,
    };

    // ── RPC ───────────────────────────────────────────────────────────────────
    [MethodRpc((uint)MiraModuleRpc.ChaosTokenRoll)]
    public static void RpcRollEffect(PlayerControl player, int effectId)
    {
        if (!player.TryGetModifier<ChaosTokenModifier>(out var modifier)) return;
        modifier.ApplyEffect((ChaosEffect)effectId);
    }

    // ── Apply / Unapply ───────────────────────────────────────────────────────
    private void ApplyEffect(ChaosEffect effect)
    {
        var info = ChaosEffectData.Get(effect);
        _usedEffects.Add(effect);

        // Clear existing timed effect if non-stacking
        if (!Opts.EffectsStack && ActiveEffect.HasValue &&
            info.Duration == ChaosEffectDuration.Timed)
        {
            UnapplyEffect(ActiveEffect.Value);
        }

        // Register duration bucket
        switch (info.Duration)
        {
            case ChaosEffectDuration.Timed:
                ActiveEffect        = effect;
                EffectTimeRemaining = Opts.TimedEffectDuration;
                break;
            case ChaosEffectDuration.UntilMeeting:
                if (!_meetingEffects.Contains(effect)) _meetingEffects.Add(effect);
                break;
            case ChaosEffectDuration.Permanent:
                if (!_permanentEffects.Contains(effect)) _permanentEffects.Add(effect);
                break;
        }

        // Stat changes
        switch (effect)
        {
            case ChaosEffect.SpeedBoost:        SpeedMultiplier *= 1.5f;  RefreshSpeed(); break;
            case ChaosEffect.SpeedSlow:         SpeedMultiplier *= 0.6f;  RefreshSpeed(); break;
            case ChaosEffect.DoubleSpeedBurst:  SpeedMultiplier *= 2.0f;  RefreshSpeed(); break;
            case ChaosEffect.SuperSpeed:        SpeedMultiplier *= 3.0f;  RefreshSpeed(); break;

            case ChaosEffect.VisionBoost:
                CaptureBaseVision(); VisionMultiplier *= 1.75f; break;
            case ChaosEffect.VisionReduced:
                CaptureBaseVision(); VisionMultiplier *= 0.4f;  break;
            case ChaosEffect.PerfectVision:
                CaptureBaseVision(); VisionMultiplier  = 10f;   break;
            case ChaosEffect.BlindVision:
                CaptureBaseVision(); VisionMultiplier *= 0.05f; break;

            case ChaosEffect.CooldownReduction: CooldownAdjustment -= 10f; break;
            case ChaosEffect.CooldownIncrease:  CooldownAdjustment += 10f; break;

            case ChaosEffect.RandomTeleport:
                if (Player.AmOwner) Coroutines.Start(CoTeleport()); break;

            case ChaosEffect.Invincibility: IsInvincible = true; break;
        }

        // Notify local player
        if (Player.AmOwner)
        {
            var colorHex = ColorUtility.ToHtmlStringRGB(info.Color);
            MiscUtils.AddFakeChat(
                Player.Data,
                $"<color=#{colorHex}>🎲 Chaos Tokens</color>",
                $"<b>{info.Name}</b> — {info.Description}",
                showHeadsup: true);

            Coroutines.Start(MiscUtils.CoFlash(info.Color, 0.3f, 0.25f));
        }
    }

    private void UnapplyEffect(ChaosEffect effect)
    {
        switch (effect)
        {
            case ChaosEffect.SpeedBoost:        SpeedMultiplier /= 1.5f;  RefreshSpeed(); break;
            case ChaosEffect.SpeedSlow:         SpeedMultiplier /= 0.6f;  RefreshSpeed(); break;
            case ChaosEffect.DoubleSpeedBurst:  SpeedMultiplier /= 2.0f;  RefreshSpeed(); break;
            case ChaosEffect.SuperSpeed:        SpeedMultiplier /= 3.0f;  RefreshSpeed(); break;

            case ChaosEffect.VisionBoost:    VisionMultiplier /= 1.75f; CheckRestoreVision(); break;
            case ChaosEffect.VisionReduced:  VisionMultiplier /= 0.4f;  CheckRestoreVision(); break;
            case ChaosEffect.PerfectVision:  VisionMultiplier  = 1f;    CheckRestoreVision(); break;
            case ChaosEffect.BlindVision:    VisionMultiplier /= 0.05f; CheckRestoreVision(); break;

            case ChaosEffect.CooldownReduction: CooldownAdjustment += 10f; break;
            case ChaosEffect.CooldownIncrease:  CooldownAdjustment -= 10f; break;

            case ChaosEffect.Invincibility: IsInvincible = false; break;
        }
    }

    private void CheckRestoreVision()
    {
        // If multiplier is back to neutral, restore the base value immediately
        if (Math.Abs(VisionMultiplier - 1f) < 0.001f)
            RestoreVision();
    }

    private void RefreshSpeed()
    {
        _appearanceDirty = true;
        if (Player != null && !Player.HasDied())
            Player.RawSetAppearance(this);
    }

    private System.Collections.IEnumerator CoTeleport()
    {
        yield return new WaitForSeconds(0.1f);
        if (!ShipStatus.Instance || Player.HasDied()) yield break;

        var allSpawns = ShipStatus.Instance
            .GetComponentsInChildren<Transform>()
            .Where(t => t.name.Contains("Spawn", System.StringComparison.OrdinalIgnoreCase))
            .Select(t => t.position)
            .ToList();

        Vector3 dest;
        if (allSpawns.Count > 0)
        {
            dest = allSpawns[UnityEngine.Random.Range(0, allSpawns.Count)];
        }
        else
        {
            var center = (Vector3)ShipStatus.Instance.InitialSpawnCenter;
            dest = center + new Vector3(
                UnityEngine.Random.Range(-10f, 10f),
                UnityEngine.Random.Range(-6f,  6f),
                0f);
        }

        Player.NetTransform.SnapTo(dest);
    }
}
