
using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Shooting")]
    public GameObject projectilePrefab;
    public float shootInterval = 2f;
    public float spawnOffsetX = 0.8f;

    private float shootTimer;

    void Update()
    {
        shootTimer -= Time.deltaTime;

        if (shootTimer <= 0f)
        {
            Shoot();
            shootTimer = shootInterval;
        }
    }

    void Shoot()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError(
                "Projectile Prefab belum diisi!",
                this
            );
            return;
        }

        Vector3 spawnPosition = transform.position
            + Vector3.right * spawnOffsetX;

        GameObject bullet = Instantiate(
            projectilePrefab,
            spawnPosition,
            Quaternion.identity
        );

        EnemyProjectile projectile =
            bullet.GetComponent<EnemyProjectile>();

        if (projectile != null)
        {
            projectile.SetShooter(transform);
        }
        else
        {
            Debug.LogError(
                "Prefab peluru tidak memiliki EnemyProjectile!",
                bullet
            );
        }

        Debug.Log("Enemy menembakkan peluru!");
    }
}

