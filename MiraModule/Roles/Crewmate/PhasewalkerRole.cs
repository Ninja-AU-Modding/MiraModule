using System.Text;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraModule.Assets;
using MiraModule.Buttons.Crewmates;
using MiraModule.Options.Roles.Crewmates;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using TownOfUs.Modules.Localization;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace MiraModule.Roles.Crewmate;

/// <summary>
/// Phasewalker — a crewmate role that can temporarily phase through walls.
/// While phased, the player is semi-transparent, moves slightly slower, cannot
/// interact with tasks/buttons/bodies/consoles, and cannot report bodies.
/// </summary>
public sealed class PhasewalkerRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IVisualAppearance
{
    // ── ITownOfUsRole ────────────────────────────────────────────────────────
    public string LocaleKey => "Phasewalker";
    public string RoleName => TouLocale.Get("MiraRolePhasewalker", "Phasewalker");
    public string RoleDescription => TouLocale.GetParsed("MiraRolePhasewalkerIntroBlurb", "Phase through walls to escape danger or reach tasks.");
    public string RoleLongDescription => TouLocale.GetParsed("MiraRolePhasewalkerTabDescription",
        "Use your Phase ability to walk through walls. While phased you are semi-transparent and cannot interact with anything.");

    public string GetAdvancedDescription() =>
        TouLocale.GetParsed("MiraRolePhasewalkerWikiDescription",
            "The Phasewalker can temporarily phase through walls. Balance: reduced speed, no interactions, cannot report bodies.") +
        MiscUtils.AppendOptionsText(GetType());

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities =>
    [
        new(TouLocale.GetParsed("MiraRolePhasewalkerPhase", "Phase"),
            TouLocale.GetParsed("MiraRolePhasewalkerPhaseWikiDescription",
                "Temporarily phase through walls. Lasts a short duration. You cannot interact with anything while phased."),
            RoleIcons.Phasewalker),
    ];

