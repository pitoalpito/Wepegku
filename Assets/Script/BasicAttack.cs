
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public KeyCode attackKey = KeyCode.J;
    public KeyCode uppercutKey = KeyCode.K;
    public float attackDuration = 0.15f;
    public int attackDamage = 10;
    public float attackCooldown = 0.3f;

    [Header("Hitbox Settings")]
    public Transform attackHitbox;
    public BoxCollider2D attackCollider;
    public SpriteRenderer hitboxRenderer;

    public Vector2 hitboxSize = new Vector2(1.2f, 0.8f);
    public float hitboxOffsetX = 0.8f;
    public float hitboxOffsetY = 0f;

    [Header("Attack VFX")]
    public GameObject basicAttackVFXPrefab;
    public GameObject uppercutVFXPrefab;
    public Transform attackVFXPoint;
    public Vector2 attackVFXOffset = new Vector2(0.5f, 0f);

    [Header("Boss Reference")]
    public BossController bossController;

    [Header("Debug Visual")]
    public Color hitboxColor = new Color(1f, 0f, 0f, 0.5f);

    private int facingDirection = 1;
    private Coroutine attackCoroutine;
    private Vector3 initialLocalPosition;
    private float nextAttackTime;

    private readonly HashSet<EnemyHitReaction> hitEnemies =
        new HashSet<EnemyHitReaction>();

    private readonly HashSet<EnemyProjectile> hitProjectiles =
        new HashSet<EnemyProjectile>();

    private readonly HashSet<BossHand> hitBossHands =
        new HashSet<BossHand>();

    private readonly HashSet<BossHead> hitBossHeads =
        new HashSet<BossHead>();

    public EnemyHitReaction.AttackType CurrentAttackType
    {
        get;
        private set;
    } = EnemyHitReaction.AttackType.Basic;

    void Start()
    {
        if (attackHitbox == null)
        {
            Debug.LogError("AttackHitbox belum dipasang!", this);
            enabled = false;
            return;
        }

        if (attackCollider == null)
            attackCollider = attackHitbox.GetComponent<BoxCollider2D>();

        if (hitboxRenderer == null)
            hitboxRenderer = attackHitbox.GetComponent<SpriteRenderer>();

        if (attackCollider == null)
        {
            Debug.LogError("BoxCollider2D hitbox tidak ditemukan!", this);
            enabled = false;
            return;
        }

        initialLocalPosition = attackHitbox.localPosition;

        attackCollider.isTrigger = true;
        attackCollider.size = hitboxSize;
        attackCollider.enabled = false;

        if (hitboxRenderer != null)
        {
            hitboxRenderer.color = hitboxColor;
            hitboxRenderer.enabled = false;
        }

        UpdateHitboxPosition();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.LeftArrow))
        {
            facingDirection = -1;
        }

        if (Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.RightArrow))
        {
            facingDirection = 1;
        }

        if (Input.GetKeyDown(attackKey))
            StartAttack(EnemyHitReaction.AttackType.Basic);

        if (Input.GetKeyDown(uppercutKey))
            StartAttack(EnemyHitReaction.AttackType.Uppercut);
    }

    void StartAttack(EnemyHitReaction.AttackType attackType)
    {
        if (attackCollider == null)
            return;

        // Cek cooldown.
        if (Time.time < nextAttackTime)
            return;

        // Mulai cooldown.
        nextAttackTime = Time.time + attackCooldown;

        CurrentAttackType = attackType;

        // Spawn VFX sesuai jenis serangan.
        SpawnAttackVFX(attackType);

        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        attackCollider.enabled = false;

        if (hitboxRenderer != null)
            hitboxRenderer.enabled = false;

        hitEnemies.Clear();
        hitProjectiles.Clear();
        hitBossHands.Clear();
        hitBossHeads.Clear();

        UpdateHitboxPosition();

        attackCollider.enabled = true;

        if (hitboxRenderer != null)
            hitboxRenderer.enabled = true;

        Debug.Log("Player menyerang: " + attackType);

        attackCoroutine = StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.useLayerMask = false;

        Collider2D[] results = new Collider2D[50];
        float elapsed = 0f;

        while (elapsed < attackDuration)
        {
            if (attackCollider == null || !attackCollider.enabled)
                break;

            int count = attackCollider.Overlap(filter, results);

            for (int i = 0; i < count; i++)
            {
                Collider2D hit = results[i];

                if (hit == null)
                    continue;

                // Abaikan collider milik player sendiri.
                if (hit.transform == transform ||
                    hit.transform.IsChildOf(transform))
                {
                    continue;
                }

                // 1. Cek projectile terlebih dahulu.
                EnemyProjectile projectile =
                    hit.GetComponent<EnemyProjectile>();

                if (projectile == null)
                    projectile =
                        hit.GetComponentInParent<EnemyProjectile>();

                if (projectile != null)
                {
                    if (hitProjectiles.Add(projectile))
                    {
                        projectile.ReflectBack();

                        Debug.Log(
                            "Projectile terdeteksi dan ReflectBack dipanggil!",
                            projectile
                        );
                    }

                    continue;
                }

                // 2. Cek kepala boss.
                BossHead bossHead =
                    hit.GetComponentInParent<BossHead>();

                if (bossHead != null)
                {
                    if (hitBossHeads.Add(bossHead))
                    {
                        bossHead.TakeDamage(attackDamage);

                        Debug.Log(
                            "Kepala boss terkena serangan!",
                            bossHead
                        );
                    }

                    continue;
                }

                // 3. Cek tangan boss.
                BossHand bossHand =
                    hit.GetComponent<BossHand>();

                if (bossHand == null)
                    bossHand = hit.GetComponentInParent<BossHand>();

                if (bossHand != null)
                {
                    if (hitBossHands.Add(bossHand) &&
                        !bossHand.IsDefeated)
                    {
                        bossHand.TakeDamage(attackDamage);

                        Debug.Log(
                            "Tangan boss terkena serangan! Damage: " +
                            attackDamage,
                            bossHand
                        );
                    }

                    continue;
                }

                // 4. Sistem enemy biasa.
                EnemyHitReaction target =
                    hit.GetComponent<EnemyHitReaction>();

                if (target == null)
                    target = hit.GetComponentInParent<EnemyHitReaction>();

                if (target == null || hitEnemies.Contains(target))
                    continue;

                hitEnemies.Add(target);

                Vector2 direction =
                    new Vector2(facingDirection, 0f);

                target.TakeHit(direction, CurrentAttackType);

                EnemyHealth enemyHealth =
                    target.GetComponentInParent<EnemyHealth>();

                if (enemyHealth != null && !enemyHealth.IsDead)
                    enemyHealth.TakeDamage(attackDamage);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (attackCollider != null)
            attackCollider.enabled = false;

        if (hitboxRenderer != null)
            hitboxRenderer.enabled = false;

        attackCoroutine = null;
    }

    void SpawnAttackVFX(EnemyHitReaction.AttackType attackType)
    {
        GameObject selectedVFX = null;

        // Pilih prefab berdasarkan jenis serangan.
        if (attackType == EnemyHitReaction.AttackType.Basic)
        {
            selectedVFX = basicAttackVFXPrefab;
        }
        else if (attackType == EnemyHitReaction.AttackType.Uppercut)
        {
            selectedVFX = uppercutVFXPrefab;
        }

        if (selectedVFX == null)
            return;

        // Tentukan titik kemunculan VFX.
        Transform spawnPoint = attackVFXPoint != null
            ? attackVFXPoint
            : attackHitbox;

        if (spawnPoint == null)
            return;

        Vector3 spawnPosition = spawnPoint.position;

        spawnPosition.x += attackVFXOffset.x * facingDirection;
        spawnPosition.y += attackVFXOffset.y;

        // Buat VFX.
        GameObject vfx = Instantiate(
            selectedVFX,
            spawnPosition,
            Quaternion.identity
        );

        // Sesuaikan arah VFX dengan arah pemain.
        Vector3 scale = vfx.transform.localScale;
        scale.x *= facingDirection;
        vfx.transform.localScale = scale;
    }

    void UpdateHitboxPosition()
    {
        if (attackHitbox == null)
            return;

        attackHitbox.localPosition = new Vector3(
            initialLocalPosition.x + hitboxOffsetX * facingDirection,
            initialLocalPosition.y + hitboxOffsetY,
            initialLocalPosition.z
        );
    }

    void OnDisable()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        if (attackCollider != null)
            attackCollider.enabled = false;

        if (hitboxRenderer != null)
            hitboxRenderer.enabled = false;

        hitEnemies.Clear();
        hitProjectiles.Clear();
        hitBossHands.Clear();
        hitBossHeads.Clear();
    }
}
