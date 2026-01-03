using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class DroneBoss2D : MonoBehaviour
{
    enum BossState
    {
        Idle,
        Dash,
        FanShot,
        RingShot,
        SpawnDrones,
        Cooldown
    }

    [Header("References")]
    public GameObject bulletPrefab;
    public GameObject smallDronePrefab;
    public Transform shootPoint;
    public Rigidbody2D rb;

    [Header("Idle Movement (Floaty)")]
    public float idleSpeed = 2f;
    public float floatStrength = 1.2f;
    public float floatChangeTime = 2f;

    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashDuration = 1.2f;
    public float dashCooldown = 2f;

    [Header("Fan Attack")]
    public float fanBulletSpeed = 8f;
    public int fanBulletCount = 7;
    public float fanCooldown = 2f;

    [Header("Ring Attack")]
    public float ringBulletSpeed = 6f;
    public int ringBulletCount = 12;
    public float ringCooldown = 3f;

    [Header("Spawn Attack")]
    public int spawnCount = 3;
    public float spawnRadius = 2f;
    public float spawnCooldown = 4f;

    BossState currentState = BossState.Idle;
    Transform player;

    Vector2 moveDirection;
    Vector2 floatOffset;

    float stateTimer;
    float floatTimer;
    float cooldownTimer;

    void Awake()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        PickNewFloatOffset();
    }

    void Update()
    {
        stateTimer += Time.deltaTime;

        switch (currentState)
        {
            case BossState.Idle:
                UpdateIdle();
                break;

            case BossState.Dash:
                if (stateTimer >= dashDuration)
                    EnterCooldown(dashCooldown);
                break;

            case BossState.Cooldown:
                cooldownTimer -= Time.deltaTime;
                if (cooldownTimer <= 0f)
                    EnterIdle();
                break;
        }
    }

    void FixedUpdate()
    {
        Move();
        RotateToVelocity();
    }
    void EnterIdle()
    {
        currentState = BossState.Idle;
        stateTimer = 0f;
    }

    void UpdateIdle()
    {
        floatTimer += Time.deltaTime;
        if (floatTimer >= floatChangeTime)
        {
            PickNewFloatOffset();
            floatTimer = 0f;
        }


        moveDirection = Vector2.Lerp(moveDirection, floatOffset, 0.05f);

        if (stateTimer > 1.5f)
            PickNextAttack();
    }

    void PickNextAttack()
    {
        stateTimer = 0f;

        int choice = Random.Range(0, 4);
        switch (choice)
        {
            case 0: EnterDash(); break;
            case 1: FireFan(); EnterCooldown(fanCooldown); break;
            case 2: FireRing(); EnterCooldown(ringCooldown); break;
            case 3: SpawnDrones(); EnterCooldown(spawnCooldown); break;
        }
    }

    void EnterDash()
    {
        currentState = BossState.Dash;
        stateTimer = 0f;

        if (player != null)
            moveDirection = (player.position - transform.position).normalized;
    }

    void EnterCooldown(float time)
    {
        currentState = BossState.Cooldown;
        cooldownTimer = time;
    }

    void PickNewFloatOffset()
    {
        Vector2 v;
        do
        {
            v = Random.insideUnitCircle;
        } while (v.sqrMagnitude < 0.01f);

        floatOffset = v.normalized * floatStrength;
    }
    void Move()
    {
        switch (currentState)
        {
            case BossState.Dash:
                rb.velocity = moveDirection * dashSpeed;
                break;

            case BossState.Idle:
            case BossState.Cooldown:
                rb.velocity = moveDirection * idleSpeed;
                break;

            default:
                rb.velocity = Vector2.zero;
                break;
        }
    }

void RotateToVelocity()
{
    float targetAngle = rb.rotation; 

    switch(currentState)
    {
        case BossState.Idle:
        case BossState.FanShot:
        case BossState.RingShot:
        case BossState.SpawnDrones:
        case BossState.Cooldown:
            if (player == null) return;
            Vector2 dirToPlayer = (player.position - transform.position);
            if (dirToPlayer.sqrMagnitude < 0.01f) return; 
            targetAngle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
            break;

        case BossState.Dash:
            if (rb.velocity.sqrMagnitude < 0.01f) return;
            targetAngle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            break;
    }

    rb.rotation = Mathf.LerpAngle(rb.rotation, targetAngle, 15f * Time.fixedDeltaTime);
}
    void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 normal = collision.contacts[0].normal;
        moveDirection = Vector2.Reflect(moveDirection, normal).normalized;
    }
    void FireFan()
    {
        if (player == null) return;

        Vector2 baseDir = (player.position - transform.position).normalized;
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
        float spread = 180f;
        float step = spread / (fanBulletCount - 1);
        float start = baseAngle - spread / 2f;

        for (int i = 0; i < fanBulletCount; i++)
        {
            float angle = start + step * i;
            ShootBullet(Quaternion.Euler(0, 0, angle) * Vector2.right, fanBulletSpeed);
        }
    }

    void FireRing()
    {
        for (int i = 0; i < ringBulletCount; i++)
        {
            float angle = i * 360f / ringBulletCount;
            ShootBullet(Quaternion.Euler(0, 0, angle) * Vector2.right, ringBulletSpeed);
        }
    }

    void SpawnDrones()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;
            Instantiate(smallDronePrefab, pos, Quaternion.identity);
        }
    }

void ShootBullet(Vector2 dir, float speed)
{
    GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
    Rigidbody2D rbBullet = bullet.GetComponent<Rigidbody2D>();
    Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
    Collider2D bossCollider = GetComponent<Collider2D>();

    bullet.transform.localScale = new Vector3(2f, 2f, 1f);
    if (bulletCollider != null && bossCollider != null)
        Physics2D.IgnoreCollision(bulletCollider, bossCollider);

    if (rbBullet != null)
        rbBullet.velocity = dir.normalized * speed;

    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
}

}
