
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 8f;
    public float lifeTime = 5f;

    [Header("Damage")]
    public int damage = 10;
    public int reflectedDamage = 25;

    private Vector2 moveDirection = Vector2.right;

    private Transform shooter;
    private Rigidbody2D rb;
    private Collider2D projectileCollider;

    private bool isReflected = false;
    private bool isDestroyed = false;

    public bool IsReflected => isReflected;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        projectileCollider = GetComponent<Collider2D>();

        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        moveDirection = Vector2.right;

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (isDestroyed)
            return;

        transform.position +=
            (Vector3)(moveDirection * speed * Time.deltaTime);
    }

    // Dipanggil oleh EnemyShooter.cs
    public void SetShooter(Transform owner)
    {
        shooter = owner;
    }

    // =====================================
    // REFLECT PROJECTILE
    // =====================================

    public void ReflectBack()
    {
        if (isReflected || isDestroyed)
            return;

        isReflected = true;

        // Arahkan projectile kembali ke posisi shooter.
        if (shooter != null)
        {
            Vector2 directionToShooter =
                (Vector2)(shooter.position - transform.position);

            if (directionToShooter.sqrMagnitude > 0.001f)
            {
                moveDirection = directionToShooter.normalized;
            }
            else
            {
                moveDirection = -moveDirection;
            }
        }
        else
        {
            moveDirection = -moveDirection;
        }

        Debug.Log("PROJECTILE BERHASIL DIPANTULKAN!");
        Debug.Log("Arah pantulan: " + moveDirection);
    }

    // =====================================
    // COLLISION
    // =====================================

    void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.collider);
    }

    void HandleHit(Collider2D other)
    {
        if (isDestroyed || other == null)
            return;

        // =====================================
        // DETEKSI PLAYER
        // =====================================

        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        PlayerParry playerParry =
            other.GetComponentInParent<PlayerParry>();

        bool isPlayer =
            playerHealth != null ||
            playerParry != null ||
            other.CompareTag("Player");

        // =====================================
        // PROJECTILE SUDAH DIPANTULKAN
        // =====================================

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
                isDestroyed = true;

                if (!bossHand.IsDefeated)
                {
                    bossHand.TakeDamage(reflectedDamage);
                    Debug.Log("Projectile mengenai BossHand!");
                }

                Destroy(gameObject);
                return;
            }

            // Mengenai BossHead.
            BossHead bossHead =
                other.GetComponentInParent<BossHead>();

            if (bossHead != null)
            {
                isDestroyed = true;

                bossHead.TakeDamage(reflectedDamage);

                Debug.Log("Projectile mengenai BossHead!");

                Destroy(gameObject);
                return;
            }

            // Mengenai shooter utama.
            if (shooter != null &&
                (other.transform == shooter ||
                 other.transform.IsChildOf(shooter)))
            {
                isDestroyed = true;

                EnemyHealth health =
                    shooter.GetComponentInParent<EnemyHealth>();

                if (health != null && !health.IsDead)
                {
                    health.TakeDamage(reflectedDamage);
                    Debug.Log("Projectile mengenai shooter!");
                }

                Destroy(gameObject);
                return;
            }

            // Wall dan Ground.
            if (other.CompareTag("Wall") ||
                other.CompareTag("Ground"))
            {
                isDestroyed = true;
                Destroy(gameObject);
            }

            return;
        }

        // =====================================
        // SHOOTER ASLI
        // =====================================

        // Projectile belum dipantulkan, abaikan shooter.
        if (shooter != null &&
            (other.transform == shooter ||
             other.transform.IsChildOf(shooter)))
        {
            return;
        }

        // =====================================
        // MENGENAI PLAYER
        // =====================================

        if (isPlayer)
        {
            // Coba parry terlebih dahulu.
            if (playerParry != null)
            {
                Debug.Log("PLAYER PARRY TERDETEKSI!");
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
            isDestroyed = true;

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

            Debug.Log("Player terkena projectile!");

            Destroy(gameObject);
            return;
        }

        // =====================================
        // WALL DAN GROUND
        // =====================================

        if (other.CompareTag("Wall") ||
            other.CompareTag("Ground"))
        {
            isDestroyed = true;
            Destroy(gameObject);
        }
    }

    // =====================================
    // IGNORE COLLISION PLAYER
    // =====================================

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
