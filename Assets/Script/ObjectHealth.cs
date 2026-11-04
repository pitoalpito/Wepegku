
using UnityEngine;

public class ObjectHealth : MonoBehaviour
{
    [Header("Object Health")]
    public int maxHealth = 30;

    public int CurrentHealth { get; private set; }
    public bool IsDestroyed { get; private set; }

    void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (IsDestroyed || damage <= 0)
            return;

        CurrentHealth -= damage;
        CurrentHealth = Mathf.Max(CurrentHealth, 0);

        Debug.Log(
            gameObject.name + " HP: " +
            CurrentHealth + "/" + maxHealth
        );

        if (CurrentHealth <= 0)
        {
            BreakObject();
        }
    }

    void BreakObject()
    {
        if (IsDestroyed)
            return;

        IsDestroyed = true;

        Debug.Log(gameObject.name + " hancur!");

        Destroy(gameObject);
    }
}
