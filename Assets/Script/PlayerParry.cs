
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

    [Header("Parry VFX")]
    public GameObject parryStartVFX;
    public GameObject successfulParryVFX;

    [Header("VFX Spawn Settings")]
    public Transform vfxSpawnPoint;
    public Vector3 vfxOffset = Vector3.zero;
    public float vfxDestroyTime = 2f;

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
        if (Time.time < nextParryTime)
            return;

        nextParryTime = Time.time + parryCooldown;

        if (parryCoroutine != null)
            StopCoroutine(parryCoroutine);

        parryCoroutine = StartCoroutine(ParryRoutine());

        // Lokasi spawn VFX mulai parry
        SpawnParryVFX(parryStartVFX);

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
    // TRY PARRY ENEMY PROJECTILE
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

        // Lokasi spawn VFX ketika parry berhasil
        SpawnParryVFX(successfulParryVFX);

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

        // Lokasi spawn VFX ketika parry berhasil
        SpawnParryVFX(successfulParryVFX);

        nextParryTime = Time.time;

        Debug.Log("Boss projectile sudah diperintahkan untuk berbalik.");

        return true;
    }

    // =========================================
    // SPAWN PARRY VFX
    // =========================================

    void SpawnParryVFX(GameObject vfxPrefab)
    {
        if (vfxPrefab == null)
            return;

        Vector3 spawnPosition;

        if (vfxSpawnPoint != null)
        {
            spawnPosition = vfxSpawnPoint.position;
        }
        else
        {
            spawnPosition = transform.position;
        }

        // lokasi spawn vfx
        spawnPosition += vfxOffset;

        GameObject vfx = Instantiate(
            vfxPrefab,
            spawnPosition,
            Quaternion.identity
        );

        if (vfxDestroyTime > 0f)
        {
            Destroy(vfx, vfxDestroyTime);
        }
    }

    // =========================================
    // DISABLE
    // =========================================

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
