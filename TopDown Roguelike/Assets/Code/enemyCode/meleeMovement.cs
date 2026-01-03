using UnityEngine;

public class meleeMovement : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 3f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;
    public int damage = 1;
    public Transform weaponPoint; 
    private Animator swing;
    private float cooldownTimer = 0f;
    private Rigidbody2D rb;
    
    private void Awake(){
        rb = GetComponent<Rigidbody2D>();
            if (player == null)
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }
    }

    void Update()
    {
        if (player == null) return;
        float distance = Vector2.Distance(transform.position, player.position);
        Vector3 dir = player.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);


        cooldownTimer -= Time.deltaTime;
        if (distance <= attackRange && cooldownTimer <= 0f)
        {
            SwingWeapon();
            cooldownTimer = attackCooldown;
        }
    }

        private void FixedUpdate()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > attackRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            Vector2 move = direction * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);
        }
    }

    void SwingWeapon()
    {
        if (player != null)
        {
            Collider2D hit = Physics2D.OverlapCircle(weaponPoint.position, 0.5f, LayerMask.GetMask("Player"));
            if (hit != null)
            {
                hit.GetComponent<MaskHealth>()?.TakeDamage(damage);
            }
        }
    }


    void OnDrawGizmosSelected()
    {
        if (weaponPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(weaponPoint.position, 0.5f);
        }
    }
}
