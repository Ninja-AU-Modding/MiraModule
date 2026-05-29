using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using System.Collections;
using UnityEngine;
using static MiraOverloaded.Assets.Assets;

namespace MiraOverloaded.Patches;

[HarmonyPatch]
internal static class SabotagedSplashPatch
{
    private static GameObject? _sabotagedLogo;
    private static bool _setupComplete;

    private const float BaseScale = 0.6f;

    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Start))]
    [HarmonyPostfix]
    private static void StartPostfix(SplashManager __instance)
    {
        _setupComplete = false;

        Transform logoRoot = __instance.logoAnimFinish.transform.Find("LogoRoot");

        _sabotagedLogo = new GameObject("SabotagedLogo");
        _sabotagedLogo.transform.SetParent(logoRoot, false);

        var spriteRenderer = _sabotagedLogo.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = SabotagedAuModdingLogo.LoadAsset();

        _sabotagedLogo.transform.localScale = new Vector3(BaseScale, BaseScale, 1f);
        _sabotagedLogo.SetActive(false);
    }

    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Update))]
    [HarmonyPostfix]
    private static void UpdatePostfix(SplashManager __instance)
    {
        if (_setupComplete || _sabotagedLogo == null) return;

        Transform logoRoot = __instance.logoAnimFinish.transform.Find("LogoRoot");
        Transform isLogo = logoRoot.Find("ISLogo");
        Transform pewLogo = logoRoot.Find("PEWLogo");

        if (pewLogo != null && pewLogo.gameObject.activeSelf)
        {
            _setupComplete = true;

            isLogo.localPosition += new Vector3(0f, 1.75f, 0f);
            pewLogo.localPosition += new Vector3(0f, 1.75f, 0f);

            float midX = (isLogo.localPosition.x + pewLogo.localPosition.x) / 2f;
            _sabotagedLogo.transform.localPosition = new Vector3(midX, -1.75f, 0f);
            _sabotagedLogo.SetActive(true);

            var animator = __instance.logoAnimFinish.GetComponent<Animator>();
            if (animator != null)
            {
                animator.speed = 1.5f;
            }

            __instance.StartCoroutine(PulseLogoRoutine(_sabotagedLogo, 1.53f));
        }
    }

    private static IEnumerator PulseLogoRoutine(GameObject targetLogo, float startDelay)
    {
        yield return new WaitForSeconds(startDelay);
        yield return null;

        float duration = 0.2f;
        float elapsed = 0f;
        float peakScale = BaseScale * 1.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float scale = t < 0.6f
                ? Mathf.Lerp(BaseScale, peakScale, t / 0.6f)
                : Mathf.Lerp(peakScale, BaseScale, (t - 0.6f) / 0.4f);

            if (targetLogo != null)
            {
                targetLogo.transform.localScale = new Vector3(scale, scale, 1f);
            }

            yield return null;
        }

        if (targetLogo != null)
        {
            targetLogo.transform.localScale = new Vector3(BaseScale, BaseScale, 1f);
        }
    }
}