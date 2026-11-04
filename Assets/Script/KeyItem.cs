
using UnityEngine;

public class KeyItem : MonoBehaviour
{
    [Header("Key Settings")]
    public bool canBeUsed = true;

    private Rigidbody2D rb;
    private bool isUsed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void HitKey(Vector2 direction, float forceX, float forceY)
    {
        if (isUsed || rb == null)
            return;

        rb.linearVelocity = new Vector2(
            direction.x * forceX,
            forceY
        );
    }

    public void UseKey()
    {
        if (isUsed || !canBeUsed)
            return;

        isUsed = true;
        Destroy(gameObject);
    }
}
