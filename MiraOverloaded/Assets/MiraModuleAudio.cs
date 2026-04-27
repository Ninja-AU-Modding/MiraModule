using AmongUs.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraOverloaded.Assets;

public static class MiraOverloadedAudio
{
    private const string ShortPath = "MiraOverloaded.Resources.Audio";
    public static LoadableAsset<AudioClip> HiveMind =>
        new LoadableAudioResourceAsset($"{ShortPath}.HiveMind.wav");
}