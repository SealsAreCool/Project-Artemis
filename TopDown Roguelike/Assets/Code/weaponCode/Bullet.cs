using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 0;
    public float lifetime = 2f;
    public bool isEnemy;

    void Start()
    {
    GetComponent<Rigidbody2D>().velocity = transform.right * speed;
        Destroy(gameObject, lifetime); 
    }

    void Update()
    {
    }

void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log(isEnemy);
        if (col.collider.CompareTag("Enemy"))
        {
            col.collider.GetComponent<Enemy>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
        if(col.collider.CompareTag("Player")){
            col.collider.GetComponent<MaskHealth>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (col.collider.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
