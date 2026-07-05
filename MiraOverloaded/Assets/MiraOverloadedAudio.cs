using AmongUs.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraOverloaded.Assets;

public static class MiraOverloadedAudio
{
    private const string ShortPath = "MiraOverloaded.Resources.Audio";
    public static LoadableAsset<AudioClip> HiveMindIntro =>
        new LoadableAudioResourceAsset($"{ShortPath}.HiveMindIntro.wav");
    public static LoadableAsset<AudioClip> HiveMindAwaken =>
        new LoadableAudioResourceAsset($"{ShortPath}.HiveMindAwaken.wav");
    public static LoadableAsset<AudioClip> HiveMindKilled =>
        new LoadableAudioResourceAsset($"{ShortPath}.HiveMindKilled.wav");
    public static LoadableAsset<AudioClip> HiveMindDeath =>
        new LoadableAudioResourceAsset($"{ShortPath}.HiveMindDeath.wav");
}
