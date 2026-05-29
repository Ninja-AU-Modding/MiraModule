using System;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace MiraOverloaded.Utilities;

public enum PopupImagePosition
{
    OverlapText,
    LeftOfText,
    RightOfText,
    AboveText,
    BelowText
}

public enum PopupTextAlignment
{
    Left,
    Center,
    Right,
    Justify
}

public static class ModPopupUtility
{
    private static Type _reactorPopupType;

    public static void Show(string message, Vector2 backgroundSize, Sprite customImage = null, float imageScale = 1f, PopupImagePosition imagePos = PopupImagePosition.AboveText, PopupTextAlignment textAlignment = PopupTextAlignment.Center)
    {
        try
        {
            if (_reactorPopupType == null)
            {
                _reactorPopupType = Type.GetType("Reactor.Utilities.UI.ReactorPopup, Reactor");
                if (_reactorPopupType == null)
                {
                    Error("Cannot find ReactorPopup in Assembly!");
                    return;
                }
            }

            string uniquePopupName = "MiraPopup_" + Guid.NewGuid().ToString("N");

            var createMethod = AccessTools.Method(_reactorPopupType, "Create");
            var popupInstance = createMethod.Invoke(null, new object[] { uniquePopupName });

            float bgCenterY = 0.20f;
            float bgWidth = backgroundSize.x;
            float bgHeight = backgroundSize.y;

            var backgroundObj = GetMemberValue(_reactorPopupType, popupInstance, "Background");
            if (backgroundObj is SpriteRenderer background)
            {
                background.transform.localPosition = new Vector3(0, bgCenterY, 0);
                background.size = backgroundSize;
            }

            Vector3 textPos = new Vector3(0, bgCenterY, -1f);
            Vector3 imgPos = new Vector3(0, bgCenterY, -1f);
            Vector2 textSize = new Vector2(bgWidth * 0.9f, bgHeight * 0.8f);

            Vector3 btnPos = new Vector3(0, bgCenterY - (bgHeight / 2f) + 0.6f, -1f);

            if (customImage != null)
            {
                switch (imagePos)
                {
                    case PopupImagePosition.AboveText:
                        textSize = new Vector2(bgWidth * 0.9f, bgHeight * 0.5f);
                        imgPos = new Vector3(0, bgCenterY + (bgHeight * 0.3f), -1f);
                        textPos = new Vector3(0, bgCenterY - (bgHeight * 0.05f), -1f);
                        break;
                    case PopupImagePosition.BelowText:
                        textSize = new Vector2(bgWidth * 0.9f, bgHeight * 0.5f);
                        textPos = new Vector3(0, bgCenterY + (bgHeight * 0.2f), -1f);
                        imgPos = new Vector3(0, bgCenterY - (bgHeight * 0.3f), -1f);
                        break;
                    case PopupImagePosition.LeftOfText:
                        textSize = new Vector2(bgWidth * 0.55f, bgHeight * 0.8f);
                        imgPos = new Vector3(-(bgWidth * 0.3f), bgCenterY, -1f);
                        textPos = new Vector3(bgWidth * 0.2f, bgCenterY, -1f);
                        break;
                    case PopupImagePosition.RightOfText:
                        textSize = new Vector2(bgWidth * 0.55f, bgHeight * 0.8f);
                        imgPos = new Vector3(bgWidth * 0.3f, bgCenterY, -1f);
                        textPos = new Vector3(-(bgWidth * 0.2f), bgCenterY, -1f);
                        break;
                    case PopupImagePosition.OverlapText:
                        break;
                }

                if (backgroundObj is SpriteRenderer bg)
                {
                    AddImageToPopup(bg.gameObject, customImage, imageScale, imgPos);
                }
            }

            var textAreaObj = GetMemberValue(_reactorPopupType, popupInstance, "TextArea");
            if (textAreaObj is TextMeshPro tmp)
            {
                tmp.alignment = textAlignment switch
                {
                    PopupTextAlignment.Left => TextAlignmentOptions.Left,
                    PopupTextAlignment.Right => TextAlignmentOptions.Right,
                    PopupTextAlignment.Justify => TextAlignmentOptions.Justified,
                    _ => TextAlignmentOptions.Center
                };

                tmp.rectTransform.sizeDelta = textSize;
                tmp.transform.localPosition = textPos;
            }

            var backButtonObj = GetMemberValue(_reactorPopupType, popupInstance, "BackButton");
            if (backButtonObj is MonoBehaviour backButton)
            {
                btnPos.z = backButton.transform.localPosition.z;
                backButton.transform.localPosition = btnPos;
            }

            var showMethod = AccessTools.Method(_reactorPopupType, "Show");
            showMethod.Invoke(popupInstance, new object[] { message });
        }
        catch (Exception ex)
        {
            Error($"Reflection failed to hijack ReactorPopup: {ex}");
        }
    }

    private static object GetMemberValue(Type type, object instance, string name)
    {
        var prop = AccessTools.Property(type, name);
        if (prop != null) return prop.GetValue(instance, null);

        var field = AccessTools.Field(type, name);
        if (field != null) return field.GetValue(instance);

        return null;
    }

    private static void AddImageToPopup(GameObject parent, Sprite sprite, float scale, Vector3 position)
    {
        var imageObject = new GameObject("PopupImage");
        imageObject.transform.SetParent(parent.transform, false);

        imageObject.transform.localPosition = position;
        imageObject.transform.localScale = new Vector3(scale, scale, scale);

        var renderer = imageObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingLayerName = "UI";
        renderer.sortingOrder = 100;
    }
}