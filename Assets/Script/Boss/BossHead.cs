
using System.Collections;
using UnityEngine;

public class BossHead : MonoBehaviour
{
    [Header("References")]
    public BossController bossController;
    public SpriteRenderer spriteRenderer;
    public Collider2D headCollider;

    [Header("Hit Sprites")]
    public Sprite hitSprite;

    [Header("Hit Reaction")]
    public Color hitColor = Color.white;
    public float flashDuration = 0.08f;
    public float knockbackDistance = 0.2f;
    public float knockbackDuration = 0.08f;
    public float fadeOutDuration = 0.15f;

    [Header("Head Slam Damage")]
    public int slamDamage = 20;

    private Color originalColor;
    private Sprite originalSprite;

    private Coroutine hitRoutine;
    private bool hasHitPlayer;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (headCollider == null)
            headCollider = GetComponent<Collider2D>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            originalSprite = spriteRenderer.sprite;
        }
    }

    // =========================================
    // DAMAGE
    // =========================================

    public void TakeDamage(int damage)
    {
        if (bossController == null ||
            !bossController.IsHeadVulnerable ||
            damage <= 0)
            return;

        bossController.DamageHead(damage);

        if (hitRoutine != null)
            StopCoroutine(hitRoutine);

        hitRoutine = StartCoroutine(HitReactionRoutine());
    }

    // =========================================
    // PROJECTILE REFLECTED DAMAGE
    // =========================================
    // Dipakai ketika projectile yang dipantulkan
    // (terutama projectile mulut pada Phase 2)
    // mengenai kepala boss. Tidak bergantung pada
    // status vulnerable agar counter parry tetap
    // memberikan damage.
    public void TakeProjectileDamage(int damage)
    {
        if (bossController == null || damage <= 0)
            return;

        bossController.DamageBoss(damage);

        if (hitRoutine != null)
            StopCoroutine(hitRoutine);

        hitRoutine = StartCoroutine(HitReactionRoutine());
    }

    // =========================================
    // HEAD SLAM DAMAGE
    // =========================================

    public void ResetSlamDamage()
    {
        hasHitPlayer = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (bossController == null || !bossController.IsHeadSlamming)
            return;

        if (hasHitPlayer)
            return;

        if (!other.CompareTag("Player"))
            return;

        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogWarning("PlayerHealth tidak ditemukan!");
            return;
        }

        hasHitPlayer = true;
        playerHealth.TakeDamage(slamDamage);

        Debug.Log("Player terkena Head Slam!", other.gameObject);
    }

    // =========================================
    // HIT REACTION
    // =========================================

    IEnumerator HitReactionRoutine()
    {
        if (spriteRenderer != null)
        {
            if (hitSprite != null)
                spriteRenderer.sprite = hitSprite;

            spriteRenderer.color = hitColor;
        }

        float direction = 1f;

        if (bossController != null && bossController.player != null)
        {
            direction = transform.position.x <
                        bossController.player.position.x ? -1f : 1f;
        }

        Vector3 startPosition = transform.localPosition;

        Vector3 targetPosition = startPosition +
            new Vector3(direction * knockbackDistance, 0f, 0f);

        // 1. Knockback
        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            float t = elapsed / knockbackDuration;

            transform.localPosition = Vector3.Lerp(
                startPosition,
                targetPosition,
                t
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPosition;

        // 2. Menunggu flash sebentar
        yield return new WaitForSeconds(flashDuration);

        // 3. Mengembalikan posisi kepala
        elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            float t = elapsed / knockbackDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localPosition = Vector3.Lerp(
                targetPosition,
                startPosition,
                t
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = startPosition;

        // 4. Mengembalikan warna secara perlahan
        elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            float t = elapsed / fadeOutDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(
                    hitColor,
                    originalColor,
                    t
                );
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 5. Mengembalikan sprite dan warna asli
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = originalSprite;
            spriteRenderer.color = originalColor;
        }

        hitRoutine = null;
    }
}
