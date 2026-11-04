
using UnityEngine;
using System.Collections;

public class LockReaction : MonoBehaviour
{
    public bool isUnlocked;
    public GameObject unlockedVisual;

    public float shakeDuration = 0.2f;
    public float shakeAngle = 8f;

    private bool isReacting;
    private Quaternion originalRotation;

    void Awake()
    {
        originalRotation = transform.localRotation;

        if (unlockedVisual != null)
            unlockedVisual.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Gembok terkena trigger: " + other.name);
        TryUnlock(other);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Gembok terkena collision: " + collision.collider.name);
        TryUnlock(collision.collider);
    }

    void TryUnlock(Collider2D other)
    {
        if (isUnlocked || isReacting)
            return;

        // Cari KeyItem di collider yang menyentuh dan parent-nya.
        KeyItem key = other.GetComponent<KeyItem>();

        if (key == null)
            key = other.GetComponentInParent<KeyItem>();

        if (key == null)
        {
            Debug.Log("Objek yang menyentuh bukan KeyItem.");
            return;
        }

        Debug.Log("Kunci terdeteksi! Gembok terbuka.");

        key.UseKey();
        Unlock();
    }

    void Unlock()
{
    isUnlocked = true;
    isReacting = true;

    Debug.Log("Gembok terbuka!");

    StartCoroutine(UnlockRoutine());
}

IEnumerator UnlockRoutine()
{
    float elapsed = 0f;

    while (elapsed < shakeDuration)
    {
        float angle = Random.Range(-shakeAngle, shakeAngle);

        transform.localRotation =
            originalRotation *
            Quaternion.Euler(0f, 0f, angle);

        elapsed += Time.deltaTime;
        yield return null;
    }

    // Kembalikan rotasi sebelum menghilangkan gembok.
    transform.localRotation = originalRotation;
    gameObject.SetActive(false);
}

    IEnumerator ShakeLock()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float angle = Random.Range(-shakeAngle, shakeAngle);

            transform.localRotation =
                originalRotation *
                Quaternion.Euler(0f, 0f, angle);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = originalRotation;
        isReacting = false;
    }
}
