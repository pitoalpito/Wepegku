
using UnityEngine;

public class CameraPoint : MonoBehaviour
{
    [Header("Camera Settings")]
    [Min(0.1f)]
    public float cameraSize = 5f;

    [Header("Next Camera Points")]
    public CameraPoint left;
    public CameraPoint right;
    public CameraPoint up;
    public CameraPoint down;
}
