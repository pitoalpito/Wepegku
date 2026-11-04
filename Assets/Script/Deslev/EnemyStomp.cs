
using UnityEngine;

public class EnemyStomp : MonoBehaviour
{
    [Header("Bounce Settings")]
    public float bounceForce = 10f;

    private bool isDead = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            ContactPoint2D contact = collision.GetContact(0);

            // Player harus menginjak enemy dari atas.
            if (contact.normal.y < -0.5f)
            {
                isDead = true;

                Rigidbody2D playerRb =
                    collision.gameObject.GetComponent<Rigidbody2D>();

                if (playerRb != null)
                {
                    // Hapus kecepatan jatuh, lalu pantulkan ke atas.
                    playerRb.linearVelocity = new Vector2(
                        playerRb.linearVelocity.x,
                        bounceForce
                    );
                }

                Destroy(gameObject);
            }
        }
    }
}
