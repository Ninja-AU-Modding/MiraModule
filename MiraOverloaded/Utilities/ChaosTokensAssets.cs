using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using UnityEngine;

namespace MiraOverloaded.Utilities;

public static class ChaosTokensAssets
{
    public static AssetBundle ShaderBundle { get; } = AssetBundleManager.Load("tokenshader");
    
    public static LoadableAsset<Sprite> DiceButton { get; } = new LoadableResourceAsset("MiraOverloaded.Resources.ChaosTokens.DiceButton.png"); 
    public static LoadableAsset<Sprite> DiceSprite { get; } = new LoadableResourceAsset("MiraOverloaded.Resources.ChaosTokens.Dice.png"); 
    public static LoadableAsset<Sprite> TokenDeathSprite { get; } = new LoadableResourceAsset("MiraOverloaded.Resources.ChaosTokens.TokenDeath.png", 300);
    public static LoadableAsset<Sprite> FilterSprite { get; } = new LoadableResourceAsset("MiraOverloaded.Resources.ChaosTokens.Filter.png"); 
    
    public static LoadableBundleAsset<Material> NauseaMaterial { get; } = new("NauseaMaterial", ShaderBundle);
    public static LoadableBundleAsset<Material> ColorblindMaterial { get; } = new("ColorblindMaterial", ShaderBundle);
}
