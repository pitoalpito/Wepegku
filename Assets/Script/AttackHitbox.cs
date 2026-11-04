
using UnityEngine;

public class AttackHitboxDamage : MonoBehaviour
{
    [Header("Attack Settings")]
    public int damage = 20;

    private void OnTriggerEnter2D(Collider2D other)
    {
        KeyItem key = other.GetComponentInParent<KeyItem>();

        if (key != null)
        {
            Vector2 keyDirection =
                key.transform.position - transform.root.position;

            keyDirection.y = 0f;

            if (keyDirection.x == 0f)
                keyDirection.x = 1f;

            keyDirection.Normalize();

            BasicAttack keyPlayerAttack =
                transform.root.GetComponent<BasicAttack>();

            if (keyPlayerAttack != null)
            {
                if (keyPlayerAttack.CurrentAttackType ==
                    EnemyHitReaction.AttackType.Uppercut)
                {
                    key.HitKey(keyDirection, 2f, 10f);
                }
                else
                {
                    key.HitKey(keyDirection, 8f, 2f);
                }
            }

            return;
        }

        EnemyHitReaction enemy =
            other.GetComponentInParent<EnemyHitReaction>();

        EnemyHealth health =
            other.GetComponentInParent<EnemyHealth>();

        if (enemy == null || health == null || health.IsDead)
            return;

        // Arah knockback dari player menuju enemy.
        Vector2 direction =
            enemy.transform.position - transform.root.position;

        direction.y = 0f;

        if (direction.x == 0f)
            direction.x = 1f;

        direction.Normalize();

        // Jalankan efek serangan sesuai tipe attack player.
        BasicAttack playerAttack =
            transform.root.GetComponent<BasicAttack>();

        if (playerAttack != null)
        {
            enemy.TakeHit(
                direction,
                playerAttack.CurrentAttackType
            );
        }
        else
        {
            enemy.TakeHit(
                direction,
                EnemyHitReaction.AttackType.Basic
            );
        }

        // Kurangi HP enemy.
        health.TakeDamage(damage);
    }


}
