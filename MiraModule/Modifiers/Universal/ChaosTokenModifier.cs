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

/// <summary>
/// Chaos Tokens modifier. Grants a dice button that rolls a random effect on use.
/// Effects never stack — each roll replaces any previous active effect.
/// The same effect can never roll twice in a row.
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

    // ── Spawn config ─────────────────────────────────────────────────────────
    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<ChaosTokenModifierOptions>.Instance.ChaosTokenChance;
    public override int GetAmountPerGame() =>
        (int)OptionGroupSingleton<ChaosTokenModifierOptions>.Instance.ChaosTokenAmount;

    // ── Options shortcut ─────────────────────────────────────────────────────
    private static ChaosTokenOptions Opts =>
        OptionGroupSingleton<ChaosTokenOptions>.Instance;

    // ── Token state ───────────────────────────────────────────────────────────
    public int Tokens { get; private set; }

    // ── Active timed effect tracking ─────────────────────────────────────────
    public ChaosEffect? ActiveEffect        { get; private set; }
    public float        EffectTimeRemaining { get; private set; } = -1f;

    // ── Per-effect state ──────────────────────────────────────────────────────
    private float _speedMultiplier  = 1f;
    private bool  _speedDirty;

    private float _baseCrewVision   = -1f;
    private float _visionMultiplier = 1f;

    public bool HasOneTimeKill  { get; private set; }
    public bool OneTimeKillUsed { get; private set; }

    public bool HasDeathSentence { get; private set; }

    // ── Roll history ──────────────────────────────────────────────────────────
    private readonly HashSet<ChaosEffect> _usedEffects = [];
    private ChaosEffect? _lastRolledEffect;

    // ── IVisualAppearance ─────────────────────────────────────────────────────
    public VisualAppearance GetVisualAppearance()
    {
        var appearance = Player.GetDefaultAppearance();
        appearance.Speed = _speedMultiplier;
        return appearance;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static ChaosTokenButton? FindButton() =>
        CustomButtonManager.Buttons.OfType<ChaosTokenButton>().FirstOrDefault();

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    public override void OnActivate()
    {
        Tokens              = (int)Opts.StartingTokens;
        ActiveEffect        = null;
        EffectTimeRemaining = -1f;
        _speedMultiplier    = 1f;
        _speedDirty         = false;
        _baseCrewVision     = -1f;
        _visionMultiplier   = 1f;
        HasOneTimeKill      = false;
        OneTimeKillUsed     = false;
        HasDeathSentence    = false;
        _usedEffects.Clear();
        _lastRolledEffect   = null;
    }

    public override void OnDeactivate()
    {
        if (_speedDirty && Player != null)
            Player.ResetAppearance(fullReset: true);
        RestoreVision();
    }

    public override void OnMeetingStart()
    {
        ClearActiveEffect();

        if (HasDeathSentence && Player.AmOwner)
            Coroutines.Start(CoDeathSentenceResolve());

        if (Player.AmOwner && Opts.GainOnMeeting)
            GainTokens((int)Opts.TokensPerMeeting);
    }

    public override void FixedUpdate()
    {
        if (ActiveEffect.HasValue && EffectTimeRemaining > 0f)
        {
            EffectTimeRemaining -= Time.fixedDeltaTime;
            if (EffectTimeRemaining <= 0f)
                ClearActiveEffect();
        }

        if (Player.AmOwner && _visionMultiplier is not 1f && !MeetingHud.Instance)
        {
            GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod =
                _baseCrewVision * _visionMultiplier;
        }
    }

    // ── Vision helpers ────────────────────────────────────────────────────────
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
            _baseCrewVision   = -1f;
            _visionMultiplier = 1f;
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

    // ── Effect rolling ────────────────────────────────────────────────────────
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
            candidates = candidates.Where(e => e.Category != ChaosEffectCategory.Negative);

        if (!Opts.AllowRepeatEffects)
            candidates = candidates.Where(e => !_usedEffects.Contains(e.Effect));

        // Never allow the same effect two rolls in a row
        if (_lastRolledEffect.HasValue)
            candidates = candidates.Where(e => e.Effect != _lastRolledEffect.Value);

        var pool = candidates.ToList();
        if (pool.Count == 0) return null;

        float totalWeight = pool.Sum(e => GetWeight(e.Rarity));
        float roll        = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative  = 0f;

        foreach (var e in pool)
        {
            cumulative += GetWeight(e.Rarity);
            if (roll <= cumulative)
                return e.Effect;
        }

        return pool[^1].Effect;
    }

    private static float GetWeight(ChaosEffectRarity rarity) => rarity switch
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

    // ── Apply ─────────────────────────────────────────────────────────────────
    private void ApplyEffect(ChaosEffect effect)
    {
        var info = ChaosEffectData.Get(effect);
        _usedEffects.Add(effect);
        _lastRolledEffect = effect;

        ClearActiveEffect();
        ActiveEffect = effect;

        switch (effect)
        {
            // ── Positive ──────────────────────────────────────────────────────

            case ChaosEffect.Speed:
                var mult = UnityEngine.Random.Range(Opts.SpeedMultiplierMin, Opts.SpeedMultiplierMax);
                _speedMultiplier    = mult;
                _speedDirty         = true;
                EffectTimeRemaining = 30f;
                if (Player != null && !Player.Data.IsDead)
                    Player.RawSetAppearance(this);
                break;

            case ChaosEffect.MoreTokens:
                var bonus = UnityEngine.Random.Range((int)Opts.BonusTokensMin, (int)Opts.BonusTokensMax + 1);
                GainTokens(bonus);
                ActiveEffect = null;
                break;

            case ChaosEffect.OneTimeKill:
                HasOneTimeKill  = true;
                OneTimeKillUsed = false;
                ActiveEffect    = null;
                break;

            case ChaosEffect.Vision:
                CaptureBaseVision();
                _visionMultiplier   = Opts.VisionMultiplier;
                EffectTimeRemaining = Opts.VisionDuration;
                break;

            // ── Neutral ───────────────────────────────────────────────────────

            case ChaosEffect.PositionSwap:
                if (Player.AmOwner)
                    Coroutines.Start(CoPositionSwap());
                ActiveEffect = null;
                break;

            case ChaosEffect.Revive:
                if (AmongUsClient.Instance.AmHost)
                    Coroutines.Start(CoReviveRandomPlayer());
                ActiveEffect = null;
                break;

            // ── Negative ──────────────────────────────────────────────────────

            case ChaosEffect.Death:
                HasDeathSentence = true;
                ActiveEffect     = null;
                break;
        }

        // On-screen notification
        if (Player is not null && Player.AmOwner)
        {
            var colorHex = ColorUtility.ToHtmlStringRGB(info.Color);
            var emoji = info.Category switch
            {
                ChaosEffectCategory.Positive => "✅",
                ChaosEffectCategory.Neutral  => "⚡",
                ChaosEffectCategory.Negative => "💀",
                _                             => "🎲",
            };

            var notif = Helpers.CreateAndShowNotification(
                $"<b><color=#{colorHex}>{emoji} {info.Name}</color>\n<size=70%>{info.Description}</size></b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: ModifierIcons.ChaosToken.LoadAsset());
            notif.AdjustNotification();

            MiscUtils.AddFakeChat(
                Player.Data,
                $"<color=#{colorHex}>🎲 Chaos Tokens</color>",
                $"{emoji} <b>{info.Name}</b> — {info.Description}",
                showHeadsup: false);

            Coroutines.Start(MiscUtils.CoFlash(info.Color, 0.3f, 0.25f));
        }
    }

    // ── Clear active timed effect ─────────────────────────────────────────────
    private void ClearActiveEffect()
    {
        if (!ActiveEffect.HasValue) return;

        switch (ActiveEffect.Value)
        {
            case ChaosEffect.Speed:
                _speedMultiplier = 1f;
                _speedDirty      = true;
                if (Player != null && !Player.Data.IsDead)
                    Player.RawSetAppearance(this);
                break;

            case ChaosEffect.Vision:
                RestoreVision();
                break;
        }

        ActiveEffect        = null;
        EffectTimeRemaining = -1f;
    }

    // ── Coroutines ────────────────────────────────────────────────────────────

    private System.Collections.IEnumerator CoPositionSwap()
    {
        yield return new WaitForSeconds(0.1f);
        if (!ShipStatus.Instance || Player.Data.IsDead) yield break;

        var alivePlayers = PlayerControl.AllPlayerControls.ToArray()
            .Where(p => !p.Data.IsDead && p.PlayerId != Player.PlayerId)
            .ToList();

        if (alivePlayers.Count == 0) yield break;

        var target   = alivePlayers[UnityEngine.Random.Range(0, alivePlayers.Count)];
        var myPos    = Player.transform.position;
        var theirPos = target.transform.position;

        Player.NetTransform.SnapTo(theirPos);
        target.NetTransform.SnapTo(myPos);
    }

    private System.Collections.IEnumerator CoReviveRandomPlayer()
    {
        yield return new WaitForSeconds(0.5f);
        if (!ShipStatus.Instance) yield break;

        var deadThisRound = PlayerControl.AllPlayerControls.ToArray()
            .Where(p => p.Data.IsDead &&
                        p.TryGetModifier<TownOfUs.Modifiers.DeathHandlerModifier>(out var d) &&
                        d.DiedThisRound)
            .ToList();

        if (deadThisRound.Count == 0) yield break;

        var target = deadThisRound[UnityEngine.Random.Range(0, deadThisRound.Count)];
        target.Revive();

        if (Player.AmOwner)
        {
            var colorHex = ColorUtility.ToHtmlStringRGB(ModColor);
            MiscUtils.AddFakeChat(
                Player.Data,
                $"<color=#{colorHex}>🎲 Chaos Tokens</color>",
                $"⚡ <b>Revive</b> — {target.Data.PlayerName} has been brought back to life!",
                showHeadsup: true);
        }
    }

    private System.Collections.IEnumerator CoDeathSentenceResolve()
    {
        while (MeetingHud.Instance)
            yield return null;

        yield return new WaitForSeconds(1f);

        if (!HasDeathSentence || Player.Data.IsDead) yield break;

        HasDeathSentence = false;
        Player.RpcMurderPlayer(Player, true);
    }

    public void MarkOneTimeKillUsed()
    {
        OneTimeKillUsed = true;
        HasOneTimeKill  = false;
    }
}
