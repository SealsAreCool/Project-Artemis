using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class FloatingRangedEnemy : MonoBehaviour
{
    [Header("Targeting")]
    public Transform player;
    public float detectionRadius = 10f;
    public float preferredDistance = 5f;
    public float distanceTolerance = 1f;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float floatStrength = 1.5f;
    public float floatChangeInterval = 2f;

    [Header("Dash Attack")]
    public float dashSpeed = 12f;
    public float dashDuration = 0.35f;
    public float dashCooldown = 3f;
    public float dashTriggerDistance = 6f;

    [Header("Collision")]
    public LayerMask wallLayer;
    public float wallCheckPadding = 0.05f;

    [Header("Combat")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireCooldown = 2f;
    public float projectileSpeed = 10f;

    Rigidbody2D rb;
    Collider2D col;

    Vector2 floatOffset;
    float floatTimer;
    float fireTimer;

    bool isDashing;
    float dashTimer;
    float dashCooldownTimer;
    Vector2 dashDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        PickNewFloatOffset();
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > detectionRadius)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        RotateTowardsPlayer();
        HandleFloating();
        HandleShooting(dist);

        dashCooldownTimer += Time.deltaTime;

        if (!isDashing &&
            dashCooldownTimer >= dashCooldown &&
            dist <= dashTriggerDistance)
        {
            TryStartDash();
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        if (isDashing)
        {
            DashMove();
            return;
        }

        NormalMove();
    }

    void NormalMove()
    {
        float dist = Vector2.Distance(transform.position, player.position);
        Vector2 dir = (player.position - transform.position).normalized;
        Vector2 desired = Vector2.zero;

        if (dist > preferredDistance + distanceTolerance)
            desired = dir * moveSpeed;
        else if (dist < preferredDistance - distanceTolerance)
            desired = -dir * moveSpeed;

        desired += floatOffset;

        MoveWithWallCheck(desired);
    }

    void DashMove()
    {
        dashTimer += Time.fixedDeltaTime;

        if (dashTimer >= dashDuration)
        {
            EndDash();
            return;
        }

        MoveWithWallCheck(dashDirection * dashSpeed, true);
    }

    // 🔑 THIS IS THE CORE FIX
    void MoveWithWallCheck(Vector2 velocity, bool isDash = false)
    {
        float distance = velocity.magnitude * Time.fixedDeltaTime;
        Vector2 dir = velocity.normalized;

        RaycastHit2D hit = Physics2D.CircleCast(
            col.bounds.center,
            col.bounds.extents.x,
            dir,
            distance + wallCheckPadding,
            wallLayer
        );

        if (hit.collider != null)
        {
            if (isDash)
            {
                EndDash();
                return;
            }

            // Slide along wall
            Vector2 slideDir = Vector2.Perpendicular(hit.normal);
            rb.velocity = slideDir * velocity.magnitude * 0.5f;
        }
        else
        {
            rb.velocity = velocity;
        }
    }

    void TryStartDash()
    {
        dashDirection = (player.position - transform.position).normalized;
        dashTimer = 0f;
        dashCooldownTimer = 0f;
        isDashing = true;
    }

    void EndDash()
    {
        isDashing = false;
        rb.velocity = Vector2.zero;
    }

    void RotateTowardsPlayer()
    {
        Vector2 dir = player.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void HandleFloating()
    {
        floatTimer += Time.deltaTime;
        if (floatTimer >= floatChangeInterval)
        {
            PickNewFloatOffset();
            floatTimer = 0f;
        }
    }

    void PickNewFloatOffset()
    {
        floatOffset = Random.insideUnitCircle * floatStrength;
    }

    void HandleShooting(float dist)
    {
        if (isDashing || projectilePrefab == null || firePoint == null) return;

        fireTimer += Time.deltaTime;
        if (fireTimer < fireCooldown) return;

        fireTimer = 0f;

        Vector2 shootDir = (player.position - firePoint.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        Rigidbody2D rbProj = proj.GetComponent<Rigidbody2D>();
        if (rbProj != null)
            rbProj.velocity = shootDir * projectileSpeed;

        float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, preferredDistance);
    }
}
