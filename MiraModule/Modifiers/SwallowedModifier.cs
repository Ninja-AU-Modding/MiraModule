using MiraAPI.Modifiers;
using MiraModule.Roles.Neutral;
using TMPro;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraModule.Modifiers;

/// <summary>
/// Applied to a player swallowed by the Abyss.
///
/// FROM THE OUTSIDE (other players):
///   - Victim is invisible. Their body stays at the position where they were gulped.
///
/// FROM THE VICTIM'S SCREEN:
///   - Camera follows the Abyss (they watch Abyss move around but see their own name)
///   - Movement completely blocked (physics patch in SwallowedMovementPatch)
///   - Dark blue tint overlay + "You have been swallowed" text
///   - HUD hidden
///
/// ON MEETING: body spawns at Abyss's current position (AbyssMeetingPatch teleports then kills)
/// ON ABYSS DEATH: victim is released alive where they were standing
/// </summary>
public sealed class SwallowedModifier : BaseModifier, IVisualAppearance
{
    public byte AbyssPlayerId { get; private set; }

    private SpriteRenderer? _tintOverlay;
    private TextMeshPro? _swallowedText;

    public override string ModifierName => "Swallowed";
    public override bool HideOnUi => true;
    public override bool Unique => false; // multiple victims allowed

    public SwallowedModifier(byte abyssPlayerId)
    {
        AbyssPlayerId = abyssPlayerId;
    }

    public PlayerControl? GetAbyssPlayer()
    {
        foreach (var pc in PlayerControl.AllPlayerControls)
            if (pc.PlayerId == AbyssPlayerId) return pc;
        return null;
    }

    // ── IVisualAppearance ────────────────────────────────────────────────────
    // Victim renders as fully transparent to everyone (invisible on the map)
    public bool VisualPriority => true;

    public VisualAppearance? GetVisualAppearance()
    {
        var abyss = GetAbyssPlayer();
        if (abyss == null) return null;

        return new VisualAppearance(abyss.GetDefaultAppearance(), TownOfUsAppearances.PlayerOnly)
        {
            RendererColor = new Color(1f, 1f, 1f, 0f),
            NameVisible = false,
        };
    }

    // ── Lifecycle ────────────────────────────────────────────────────────────
    public override void OnActivate()
    {
        AbyssRole.SwallowedPlayers[Player.PlayerId] = AbyssPlayerId;

        // Make victim invisible for all clients
        Player.RawSetAppearance(this);
        Player.Visible = false;

        if (!Player.AmOwner) return;

        var abyss = GetAbyssPlayer();

        // Camera watches Abyss — victim sees Abyss moving, not themselves
        if (abyss != null)
        {
            HudManager.Instance.PlayerCam.SetTarget(abyss);
            // Move light source to Abyss so victim can see the Abyss's surroundings
            Player.lightSource.transform.SetParent(abyss.transform);
            Player.lightSource.Initialize(abyss.Collider.offset / 2f);
        }

        HudManager.Instance.SetHudActive(false);
        Player.moveable = false;

        // Close any open minigames
        if (Minigame.Instance) Minigame.Instance.Close();
        if (MapBehaviour.Instance) MapBehaviour.Instance.Close();

        CreateTintOverlay();
        CreateSwallowedText(abyss);
    }

    // Set to true just before Visible = true so the visibility patch lets it through
    public static readonly HashSet<byte> ReleasingPlayers = new();

    public override void OnDeactivate()
    {
        AbyssRole.SwallowedPlayers.Remove(Player.PlayerId);

        // Restore appearance and visibility.
        // Must register in ReleasingPlayers first so SwallowedVisibilityPatch allows Visible = true.
        if (!Player.HasDied())
        {
            ReleasingPlayers.Add(Player.PlayerId);
            Player.ResetAppearance(true);
            Player.Visible = true;
            ReleasingPlayers.Remove(Player.PlayerId);
        }

        DestroyOverlays();

        if (!Player.AmOwner) return;

        // Restore camera and light to self
        HudManager.Instance.PlayerCam.SetTarget(Player);
        try
        {
            Player.lightSource.transform.SetParent(Player.transform);
            Player.lightSource.Initialize(Player.Collider.offset / 2f);
        }
        catch { /* ignored */ }

        HudManager.Instance.SetHudActive(true);
        Player.moveable = true;
    }

    public override void Update()
    {
        if (!Player.AmOwner) return;

        var abyss = GetAbyssPlayer();

        // If Abyss died/disconnected, release immediately
        if (abyss == null || abyss.HasDied())
        {
            // OnDeactivate will be called via RpcRemoveModifier from Deinitialize
            return;
        }

        // Keep camera locked on Abyss every frame in case something resets it
        if (HudManager.InstanceExists)
            HudManager.Instance.PlayerCam.SetTarget(abyss);

        // Keep overlays visible
        if (_tintOverlay != null) _tintOverlay.enabled = true;
        if (_swallowedText != null) _swallowedText.enabled = true;
    }

    public override void OnMeetingStart()
    {
        // Restore locally so meeting HUD works cleanly
        // AbyssMeetingPatch handles the actual teleport + kill
        DestroyOverlays();
        if (!Player.AmOwner) return;
        HudManager.Instance.PlayerCam.SetTarget(Player);
        try
        {
            Player.lightSource.transform.SetParent(Player.transform);
            Player.lightSource.Initialize(Player.Collider.offset / 2f);
        }
        catch { /* ignored */ }
        HudManager.Instance.SetHudActive(true);
        Player.moveable = true;
    }

    public override void OnDeath(DeathReason reason)
    {
        OnDeactivate();
    }

    // ── Overlay helpers ──────────────────────────────────────────────────────
    private void CreateTintOverlay()
    {
        if (!HudManager.InstanceExists) return;

        _tintOverlay = Object.Instantiate(
            HudManager.Instance.FullScreen,
            HudManager.Instance.FullScreen.transform.parent);
        _tintOverlay.transform.localScale = HudManager.Instance.FullScreen.transform.localScale * 10f;
        _tintOverlay.color = new Color(0.05f, 0.1f, 0.35f, 0.55f);
        _tintOverlay.enabled = true;
        _tintOverlay.gameObject.SetActive(true);
    }

    private void CreateSwallowedText(PlayerControl? abyss)
    {
        if (!HudManager.InstanceExists) return;

        var go = new GameObject("AbyssSwallowedText");
        go.transform.SetParent(HudManager.Instance.transform, false);
        go.transform.localPosition = new Vector3(0f, 1.8f, -10f);

        _swallowedText = go.AddComponent<TextMeshPro>();
        _swallowedText.fontSize = 3.2f;
        _swallowedText.alignment = TextAlignmentOptions.Center;
        _swallowedText.color = new Color(0.6f, 0.85f, 1f, 1f);

        var abyssName = abyss != null ? abyss.Data.PlayerName : "The Abyss";
        _swallowedText.text =
            $"<b>You have been swallowed!</b>\n" +
            $"<size=2.2>You are trapped inside <color=#6699FF>{abyssName}</color>.\n" +
            $"You will die when a meeting is called.</size>";

        _swallowedText.enabled = true;
    }

    private void DestroyOverlays()
    {
        if (_tintOverlay != null)
        {
            _tintOverlay.enabled = false;
            Object.Destroy(_tintOverlay.gameObject);
            _tintOverlay = null;
        }
        if (_swallowedText != null)
        {
            Object.Destroy(_swallowedText.gameObject);
            _swallowedText = null;
        }
    }
}
