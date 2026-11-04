
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Tooltip("Seberapa cepat karakter mencapai kecepatan maksimum")]
    public float acceleration = 20f;

    [Tooltip("Seberapa cepat karakter berhenti saat input dilepas")]
    public float deceleration = 25f;

    [Tooltip("Seberapa cepat karakter berbalik arah")]
    public float turnSpeed = 35f;

    [Header("Jump Settings")]
    public float jumpForce = 10f;
    public float jumpHoldForce = 20f;
    public float maxJumpHoldTime = 0.25f;

    [Header("Double Jump")]
    public int maxJumps = 2;
    private int jumpsRemaining;

    [Header("Jump Feel")]
    public float gravityScale = 3f;
    public float fallGravityMultiplier = 2f;
    public float jumpCutMultiplier = 2f;

    private Rigidbody2D rb;
    private float moveInput;
    private float jumpHoldTimer;
    private bool isJumping;

    public float MoveInput => moveInput;
    private DashController dashController;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;

        jumpsRemaining = maxJumps;

        dashController = GetComponent<DashController>();
    }

    void Update()
    {
        // Input gerakan kiri dan kanan
        moveInput = 0f;

        if (Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.LeftArrow))
        {
            moveInput = -1f;
        }

        if (Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.RightArrow))
        {
            moveInput = 1f;
        }

        // Lompat dan double jump
        if (Input.GetKeyDown(KeyCode.Space) &&
            jumpsRemaining > 0)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            jumpsRemaining--;
            isJumping = true;
            jumpHoldTimer = 0f;
        }

        // Tahan Space untuk melompat lebih tinggi
        if (Input.GetKey(KeyCode.Space) &&
            isJumping &&
            rb.linearVelocity.y > 0f &&
            jumpHoldTimer < maxJumpHoldTime)
        {
            rb.linearVelocity += Vector2.up
                * jumpHoldForce * Time.deltaTime;

            jumpHoldTimer += Time.deltaTime;
        }

        // Lepas Space untuk lompatan lebih pendek
        if (Input.GetKeyUp(KeyCode.Space) &&
            rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y / jumpCutMultiplier
            );

            isJumping = false;
        }
    }

    void FixedUpdate()
    {
        if (dashController != null &&
    dashController.IsDashing)
        {
            return;
        }

        // Menentukan target kecepatan horizontal
        float targetSpeed = moveInput * moveSpeed;

        // Menentukan seberapa cepat kecepatan berubah
        float speedChange;

        if (moveInput == 0f)
        {
            // Melambat ketika input dilepas
            speedChange = deceleration;
        }
        else if (Mathf.Sign(rb.linearVelocity.x) != moveInput
                 && Mathf.Abs(rb.linearVelocity.x) > 0.01f)
        {
            // Berbalik arah dengan lebih cepat
            speedChange = turnSpeed;
        }
        else
        {
            // Membangun kecepatan secara bertahap
            speedChange = acceleration;
        }

        // Mengubah kecepatan secara bertahap
        float newSpeed = Mathf.MoveTowards(
            rb.linearVelocity.x,
            targetSpeed,
            speedChange * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector2(
            newSpeed,
            rb.linearVelocity.y
        );

        // Gravitasi lebih kuat saat jatuh
        if (rb.linearVelocity.y < 0f)
        {
            rb.gravityScale =
                gravityScale * fallGravityMultiplier;
        }
        else
        {
            rb.gravityScale = gravityScale;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Reset jatah lompat saat menyentuh Ground
        if (collision.gameObject.CompareTag("Ground"))
        {
            jumpsRemaining = maxJumps;
            isJumping = false;
        }
    }
}
