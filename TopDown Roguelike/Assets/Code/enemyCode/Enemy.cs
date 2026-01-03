using UnityEngine;

public class Enemy : MonoBehaviour
{
    public bool isBoss = true;
    public int maxHealth = 4;
    protected int currentHealth;
    protected Rigidbody2D rb;
    public System.Action OnDeathCallback;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    public virtual void TakeDamage(int dmg)
    {
        Debug.Log(currentHealth);
        currentHealth -= dmg;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        OnDeathCallback?.Invoke(); 
        Destroy(gameObject);
    }

    public virtual void AddDeathCallback(System.Action callback){
        OnDeathCallback+=callback;
    }
}