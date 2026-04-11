using AmongUs.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraModule.Assets;

public static class MiraModuleAudio
{
    private const string ShortPath = "MiraModule.Resources.Audio";
    public static LoadableAsset<AudioClip> HiveMind =>
        new LoadableAudioResourceAsset($"{ShortPath}.HiveMind.wav");
}