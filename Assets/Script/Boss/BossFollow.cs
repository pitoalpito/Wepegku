
using UnityEngine;

public class BossFollowPlayer : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform movementRoot;

    [Header("Follow Settings")]
    public float smoothTime = 1.5f;
    public float maxSpeed = 5f;
    public float followOffsetX = 0f;

    [Header("Movement Limits")]
    public float minX = -20f;
    public float maxX = 20f;

    private float velocityX;
    private float fixedY;
    private float fixedZ;

    void Start()
    {
        if (movementRoot != null)
        {
            fixedY = movementRoot.position.y;
            fixedZ = movementRoot.position.z;
        }
    }

    void LateUpdate()
    {
        if (player == null || movementRoot == null)
            return;

        Vector3 currentPosition = movementRoot.position;

        float targetX = player.position.x + followOffsetX;
        targetX = Mathf.Clamp(targetX, minX, maxX);

        float newX = Mathf.SmoothDamp(
            currentPosition.x,
            targetX,
            ref velocityX,
            smoothTime,
            maxSpeed
        );

        movementRoot.position = new Vector3(
            newX,
            fixedY,
            fixedZ
        );
    }
}
    