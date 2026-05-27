using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using MiraAPI.Networking;
using MiraOverloaded.Options.Roles.Neutral;
using MiraOverloaded.Patches;
using TMPro;
using TownOfUs.Networking;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiraOverloaded.Modifiers.Universal;

public sealed class HiveMindAwakenModifier : TimedModifier
{
    private Image? _awakenBar;
    private TextMeshProUGUI? _awakenText;
    private GameObject? _awakenUI;
    private float _soundTimer = 1f;
    private bool _pendingTimeout;

    internal static bool AwakenSucceeded;

    public override string ModifierName => "Awakening";
    public override float Duration => OptionGroupSingleton<HiveMindOptions>.Instance.AwakeningWindow;
    public override bool AutoStart => true;
    public override bool HideOnUi => true;
    public override bool RemoveOnComplete => false;

    public override string GetDescription()
    {
        var roundedTime = (int)Math.Round(Math.Max(TimeRemaining, 0), 0);
        var textColor = roundedTime switch
        {
            > 10 => Color.green,
            > 5 => Color.yellow,
            _ => Color.red
        };
        return $"{textColor.ToTextColor()}<size=80%>{roundedTime}s</size></color>";
    }

    public override void OnActivate()
    {
        base.OnActivate();

        AwakenSucceeded = false;
        _pendingTimeout = false;

        if (!Player.AmOwner) return;

        _awakenUI = Object.Instantiate(TouAssets.ScatterUI.LoadAsset(), HudManager.Instance.transform);
        _awakenUI.transform.localPosition = new Vector3(-3.22f, 2.26f, -10f);
        _awakenUI.SetActive(false);

        _awakenText = _awakenUI.transform.FindChild("ScatterCanvas").FindChild("ScatterText").gameObject
            .GetComponent<TextMeshProUGUI>();
        _awakenText.text = $"Awaken: {Duration}s";
        _awakenText.gameObject.SetActive(false);

        _awakenBar = _awakenUI.transform.FindChild("ScatterCanvas").FindChild("ScatterBar").gameObject
            .GetComponent<Image>();
        _awakenBar.fillAmount = 1f;

        var awakenIcon = _awakenUI.transform.FindChild("ScatterCanvas").FindChild("ScatterIcon").gameObject
            .GetComponent<Image>();
        awakenIcon.sprite = Player.Data.Role.RoleIconSolid;
    }

    public override void FixedUpdate()
    {
        if (!Player.AmOwner || Player.HasDied() || MeetingHud.Instance)
        {
            _soundTimer = 1f;
            if (_awakenUI != null) _awakenUI.SetActive(false);
            if (_awakenText != null) _awakenText.gameObject.SetActive(false);
            return;
        }

        if (_pendingTimeout && !ExileController.Instance)
        {
            _pendingTimeout = false;
            if (!AwakenSucceeded && !Player.HasDied())
            {
                HiveMindDeathPatch.RpcHiveMindTimeout(Player);
                HiveMindDeathPatch.SuppressNextDeathBroadcast = true;
                Player.RpcSpecialMurder(Player, ignoreShield: true, causeOfDeath: "NotAwakening");
            }
            return;
        }

        if (!TimerActive)
        {
            if (_awakenUI != null) _awakenUI.SetActive(false);
            if (_awakenText != null) _awakenText.gameObject.SetActive(false);
            return;
        }

        base.FixedUpdate();

        var roundedTime = (int)Math.Round(Math.Max(TimeRemaining, 0f), 0f);
        var textColor = roundedTime switch
        {
            > 10 => Color.green,
            > 5 => Color.yellow,
            _ => Color.red
        };

        if (_awakenText != null)
            _awakenText.text = $"Awaken: {textColor.ToTextColor()}{roundedTime}s</color>";

        if (_awakenBar != null)
        {
            _awakenBar.fillAmount = Math.Clamp(TimeRemaining / Duration, 0f, 1f);
            _awakenBar.color = textColor;
        }

        if (roundedTime <= 11f)
        {
            _soundTimer -= Time.fixedDeltaTime;
            if (_soundTimer <= 0f)
            {
                var num = roundedTime / 10f;
                var pitch = 1.5f - num / 2f;
                SoundManager.Instance.PlaySoundImmediate(
                    GameManagerCreator.Instance.HideAndSeekManagerPrefab.FinalHideCountdownSFX, false, 1f, pitch,
                    SoundManager.Instance.SfxChannel);
                _soundTimer = 1f;
            }
        }

        if (_awakenUI != null) _awakenUI.SetActive(true);
        if (_awakenText != null) _awakenText.gameObject.SetActive(true);
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();

        _soundTimer = 1f;
        _pendingTimeout = false;

        if (_awakenUI != null)
        {
            Object.Destroy(_awakenUI);
            _awakenUI = null;
            _awakenBar = null;
            _awakenText = null;
        }
    }

    public override void OnTimerComplete()
    {
        // Defer the consequence rather than refunding the full duration, so meetings
        // can't be used to exploit the awakening window.
        if (MeetingHud.Instance || ExileController.Instance)
        {
            _pendingTimeout = true;
            return;
        }

        if (!Player.AmOwner || Player.HasDied()) return;

        // Awaken button was already pressed this round — don't time out as well.
        if (AwakenSucceeded) return;

        HiveMindDeathPatch.RpcHiveMindTimeout(Player);
        // Suppress the Die() postfix from also broadcasting RpcHiveMindDeath,
        // which would double the banner and cooldown bump for other hive members.
        HiveMindDeathPatch.SuppressNextDeathBroadcast = true;
        Player.RpcCustomMurder(Player);
    }
}