    public Color RoleColor => MiraModuleColors.Phasewalker;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = RoleIcons.Phasewalker,
    };

    // ── Phase state ──────────────────────────────────────────────────────────
    public bool IsPhased { get; private set; }

    // Trail particle system — created on demand
    private ParticleSystem? _trailPs;

    // ── IVisualAppearance ────────────────────────────────────────────────────
    // We implement IVisualAppearance on the role directly so the TOU appearance
    // system (RawSetAppearance / ResetAppearance) handles our alpha and speed.
    public VisualAppearance? GetVisualAppearance()
    {
        if (!IsPhased) return null;

        var opts = OptionGroupSingleton<PhasewalkerOptions>.Instance;

        // Visible to impostors/dead as a dimmed ghost; everyone else sees very faint
        var alpha = PlayerControl.LocalPlayer.IsImpostorAligned() ||
                    (PlayerControl.LocalPlayer.HasDied() && !ShipStatus.Instance) ? 0.35f : 0.15f;

        var appearance = Player.GetDefaultAppearance();
        var c = appearance.RendererColor;
        appearance.RendererColor = new Color(c.r, c.g, c.b, alpha);
        appearance.Speed = opts.PhaseSpeedMultiplier;
        return appearance;
    }

    // ── Lifecycle ────────────────────────────────────────────────────────────
    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
        IsPhased = false;
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        ForceEndPhase();
    }

    public override void OnMeetingStart()
    {
        RoleBehaviourStubs.OnMeetingStart(this);
        // Meetings forcibly end any active phase
        if (IsPhased) ForceEndPhase();
    }

    public override void OnDeath(DeathReason reason)
    {
        RoleBehaviourStubs.OnDeath(this, reason);
        ForceEndPhase();
    }

    // ── Tab text ─────────────────────────────────────────────────────────────
    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var sb = ITownOfUsRole.SetNewTabText(this);
        if (IsPhased)
            sb.AppendLine(TownOfUsPlugin.Culture, "<b><color=#64C8F0>⬡ Phasing</color></b>");
        return sb;
    }

    // ── Phase begin / end ────────────────────────────────────────────────────

    /// <summary>Called by the button (owner only). Broadcasts via RPC so all clients update.</summary>
    public void BeginPhase()
    {
        RpcSetPhase(Player, true);
    }

    /// <summary>Called when the effect timer expires or the button is pressed again.</summary>
    public void EndPhase()
    {
        if (!IsPhased) return;
        RpcSetPhase(Player, false);
    }

    /// <summary>Silent local-only end — used for meeting start / death / deinit.</summary>
    private void ForceEndPhase()
    {
        if (!IsPhased) return;
        ApplyPhase(false);
    }

    // ── RPC ──────────────────────────────────────────────────────────────────
    [MethodRpc((uint)MiraModuleRpc.PhasewalkerPhase)]
    public static void RpcSetPhase(PlayerControl player, bool active)
    {
        if (player.Data.Role is not PhasewalkerRole role) return;
        role.ApplyPhase(active);
    }

    // ── Core phase application ────────────────────────────────────────────────
    private void ApplyPhase(bool active)
    {
        IsPhased = active;

        if (active)
        {
            EnableWallPass();
            if (Player.AmOwner)
                StartDistortionTrail();
        }
        else
        {
            DisableWallPass();
            StopDistortionTrail();

            // If we ended up inside geometry, nudge to the nearest open tile
            if (Player.AmOwner)
                Coroutines.Start(PushOutOfWall());
        }

        // Refresh visual appearance for this player on all clients
        if (IsPhased)
            Player.RawSetAppearance(this);
        else
            Player.ResetAppearance();
    }

    // ── Collision management ─────────────────────────────────────────────────

    private void EnableWallPass()
    {
        // Disable collision between the player layer and map geometry layers.
        // Among Us walls/doors sit on "Default" and "Objects".
        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Players"),
            LayerMask.NameToLayer("Default"), true);
        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Players"),
            LayerMask.NameToLayer("Objects"), true);
    }

    private void DisableWallPass()
    {
        // Restore layer collisions
        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Players"),
            LayerMask.NameToLayer("Default"), false);
        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Players"),
            LayerMask.NameToLayer("Objects"), false);
    }

    // ── Push out of wall ─────────────────────────────────────────────────────

    private System.Collections.IEnumerator PushOutOfWall()
    {
        // Wait one physics frame for colliders to re-enable
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        if (Player == null || Player.Data.IsDead || !ShipStatus.Instance) yield break;

        var pos = Player.transform.position;

        // Check if we're overlapping any solid geometry
        var hits = Physics2D.OverlapCircleAll(pos, 0.3f,
            LayerMask.GetMask("Default", "Objects"));

        if (hits.Length == 0) yield break; // Already in open space

        // Sample 16 directions at increasing radii to find nearest clear spot
        const int directions = 16;
        const float step = 0.25f;
        const float maxRadius = 3f;

        for (var radius = step; radius <= maxRadius; radius += step)
        {
            for (var i = 0; i < directions; i++)
            {
                var angle = i * (360f / directions) * Mathf.Deg2Rad;
                var candidate = pos + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);

                var check = Physics2D.OverlapCircle(candidate, 0.2f,
                    LayerMask.GetMask("Default", "Objects"));

                if (!check)
                {
                    Player.NetTransform.SnapTo(candidate);
                    yield break;
                }
            }
        }
    }

    // ── Distortion trail ─────────────────────────────────────────────────────

    private void StartDistortionTrail()
    {
        var opts = OptionGroupSingleton<PhasewalkerOptions>.Instance;
        if (!opts.ShowDistortionTrail) return;
        if (_trailPs != null) return;

        var go = new GameObject("PhasewalkerTrail");
        go.transform.SetParent(Player.transform, false);
        go.transform.localPosition = Vector3.zero;

        _trailPs = go.AddComponent<ParticleSystem>();

        var main = _trailPs.main;
        main.loop = true;
        main.duration = 1f;
        main.startLifetime = 0.4f;
        main.startSpeed = 0f;
        main.startSize = 0.18f;
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.4f, 0.78f, 0.94f, 0.7f),  // cyan-blue start
            new Color(0.4f, 0.78f, 0.94f, 0f));    // fade to transparent

        var emission = _trailPs.emission;
        emission.rateOverTime = 20f;

        var shape = _trailPs.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.15f;

        var colorOverLifetime = _trailPs.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(new Color(0.4f, 0.78f, 0.94f), 0f), new GradientColorKey(new Color(0.4f, 0.78f, 0.94f), 1f) },
            new[] { new GradientAlphaKey(0.65f, 0f), new GradientAlphaKey(0f, 1f) });
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        // Render below the player
        var renderer = _trailPs.GetComponent<ParticleSystemRenderer>();
        renderer.sortingLayerName = "Players";
        renderer.sortingOrder = -1;

        _trailPs.Play();
    }

    private void StopDistortionTrail()
    {
        if (_trailPs == null) return;
        _trailPs.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        UnityEngine.Object.Destroy(_trailPs.gameObject);
        _trailPs = null;
    }
}
