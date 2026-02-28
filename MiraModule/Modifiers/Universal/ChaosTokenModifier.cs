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
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace MiraModule.Modifiers.Universal;

/// <summary>
/// Chaos Tokens modifier. Grants a dice button that rolls a random effect on use.
/// Tokens are earned via tasks, meetings, and kills (all configurable).
/// Effects never stack — each roll replaces any previous active effect.
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

    // ── Active timed effect tracking ─────────────────────────────────────────
    // Only one active effect at a time — effects never stack.
    public ChaosEffect? ActiveEffect       { get; private set; }
    public float        EffectTimeRemaining { get; private set; } = -1f;

    // ── Per-effect state ──────────────────────────────────────────────────────
    // Speed
    private float _speedMultiplier = 1f;
    private bool  _speedDirty;

    // Vision
    private float _baseCrewVision  = -1f;
    private float _visionMultiplier = 1f;

    // Defense (shield)
    public bool IsShielded { get; private set; }

    // Votes — stored, applied when meeting starts
    public int BonusVotesNextMeeting { get; private set; }

    // One-time kill — flag checked by the kill button
    public bool HasOneTimeKill  { get; private set; }
    public bool OneTimeKillUsed { get; private set; }

    // Invisible — handled via ConcealedModifier-compatible flag
    public bool IsInvisible { get; private set; }

    // Death sentence — resolved at end of next meeting
    public bool HasDeathSentence { get; private set; }

    // Used effects set (for no-repeat option)
    private readonly HashSet<ChaosEffect> _usedEffects = [];

    // Last rolled effect — never allowed to roll the same one twice in a row
    private ChaosEffect? _lastRolledEffect;

    // ── IVisualAppearance ─────────────────────────────────────────────────────
    // Track whether we want invisible rendering this frame
    private bool _wantsInvisible;

    public VisualAppearance GetVisualAppearance()
    {
        var appearance = Player.GetDefaultAppearance();
        appearance.Speed = _speedMultiplier;
        if (_wantsInvisible)
        {
            var isImpostor = PlayerControl.LocalPlayer.IsImpostorAligned();
            appearance.RendererColor = isImpostor ? new Color(0f, 0f, 0f, 0.1f) : Color.clear;
            appearance.NameColor = Color.clear;
            appearance.ColorBlindTextColor = Color.clear;
        }
        return appearance;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private ChaosTokenButton? FindButton() =>
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
        IsShielded          = false;
        BonusVotesNextMeeting = 0;
        HasOneTimeKill      = false;
        OneTimeKillUsed     = false;
        IsInvisible         = false;
        HasDeathSentence    = false;
        _usedEffects.Clear();
        _lastRolledEffect = null;
    }

    public override void OnDeactivate()
    {
        if (_speedDirty && Player != null)
            Player.ResetAppearance(fullReset: true);
        RestoreVision();
    }

    public override void OnMeetingStart()
    {
        // Expire the active timed effect at meeting start
        ClearActiveEffect();

        // Apply bonus votes for this meeting
        // (We record the count; the meeting vote patch must read BonusVotesNextMeeting to honour it.)
        // Votes reset when the next meeting starts.
        BonusVotesNextMeeting = 0;

        // Resolve death sentence — the player dies after the meeting ends
        if (HasDeathSentence && Player.AmOwner)
        {
            // Schedule death for end of meeting via coroutine
            Coroutines.Start(CoDeathSentenceResolve());
        }

        // Meeting token gain
        if (Player.AmOwner && Opts.GainOnMeeting)
            GainTokens((int)Opts.TokensPerMeeting);
    }

    public override void FixedUpdate()
    {
        // Tick down timed effects
        if (ActiveEffect.HasValue && EffectTimeRemaining > 0f)
        {
            EffectTimeRemaining -= Time.fixedDeltaTime;
            if (EffectTimeRemaining <= 0f)
                ClearActiveEffect();
        }

        // Apply vision multiplier each frame
        if (Player.AmOwner && _visionMultiplier != 1f && !MeetingHud.Instance)
        {
            var opts = GameOptionsManager.Instance.currentNormalGameOptions;
            opts.CrewLightMod = _baseCrewVision * _visionMultiplier;
        }

        // Invisible: auto-conceal when the player stops moving
        if (Player.AmOwner && IsInvisible && ActiveEffect == ChaosEffect.Invisible && !MeetingHud.Instance)
        {
            var isMoving = Player.MyPhysics.Velocity.sqrMagnitude > 0.01f;
            _wantsInvisible = !isMoving;
            Player.RawSetAppearance(this);
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

        if (!Opts.AllowNeutralEffects)
            candidates = candidates.Where(e => e.Category != ChaosEffectCategory.Neutral);

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

    // ── Apply ─────────────────────────────────────────────────────────────────
    private void ApplyEffect(ChaosEffect effect)
    {
        var info = ChaosEffectData.Get(effect);
        _usedEffects.Add(effect);
        _lastRolledEffect = effect;

        // Always clear the previous active timed effect before applying a new one
        ClearActiveEffect();

        ActiveEffect = effect;

        switch (effect)
        {
            // ── Positive ──────────────────────────────────────────────────────

            case ChaosEffect.Defense:
                IsShielded          = true;
                EffectTimeRemaining = Opts.ShieldDuration;
                // Add TOU invulnerability modifier — protects from attacks and guesses
                if (Player.AmOwner || AmongUsClient.Instance.AmHost)
                    Player.RpcAddModifier<InvulnerabilityModifier>(false, false, false);
                break;

            case ChaosEffect.Speed:
                var randomMultiplier = UnityEngine.Random.Range(Opts.SpeedMultiplierMin, Opts.SpeedMultiplierMax);
                _speedMultiplier = randomMultiplier;
                _speedDirty      = true;
                EffectTimeRemaining = 30f; // Speed lasts 30 seconds
                if (Player != null && !Player.Data.IsDead)
                    Player.RawSetAppearance(this);
                break;

            case ChaosEffect.Votes:
                var bonusVotes = UnityEngine.Random.Range((int)Opts.BonusVotesMin, (int)Opts.BonusVotesMax + 1);
                BonusVotesNextMeeting += bonusVotes;
                ActiveEffect = null; // Instant — no timed tracking needed
                break;

            case ChaosEffect.MoreTokens:
                var bonusTokens = UnityEngine.Random.Range((int)Opts.BonusTokensMin, (int)Opts.BonusTokensMax + 1);
                GainTokens(bonusTokens);
                ActiveEffect = null; // Instant
                break;

            case ChaosEffect.OneTimeKill:
                HasOneTimeKill  = true;
                OneTimeKillUsed = false;
                ActiveEffect    = null; // Lasts until used, no timer
                break;

            case ChaosEffect.Tasks:
                if (Player.AmOwner)
                    Coroutines.Start(CoCompleteTasks());
                ActiveEffect = null; // Instant
                break;

            case ChaosEffect.Vision:
                CaptureBaseVision();
                _visionMultiplier   = Opts.VisionMultiplier;
                EffectTimeRemaining = Opts.VisionDuration;
                break;

            case ChaosEffect.Invisible:
                IsInvisible         = true;
                EffectTimeRemaining = Opts.InvisibleDuration;
                break;

            // ── Neutral ───────────────────────────────────────────────────────

            case ChaosEffect.RevealRandomPlayer:
                if (Player.AmOwner || AmongUsClient.Instance.AmHost)
                    Coroutines.Start(CoRevealRandomPlayer());
                ActiveEffect = null; // Instant
                break;

            case ChaosEffect.PositionSwap:
                if (Player.AmOwner)
                    Coroutines.Start(CoPositionSwap());
                ActiveEffect = null; // Instant
                break;

            case ChaosEffect.RoleSwap:
                // Role swap is host-only and complex — notify only for now, stub for future
                ActiveEffect = null;
                break;

            case ChaosEffect.Revive:
                if (AmongUsClient.Instance.AmHost)
                    Coroutines.Start(CoReviveRandomPlayer());
                ActiveEffect = null; // Instant
                break;

            // ── Negative ──────────────────────────────────────────────────────

            case ChaosEffect.Death:
                HasDeathSentence = true;
                ActiveEffect     = null; // Resolved at meeting end, not timed
                break;
        }

        // Notify the local player of what they rolled
        if (Player.AmOwner)
        {
            var colorHex = ColorUtility.ToHtmlStringRGB(info.Color);
            var category = info.Category switch
            {
                ChaosEffectCategory.Positive => "✅",
                ChaosEffectCategory.Neutral  => "⚡",
                ChaosEffectCategory.Negative => "💀",
                _                             => "🎲",
            };

            // On-screen pop-up notification
            var notif = Helpers.CreateAndShowNotification(
                $"<b><color=#{colorHex}>{category} {info.Name}</color>\n<size=70%>{info.Description}</size></b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: ModifierIcons.ChaosToken.LoadAsset());
            notif.AdjustNotification();

            // Also send to chat
            MiscUtils.AddFakeChat(
                Player.Data,
                $"<color=#{colorHex}>🎲 Chaos Tokens</color>",
                $"{category} <b>{info.Name}</b> — {info.Description}",
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
            case ChaosEffect.Defense:
                IsShielded = false;
                // Remove invulnerability modifier if it exists
                if (Player.HasModifier<InvulnerabilityModifier>())
                    Player.RpcRemoveModifier<InvulnerabilityModifier>();
                break;

            case ChaosEffect.Speed:
                _speedMultiplier = 1f;
                _speedDirty      = true;
                if (Player != null && !Player.Data.IsDead)
                    Player.RawSetAppearance(this);
                break;

            case ChaosEffect.Vision:
                RestoreVision();
                break;

            case ChaosEffect.Invisible:
                IsInvisible = false;
                _wantsInvisible = false;
                if (Player != null && !Player.Data.IsDead)
                    Player.ResetAppearance();
                break;
        }

        ActiveEffect        = null;
        EffectTimeRemaining = -1f;
    }

    // ── Coroutines ────────────────────────────────────────────────────────────

    private System.Collections.IEnumerator CoCompleteTasks()
    {
        yield return new WaitForSeconds(0.1f);
        if (Player.Data.IsDead) yield break;

        var incompleteTasks = Player.myTasks.ToArray()
            .Where(t => !t.IsComplete && t.TryCast<NormalPlayerTask>() != null)
            .ToList();

        if (incompleteTasks.Count == 0) yield break;

        incompleteTasks.Shuffle();

        var count = UnityEngine.Random.Range((int)Opts.TasksMin, (int)Opts.TasksMax + 1);
        count = Math.Min(count, incompleteTasks.Count);

        for (var i = 0; i < count; i++)
        {
            var task = incompleteTasks[i];
            HudManager.Instance.ShowTaskComplete();
            Player.RpcCompleteTask(task.Id);
            yield return new WaitForSeconds(0.3f);
        }
    }

    private System.Collections.IEnumerator CoPositionSwap()
    {
        yield return new WaitForSeconds(0.1f);
        if (!ShipStatus.Instance || Player.Data.IsDead) yield break;

        var alivePlayers = PlayerControl.AllPlayerControls.ToArray()
            .Where(p => !p.Data.IsDead && p.PlayerId != Player.PlayerId)
            .ToList();

        if (alivePlayers.Count == 0) yield break;

        var target = alivePlayers[UnityEngine.Random.Range(0, alivePlayers.Count)];
        var myPos  = Player.transform.position;
        var theirPos = target.transform.position;

        Player.NetTransform.SnapTo(theirPos);
        target.NetTransform.SnapTo(myPos);
    }

    private System.Collections.IEnumerator CoRevealRandomPlayer()
    {
        yield return new WaitForSeconds(0.1f);

        var alivePlayers = PlayerControl.AllPlayerControls.ToArray()
            .Where(p => !p.Data.IsDead && p.PlayerId != Player.PlayerId)
            .ToList();

        if (alivePlayers.Count == 0) yield break;

        var target = alivePlayers[UnityEngine.Random.Range(0, alivePlayers.Count)];

        // Announce in chat to everyone (only local notification since this is owner-driven)
        var roleName = target.Data.Role.GetRoleName();
        var colorHex = ColorUtility.ToHtmlStringRGB(ModColor);

        // Notify all local players of the reveal
        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (player.AmOwner)
            {
                MiscUtils.AddFakeChat(
                    Player.Data,
                    $"<color=#{colorHex}>🎲 Chaos Tokens</color>",
                    $"⚡ <b>Chaos reveals:</b> {target.Data.PlayerName} is a <b>{roleName}</b>!",
                    showHeadsup: true);
            }
        }
    }

    private System.Collections.IEnumerator CoReviveRandomPlayer()
    {
        yield return new WaitForSeconds(0.5f);
        if (!ShipStatus.Instance) yield break;

        // Find players who died this round (have DeathHandlerModifier marked DiedThisRound)
        var deadThisRound = PlayerControl.AllPlayerControls.ToArray()
            .Where(p => p.Data.IsDead && p.TryGetModifier<TownOfUs.Modifiers.DeathHandlerModifier>(out var d) && d.DiedThisRound)
            .ToList();

        if (deadThisRound.Count == 0) yield break;

        var target = deadThisRound[UnityEngine.Random.Range(0, deadThisRound.Count)];

        // Revive via game's revive API
        target.Revive();

        var colorHex = ColorUtility.ToHtmlStringRGB(ModColor);
        if (Player.AmOwner)
        {
            MiscUtils.AddFakeChat(
                Player.Data,
                $"<color=#{colorHex}>🎲 Chaos Tokens</color>",
                $"⚡ <b>Revive</b> — {target.Data.PlayerName} has been brought back to life!",
                showHeadsup: true);
        }
    }

    private System.Collections.IEnumerator CoDeathSentenceResolve()
    {
        // Wait for meeting to end
        while (MeetingHud.Instance)
            yield return null;

        yield return new WaitForSeconds(1f);

        if (!HasDeathSentence || Player.Data.IsDead) yield break;

        HasDeathSentence = false;
        // Kill the player — use the game's murder system
        Player.RpcMurderPlayer(Player, true);
    }

    public void MarkOneTimeKillUsed()
    {
        OneTimeKillUsed = true;
        HasOneTimeKill  = false;
    }
}
