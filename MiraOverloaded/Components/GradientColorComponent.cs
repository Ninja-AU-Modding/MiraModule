using System;
using MiraAPI.Utilities;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace MiraOverloaded.Components;

[RegisterInIl2Cpp]
public class GradientColorComponent(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    private SpriteRenderer? _renderer;
    private Material? _mat;
    private bool _isGradient;
    private bool _followPrimary;
    private static readonly int BodyColorId = ShaderID.Get("_BodyColor");
    private static readonly int BackColorId = ShaderID.Get("_BackColor");
    private static readonly int BodyColor2Id = ShaderID.Get("_BodyColor2");
    private static readonly int BackColor2Id = ShaderID.Get("_BackColor2");

    public void SetColor(int primaryColor, int secondaryColor)
    {
        _renderer = GetComponent<SpriteRenderer>();
        if (!_renderer)
        {
            return;
        }

        if (secondaryColor < 0 || secondaryColor >= Palette.PlayerColors.Length)
        {
            if (primaryColor >= 0 && primaryColor < Palette.PlayerColors.Length)
            {
                _mat = _renderer.material;
                PlayerMaterial.SetColors(primaryColor, _mat);
            }
            _isGradient = false;
            return;
        }

        _mat = _renderer.material;

        PlayerMaterial.SetColors(primaryColor, _mat);

        _mat.SetColor(BodyColor2Id, Palette.PlayerColors[secondaryColor]);
        _mat.SetColor(BackColor2Id, Palette.ShadowColors[secondaryColor]);

        _isGradient = primaryColor != secondaryColor;
        _followPrimary = !_isGradient;
    }

    public void SetColor(int secondaryColor)
    {
        _renderer = GetComponent<SpriteRenderer>();
        if (!_renderer)
        {
            return;
        }

        if (secondaryColor < 0 || secondaryColor >= Palette.PlayerColors.Length)
        {
            return;
        }

        _mat = _renderer.material;
        _mat.SetColor(BodyColor2Id, Palette.PlayerColors[secondaryColor]);
        _mat.SetColor(BackColor2Id, Palette.ShadowColors[secondaryColor]);
        _isGradient = true;
        _followPrimary = false;
    }

    public void SetColor(Color baseColor, Color gradientColor)
    {
        _renderer = GetComponent<SpriteRenderer>();
        if (!_renderer)
        {
            return;
        }

        _mat = _renderer.material;
        PlayerMaterial.SetColors(baseColor, _mat);

        _mat.SetColor(BodyColor2Id, gradientColor);
        _mat.SetColor(BackColor2Id, ((Color32)gradientColor).GetShadowColor(60));
        _isGradient = baseColor != gradientColor;
        _followPrimary = !_isGradient;
    }

    public void Update()
    {
        if (!_renderer || !_mat)
        {
            return;
        }

        _mat?.SetFloat(ShaderID.Get("_Flip"), _renderer?.flipX == true ? 1 : 0);

        if (_followPrimary)
        {
            var primary = _mat?.GetColor(BodyColorId);
            var shadow = _mat?.GetColor(BackColorId);
            _mat?.SetColor(BodyColor2Id, primary ?? Color.white);
            _mat?.SetColor(BackColor2Id, shadow ?? Color.white);
        }
    }

    public bool IsGradient => _isGradient;
}
