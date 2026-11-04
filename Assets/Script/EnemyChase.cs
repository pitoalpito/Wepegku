
using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float chaseRange = 8f;
    public float stopDistance = 1f;

    [Header("References")]
    public EnemyHitReaction hitReaction;
    public SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (hitReaction == null)
            hitReaction = GetComponent<EnemyHitReaction>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        // Cari player otomatis jika belum diisi Inspector.
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }
    }

    void FixedUpdate()
    {
        if (rb == null || player == null)
            return;

        // Jangan ganggu knockback, uppercut, atau wall bounce.
        if (hitReaction != null &&
            (hitReaction.IsStunned || hitReaction.IsInHitMotion))
        {
            return;
        }

        float distanceX = player.position.x - transform.position.x;
        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        // Player di luar jarak kejar.
        if (distance > chaseRange)
        {
            rb.linearVelocity = new Vector2(
                0f,
                rb.linearVelocity.y
            );

            return;
        }

        // Player sudah cukup dekat.
        if (Mathf.Abs(distanceX) <= stopDistance)
        {
            rb.linearVelocity = new Vector2(
                0f,
                rb.linearVelocity.y
            );

            return;
        }

        // Bergerak menuju player tanpa mengubah velocity vertikal.
        float directionX = Mathf.Sign(distanceX);

        rb.linearVelocity = new Vector2(
            directionX * moveSpeed,
            rb.linearVelocity.y
        );

        // Balik sprite sesuai arah player.
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = directionX < 0f;
        }
    }
}
