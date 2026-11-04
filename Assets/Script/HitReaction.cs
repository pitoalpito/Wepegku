
using System.Collections;
using UnityEngine;

public class EnemyHitReaction : MonoBehaviour
{
    public enum AttackType
    {
        Basic,
        Uppercut,
        WallBounce
    }

    [Header("Ground Basic Attack")]
    public float groundKnockbackForce = 8f;
    public float groundUpwardForce = 2f;

    [Header("Uppercut")]
    public float uppercutHorizontalForce = 1f;
    public float uppercutUpwardForce = 10f;

    [Header("Basic Attack After Uppercut")]
    public float airKnockbackForce = 12f;
    public float airUpwardForce = 1.5f;

    [Header("Wall Bounce")]
    public LayerMask wallLayers;
    public float wallBounceHorizontalForce = 12f;
    public float wallBounceUpwardForce = 4f;

    [Header("Hit Freeze")]
    public float hitFreezeDuration = 0.08f;
    public float stunDuration = 0.25f;

    [Header("Hit Flash")]
    public SpriteRenderer spriteRenderer;
    public Color hitColor = Color.white;
    public float hitFlashDuration = 0.12f;

    [Header("Flinch")]
    public Transform visualTransform;
    public float shakeDuration = 0.15f;
    public float shakeAngle = 8f;

    private Rigidbody2D rb;
    private Color originalColor;
    private Quaternion originalRotation;
    private RigidbodyConstraints2D originalConstraints;
    private Coroutine hitCoroutine;

    private bool stopOnLanding;
    private bool canWallBounce;

    public bool IsStunned { get; private set; }
    public bool afterUppercut { get; private set; }
    public bool IsInHitMotion => stopOnLanding || afterUppercut;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (visualTransform == null && spriteRenderer != null)
            visualTransform = spriteRenderer.transform;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (visualTransform != null)
            originalRotation = visualTransform.localRotation;

        if (rb != null)
            originalConstraints = rb.constraints;
    }

    public void TakeHit(Vector2 direction, AttackType attackType)
    {
        if (rb == null)
            return;

        if (hitCoroutine != null)
        {
            StopCoroutine(hitCoroutine);
            hitCoroutine = null;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        if (visualTransform != null)
            visualTransform.localRotation = originalRotation;

        rb.constraints = originalConstraints;

        hitCoroutine = StartCoroutine(
            HitRoutine(direction, attackType)
        );
    }

    IEnumerator HitRoutine(Vector2 direction, AttackType attackType)
    {
        IsStunned = true;

        float directionX = Mathf.Sign(direction.x);

        if (directionX == 0f)
            directionX = 1f;

        rb.linearVelocity = Vector2.zero;

        rb.constraints |= RigidbodyConstraints2D.FreezePositionX |
                          RigidbodyConstraints2D.FreezePositionY;

        float elapsed = 0f;
        float feedbackDuration = Mathf.Max(
            hitFlashDuration,
            shakeDuration
        );

        while (elapsed < feedbackDuration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color =
                    elapsed < hitFlashDuration
                    ? hitColor
                    : originalColor;
            }

            if (visualTransform != null)
            {
                if (elapsed < shakeDuration)
                {
                    float angle = Random.Range(
                        -shakeAngle,
                        shakeAngle
                    );

                    visualTransform.localRotation =
                        originalRotation *
                        Quaternion.Euler(0f, 0f, angle);
                }
                else
                {
                    visualTransform.localRotation = originalRotation;
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        if (visualTransform != null)
            visualTransform.localRotation = originalRotation;

        yield return new WaitForSeconds(hitFreezeDuration);

        rb.constraints = originalConstraints;

        if (attackType == AttackType.Uppercut)
        {
            afterUppercut = true;
            canWallBounce = false;
            stopOnLanding = false;

            rb.linearVelocity = new Vector2(
                directionX * uppercutHorizontalForce,
                uppercutUpwardForce
            );
        }
        else if (attackType == AttackType.WallBounce)
        {
            // Bounce menjauhi dinding.
            afterUppercut = true;
            stopOnLanding = true;

            rb.linearVelocity = new Vector2(
                directionX * wallBounceHorizontalForce,
                wallBounceUpwardForce
            );
        }
        else if (afterUppercut)
        {
            // Basic attack lanjutan di udara.
            canWallBounce = true;
            stopOnLanding = true;

            rb.linearVelocity = new Vector2(
                directionX * airKnockbackForce,
                airUpwardForce
            );
        }
        else
        {
            // Basic attack biasa di darat.
            canWallBounce = false;
            stopOnLanding = true;

            rb.linearVelocity = new Vector2(
                directionX * groundKnockbackForce,
                groundUpwardForce
            );
        }

        yield return new WaitForSeconds(stunDuration);

        IsStunned = false;
        hitCoroutine = null;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        CheckWallBounce(collision);
        CheckLanding(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        CheckWallBounce(collision);
        CheckLanding(collision);
    }

    void CheckWallBounce(Collision2D collision)
    {
        if (!canWallBounce || hitCoroutine != null)
            return;

        // Hanya collider pada layer dinding yang bisa memicu bounce.
        int otherLayer = collision.gameObject.layer;

        if ((wallLayers.value & (1 << otherLayer)) == 0)
            return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);

            // Normal horizontal menandakan permukaan samping.
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                // Cegah pemicu berulang saat masih menyentuh dinding.
                canWallBounce = false;

                // Normal mengarah menjauhi dinding.
                Vector2 bounceDirection =
                    new Vector2(contact.normal.x, 0f);

                TakeHit(bounceDirection, AttackType.WallBounce);
                return;
            }
        }
    }

    void CheckLanding(Collision2D collision)
    {
        if (rb == null)
            return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);

            if (contact.normal.y > 0.5f &&
                rb.linearVelocity.y <= 0.1f)
            {
                if (stopOnLanding)
                {
                    rb.linearVelocity = new Vector2(
                        0f,
                        rb.linearVelocity.y
                    );

                    stopOnLanding = false;
                }

                if (afterUppercut)
                {
                    afterUppercut = false;
                    canWallBounce = false;
                }

                break;
            }
        }
    }

    void OnDisable()
    {
        if (hitCoroutine != null)
        {
            StopCoroutine(hitCoroutine);
            hitCoroutine = null;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        if (visualTransform != null)
            visualTransform.localRotation = originalRotation;

        if (rb != null)
            rb.constraints = originalConstraints;

        IsStunned = false;
        afterUppercut = false;
        stopOnLanding = false;
        canWallBounce = false;
    }
}
