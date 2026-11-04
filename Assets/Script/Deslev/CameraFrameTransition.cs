
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraFrameTransition : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public CameraPoint currentPoint;

    [Header("Transition")]
    public float transitionDuration = 0.4f;

    private Camera cam;
    private bool isTransitioning;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (currentPoint != null)
        {
            transform.position = new Vector3(
                currentPoint.transform.position.x,
                currentPoint.transform.position.y,
                transform.position.z
            );

            cam.orthographicSize = currentPoint.cameraSize;
        }
    }

    private void Update()
    {
        if (player == null || currentPoint == null)
            return;

        if (isTransitioning)
            return;

        if (!cam.orthographic)
            return;

        Vector3 cameraPos = transform.position;
        Vector3 playerPos = player.position;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float dx = playerPos.x - cameraPos.x;
        float dy = playerPos.y - cameraPos.y;

        CameraPoint nextPoint = null;

        // Deteksi player keluar frame.
        if (Mathf.Abs(dx) > halfWidth ||
            Mathf.Abs(dy) > halfHeight)
        {
            float horizontalRatio =
                Mathf.Abs(dx) / halfWidth;

            float verticalRatio =
                Mathf.Abs(dy) / halfHeight;

            if (horizontalRatio >= verticalRatio)
            {
                nextPoint = dx > 0
                    ? currentPoint.right
                    : currentPoint.left;
            }
            else
            {
                nextPoint = dy > 0
                    ? currentPoint.up
                    : currentPoint.down;
            }
        }

        if (nextPoint != null)
        {
            StartCoroutine(MoveCamera(nextPoint));
        }
    }

    private IEnumerator MoveCamera(CameraPoint target)
    {
        isTransitioning = true;

        Vector3 startPosition = transform.position;

        Vector3 targetPosition = new Vector3(
            target.transform.position.x,
            target.transform.position.y,
            transform.position.z
        );

        float startSize = cam.orthographicSize;
        float targetSize = target.cameraSize;

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;

            float t = transitionDuration > 0f
                ? Mathf.Clamp01(elapsed / transitionDuration)
                : 1f;

            // Smooth transition.
            float smoothT = t * t * (3f - 2f * t);

            // Gerakkan posisi kamera.
            transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                smoothT
            );

            // Ubah zoom kamera.
            cam.orthographicSize = Mathf.Lerp(
                startSize,
                targetSize,
                smoothT
            );

            yield return null;
        }

        // Pastikan nilai akhirnya tepat.
        transform.position = targetPosition;
        cam.orthographicSize = targetSize;

        currentPoint = target;

        isTransitioning = false;
    }
}
