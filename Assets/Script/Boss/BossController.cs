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
    public Transform[] fallingRockSpawnPoints;
    public GameObject fallingRockPrefab;

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
    public int volleyCountPhaseOne = 3;
    public int volleyCountPhaseTwo = 5;
    public float projectileInterval = 0.3f;
    public float projectileSpeed = 8f;
    public int projectileDamage = 10;

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
    public float handSlamDistance = 1.5f;

    public float handRaiseDuration = 0.3f;
    public float handSlamDuration = 0.12f;
    public float handReturnDuration = 0.35f;

    private Vector3 leftHandStartPosition;
    private Vector3 rightHandStartPosition;

    [Header("Rock Counter")]
    public int rockDamage = 40;

    public BossState currentState = BossState.Intro;

    private Coroutine bossRoutine;
    private bool recoveryInterrupted;
    private bool isDead;

    [Header("Boss Intro")]
    public CameraFollow cameraFollow;
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


    public void OnHandDefeated(BossHand hand)
    {
        Debug.Log(hand.name + " telah dikalahkan!");
    }

    void Start()
    {
        currentHealth = maxHealth;

        if (leftHandRoot != null)
            leftHandStartPosition = leftHandRoot.localPosition;

        if (rightHandRoot != null)
            rightHandStartPosition = rightHandRoot.localPosition;

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
            // Phase 1 = 5 pasang serangan
            // Phase 2 = 10 pasang serangan
            int attackPairs = phase == 1 ? 5 : 10;

            for (int i = 0; i < attackPairs; i++)
            {
                if (isDead)
                    yield break;

                Debug.Log(
                    "Phase " + phase +
                    " - Pasangan serangan " +
                    (i + 1) + "/" + attackPairs
                );

                // 1. Missile
                yield return StartCoroutine(ProjectileVolley());

                if (isDead)
                    yield break;

                // Jeda pergantian serangan.
                // Phase 2 berganti lebih cepat.
                float attackGap = phase == 1 ? 0.3f : 0.1f;
                yield return new WaitForSeconds(attackGap);

                if (isDead)
                    yield break;

                // 2. Laser
                yield return StartCoroutine(EyeLaserAttack());

                if (isDead)
                    yield break;

                // Jeda sebelum pasangan serangan berikutnya.
                float pairGap = phase == 1 ? 0.4f : 0.15f;
                yield return new WaitForSeconds(pairGap);
            }

            if (isDead)
                yield break;

            // Semua pasangan serangan selesai.
            // Sekarang boss melakukan Ground Slam.
            yield return StartCoroutine(GroundSlam());

            if (isDead)
                yield break;

            // Transisi Phase 1 ke Phase 2.
            if (phase == 1 && currentHealth <= maxHealth / 2)
            {
                yield return StartCoroutine(EnterPhaseTwo());

                if (isDead)
                    yield break;
            }
        }
    }


    IEnumerator EnterPhaseTwo()
    {
        currentState = BossState.PhaseTransition;

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
        currentState = BossState.VolleyTelegraph;

        float telegraphTime = phase == 1 ? 0.6f : 0.3f;
        yield return new WaitForSeconds(telegraphTime);

        if (isDead)
            yield break;

        currentState = BossState.VolleyAttack;

        int count = phase == 1
            ? volleyCountPhaseOne
            : volleyCountPhaseTwo;

        for (int i = 0; i < count; i++)
        {
            if (isDead)
                yield break;

            if (leftHand != null && !leftHand.IsDefeated)
                FireProjectile(leftProjectileSpawn, leftHand);

            if (rightHand != null && !rightHand.IsDefeated)
                FireProjectile(rightProjectileSpawn, rightHand);

            yield return new WaitForSeconds(projectileInterval);
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


    IEnumerator GroundSlam()
    {
        currentState = BossState.SlamTelegraph;

        // 1. RAISE: tangan terangkat
        yield return StartCoroutine(
            MoveHands(
                leftHandStartPosition + Vector3.up * handRaiseHeight,
                rightHandStartPosition + Vector3.up * handRaiseHeight,
                handRaiseDuration
            )
        );

        // 2. SHAKE: tangan bergetar sebelum menyerang
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

        // 3. SLAM: tangan turun dengan cepat
        currentState = BossState.SlamAttack;

        yield return StartCoroutine(
            MoveHands(
                leftHandStartPosition + Vector3.down * handSlamDistance,
                rightHandStartPosition + Vector3.down * handSlamDistance,
                handSlamDuration
            )
        );

        if (isDead)
            yield break;

        // 4. IMPACT: batu muncul saat tangan mencapai bawah
        SpawnFallingRocks();

        recoveryInterrupted = false;

        if (leftHand != null && !leftHand.IsDefeated)
            leftHand.SetSlamRecovery(true);

        if (rightHand != null && !rightHand.IsDefeated)
            rightHand.SetSlamRecovery(true);

        // 5. RECOVERY: tangan berada di bawah
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

        // 6. Kembali ke posisi awal
        if (!isDead)
        {
            yield return StartCoroutine(
                MoveHands(
                    leftHandStartPosition,
                    rightHandStartPosition,
                    handReturnDuration
                )
            );
        }
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


    void SpawnFallingRocks()
    {
        if (fallingRockPrefab == null ||
            fallingRockSpawnPoints == null)
        {
            return;
        }

        foreach (Transform spawnPoint in fallingRockSpawnPoints)
        {
            if (spawnPoint == null)
                continue;

            Instantiate(
                fallingRockPrefab,
                spawnPoint.position,
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
        particle.transform.position = (start + end) * 0.5f;

        // Rotasi mengikuti arah garis.
        float angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        particle.transform.rotation = Quaternion.Euler(
            0f,
            0f,
            angle
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
}