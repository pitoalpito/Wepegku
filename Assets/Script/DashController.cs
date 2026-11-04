
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DashController : MonoBehaviour
{
    [Header("Dash Settings")]
    public KeyCode dashKey = KeyCode.L;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1.5f;

    [Header("Cooldown UI")]
    public Image cooldownImage;

    [Header("Screen Shake")]
    public CameraFollow cameraFollow;
    public float screenShakeIntensity = 0.15f;
    public float screenShakeDuration = 0.1f;

    [Header("Air Dash Shake")]
    public float airDashShakeIntensity = 0.3f;
    public float airDashShakeDuration = 0.14f;

    [Header("Ground Detection")]
    public string groundTag = "Ground";
    public float groundCheckDistance = 0.1f;

    private Rigidbody2D rb;
    private PlayerMovement playerMovement;
    private Collider2D playerCollider;

    private float dashDirection = 1f;
    private float cooldownTimer = 0f;
    private bool isDashing = false;

    private float originalGravity;

    public bool IsDashing => isDashing;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        playerCollider = GetComponent<Collider2D>();

        if (rb != null)
        {
            originalGravity = rb.gravityScale;
        }

        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 1f;
        }

        if (cameraFollow == null)
        {
            cameraFollow =
                FindAnyObjectByType<CameraFollow>();
        }
    }

    void Update()
    {
        // Update cooldown
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            cooldownTimer = Mathf.Max(0f, cooldownTimer);
        }

        // Update radial cooldown UI
        if (cooldownImage != null)
        {
            if (dashCooldown <= 0f)
            {
                cooldownImage.fillAmount = 1f;
            }
            else
            {
                cooldownImage.fillAmount =
                    1f - (cooldownTimer / dashCooldown);
            }
        }

        // Input dash
        if (Input.GetKeyDown(dashKey) &&
            cooldownTimer <= 0f &&
            !isDashing)
        {
            StartCoroutine(PerformDash());
        }
    }

    IEnumerator PerformDash()
    {
        isDashing = true;

        // Tentukan arah dash
        if (playerMovement != null &&
            playerMovement.MoveInput != 0f)
        {
            dashDirection = Mathf.Sign(
                playerMovement.MoveInput
            );
        }

        // Mulai cooldown
        cooldownTimer = dashCooldown;

        // Deteksi apakah player sedang di udara
        bool isAirDash = !IsGrounded();

        // Screen shake sesuai kondisi player
        if (cameraFollow != null)
        {
            if (isAirDash)
            {
                cameraFollow.TriggerShake(
                    airDashShakeIntensity,
                    airDashShakeDuration
                );
            }
            else
            {
                cameraFollow.TriggerShake(
                    screenShakeIntensity,
                    screenShakeDuration
                );
            }
        }

        // Simpan gravity sebelum dash
        originalGravity = rb.gravityScale;

        // Mulai dash
        rb.gravityScale = 0f;

        rb.linearVelocity = new Vector2(
            dashDirection * dashSpeed,
            0f
        );

        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            rb.linearVelocity = new Vector2(
                dashDirection * dashSpeed,
                0f
            );

            elapsed += Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }

        // Hentikan gerakan horizontal dash
        rb.linearVelocity = new Vector2(
            0f,
            rb.linearVelocity.y
        );

        // Pulihkan gravity
        rb.gravityScale = originalGravity;

        isDashing = false;
    }

    bool IsGrounded()
    {
        if (playerCollider == null)
            return false;

        Vector2 origin = new Vector2(
            playerCollider.bounds.center.x,
            playerCollider.bounds.min.y + 0.02f
        );

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            Vector2.down,
            groundCheckDistance + 0.02f
        );

        return hit.collider != null &&
               hit.collider != playerCollider &&
               hit.collider.CompareTag(groundTag);
    }

    void OnDisable()
    {
        if (rb != null && isDashing)
        {
            StopAllCoroutines();

            rb.gravityScale = originalGravity;
            isDashing = false;
        }
    }
}
