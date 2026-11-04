
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;

    [Header("Item Drop")]
    public GameObject dropPrefab;
    [Range(0f, 1f)]
    public float dropChance = 1f;
    public float dropForceX = 2f;
    public float dropForceY = 4f;

    public int CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    private Rigidbody2D rb;
    private EnemyChase enemyChase;
    private EnemyHitReaction hitReaction;
    private Collider2D[] colliders;

    void Awake()
    {
        CurrentHealth = maxHealth;

        rb = GetComponent<Rigidbody2D>();
        enemyChase = GetComponent<EnemyChase>();
        hitReaction = GetComponent<EnemyHitReaction>();
        colliders = GetComponentsInChildren<Collider2D>();
    }

    public void TakeDamage(int damage)
    {
        if (IsDead || damage <= 0)
            return;

        CurrentHealth -= damage;
        CurrentHealth = Mathf.Max(CurrentHealth, 0);

        Debug.Log(
            gameObject.name + " HP: " +
            CurrentHealth + "/" + maxHealth
        );

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (IsDead)
            return;

        IsDead = true;

        // Hentikan AI dan efek hit.
        if (enemyChase != null)
            enemyChase.enabled = false;

        if (hitReaction != null)
            hitReaction.enabled = false;

        // Hentikan fisika enemy.
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // Matikan collider supaya enemy tidak menghalangi player.
        foreach (Collider2D col in colliders)
        {
            if (col != null)
                col.enabled = false;
        }

        // Drop item.
        if (dropPrefab != null && Random.value <= dropChance)
        {
            GameObject droppedItem = Instantiate(
                dropPrefab,
                transform.position,
                Quaternion.identity
            );

            Rigidbody2D itemRb =
                droppedItem.GetComponent<Rigidbody2D>();

            if (itemRb != null)
            {
                float directionX =
                    Random.value < 0.5f ? -1f : 1f;

                itemRb.linearVelocity = new Vector2(
                    directionX * dropForceX,
                    dropForceY
                );
            }
        }

        Debug.Log(gameObject.name + " mati!");

        // Beri jeda singkat sebelum menghapus enemy.
        Destroy(gameObject, 0.2f);
    }
}
