
using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 8f;
    public int damage = 10;
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
    private bool hasHit;

    private GameObject projectileVFX;
    private GameObject trailVFX;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(
        Vector2 launchDirection,
        float launchSpeed,
        int launchDamage,
        GameObject projectileOwner,
        bool isReflected)
    {
        direction = launchDirection.normalized;
        speed = launchSpeed;
        damage = launchDamage;
        owner = isReflected ? null : projectileOwner;
        hasHit = false;

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = direction * speed;
        }

        SpawnProjectileVFX();
        SpawnTrailVFX();
        //SFX bisa ditaruh disini

        Debug.Log("Boss projectile launched. Direction: " + direction);
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

        // Arahkan visual mengikuti arah projectile.
        float angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        projectileVFX.transform.localRotation =
            Quaternion.Euler(0f, 0f, angle);
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

        // Abaikan pemilik projectile.
        if (owner != null &&
            (other.gameObject == owner ||
             other.transform.IsChildOf(owner.transform)))
        {
            return;
        }

        // Abaikan bagian tubuh boss sendiri.
        if (other.GetComponentInParent<BossController>() != null ||
            other.GetComponentInParent<BossHand>() != null)
        {
            return;
        }

        // Mengenai Player.
        if (other.CompareTag("Player"))
        {
            hasHit = true;

            PlayerHealth playerHealth =
                other.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
                playerHealth.TakeDamage(damage);
            else
                Debug.LogWarning(
                    "PlayerHealth tidak ditemukan!",
                    other.gameObject
                );

            Explode();
            return;
        }

        // Mengenai objek solid.
        if (!other.isTrigger)
        {
            hasHit = true;
            Explode();
        }
    }
}
