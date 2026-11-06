using System.Collections;
using UnityEngine;

public class BossHand : MonoBehaviour
{
    [Header("Hand Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("References")]
    public BossController bossController;
    public SpriteRenderer spriteRenderer;
    public Collider2D handCollider;

    [Header("Hit Reaction")]
    public Color hitColor = Color.white;
    public float flashDuration = 0.08f;
    public float knockbackDistance = 0.2f;
    public float knockbackDuration = 0.08f;
    public float fadeOutDuration = 0.15f;

    [Header("Missile Recoil")]
    public Transform handRoot;
    public float recoilDistance = 0.25f;
    public float recoilDuration = 0.08f;
    public float recoilReturnDuration = 0.18f;

    [Header("Recovery")]
    public bool IsSlamRecovery { get; private set; }
    public bool IsDefeated { get; private set; }

    private Color originalColor;
    private Vector3 originalLocalPosition;
    private Vector3 originalHandRootPosition;

    private Coroutine hitRoutine;
    private Coroutine recoilRoutine;

    void Awake()
    {
        currentHealth = maxHealth;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (handCollider == null)
            handCollider = GetComponent<Collider2D>();

        // Parent LeftHand / RightHand menjadi target animasi.
        if (handRoot == null)
            handRoot = transform.parent;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        // Posisi awal sprite child.
        originalLocalPosition = transform.localPosition;

        // Posisi awal parent tangan.
        if (handRoot != null)
            originalHandRootPosition = handRoot.localPosition;
    }

    // =========================================
    // MISSILE RECOIL
    // =========================================

    public void PlayMissileRecoil()
    {
        if (IsDefeated || handRoot == null)
            return;

        // Jika recoil sebelumnya belum selesai, hentikan dahulu.
        if (recoilRoutine != null)
        {
            StopCoroutine(recoilRoutine);

            handRoot.localPosition = originalHandRootPosition;
        }

        recoilRoutine = StartCoroutine(MissileRecoilRoutine());
    }

    IEnumerator MissileRecoilRoutine()
    {
        // Tentukan arah recoil berdasarkan posisi tangan.
        // Tangan kiri mundur ke kiri, tangan kanan mundur ke kanan.
        float direction = -1f;

        if (bossController != null)
        {
            direction = handRoot.position.x <
                        bossController.transform.position.x
                ? -1f : 1f;
        }

        Vector3 startPosition = originalHandRootPosition;

        Vector3 recoilPosition = startPosition +
            new Vector3(direction * recoilDistance, 0f, 0f);

        // 1. Tangan bergerak mundur dengan cepat.
        float elapsed = 0f;

        while (elapsed < recoilDuration)
        {
            float t = elapsed / recoilDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            handRoot.localPosition = Vector3.Lerp(
                startPosition,
                recoilPosition,
                t
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        handRoot.localPosition = recoilPosition;

        // 2. Tangan kembali ke posisi awal.
        elapsed = 0f;

        while (elapsed < recoilReturnDuration)
        {
            float t = elapsed / recoilReturnDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            handRoot.localPosition = Vector3.Lerp(
                recoilPosition,
                startPosition,
                t
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        handRoot.localPosition = startPosition;
        recoilRoutine = null;
    }

    // =========================================
    // DAMAGE
    // =========================================

    public void TakeDamage(int damage)
    {
        if (IsDefeated || damage <= 0)
            return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        Debug.Log(
            name + " terkena " + damage +
            " damage. HP: " + currentHealth + "/" + maxHealth
        );

        if (IsSlamRecovery && bossController != null)
            bossController.EndHandRecovery(this);

        if (hitRoutine != null)
            StopCoroutine(hitRoutine);

        hitRoutine = StartCoroutine(HitReactionRoutine());

        if (currentHealth <= 0)
        {
            IsDefeated = true;

            if (handCollider != null)
                handCollider.enabled = false;
            
            // gameObject.SetActive(false);

            Debug.Log(name + " berhasil dikalahkan!");

            if (bossController != null)
                bossController.OnHandDefeated(this);
        }
    }

    IEnumerator HitReactionRoutine()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = hitColor;

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

        // 3. Mengembalikan posisi tangan
        elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            float t = elapsed / knockbackDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localPosition = Vector3.Lerp(
                targetPosition,
                originalLocalPosition,
                t
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalLocalPosition;

        // 4. Smooth fade dari putih ke warna asli
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

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        hitRoutine = null;
    }

    public void SetSlamRecovery(bool value)
    {
        if (IsDefeated)
            return;

        IsSlamRecovery = value;
    }
}