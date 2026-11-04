
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ThrownObjectCollision : MonoBehaviour
{
    [Header("Collision Damage")]
    public int enemyDamage = 15;
    public int objectDamage = 10;

    [Header("Boss Damage")]
    public int bossHandDamage = 40;

    [Header("Object Bounce")]
    [Range(0f, 1.5f)]
    public float bounceSpeedMultiplier = 0.8f;

    public float minimumBounceSpeed = 5f;

    private Rigidbody2D rb;
    private EnemyHitReaction objectHitReaction;
    private ObjectHealth objectHealth;

    // Mencegah enemy menerima damage berulang.
    private readonly HashSet<EnemyHealth> hitEnemies = new();

    // Mencegah tangan boss menerima damage berulang.
    private readonly HashSet<BossHand> hitBossHands = new();

    private bool wasInHitMotion;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        objectHitReaction = GetComponent<EnemyHitReaction>();
        objectHealth = GetComponent<ObjectHealth>();
    }

    void Update()
    {
        if (objectHitReaction == null)
            return;

        bool isInHitMotion =
            objectHitReaction.IsInHitMotion ||
            objectHitReaction.IsStunned;

        // Reset daftar hit ketika gerakan lempar selesai.
        if (wasInHitMotion && !isInHitMotion)
        {
            hitEnemies.Clear();
            hitBossHands.Clear();
        }

        wasInHitMotion = isInHitMotion;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (objectHitReaction == null || rb == null)
            return;

        // Hanya proses tabrakan ketika objek sedang dilempar.
        if (!objectHitReaction.IsInHitMotion &&
            !objectHitReaction.IsStunned)
        {
            return;
        }

        // =====================================
        // 1. TABRAKAN DENGAN TANGAN BOSS
        // =====================================

        BossHand bossHand =
            collision.collider.GetComponentInParent<BossHand>();

        if (bossHand != null)
        {
            if (bossHand.IsDefeated)
                return;

            // Pastikan tangan yang sama tidak diproses ulang.
            if (!hitBossHands.Add(bossHand))
                return;

            int damage = bossHandDamage;

            // Gunakan nilai rockDamage dari BossController
            // jika referensinya tersedia.
            if (bossHand.bossController != null)
            {
                damage = bossHand.bossController.rockDamage;
            }

            // Berikan damage ke tangan boss.
            bossHand.TakeDamage(damage);

            Debug.Log(
                "Batu mengenai tangan boss! Damage: " + damage,
                bossHand
            );

            // Hancurkan batu setelah memberikan damage.
            Destroy(gameObject);

            return;
        }

        // =====================================
        // 2. TABRAKAN DENGAN ENEMY BIASA
        // =====================================

        EnemyHitReaction enemyReaction =
            collision.collider.GetComponentInParent<EnemyHitReaction>();

        if (enemyReaction == null ||
            enemyReaction == objectHitReaction)
        {
            return;
        }

        EnemyHealth enemyHealth =
            enemyReaction.GetComponentInParent<EnemyHealth>();

        // Target harus memiliki EnemyHealth.
        if (enemyHealth == null || enemyHealth.IsDead)
            return;

        // Cegah damage berulang pada enemy yang sama.
        if (!hitEnemies.Add(enemyHealth))
            return;

        // Arah dari batu menuju enemy.
        Vector2 direction =
            enemyReaction.transform.position - transform.position;

        if (direction.sqrMagnitude < 0.001f)
            direction = -rb.linearVelocity;

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector2.right;

        direction.Normalize();

        // Reaksi enemy menggunakan serangan Basic.
        enemyReaction.TakeHit(
            direction,
            EnemyHitReaction.AttackType.Basic
        );

        // Berikan damage ke enemy.
        enemyHealth.TakeDamage(enemyDamage);

        // Batu memantul menjauhi enemy.
        BounceObject(enemyReaction.transform.position);

        // Damage balik ke batu hanya saat mengenai enemy biasa.
        if (objectHealth != null)
        {
            objectHealth.TakeDamage(objectDamage);
        }
    }

    void BounceObject(Vector2 enemyPosition)
    {
        Vector2 awayFromEnemy =
            (Vector2)transform.position - enemyPosition;

        if (awayFromEnemy.sqrMagnitude < 0.001f)
        {
            awayFromEnemy = -rb.linearVelocity;
        }

        if (awayFromEnemy.sqrMagnitude < 0.001f)
        {
            awayFromEnemy = Vector2.right;
        }

        awayFromEnemy.Normalize();

        // Hitung pantulan berdasarkan velocity batu.
        Vector2 incomingVelocity = rb.linearVelocity;
        Vector2 bouncedVelocity;

        if (incomingVelocity.sqrMagnitude > 0.01f)
        {
            bouncedVelocity = Vector2.Reflect(
                incomingVelocity,
                awayFromEnemy
            );

            bouncedVelocity *= bounceSpeedMultiplier;
        }
        else
        {
            bouncedVelocity =
                awayFromEnemy * minimumBounceSpeed;
        }

        // Pastikan batu bergerak menjauhi enemy.
        if (Vector2.Dot(bouncedVelocity, awayFromEnemy) < 0f)
        {
            bouncedVelocity = Vector2.Reflect(
                bouncedVelocity,
                awayFromEnemy
            );
        }

        // Pertahankan kecepatan minimum pantulan.
        if (bouncedVelocity.magnitude < minimumBounceSpeed)
        {
            bouncedVelocity =
                awayFromEnemy * minimumBounceSpeed;
        }

        rb.linearVelocity = bouncedVelocity;
    }
}
