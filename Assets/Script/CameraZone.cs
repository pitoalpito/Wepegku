
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CameraZone : MonoBehaviour
{
    [Header("Zone Settings")]
    public float zoneZoom = 5f;

    [Header("References")]
    public CameraFollow cameraFollow;

    private void Reset()
    {
        // Area harus menjadi Trigger
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Start()
    {
        if (cameraFollow == null)
        {
            Camera mainCamera = Camera.main;

            if (mainCamera != null)
            {
                cameraFollow =
                    mainCamera.GetComponent<CameraFollow>();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (cameraFollow != null)
        {
            cameraFollow.SetZoom(zoneZoom);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (cameraFollow != null)
        {
            cameraFollow.ResetZoom();
        }
    }
}
