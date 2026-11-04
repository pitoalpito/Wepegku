
using UnityEngine;

public class PlayerObjectGrab : MonoBehaviour
{
    [Header("Grab Settings")]
    public KeyCode grabKey = KeyCode.H;
    public float grabRange = 1.5f;
    public Transform grabPoint;
    public LayerMask objectLayers = ~0;

    [Header("Physics")]
    public float breakForce = 1000f;

    private Rigidbody2D playerRb;
    private Rigidbody2D grabbedRb;
    private FixedJoint2D grabJoint;

    void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Input.GetKeyDown(grabKey))
        {
            TryGrabObject();
        }

        if (Input.GetKeyUp(grabKey))
        {
            ReleaseObject();
        }
    }

    void TryGrabObject()
    {
        if (grabbedRb != null)
            return;

        Vector2 grabPosition = grabPoint != null
            ? (Vector2)grabPoint.position
            : (Vector2)transform.position;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            grabPosition,
            grabRange,
            objectLayers
        );

        float closestDistance = float.MaxValue;
        Rigidbody2D closestRb = null;

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            Rigidbody2D targetRb = hit.attachedRigidbody;

            if (targetRb == null || targetRb == playerRb)
                continue;

            // Hanya grab objek yang memiliki sistem lempar.
            if (targetRb.GetComponent<ThrownObjectCollision>() == null)
                continue;

            float distance = Vector2.Distance(
                grabPosition,
                targetRb.worldCenterOfMass
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestRb = targetRb;
            }
        }

        if (closestRb == null)
            return;

        grabbedRb = closestRb;

        // Buat joint pada batu agar pemain bisa
        // menarik dan mendorongnya secara fisik.
        grabJoint = grabbedRb.gameObject.AddComponent<FixedJoint2D>();
        grabJoint.connectedBody = playerRb;
        grabJoint.autoConfigureConnectedAnchor = false;
        grabJoint.anchor = grabbedRb.transform.InverseTransformPoint(
            grabbedRb.worldCenterOfMass
        );

        Vector2 playerAnchorPosition = grabPoint != null
            ? (Vector2)grabPoint.position
            : (Vector2)transform.position;

        grabJoint.connectedAnchor = playerRb.transform.InverseTransformPoint(
            playerAnchorPosition
        );

        grabJoint.enableCollision = true;
        grabJoint.breakForce = breakForce;

        Debug.Log("Batu berhasil di-grab!");
    }

    void ReleaseObject()
    {
        if (grabJoint != null)
            Destroy(grabJoint);

        grabJoint = null;
        grabbedRb = null;
    }

    void OnJointBreak2D(Joint2D brokenJoint)
    {
        if (brokenJoint == grabJoint)
        {
            grabJoint = null;
            grabbedRb = null;
        }
    }

    void OnDisable()
    {
        ReleaseObject();
    }

    void OnDrawGizmosSelected()
    {
        Vector3 position = grabPoint != null
            ? grabPoint.position
            : transform.position;

        
    }
}
