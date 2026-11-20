
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerParry : MonoBehaviour
{
    [Header("Parry Settings")]
    public KeyCode parryKey = KeyCode.E;
    public float parryDuration = 0.2f;
    public float parryCooldown = 0.5f;

    [Header("Parry Feedback")]
    public bool showDebug = true;

    [Header("Parry Cooldown UI")]
    public Slider cooldownSlider;

    public bool IsParrying { get; private set; }

    private float nextParryTime;
    private Coroutine parryCoroutine;

    void Awake()
    {
        if (cooldownSlider != null)
        {
            cooldownSlider.minValue = 0f;
            cooldownSlider.maxValue = 1f;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(parryKey))
        {
            StartParry();
        }

        UpdateCooldownUI();
    }

    void UpdateCooldownUI()
    {
        if (cooldownSlider == null)
            return;

        if (parryCooldown <= 0f)
        {
            cooldownSlider.value = 1f;
            return;
        }

        // 0 = baru dipakai, 1 = siap parry lagi.
        float cooldownStart = nextParryTime - parryCooldown;
        float progress = Mathf.Clamp01(
            (Time.time - cooldownStart) / parryCooldown
        );

        cooldownSlider.value = progress;
    }

    // =========================================
    // START PARRY
    // =========================================

    void StartParry()
    {
        // Cek cooldown.
        if (Time.time < nextParryTime)
            return;

        nextParryTime = Time.time + parryCooldown;

        if (parryCoroutine != null)
            StopCoroutine(parryCoroutine);

        parryCoroutine = StartCoroutine(ParryRoutine());

        if (showDebug)
            Debug.Log("Player mulai melakukan PARRY!");
    }

    // =========================================
    // PARRY WINDOW
    // =========================================

    IEnumerator ParryRoutine()
    {
        IsParrying = true;

        yield return new WaitForSeconds(parryDuration);

        IsParrying = false;
        parryCoroutine = null;

        if (showDebug)
            Debug.Log("Parry window berakhir.");
    }

    // =========================================
    // TRY PARRY
    // =========================================

    public bool TryParry(EnemyProjectile projectile)
    {
        if (!IsParrying || projectile == null || projectile.IsReflected)
        {
            Debug.Log("PARRY GAGAL: Kondisi tidak terpenuhi.");
            return false;
        }

        Debug.Log("PARRY BERHASIL: TryParry terpanggil!");

        projectile.ReflectBack();

        // Reset cooldown ketika parry berhasil,
        // agar player bisa parry lagi tanpa menunggu.
        nextParryTime = Time.time;

        Debug.Log("Projectile sudah diperintahkan untuk berbalik.");

        return true;
    }

    // =========================================
    // TRY PARRY BOSS PROJECTILE
    // =========================================

    public bool TryParry(BossProjectile projectile)
    {
        if (!IsParrying || projectile == null || projectile.IsReflected)
        {
            Debug.Log("PARRY GAGAL (BOSS): Kondisi tidak terpenuhi.");
            return false;
        }

        Debug.Log("PARRY BERHASIL (BOSS): TryParry terpanggil!");

        projectile.ReflectBack();

        // Reset cooldown ketika parry berhasil.
        nextParryTime = Time.time;

        Debug.Log("Boss projectile sudah diperintahkan untuk berbalik.");

        return true;
    }

    void OnDisable()
    {
        IsParrying = false;

        if (parryCoroutine != null)
        {
            StopCoroutine(parryCoroutine);
            parryCoroutine = null;
        }
    }
    
}
