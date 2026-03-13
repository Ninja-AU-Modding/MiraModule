using Reactor.Utilities.Attributes;
using UnityEngine;
using MiraAPI.Utilities;

namespace MiraModule.Modules.Rainbow;

[RegisterInIl2Cpp]
public sealed class CustomRainbowRenderer(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    public Renderer PlayerRender;
    public int ColorId;
    public MiraModuleRainbowDef Def;

    public void Initialize(int colorId, Renderer rend, MiraModuleRainbowDef def)
    {
        ColorId = colorId;
        PlayerRender = rend;
        Def = def;
    }

    public void Update()
    {
        if (PlayerRender == null)
        {
            return;
        }

        if (Def.Colors.Length == 0)
        {
            return;
        }

        var body = CustomRainbowUtils.GetColor(Def.Speed, Def.Colors);
        var shadow = CustomRainbowUtils.GetShadow(body);

        PlayerRender.material.SetColor(ShaderID.BodyColor, body);
        PlayerRender.material.SetColor(ShaderID.BackColor, shadow);
        PlayerRender.material.SetColor(ShaderID.VisorColor, Palette.VisorColor);

        // Keep gradient material from blacking out the secondary colors.
        PlayerRender.material.SetColor(ShaderID.Get("_BodyColor2"), body);
        PlayerRender.material.SetColor(ShaderID.Get("_BackColor2"), shadow);
    }
}
