using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 8f;
    public int damage = 10;
    public int reflectedDamage = 25;
    public float lifetime = 5f;

    [Header("Projectile VFX")]
    public GameObject projectileVFXPrefab;
    public GameObject trailVFXPrefab;
    public GameObject explosionVFXPrefab;

    [Header("VFX Settings")]
    public Vector3 projectileVFXOffset = Vector3.zero;
    public Vector3 trailVFXOffset = Vector3.zero;
    public Vector3 explosionVFXOffset = Vector3.zero;

    private Vector2 direction = Vector2.left;
    private GameObject owner;
    private Rigidbody2D rb;
    private Collider2D projectileCollider;
    private bool hasHit;
    private bool isReflected;

    private GameObject projectileVFX;
    private GameObject trailVFX;

    public bool IsReflected => isReflected;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        projectileCollider = GetComponent<Collider2D>();
    }

    public void Launch(
        Vector2 launchDirection,
        float launchSpeed,
        int launchDamage,
        GameObject projectileOwner,
        bool isReflectedProjectile)
    {
        direction = launchDirection.normalized;
        speed = launchSpeed;
        damage = launchDamage;
        owner = isReflectedProjectile ? null : projectileOwner;
        hasHit = false;
        isReflected = isReflectedProjectile;

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = direction * speed;
        }

        SpawnProjectileVFX();
        SpawnTrailVFX();

        Debug.Log("Boss projectile launched. Direction: " + direction);
    }

    public void ReflectBack()
    {
        if (isReflected || hasHit)
            return;

        isReflected = true;
        owner = null;

        direction = -direction;

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.right;

        if (rb != null)
            rb.linearVelocity = direction * speed;

        Debug.Log("BOSS PROJECTILE BERHASIL DIPANTULKAN!");
        Debug.Log("Arah pantulan: " + direction);
    }

    void SpawnProjectileVFX()
    {
        if (projectileVFXPrefab == null)
            return;

        projectileVFX = Instantiate(
            projectileVFXPrefab,
            transform.position + projectileVFXOffset,
            Quaternion.identity,
            transform
        );

        float angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        projectileVFX.transform.localRotation =
            Quaternion.Euler(0f, 0f, angle);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("Projectile");
    }

    void SpawnTrailVFX()
    {
        if (trailVFXPrefab == null)
            return;

        trailVFX = Instantiate(
            trailVFXPrefab,
            transform.position + trailVFXOffset,
            Quaternion.identity,
            transform
        );
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (rb == null && !hasHit)
        {
            transform.position +=
                (Vector3)(direction * speed * Time.deltaTime);
        }
    }

    void Explode()
    {
        if (explosionVFXPrefab != null)
        {
            Instantiate(
                explosionVFXPrefab,
                transform.position + explosionVFXOffset,
                Quaternion.identity
            );
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit)
            return;

        // Abaikan pemilik projectile (boss) saat projectile belum dipantulkan.
        if (owner != null &&
            (other.gameObject == owner ||
             other.transform.IsChildOf(owner.transform)))
        {
            return;
        }

        // Abaikan boss hanya saat projectile belum dipantulkan.
        // Saat sudah dipantulkan, projectile boleh mengenai boss/tangan boss.
        if (!isReflected &&
            (other.GetComponentInParent<BossController>() != null ||
             other.GetComponentInParent<BossHand>() != null ||
             other.GetComponentInParent<BossHead>() != null))
        {
            return;
        }

        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        PlayerParry playerParry =
            other.GetComponentInParent<PlayerParry>();

        bool isPlayer =
            playerHealth != null ||
            playerParry != null ||
            other.CompareTag("Player");

        // =========================================
        // PROJECTILE SUDAH DIPANTULKAN
        // =========================================
        if (isReflected)
        {
            // Projectile pantulan tidak boleh mengenai Player.
            if (isPlayer)
                return;

            // Mengenai BossHand.
            BossHand bossHand =
                other.GetComponentInParent<BossHand>();

            if (bossHand != null)
            {
                hasHit = true;

                if (!bossHand.IsDefeated)
                    bossHand.TakeDamage(reflectedDamage);

                Debug.Log("Boss projectile pantulan mengenai BossHand!");

                Explode();
                return;
            }

            // Mengenai BossHead.
            BossHead bossHead =
                other.GetComponentInParent<BossHead>();

            if (bossHead != null)
            {
                hasHit = true;

                // Phase 2: projectile dari mulut yang dipantulkan
                // harus bisa mendamage kepala boss.
                bossHead.TakeProjectileDamage(reflectedDamage);

                Debug.Log("Boss projectile pantulan mengenai BossHead!");

                Explode();
                return;
            }

            // Mengenai BossController / body boss.
            BossController bossController =
                other.GetComponentInParent<BossController>();

            if (bossController != null)
            {
                hasHit = true;

                bossController.DamageBoss(reflectedDamage);

                Debug.Log("Boss projectile pantulan mengenai Boss!");

                Explode();
                return;
            }

            // Wall dan Ground.
            if (other.CompareTag("Wall") ||
                other.CompareTag("Ground"))
            {
                hasHit = true;
                Explode();
                return;
            }

            if (!other.isTrigger)
            {
                hasHit = true;
                Explode();
            }

            return;
        }

        // =========================================
        // MENGENAI PLAYER
        // =========================================
        if (isPlayer)
        {
            // Coba parry terlebih dahulu.
            if (playerParry != null)
            {
                Debug.Log("PLAYER PARRY TERDETEKSI (BOSS PROJECTILE)!");
                Debug.Log("IsParrying: " + playerParry.IsParrying);

                bool success = playerParry.TryParry(this);

                Debug.Log("HASIL PARRY: " + success);

                if (success)
                {
                    IgnorePlayerCollision(other, playerHealth, playerParry);
                    return;
                }
            }

            // Parry gagal, Player menerima damage.
            hasHit = true;

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            else
            {
                other.SendMessageUpwards(
                    "TakeDamage",
                    damage,
                    SendMessageOptions.DontRequireReceiver
                );
            }

            Debug.Log("Player terkena boss projectile!");

            Explode();
            return;
        }

        // =========================================
        // OBJEK SOLID
        // =========================================
        if (!other.isTrigger)
        {
            hasHit = true;
            Explode();
        }
    }

    void IgnorePlayerCollision(
        Collider2D other,
        PlayerHealth playerHealth,
        PlayerParry playerParry)
    {
        Transform playerRoot = null;

        if (playerHealth != null)
        {
            playerRoot = playerHealth.transform;
        }
        else if (playerParry != null)
        {
            playerRoot = playerParry.transform;
        }
        else
        {
            playerRoot = other.transform;
        }

        Collider2D[] playerColliders =
            playerRoot.GetComponentsInChildren<Collider2D>(true);

        foreach (Collider2D playerCollider in playerColliders)
        {
            if (playerCollider != null &&
                projectileCollider != null)
            {
                Physics2D.IgnoreCollision(
                    projectileCollider,
                    playerCollider,
                    true
                );
            }
        }
    }
}