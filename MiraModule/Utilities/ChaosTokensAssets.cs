using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using UnityEngine;

namespace MiraModule.Utilities;

public static class ChaosTokensAssets
{
    public static AssetBundle ShaderBundle = AssetBundleManager.Load("tokenshader");
    
    public static LoadableAsset<Sprite> DiceButton { get; } = new LoadableResourceAsset("MiraModule.Resources.ChaosTokens.DiceButton.png"); 
    public static LoadableAsset<Sprite> DiceSprite { get; } = new LoadableResourceAsset("MiraModule.Resources.ChaosTokens.Dice.png"); 
    public static LoadableAsset<Sprite> TokenDeathSprite { get; } = new LoadableResourceAsset("MiraModule.Resources.ChaosTokens.TokenDeath.png", 300);
    public static LoadableAsset<Sprite> FilterSprite { get; } = new LoadableResourceAsset("MiraModule.Resources.ChaosTokens.Filter.png"); 
    
    public static LoadableBundleAsset<Material> NauseaMaterial = new("NauseaMaterial", ShaderBundle);
    public static LoadableBundleAsset<Material> ColorblindMaterial = new("ColorblindMaterial", ShaderBundle);
}
