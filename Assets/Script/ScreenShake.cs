
using UnityEngine;

public class DashScreenShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public float shakeIntensity = 0.15f;
    public float shakeDuration = 0.1f;

    private Vector3 originalPosition;
    private float shakeTimer;

    void LateUpdate()
    {
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;

            // Intensitas berkurang saat efek hampir selesai
            float strength = shakeIntensity *
                (shakeTimer / shakeDuration);

            Vector2 offset = Random.insideUnitCircle * strength;

            transform.localPosition = originalPosition +
                new Vector3(offset.x, offset.y, 0f);
        }
        else
        {
            // Kembalikan kamera ke posisi semula
            transform.localPosition = originalPosition;
        }
    }

    void Awake()
    {
        originalPosition = transform.localPosition;
    }

    public void TriggerShake(float intensity, float duration)
    {
        shakeIntensity = intensity;
        shakeDuration = duration;
        shakeTimer = duration;
    }
}
