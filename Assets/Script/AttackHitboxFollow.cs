
using UnityEngine;

public class AttackHitboxFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform player;
    public float forwardOffset = 0.8f;
    public float verticalOffset = 0f;

    private Vector3 initialLocalPosition;
    private Vector3 initialScale;
    private int facingDirection = 1;

    void Start()
    {
        if (player == null)
            player = transform.root;

        initialLocalPosition = transform.localPosition;
        initialScale = transform.localScale;

        UpdateDirection();
    }

    void LateUpdate()
    {
        UpdateDirection();
    }

    void UpdateDirection()
    {
        if (player == null)
            return;

        // Membaca arah hadap dari flip SpriteRenderer.
        SpriteRenderer sr =
            player.GetComponentInChildren<SpriteRenderer>();

        if (sr != null)
        {
            facingDirection = sr.flipX ? -1 : 1;
        }
        else
        {
            // Alternatif jika karakter menghadap dengan scale X.
            facingDirection = player.lossyScale.x < 0 ? -1 : 1;
        }

        // Pindahkan hitbox ke depan karakter.
        Vector3 position = initialLocalPosition;
        position.x = Mathf.Abs(forwardOffset) * facingDirection;
        position.y = verticalOffset;

        transform.localPosition = position;
    }
}
