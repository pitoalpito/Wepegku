using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossController : MonoBehaviour
{
    public enum BossState
    {
        Intro,
        LaserTelegraph,
        LaserAttack,
        VolleyTelegraph,
        VolleyAttack,
        SlamTelegraph,
        SlamAttack,
        SlamRecovery,
        HeadSlamTelegraph,
        HeadSlamAttack,
        HeadStunned,
        PhaseTransition,
        Dead
    }

    [Header("Phase")]
    public int phase = 1;
    public int maxHealth = 500;
    public int currentHealth = 500;

    [Header("Hands")]
    public BossHand leftHand;
    public BossHand rightHand;

    [Header("Camera Effects")]
    public VignettePulse cameraVignette;

    [Header("Eyes")]
    public Transform leftLaserOrigin;
    public Transform rightLaserOrigin;

    [Header("Arena")]
    public Transform player;
    public GameObject fallingRockPrefab;

    [Header("Falling Rock Spawn")]
    public float fallingRockMinX = -10f;
    public float fallingRockMaxX = 10f;
    public float fallingRockSpawnY = 8f;

    [Header("Laser")]
    public LineRenderer leftLaserLine;
    public LineRenderer rightLaserLine;
    public float laserChargeTime = 1f;
    public float laserLockPause = 0.4f;
    public float laserDuration = 1.5f;
    public float laserRange = 20f;
    public int laserDamage = 15;
    public float laserDamageInterval = 0.5f;
    public LayerMask laserObstacleLayers;

    [Header("Laser Visual")]
    public float telegraphWidth = 0.06f;
    public float firingWidth = 0.18f;
    public Color laserColor = Color.red;

    [Header("Laser VFX Prefabs")]
    public ParticleSystem leftWindupPrefab;
    public ParticleSystem rightWindupPrefab;

    public ParticleSystem leftLaserVFXPrefab;
    public ParticleSystem rightLaserVFXPrefab;

    [Header("Laser VFX Settings")]
    public float windupParticleWidth = 0.3f;
    public float laserParticleWidth = 0.5f;

    private ParticleSystem leftWindupInstance;
    private ParticleSystem rightWindupInstance;

    private ParticleSystem leftLaserInstance;
    private ParticleSystem rightLaserInstance;

    [Header("Eye Animation")]
    public Transform leftEyeVisual;
    public Transform rightEyeVisual;

    [Range(0.5f, 1f)]
    public float chargeEyeScale = 0.85f;

    public float firingEyeScale = 1.2f;

    public float pulseMinScale = 1.05f;
    public float pulseSpeed = 12f;

    public float eyeEnlargeDuration = 0.08f;

    private Vector3 leftEyeOriginalScale;
    private Vector3 rightEyeOriginalScale;

    [Header("Volley")]
    public GameObject projectilePrefab;
    public Transform leftProjectileSpawn;
    public Transform rightProjectileSpawn;
    public Transform mouthProjectileSpawn;
    public int volleyCountPhaseOne = 3;
    public int volleyCountPhaseTwo = 5;
    public float projectileInterval = 0.3f;
    public float projectileSpeed = 8f;
    public int projectileDamage = 10;

    [Header("Mouth Animation - Phase 2")]
    public Animator mouthAnimator;
    public float mouthCloseDuration = 0.35f;

    [Header("Ground Slam")]
    public float slamTelegraphTime = 1f;
    public float slamRecoveryTime = 2.5f;
    public int slamDamage = 20;

    [Header("Ground Slam Animation")]
    public Transform leftHandRoot;
    public Transform rightHandRoot;

    public float handRaiseHeight = 0.8f;
    public float handShakeDuration = 0.4f;
    public float handShakeAmount = 0.12f;
    public float handShakeSpeed = 30f;
    // public float handSlamDistance = 1.5f;

    public float slamGroundOffset = 0f;
    public float slamRaycastDistance = 100f;

    public float handRaiseDuration = 0.3f;
    public float handSlamDuration = 0.12f;
    public float handReturnDuration = 0.35f;

    private Vector3 leftHandStartPosition;
    private Vector3 rightHandStartPosition;
    private Transform leftHandHomeParent;
    private Transform rightHandHomeParent;

    [Header("Rock Counter")]
    public int rockDamage = 40;

    [Header("Phase 2 - Head Slam")]
    public Transform headRoot;
    public Collider2D headCollider;
    public float headRaiseHeight = 0.8f;
    public float headRaiseDuration = 0.35f;
    public float headSlamDuration = 0.15f;
    public float headReturnDuration = 0.4f;
    public float headStunDuration = 3f;
    public float headSlamGroundOffset = 0f;
    public float headSlamRaycastDistance = 100f;
    public float headShakeDuration = 0.35f;
    public float headShakeAmount = 0.08f;
    public float headShakeSpeed = 30f;
    public int headDamage = 20;

    private Vector3 headStartPosition;
    private bool headCanBeDamaged;

    public bool IsHeadVulnerable => headCanBeDamaged;
    public bool IsHeadSlamming => currentState == BossState.HeadSlamAttack;

    public BossState currentState = BossState.Intro;

    private Coroutine bossRoutine;
    private bool recoveryInterrupted;
    private bool isDead;

    [Header("Boss Intro")]
    public CameraFollow cameraFollow;

    [Header("Boss Screen Shake")]
    public float groundSlamShakeIntensity = 0.3f;
    public float groundSlamShakeDuration = 0.2f;
    public float headSlamShakeIntensity = 0.4f;
    public float headSlamShakeDuration = 0.25f;

    public GameObject bossIntroPanel;

    public TMP_Text bossNameText;
    public TMP_Text readyFightText;

    public float bossNameDuration = 2f;
    public float readyDuration = 1f;
    public float fightDuration = 0.8f;

    [Header("Player Control")]
    public MonoBehaviour playerMovementScript;

    [Header("Boss Movement")]
    public MonoBehaviour bossMovementScript;

    private bool bossMovementWasEnabled;

    [Header("Boss VFX Prefabs")]
    public ParticleSystem phaseTransitionVFXPrefab;
    public ParticleSystem bossDeathVFXPrefab;
    public ParticleSystem groundSlamVFXPrefab;
    public ParticleSystem headSlamVFXPrefab;


    public void OnHandDefeated(BossHand hand)
    {
        Debug.Log(hand.name + " telah dikalahkan!");

        // Jika tangan kiri kalah saat terlepas dari parent.
        if (hand == leftHand && leftHandRoot != null)
        {
            leftHandRoot.SetParent(leftHandHomeParent, true);
        }

        // Jika tangan kanan kalah saat terlepas dari parent.
        if (hand == rightHand && rightHandRoot != null)
        {
            rightHandRoot.SetParent(rightHandHomeParent, true);
        }

        if (leftHand != null && rightHand != null &&
            leftHand.IsDefeated && rightHand.IsDefeated)
        {
            Debug.Log("Kedua tangan telah dikalahkan. Boss akan memasuki Phase 2.");
        }
    }

    void Start()
    {
        Time.timeScale = 1f; // Pastikan waktu berjalan normal saat memulai
        currentHealth = maxHealth;

        // Pastikan mulut berada dalam kondisi tertutup saat boss mulai.
        SetMouthOpen(false);

        if (leftHandRoot != null)
        {
            leftHandStartPosition = leftHandRoot.localPosition;
            leftHandHomeParent = leftHandRoot.parent;
        }

        if (rightHandRoot != null)
        {
            rightHandStartPosition = rightHandRoot.localPosition;
            rightHandHomeParent = rightHandRoot.parent;
        }

        if (headRoot != null)
            headStartPosition = headRoot.localPosition;

        if (headCollider != null)
            headCollider.isTrigger = true;

        if (leftEyeVisual != null)
            leftEyeOriginalScale = leftEyeVisual.localScale;

        if (rightEyeVisual != null)
            rightEyeOriginalScale = rightEyeVisual.localScale;

        if (leftHand != null)
            leftHand.bossController = this;

        if (rightHand != null)
            rightHand.bossController = this;

        SetupLaserLine(leftLaserLine);
        SetupLaserLine(rightLaserLine);
        HideLasers();

        // Matikan kamera selama intro
        if (cameraFollow != null)
            cameraFollow.enabled = false;

        // Matikan kontrol player selama intro
        if (playerMovementScript != null)
            playerMovementScript.enabled = false;

        if (bossIntroPanel != null)
            bossIntroPanel.SetActive(false);

        if (bossNameText != null)
            bossNameText.gameObject.SetActive(false);

        if (readyFightText != null)
            readyFightText.gameObject.SetActive(false);

        bossRoutine = StartCoroutine(BossIntroSequence());
    }


    void SetupLaserLine(LineRenderer line)
    {
        if (line == null)
            return;

        line.useWorldSpace = true;
        line.positionCount = 2;
        line.startWidth = telegraphWidth;
        line.endWidth = telegraphWidth;
        line.startColor = laserColor;
        line.endColor = laserColor;
        line.enabled = false;
    }



    IEnumerator BossLoop()
    {
        currentState = BossState.Intro;
        yield return new WaitForSeconds(1f);

        while (!isDead)
        {
            // Phase 1 berakhir segera setelah kedua tangan dikalahkan.
            if (phase == 1 && AreBothHandsDefeated())
            {
                yield return StartCoroutine(EnterPhaseTwo());
                continue;
            }

            int attackPairs = 3;

            for (int i = 0; i < attackPairs; i++)
            {
                if (isDead)
                    yield break;

                if (phase == 1 && AreBothHandsDefeated())
                    break;

                Debug.Log(
                    "Phase " + phase +
                    " - Pasangan serangan " + (i + 1) + "/" + attackPairs
                );

                // 1. Projectile: tangan pada Phase 1, mulut pada Phase 2.
                yield return StartCoroutine(ProjectileVolley());
                if (isDead) yield break;

                if (phase == 1 && AreBothHandsDefeated())
                    break;

                float attackGap = phase == 1 ? 0.3f : 0.1f;
                yield return new WaitForSeconds(attackGap);
                if (isDead) yield break;

                // 2. Laser mata tetap digunakan pada kedua fase.
                yield return StartCoroutine(EyeLaserAttack());
                if (isDead) yield break;

                if (phase == 1 && AreBothHandsDefeated())
                    break;

                float pairGap = phase == 1 ? 0.4f : 0.15f;
                yield return new WaitForSeconds(pairGap);
            }

            if (isDead)
                yield break;

            if (phase == 1)
            {
                if (AreBothHandsDefeated())
                {
                    yield return StartCoroutine(EnterPhaseTwo());
                    continue;
                }

                // Ground Slam hanya untuk Phase 1.
                yield return StartCoroutine(GroundSlam());
            }
            else
            {
                // Phase 2 menggunakan Head Slam sebagai serangan penutup pola.
                yield return StartCoroutine(HeadSlam());
            }
        }
    }

    bool AreBothHandsDefeated()
    {
        return leftHand != null && rightHand != null &&
               leftHand.IsDefeated && rightHand.IsDefeated;
    }

    IEnumerator EnterPhaseTwo()
    {
        currentState = BossState.PhaseTransition;

        // LOKASI SPAWN VFX: posisi root boss saat transisi ke Phase 2.
        SpawnVFX(phaseTransitionVFXPrefab, transform.position);

        //SFX phase transition disini
        Debug.Log("Boss enters Phase 2!");

        yield return new WaitForSeconds(1.5f);

        if (!isDead)
            phase = 2;
    }

    public void EndHandRecovery(BossHand hand)
    {
        if (currentState != BossState.SlamRecovery)
            return;

        if (!hand.IsSlamRecovery)
            return;

        recoveryInterrupted = true;

        Debug.Log(hand.name + " terkena serangan saat recovery!");
    }

    public void DamageHand(BossHand hand, int damage)
    {
        if (hand != null)
            hand.TakeDamage(damage);
    }

    public void DamageBoss(int damage)
    {
        if (isDead || damage <= 0)
            return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (currentHealth <= 0)
        {
            isDead = true;
            currentState = BossState.Dead;

            if (bossRoutine != null)
                StopCoroutine(bossRoutine);

            HideLasers();

            // LOKASI SPAWN VFX: posisi root boss saat boss kalah.
            SpawnVFX(bossDeathVFXPrefab, transform.position);

            //SFX mati disini
            //fungsi selanjutnya disini
            ResultUI.Instance.MenangUI();
            Debug.Log("Boss defeated!");
        }
    }

    // =========================================
    // LASER ATTACK
    // =========================================

    IEnumerator EyeLaserAttack()
    {
        if (leftLaserOrigin == null && rightLaserOrigin == null)
        {
            Debug.LogWarning("Laser Origin belum diatur!");
            yield break;
        }

        StopBossMovement();

        currentState = BossState.LaserTelegraph;

        ShowLaserLine(leftLaserLine, telegraphWidth);
        ShowLaserLine(rightLaserLine, telegraphWidth);

        // WIND UP: muncul di mata boss, bukan sepanjang laser.
        leftWindupInstance = CreateEyeWindupVFX(
            leftWindupPrefab,
            leftLaserOrigin,
            leftWindupInstance
        );

        rightWindupInstance = CreateEyeWindupVFX(
            rightWindupPrefab,
            rightLaserOrigin,
            rightWindupInstance
        );

        float chargeTime = phase == 1
            ? laserChargeTime
            : laserChargeTime * 0.7f;

        float chargeTimer = 0f;

        ResetEyeScale();

        // CHARGING: laser tracking player.
        while (chargeTimer < chargeTime && !isDead)
        {
            float progress = chargeTimer / chargeTime;

            AnimateEyeCharging(progress);

            Vector2 leftDirection = GetLaserDirection(leftLaserOrigin);
            Vector2 rightDirection = GetLaserDirection(rightLaserOrigin);

            UpdateLaser(leftLaserLine, leftLaserOrigin, leftDirection);
            UpdateLaser(rightLaserLine, rightLaserOrigin, rightDirection);

            chargeTimer += Time.deltaTime;
            yield return null;
        }

        if (isDead)
        {
            HideLasers();
            yield break;
        }

        // LOCK ON: simpan arah terakhir.
        Vector2 lockedLeftDirection = GetLaserDirection(leftLaserOrigin);
        Vector2 lockedRightDirection = GetLaserDirection(rightLaserOrigin);

        UpdateLaser(leftLaserLine, leftLaserOrigin, lockedLeftDirection);
        UpdateLaser(rightLaserLine, rightLaserOrigin, lockedRightDirection);

        // Matikan Wind Up setelah charging.
        StopLaserVFX(leftWindupInstance);
        StopLaserVFX(rightWindupInstance);

        yield return new WaitForSeconds(laserLockPause);

        if (isDead)
        {
            HideLasers();
            yield break;
        }

        // LASER ATTACK.
        currentState = BossState.LaserAttack;

        SetLaserWidth(leftLaserLine, firingWidth);
        SetLaserWidth(rightLaserLine, firingWidth);

        // Buat Laser VFX mengikuti garis yang sudah terkunci.
        leftLaserInstance = CreateLaserBeamVFX(
            leftLaserVFXPrefab,
            leftLaserLine,
            leftLaserInstance,
            laserParticleWidth
        );

        rightLaserInstance = CreateLaserBeamVFX(
            rightLaserVFXPrefab,
            rightLaserLine,
            rightLaserInstance,
            laserParticleWidth
        );

        // Animasi mata membesar.
        float enlargeTimer = 0f;

        while (enlargeTimer < eyeEnlargeDuration && !isDead)
        {
            float t = enlargeTimer / eyeEnlargeDuration;

            float scale = Mathf.Lerp(
                chargeEyeScale,
                firingEyeScale,
                t
            );

            SetEyeScale(leftEyeVisual, leftEyeOriginalScale, scale);
            SetEyeScale(rightEyeVisual, rightEyeOriginalScale, scale);

            enlargeTimer += Time.deltaTime;
            yield return null;
        }

        if (isDead)
        {
            HideLasers();
            ResetEyeScale();
            yield break;
        }

        float fireTimer = 0f;
        float damageTimer = 0f;

        DamagePlayerOnLaser(leftLaserOrigin, lockedLeftDirection);
        DamagePlayerOnLaser(rightLaserOrigin, lockedRightDirection);

        // FIRING: arah laser terkunci.
        while (fireTimer < laserDuration && !isDead)
        {
            AnimateEyeFiring(fireTimer);

            UpdateLaser(
                leftLaserLine,
                leftLaserOrigin,
                lockedLeftDirection
            );

            UpdateLaser(
                rightLaserLine,
                rightLaserOrigin,
                lockedRightDirection
            );

            // Pastikan VFX mengikuti posisi dan panjang Line Renderer.
            UpdateLaserVFX(
                leftLaserInstance,
                leftLaserLine,
                laserParticleWidth
            );

            UpdateLaserVFX(
                rightLaserInstance,
                rightLaserLine,
                laserParticleWidth
            );

            //taruh SFX laser disini

            damageTimer += Time.deltaTime;

            if (damageTimer >= laserDamageInterval)
            {
                damageTimer = 0f;

                DamagePlayerOnLaser(leftLaserOrigin, lockedLeftDirection);
                DamagePlayerOnLaser(rightLaserOrigin, lockedRightDirection);
            }

            fireTimer += Time.deltaTime;
            yield return null;
        }

        HideLasers();
        ResetEyeScale();

        ResumeBossMovement();
    }

    void ShowLaserLine(LineRenderer line, float width)
    {
        if (line == null)
            return;

        line.enabled = true;
        line.startWidth = width;
        line.endWidth = width;
        line.startColor = laserColor;
        line.endColor = laserColor;
    }

    void SetLaserWidth(LineRenderer line, float width)
    {
        if (line == null)
            return;

        line.startWidth = width;
        line.endWidth = width;
    }

    Vector2 GetLaserDirection(Transform origin)
    {
        if (origin == null || player == null)
            return Vector2.right;

        Vector2 direction =
            (Vector2)player.position - (Vector2)origin.position;

        if (direction.sqrMagnitude < 0.001f)
            return Vector2.right;

        return direction.normalized;
    }

    void UpdateLaser(
        LineRenderer line,
        Transform origin,
        Vector2 direction)
    {
        if (line == null || origin == null)
            return;

        line.enabled = true;

        Vector2 start = origin.position;
        Vector2 end = start + direction.normalized * laserRange;

        RaycastHit2D obstacleHit = Physics2D.Raycast(
            start,
            direction,
            laserRange,
            laserObstacleLayers
        );

        if (obstacleHit.collider != null)
            end = obstacleHit.point;

        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    void DamagePlayerOnLaser(
        Transform origin,
        Vector2 direction)
    {
        if (origin == null || player == null)
            return;

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            origin.position,
            direction.normalized,
            laserRange
        );

        System.Array.Sort(
            hits,
            (a, b) => a.distance.CompareTo(b.distance)
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            // Abaikan collider boss dan semua anaknya.
            if (hit.collider.transform == transform ||
                hit.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            PlayerHealth health =
                hit.collider.GetComponentInParent<PlayerHealth>();

            if (health != null)
            {
                health.TakeDamage(laserDamage);

                Debug.Log(
                    "LASER MENGENAI PLAYER! Damage: " + laserDamage
                );

                return;
            }

            // Hentikan laser jika mengenai obstacle.
            int hitLayerMask =
                1 << hit.collider.gameObject.layer;

            if ((laserObstacleLayers.value & hitLayerMask) != 0)
                return;
        }
    }

    void HideLasers()
    {
        if (leftLaserLine != null)
            leftLaserLine.enabled = false;

        if (rightLaserLine != null)
            rightLaserLine.enabled = false;

        StopLaserVFX(leftWindupInstance);
        StopLaserVFX(rightWindupInstance);

        StopLaserVFX(leftLaserInstance);
        StopLaserVFX(rightLaserInstance);
    }

    // =========================================
    // MISSILE VOLLEY
    // =========================================

    IEnumerator ProjectileVolley()
    {
        bool usingMouth = phase == 2;

        // Phase 2: buka mulut sebelum mulai menembakkan projectile.
        if (usingMouth)
            SetMouthOpen(true);

        currentState = BossState.VolleyTelegraph;

        float telegraphTime = phase == 1 ? 0.6f : 0.3f;
        yield return new WaitForSeconds(telegraphTime);

        if (isDead)
        {
            if (usingMouth)
                SetMouthOpen(false);

            yield break;
        }

        currentState = BossState.VolleyAttack;

        int count = phase == 1
            ? volleyCountPhaseOne
            : volleyCountPhaseTwo;

        for (int i = 0; i < count; i++)
        {
            if (isDead)
            {
                if (usingMouth)
                    SetMouthOpen(false);

                yield break;
            }

            if (phase == 1)
            {
                if (leftHand != null && !leftHand.IsDefeated)
                    FireProjectile(leftProjectileSpawn, leftHand);

                if (rightHand != null && !rightHand.IsDefeated)
                    FireProjectile(rightProjectileSpawn, rightHand);
            }
            else
            {
                FireMouthProjectile();
            }

            yield return new WaitForSeconds(projectileInterval);
        }

        // Setelah semua projectile selesai, tutup mulut dan kembali ke idle.
        // Waktu tunggu memberi kesempatan animasi menutup selesai
        // sebelum EyeLaserAttack() dijalankan oleh BossLoop().
        if (usingMouth)
        {
            SetMouthOpen(false);

            if (!isDead && mouthCloseDuration > 0f)
                yield return new WaitForSeconds(mouthCloseDuration);
        }
    }

    void SetMouthOpen(bool open)
    {
        if (mouthAnimator == null)
            return;

        mouthAnimator.SetBool("Open", open);
        mouthAnimator.SetBool("Idle", !open);
    }


    void FireMouthProjectile()
    {
        if (projectilePrefab == null || mouthProjectileSpawn == null || player == null)
            return;

        Vector2 direction =
            ((Vector2)player.position - (Vector2)mouthProjectileSpawn.position).normalized;

        GameObject bullet = Instantiate(
            projectilePrefab,
            mouthProjectileSpawn.position,
            Quaternion.identity
        );

        BossProjectile projectile = bullet.GetComponent<BossProjectile>();
        if (projectile != null)
        {
            projectile.Launch(
                direction,
                projectileSpeed,
                projectileDamage,
                gameObject,
                false
            );

            if (cameraVignette != null)
                cameraVignette.TriggerVignette();
        }
    }

    void FireProjectile(Transform spawnPoint, BossHand hand)
    {
        if (projectilePrefab == null ||
            spawnPoint == null ||
            player == null)
        {
            return;
        }

        Vector2 direction =
            ((Vector2)player.position -
             (Vector2)spawnPoint.position).normalized;

        GameObject bullet = Instantiate(
            projectilePrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        BossProjectile projectile =
            bullet.GetComponent<BossProjectile>();

        if (projectile != null)
        {
            projectile.Launch(
     direction,
     projectileSpeed,
     projectileDamage,
     gameObject,
     false
 );

            if (hand != null)
                hand.PlayMissileRecoil();

            if (cameraVignette != null)
                cameraVignette.TriggerVignette();

        }


    }

    // =========================================
    // GROUND SLAM
    // =========================================



    Vector3 GetGroundSlamTarget(
        Transform handRoot,
        Vector3 lockedPlayerPosition)
    {
        if (handRoot == null)
            return Vector3.zero;

        // Percobaan pertama: cari Ground di bawah posisi X player.
        Vector2 playerSideOrigin = new Vector2(
            lockedPlayerPosition.x,
            handRoot.position.y
        );

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            playerSideOrigin,
            Vector2.down,
            slamRaycastDistance
        );

        System.Array.Sort(
            hits,
            (a, b) => a.distance.CompareTo(b.distance)
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            // Abaikan collider boss sendiri.
            if (hit.collider.transform == transform ||
                hit.collider.transform.IsChildOf(transform))
                continue;

            if (hit.collider.CompareTag("Ground"))
            {
                Debug.Log("Ground ditemukan di arah player: " + handRoot.name);

                return new Vector3(
                    hit.point.x,
                    hit.point.y + slamGroundOffset,
                    handRoot.position.z
                );
            }
        }

        // Percobaan kedua: jika tidak ada Ground di bawah player,
        // cari Ground tepat di bawah tangan.
        Vector2 handOrigin = handRoot.position;

        hits = Physics2D.RaycastAll(
            handOrigin,
            Vector2.down,
            slamRaycastDistance
        );

        System.Array.Sort(
            hits,
            (a, b) => a.distance.CompareTo(b.distance)
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            if (hit.collider.transform == transform ||
                hit.collider.transform.IsChildOf(transform))
                continue;

            if (hit.collider.CompareTag("Ground"))
            {
                Debug.Log("Ground fallback ditemukan: " + handRoot.name);

                return new Vector3(
                    handRoot.position.x,
                    hit.point.y + slamGroundOffset,
                    handRoot.position.z
                );
            }
        }

        Debug.LogWarning(
            "Ground tidak ditemukan oleh " + handRoot.name +
            ". Periksa Tag Ground dan Collider2D."
        );

        return handRoot.position;
    }

    void SetHandTrigger(bool isTrigger)
    {
        if (leftHandRoot != null)
        {
            Collider2D col = leftHandRoot.GetComponent<Collider2D>();

            if (col != null)
                col.isTrigger = isTrigger;
        }

        if (rightHandRoot != null)
        {
            Collider2D col = rightHandRoot.GetComponent<Collider2D>();

            if (col != null)
                col.isTrigger = isTrigger;
        }
    }

    IEnumerator GroundSlam()
    {
        // Kunci posisi player saat Ground Slam dimulai.
        Vector3 lockedPlayerPosition = player != null
            ? player.position
            : transform.position;

        // Simpan parent dan transform awal kedua tangan.
        Transform leftOriginalParent =
            leftHandRoot != null ? leftHandRoot.parent : null;

        Transform rightOriginalParent =
            rightHandRoot != null ? rightHandRoot.parent : null;

        Vector3 leftOriginalLocalPosition = leftHandStartPosition;
        Vector3 rightOriginalLocalPosition = rightHandStartPosition;

        Quaternion leftOriginalLocalRotation =
            leftHandRoot != null ? leftHandRoot.localRotation : Quaternion.identity;

        Quaternion rightOriginalLocalRotation =
            rightHandRoot != null ? rightHandRoot.localRotation : Quaternion.identity;

        Vector3 leftOriginalLocalScale =
            leftHandRoot != null ? leftHandRoot.localScale : Vector3.one;

        Vector3 rightOriginalLocalScale =
            rightHandRoot != null ? rightHandRoot.localScale : Vector3.one;

        // Ganti sprite Ground Slam.
        if (leftHand != null)
            leftHand.SetGroundSlamSprite(true);

        if (rightHand != null)
            rightHand.SetGroundSlamSprite(true);

        currentState = BossState.SlamTelegraph;

        // 1. RAISE: kedua tangan terangkat.
        yield return StartCoroutine(
            MoveHands(
                leftHandStartPosition + Vector3.up * handRaiseHeight,
                rightHandStartPosition + Vector3.up * handRaiseHeight,
                handRaiseDuration
            )
        );

        if (isDead)
            yield break;

        // 2. SHAKE: kedua tangan bergetar.
        float shakeTimer = 0f;

        while (shakeTimer < handShakeDuration && !isDead)
        {
            float shakeX =
                Mathf.Sin(shakeTimer * handShakeSpeed) * handShakeAmount;

            Vector3 shakeOffset = new Vector3(shakeX, 0f, 0f);

            if (leftHandRoot != null &&
                leftHand != null && !leftHand.IsDefeated)
            {
                leftHandRoot.localPosition =
                    leftHandStartPosition +
                    Vector3.up * handRaiseHeight +
                    shakeOffset;
            }

            if (rightHandRoot != null &&
                rightHand != null && !rightHand.IsDefeated)
            {
                rightHandRoot.localPosition =
                    rightHandStartPosition +
                    Vector3.up * handRaiseHeight +
                    shakeOffset;
            }

            shakeTimer += Time.deltaTime;
            yield return null;
        }

        if (isDead)
            yield break;

        currentState = BossState.SlamAttack;
        SetHandTrigger(false);

        // Hitung target kedua tangan berdasarkan posisi player yang dikunci.
        Vector3 leftSlamTarget = GetGroundSlamTarget(
            leftHandRoot,
            lockedPlayerPosition
        );

        Vector3 rightSlamTarget = GetGroundSlamTarget(
            rightHandRoot,
            lockedPlayerPosition
        );

        // 3. LEFT HAND SLAM.
        if (leftHandRoot != null &&
            leftHand != null && !leftHand.IsDefeated)
        {
            // Lepaskan tangan kiri dari parent.
            leftHandRoot.SetParent(null, true);

            yield return StartCoroutine(
                MoveHandWorld(
                    leftHandRoot,
                    leftSlamTarget,
                    handSlamDuration
                )
            );

            if (isDead)
                yield break;

            SpawnVFX(groundSlamVFXPrefab, leftHandRoot.position);
        }

        // 4. RIGHT HAND SLAM.
        if (rightHandRoot != null &&
            rightHand != null && !rightHand.IsDefeated)
        {
            // Lepaskan tangan kanan dari parent.
            rightHandRoot.SetParent(null, true);

            yield return StartCoroutine(
                MoveHandWorld(
                    rightHandRoot,
                    rightSlamTarget,
                    handSlamDuration
                )
            );

            if (isDead)
                yield break;

            SpawnVFX(groundSlamVFXPrefab, rightHandRoot.position);
        }

        if (isDead)
            yield break;

        // 5. IMPACT setelah kedua tangan menghantam.
        SpawnFallingRocks();

        if (cameraFollow != null)
        {
            cameraFollow.TriggerShake(
                groundSlamShakeIntensity,
                groundSlamShakeDuration
            );
        }

        recoveryInterrupted = false;

        if (leftHand != null && !leftHand.IsDefeated)
            leftHand.SetSlamRecovery(true);

        if (rightHand != null && !rightHand.IsDefeated)
            rightHand.SetSlamRecovery(true);

        // 6. RECOVERY: tangan tetap menancap di posisi masing-masing.
        currentState = BossState.SlamRecovery;

        float timer = 0f;

        while (timer < slamRecoveryTime &&
               !recoveryInterrupted &&
               !isDead)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (leftHand != null)
            leftHand.SetSlamRecovery(false);

        if (rightHand != null)
            rightHand.SetSlamRecovery(false);

        if (isDead)
            yield break;

        // 7. RETURN: tangan kembali ke posisi awal.
        if (leftHandRoot != null &&
            leftHand != null && !leftHand.IsDefeated)
        {
            yield return StartCoroutine(
                ReturnHandToParent(
                    leftHandRoot,
                    leftOriginalParent,
                    leftOriginalLocalPosition,
                    leftOriginalLocalRotation,
                    leftOriginalLocalScale,
                    handReturnDuration
                )
            );
        }

        if (rightHandRoot != null &&
            rightHand != null && !rightHand.IsDefeated)
        {
            yield return StartCoroutine(
                ReturnHandToParent(
                    rightHandRoot,
                    rightOriginalParent,
                    rightOriginalLocalPosition,
                    rightOriginalLocalRotation,
                    rightOriginalLocalScale,
                    handReturnDuration
                )
            );
        }

        SetHandTrigger(true);
        // Izinkan kembali objek yang dilempar mengenai boss.
        if (ThrownObjectCollision.instance != null)
            ThrownObjectCollision.instance.canHitBoss = true;

        if (leftHand != null)
            leftHand.SetGroundSlamSprite(false);

        if (rightHand != null)
            rightHand.SetGroundSlamSprite(false);
    }

    IEnumerator MoveHands(
        Vector3 leftTarget,
        Vector3 rightTarget,
        float duration)
    {
        Vector3 leftStart =
            leftHandRoot != null ? leftHandRoot.localPosition : Vector3.zero;

        Vector3 rightStart =
            rightHandRoot != null ? rightHandRoot.localPosition : Vector3.zero;

        float timer = 0f;

        while (timer < duration && !isDead)
        {
            float t = duration > 0f
                ? Mathf.Clamp01(timer / duration)
                : 1f;

            // Smooth movement
            t = Mathf.SmoothStep(0f, 1f, t);

            if (leftHandRoot != null &&
                leftHand != null && !leftHand.IsDefeated)
            {
                leftHandRoot.localPosition =
                    Vector3.Lerp(leftStart, leftTarget, t);
            }

            if (rightHandRoot != null &&
                rightHand != null && !rightHand.IsDefeated)
            {
                rightHandRoot.localPosition =
                    Vector3.Lerp(rightStart, rightTarget, t);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (leftHandRoot != null &&
            leftHand != null && !leftHand.IsDefeated)
        {
            leftHandRoot.localPosition = leftTarget;
        }

        if (rightHandRoot != null &&
            rightHand != null && !rightHand.IsDefeated)
        {
            rightHandRoot.localPosition = rightTarget;
        }
    }


    IEnumerator MoveHandWorld(
        Transform handRoot,
        Vector3 target,
        float duration)
    {
        if (handRoot == null)
            yield break;

        Vector3 startPosition = handRoot.position;
        float timer = 0f;

        while (timer < duration && !isDead)
        {
            float t = duration > 0f
                ? Mathf.Clamp01(timer / duration)
                : 1f;

            t = Mathf.SmoothStep(0f, 1f, t);

            handRoot.position = Vector3.Lerp(
                startPosition,
                target,
                t
            );

            timer += Time.deltaTime;
            yield return null;
        }

        if (!isDead)
            handRoot.position = target;
    }


    IEnumerator ReturnHandToParent(
        Transform handRoot,
        Transform originalParent,
        Vector3 originalLocalPosition,
        Quaternion originalLocalRotation,
        Vector3 originalLocalScale,
        float duration)
    {
        if (handRoot == null)
            yield break;

        // Cari posisi awal berdasarkan parent yang tersimpan.
        Vector3 returnPosition = originalParent != null
            ? originalParent.TransformPoint(originalLocalPosition)
            : originalLocalPosition;

        yield return StartCoroutine(
            MoveHandWorld(handRoot, returnPosition, duration)
        );

        if (isDead)
            yield break;

        // Pasang kembali parent.
        handRoot.SetParent(originalParent, true);

        handRoot.localPosition = originalLocalPosition;
        handRoot.localRotation = originalLocalRotation;
        handRoot.localScale = originalLocalScale;
    }



    IEnumerator HeadSlam()
    {
        if (headRoot == null)
        {
            Debug.LogWarning("Head Root belum diatur pada BossController!");
            yield break;
        }

        StopBossMovement();
        headCanBeDamaged = false;
        currentState = BossState.HeadSlamTelegraph;

        Vector3 raisedPosition = headStartPosition + Vector3.up * headRaiseHeight;

        // Saat kepala terangkat dan bergetar, boss bergerak cepat ke arah player.
        // Gerakan boss dan getaran kepala berlangsung bersamaan.
        BossFollowPlayer follow = bossMovementScript as BossFollowPlayer;
        Transform movementRoot = follow != null ? follow.movementRoot : null;

        Vector3 bossStartPosition = movementRoot != null
            ? movementRoot.position
            : Vector3.zero;

        float bossTargetX = bossStartPosition.x;

        if (movementRoot != null && player != null)
        {
            bossTargetX = Mathf.Clamp(
                player.position.x + follow.followOffsetX,
                follow.minX,
                follow.maxX
            );
        }

        float preparationDuration = headRaiseDuration + headShakeDuration;
        float preparationTimer = 0f;

        while (preparationTimer < preparationDuration && !isDead)
        {
            float deltaTime = Time.deltaTime;
            preparationTimer += deltaTime;

            // Boss bergerak menuju posisi player, tetapi tetap pada Y dan Z semula.
            if (movementRoot != null)
            {
                float bossT = Mathf.Clamp01(preparationTimer / preparationDuration);
                bossT = Mathf.SmoothStep(0f, 1f, bossT);

                Vector3 bossPosition = movementRoot.position;
                bossPosition.x = Mathf.Lerp(bossStartPosition.x, bossTargetX, bossT);
                movementRoot.position = bossPosition;
            }

            // Kepala naik terlebih dahulu, kemudian bergetar saat boss terus bergerak.
            if (preparationTimer <= headRaiseDuration)
            {
                float raiseT = headRaiseDuration > 0f
                    ? Mathf.Clamp01(preparationTimer / headRaiseDuration)
                    : 1f;
                raiseT = Mathf.SmoothStep(0f, 1f, raiseT);
                headRoot.localPosition = Vector3.Lerp(headStartPosition, raisedPosition, raiseT);
            }
            else
            {
                float shakeTimer = preparationTimer - headRaiseDuration;
                float shakeX = Mathf.Sin(shakeTimer * headShakeSpeed) * headShakeAmount;
                headRoot.localPosition = raisedPosition + new Vector3(shakeX, 0f, 0f);
            }

            yield return null;
        }

        if (isDead) yield break;

        if (movementRoot != null)
        {
            Vector3 finalBossPosition = movementRoot.position;
            finalBossPosition.x = bossTargetX;
            movementRoot.position = finalBossPosition;
        }

        headRoot.localPosition = raisedPosition;

        // Hitung ulang posisi tanah setelah kepala bergerak ke arah player.
        Vector3 slamTarget = GetHeadSlamTarget();
        currentState = BossState.HeadSlamAttack;
        yield return StartCoroutine(MoveHead(slamTarget, headSlamDuration));
        headRoot.GetComponent<BossHead>()?.ResetSlamDamage();

        // LOKASI SPAWN VFX: posisi kepala tepat setelah menghantam tanah.
        SpawnVFX(headSlamVFXPrefab, headRoot.position);

        // Camera shake saat kepala menghantam tanah.
        if (cameraFollow != null)
        {
            cameraFollow.TriggerShake(
                headSlamShakeIntensity,
                headSlamShakeDuration
            );
        }

        //SFX bisa ditaruh disini
        if (isDead) yield break;

        // Kepala diam di bawah dan dapat menerima serangan pemain.
        headCanBeDamaged = true;
        currentState = BossState.HeadStunned;
        Debug.Log("Boss terkena stun! Serang kepalanya sekarang!");

        float stunTimer = 0f;
        while (stunTimer < headStunDuration && !isDead)
        {
            stunTimer += Time.deltaTime;
            yield return null;
        }

        headCanBeDamaged = false;

        if (isDead) yield break;

        yield return StartCoroutine(MoveHead(headStartPosition, headReturnDuration));
        ResumeBossMovement();
    }

    Vector3 GetHeadSlamTarget()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            headRoot.position,
            Vector2.down,
            headSlamRaycastDistance
        );

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            // Abaikan collider boss sendiri.
            if (hit.collider.transform == transform ||
                hit.collider.transform.IsChildOf(transform))
                continue;

            if (hit.collider.CompareTag("Ground"))
            {
                Vector3 worldTarget = new Vector3(
                    headRoot.position.x,
                    hit.point.y + headSlamGroundOffset,
                    headRoot.position.z
                );

                return headRoot.parent != null
                    ? headRoot.parent.InverseTransformPoint(worldTarget)
                    : worldTarget;
            }
        }

        Debug.LogWarning("Ground tidak ditemukan di bawah Head!");
        return headStartPosition;
    }

    IEnumerator MoveHead(Vector3 target, float duration)
    {
        Vector3 start = headRoot.localPosition;
        float timer = 0f;

        while (timer < duration && !isDead)
        {
            float t = duration > 0f ? Mathf.Clamp01(timer / duration) : 1f;
            t = Mathf.SmoothStep(0f, 1f, t);
            headRoot.localPosition = Vector3.Lerp(start, target, t);
            timer += Time.deltaTime;
            yield return null;
        }

        if (!isDead)
            headRoot.localPosition = target;
    }

    public bool IsHeadCollider(Collider2D targetCollider)
    {
        if (targetCollider == null || headRoot == null)
            return false;

        Transform target = targetCollider.transform;
        return target == headRoot || target.IsChildOf(headRoot);
    }

    public void DamageHead(int damage)
    {
        if (isDead || !headCanBeDamaged || damage <= 0)
            return;

        Debug.Log("Kepala boss terkena serangan! Damage: " + damage);
        DamageBoss(damage);
    }

    void SpawnFallingRocks()
    {
        if (fallingRockPrefab == null)
            return;

        // Jumlah batu yang muncul.
        const int rockCount = 1;

        for (int i = 0; i < rockCount; i++)
        {
            float randomX = Random.Range(
                fallingRockMinX,
                fallingRockMaxX
            );

            Vector3 spawnPosition = new Vector3(
                randomX,
                fallingRockSpawnY,
                transform.position.z
            );

            Instantiate(
                fallingRockPrefab,
                spawnPosition,
                Quaternion.identity
            );
        }
    }


    IEnumerator BossIntroSequence()
    {
        // Tampilkan panel intro.
        if (bossIntroPanel != null)
            bossIntroPanel.SetActive(true);

        // 1. Tampilkan nama boss.
        if (bossNameText != null)
        {
            bossNameText.gameObject.SetActive(true);
            yield return new WaitForSeconds(bossNameDuration);
            bossNameText.gameObject.SetActive(false);
        }

        // 2. Tampilkan READY...
        if (readyFightText != null)
        {
            readyFightText.gameObject.SetActive(true);
            readyFightText.text = "READY...";
            yield return new WaitForSeconds(readyDuration);

            // 3. Ganti menjadi FIGHT!
            readyFightText.text = "FIGHT!";
            yield return new WaitForSeconds(fightDuration);

            readyFightText.gameObject.SetActive(false);
        }

        // Pastikan seluruh panel intro hilang.
        if (bossIntroPanel != null)
            bossIntroPanel.SetActive(false);

        // Intro selesai, kamera dan player aktif kembali
        if (cameraFollow != null)
            cameraFollow.enabled = true;

        if (playerMovementScript != null)
            playerMovementScript.enabled = true;

        if (!isDead)
            yield return StartCoroutine(BossLoop());

    }

    void SetEyeScale(Transform eye, Vector3 baseScale, float multiplier)
    {
        if (eye == null)
            return;

        eye.localScale = baseScale * multiplier;
    }

    void AnimateEyeCharging(float progress)
    {
        float scale = Mathf.Lerp(1f, chargeEyeScale, progress);

        SetEyeScale(leftEyeVisual, leftEyeOriginalScale, scale);
        SetEyeScale(rightEyeVisual, rightEyeOriginalScale, scale);
    }

    void AnimateEyeFiring(float timer)
    {
        // Nilai bergerak dari 1 ke -1 secara berulang.
        float pulse = (Mathf.Cos(timer * pulseSpeed) + 1f) * 0.5f;

        // Ubah menjadi ukuran antara pulseMinScale dan firingEyeScale.
        float scale = Mathf.Lerp(
            pulseMinScale,
            firingEyeScale,
            pulse
        );

        SetEyeScale(leftEyeVisual, leftEyeOriginalScale, scale);
        SetEyeScale(rightEyeVisual, rightEyeOriginalScale, scale);
    }

    void ResetEyeScale()
    {
        SetEyeScale(leftEyeVisual, leftEyeOriginalScale, 1f);
        SetEyeScale(rightEyeVisual, rightEyeOriginalScale, 1f);
    }

    void StopBossMovement()
    {
        if (bossMovementScript == null) return;

        bossMovementWasEnabled = bossMovementScript.enabled;
        bossMovementScript.enabled = false;
    }

    void ResumeBossMovement()
    {
        if (bossMovementScript == null) return;

        // Hanya aktifkan kembali jika sebelumnya memang aktif.
        if (bossMovementWasEnabled && !isDead)
            bossMovementScript.enabled = true;
    }

    ParticleSystem CreateEyeWindupVFX(
    ParticleSystem prefab,
    Transform eyeOrigin,
    ParticleSystem instance)
    {
        if (prefab == null || eyeOrigin == null)
            return null;

        if (instance == null)
        {
            instance = Instantiate(prefab, eyeOrigin);
        }
        else
        {
            instance.transform.SetParent(eyeOrigin);
        }

        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;

        instance.gameObject.SetActive(true);

        instance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        instance.Play();

        return instance;
    }

    ParticleSystem CreateLaserBeamVFX(
    ParticleSystem prefab,
    LineRenderer line,
    ParticleSystem instance,
    float particleWidth)
    {
        if (prefab == null || line == null)
            return null;

        if (instance == null)
            instance = Instantiate(prefab);

        instance.gameObject.SetActive(true);

        instance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        UpdateLaserVFX(instance, line, particleWidth);

        instance.Play();

        return instance;
    }

    void UpdateLaserVFX(
    ParticleSystem particle,
    LineRenderer line,
    float particleWidth)
    {
        if (particle == null || line == null || !line.enabled)
            return;

        Vector3 start = line.GetPosition(0);
        Vector3 end = line.GetPosition(1);

        Vector3 direction = end - start;

        float length = direction.magnitude;

        if (length <= 0.001f)
            return;

        // Posisi emitter berada di tengah garis.
        // particle.transform.position = (start + end) * 0.5f;
        particle.transform.position = start;

        // Rotasi mengikuti arah garis.
        float angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        particle.transform.rotation = Quaternion.Euler(
            -angle,
            90f,
            -90f
        );

        // Bentuk area emisi menjadi garis panjang.
        var shape = particle.shape;

        shape.shapeType = ParticleSystemShapeType.Box;

        shape.scale = new Vector3(
            length,
            particleWidth,
            0.1f
        );
    }

    void StopLaserVFX(ParticleSystem particle)
    {
        if (particle == null)
            return;

        particle.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );
    }

    // Template untuk memunculkan VFX sekali pakai di posisi tertentu.
    void SpawnVFX(ParticleSystem prefab, Vector3 spawnPosition)
    {
        if (prefab == null)
            return;

        ParticleSystem vfx = Instantiate(
            prefab,
            spawnPosition,
            Quaternion.identity
        );

        // Hancurkan instance setelah seluruh particle selesai.
        ParticleSystem.MainModule main = vfx.main;
        main.loop = false;
        main.stopAction = ParticleSystemStopAction.Destroy;

        vfx.Play();
    }
}