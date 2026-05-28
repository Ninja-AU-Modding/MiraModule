using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using MiraOverloaded.Buttons;
using MiraOverloaded.Options.Modifiers;
using MiraOverloaded.Utilities;
using TownOfUs.Modifiers.Game;
using UnityEngine;

namespace MiraOverloaded.Modifiers.ChaosTokens;

public class ChaosTokenModifier : UniversalGameModifier
{
    public ChaosTokenModifier() : this(amount: 1, showNotification: true) { }

    public ChaosTokenModifier(int amount, bool showNotification = true)
    {
        this.amount = amount;
        this.showNotification = showNotification;
    }

    private readonly int amount;
    private readonly bool showNotification;
    public override string ModifierName => $"Chaos Token{(Tokens > 1 ? "s" : string.Empty)}";
    public override LoadableAsset<Sprite> ModifierIcon => ChaosTokensAssets.DiceSprite;
    public override string GetDescription() => $"Take your chances!\n<b>Tokens left: {Tokens}</b>";
    public override ModifierFaction FactionType => ModifierFaction.UniversalUtility;
    public override Color FreeplayFileColor => MiraOverloadedColors.ChaosTokens;

    public int Tokens { get; private set; }

    // Spawn configuration - makes it appear in modifier settings
    public override int GetAssignmentChance() => 
        (int)OptionGroupSingleton<ChaosTokensModifierOptions>.Instance.ChaosTokenChance.Value;
    
    public override int GetAmountPerGame() => 
        (int)OptionGroupSingleton<ChaosTokensModifierOptions>.Instance.ChaosTokenAmount;

    public override void OnActivate()
    {
        Tokens = amount;

        if (Player.AmOwner)
        {
            CustomButtonSingleton<ChaosTokenRollButton>.Instance?.ResetCooldownAndOrEffect();
            
            if (showNotification)
                TokensReceived(amount);
        }
    }

    public override void OnDeactivate()
    {
        if (Player.AmOwner)
        {
            CustomButtonSingleton<ChaosTokenRollButton>.Instance?.Button?.gameObject.SetActive(false);
        }
    }

    public void IncreaseTokens(int amount, bool showNotification = true)
    {
        Tokens += amount;
        if (Player.AmOwner && showNotification)
            TokensReceived(amount);
    }
    
    public void DecreaseTokens(int amount)
    {
        Tokens -= amount;
    }

    private static void TokensReceived(int amount)
    {
        ChaosTokensUtils.Notification($"<b>You received {amount} token{(amount > 1 ? "s" : string.Empty)}!</b>", showSpr: true);
    }

    public override void FixedUpdate()
    {
        // Modifier stays active even when tokens are 0
        // Button will be disabled via CanUse() check
    }
}
