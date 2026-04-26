using Reactor.Utilities.Attributes;
using UnityEngine;
using MiraAPI.Utilities;

namespace MiraOverloaded.Modules.Rainbow;

[RegisterInIl2Cpp]
public sealed class CustomRainbowRenderer(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    internal Renderer PlayerRender = null!;
    internal MiraOverloadedRainbowDef Def = default!;
    internal int ColorId;

    public void Initialize(int colorId, Renderer rend, MiraOverloadedRainbowDef def)
    {
        ColorId = colorId;
        PlayerRender = rend;
        Def = def;
    }

    public void Update()
    {
        if (PlayerRender == null || Def.Colors.Length == 0) return;

        var body = CustomRainbowUtils.GetColor(Def.Speed, Def.Colors);
        var shadow = CustomRainbowUtils.GetShadow(body);

        PlayerRender.material.SetColor(ShaderID.BodyColor, body);
        PlayerRender.material.SetColor(ShaderID.BackColor, shadow);
        PlayerRender.material.SetColor(ShaderID.VisorColor, Palette.VisorColor);
        PlayerRender.material.SetColor(ShaderID.Get("_BodyColor2"), body);
        PlayerRender.material.SetColor(ShaderID.Get("_BackColor2"), shadow);
    }
}
