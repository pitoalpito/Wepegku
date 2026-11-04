
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VignettePulse : MonoBehaviour
{
    [Header("References")]
    public Volume globalVolume;

    [Header("Vignette Settings")]
    public Color vignetteColor = Color.red;
    [Range(0f, 1f)]
    public float peakIntensity = 0.45f;

    [Range(0f, 1f)]
    public float smoothness = 0.5f;

    [Header("Timing")]
    public float fadeInDuration = 0.12f;
    public float fadeOutDuration = 0.45f;

    private Vignette vignette;
    private Coroutine pulseRoutine;

    void Awake()
    {
        if (globalVolume == null)
            globalVolume = GetComponent<Volume>();

        if (globalVolume == null)
        {
            Debug.LogError("Global Volume belum di-assign!");
            return;
        }

        if (!globalVolume.profile.TryGet(out vignette))
        {
            Debug.LogError("Vignette belum ditambahkan ke Volume Profile!");
            return;
        }

        vignette.active = true;
        vignette.color.Override(vignetteColor);
        vignette.smoothness.Override(smoothness);
        vignette.intensity.Override(0f);
    }

    public void TriggerVignette()
    {
        if (vignette == null) return;

        if (pulseRoutine != null)
            StopCoroutine(pulseRoutine);

        pulseRoutine = StartCoroutine(PulseRoutine());
    }

    IEnumerator PulseRoutine()
    {
        float startIntensity = vignette.intensity.value;

        // Fade In
        float timer = 0f;

        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;

            float t = fadeInDuration > 0f
                ? Mathf.Clamp01(timer / fadeInDuration)
                : 1f;

            vignette.intensity.value =
                Mathf.Lerp(startIntensity, peakIntensity, t);

            yield return null;
        }

        vignette.intensity.value = peakIntensity;

        // Fade Out
        timer = 0f;

        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;

            float t = fadeOutDuration > 0f
                ? Mathf.Clamp01(timer / fadeOutDuration)
                : 1f;

            vignette.intensity.value =
                Mathf.Lerp(peakIntensity, 0f, t);

            yield return null;
        }

        vignette.intensity.value = 0f;
        pulseRoutine = null;
    }
}
