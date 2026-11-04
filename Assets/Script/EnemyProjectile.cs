
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
    private bool isReflected = false;
    private bool isDestroyed = false;

    public bool IsReflected => isReflected;

    void Start()
    {
        moveDirection = Vector2.right;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position +=
            (Vector3)(moveDirection * speed * Time.deltaTime);
    }

    // Dibutuhkan oleh EnemyShooter.cs
    public void SetShooter(Transform owner)
    {
        shooter = owner;
    }

    // Dibutuhkan oleh BasicAttack.cs
    public void ReflectBack()
    {
        if (isReflected || isDestroyed)
            return;

        isReflected = true;
        moveDirection = -moveDirection;

        Debug.Log("Peluru berhasil dipukul balik!");
    }

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

        // Abaikan shooter selama peluru belum dipukul balik.
        if (!isReflected && shooter != null &&
            (other.transform == shooter ||
             other.transform.IsChildOf(shooter)))
        {
            return;
        }

        // Peluru yang dipukul balik mengenai shooter.
        if (isReflected && shooter != null &&
            (other.transform == shooter ||
             other.transform.IsChildOf(shooter)))
        {
            isDestroyed = true;

            EnemyHealth health = shooter.GetComponent<EnemyHealth>();

            if (health != null && !health.IsDead)
                health.TakeDamage(reflectedDamage);

            Destroy(gameObject);
            return;
        }

        // Kena Player: damage lalu hancur.
        if (other.CompareTag("Player"))
        {
            isDestroyed = true;

            other.SendMessage(
                "TakeDamage",
                damage,
                SendMessageOptions.DontRequireReceiver
            );

            Destroy(gameObject);
            return;
        }

        // Kena Wall atau Ground: hancur.
        if (other.CompareTag("Wall") ||
            other.CompareTag("Ground"))
        {
            isDestroyed = true;
            Destroy(gameObject);
        }
    }
}
