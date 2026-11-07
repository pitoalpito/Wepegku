
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Follow Settings")]
    public float followSmoothTime = 0.15f;
    public float offsetX = 0f;

    [Range(0.1f, 0.5f)]
    public float playerScreenHeight = 0.3f;

    [Header("Zoom Settings")]
    public float defaultZoom = 5f;
    public float zoomSpeed = 3f;

    [Header("Screen Shake")]
    public float shakeIntensity = 0.15f;
    public float shakeDuration = 0.1f;

    private Camera cam;
    private float targetZoom;
    private Vector3 cameraVelocity;

    // Posisi kamera tanpa efek shake
    private Vector3 followPosition;

    // Timer screen shake
    private float shakeTimer;
    private float currentShakeIntensity;

    void Awake()
    {
        cam = GetComponent<Camera>();
        targetZoom = defaultZoom;

        if (cam != null)
        {
            cam.orthographicSize = defaultZoom;
        }

        followPosition = transform.position;
    }

    void LateUpdate()
    {
        if(player == null || cam == null)
    {
            Debug.LogWarning(
                "CameraFollow: Player atau Camera belum terisi!"
            );
            return;
        }

        // Debug.Log("Player Position: " + player.position);

        // Smooth zoom
        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetZoom,
            zoomSpeed * Time.deltaTime
        );

        // Hitung posisi target berdasarkan zoom
        float cameraHeight = cam.orthographicSize * 2f;

        float verticalOffset =
            cameraHeight * (0.5f - playerScreenHeight);

        Vector3 targetPosition = new Vector3(
            player.position.x + offsetX,
            player.position.y + verticalOffset,
            -10f
        );

        // Follow player tanpa memasukkan offset shake
        followPosition = Vector3.SmoothDamp(
            followPosition,
            targetPosition,
            ref cameraVelocity,
            followSmoothTime
        );

        // Hitung efek screen shake
        Vector3 shakeOffset = Vector3.zero;

        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            shakeTimer = Mathf.Max(0f, shakeTimer);

            float strength = currentShakeIntensity;

            if (shakeDuration > 0f)
            {
                strength *= shakeTimer / shakeDuration;
            }

            shakeOffset = new Vector3(
                Random.Range(-1f, 1f) * strength,
                Random.Range(-1f, 1f) * strength,
                0f
            );
        }

        // Terapkan posisi follow dan shake
        transform.position = followPosition + shakeOffset;
    }

    public void TriggerShake(float intensity, float duration)
    {
        if (duration <= 0f)
            return;

        currentShakeIntensity = intensity;
        shakeDuration = duration;
        shakeTimer = duration;
    }

    public void SetZoom(float newZoom)
    {
        targetZoom = newZoom;
    }

    public void ResetZoom()
    {
        targetZoom = defaultZoom;
    }
}
