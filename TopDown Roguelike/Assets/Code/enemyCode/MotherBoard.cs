/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneBoss2D : MonoBehaviour
{
    [Header("References")]
    public GameObject bulletPrefab;
    public GameObject smallDronePrefab;
    public Transform shootPoint;
    public Rigidbody2D rb;

    [Header("Movement")]
    public float moveSpeed = 3f; 
    public float roamRadius = 5f;
    public float changeDirectionTime = 2f;

    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 2f;
    public float dashCooldown = 5f;

    [Header("Fan Attack")]
    public float fanBulletSpeed = 8f;
    public int fanBulletCount = 7;
    public float fanCooldown = 5f;

    [Header("Spawn Attack")]
    public int spawnCount = 3;
    public float spawnRadius = 2f;
    public float spawnCooldown = 10f;

    [Header("Ring Attack")]
    public float ringBulletSpeed = 6f;
    public int ringBulletCount = 12;
    public float ringCooldown = 8f;

    private Vector2 roamDirection;
    private bool isDashing = false;

    private Transform player;

private void Awake()
{
    
    player = GameObject.FindWithTag("Player")?.transform;
}

    private void Start()
    {

        StartCoroutine(AttackRoutine());
        StartCoroutine(RoamRoutine());
    }

    private void Update()
    {
    if (!isDashing)
    {
        AimAtPlayer();
    }
    }

public static Vector2 rotateVector(Vector2 v, float delta){
    delta *= Mathf.Deg2Rad;
    return new Vector2(v.x*Mathf.Cos(delta)-v.y*Mathf.Sin(delta),v.x*Mathf.Sin(delta)+v.y*Mathf.Cos(delta));
}

private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Before bounce: " + roamDirection);
        float delta = Random.Range(90,180f);
        roamDirection = rotateVector(roamDirection,delta).normalized;
        rb.rotation = Mathf.Atan2(roamDirection.y, roamDirection.x);
        Debug.Log("After bounce: " + roamDirection);
    }
    private IEnumerator RoamRoutine()
    {
        while (true)
        {
            if (!isDashing)
            {
                roamDirection = Random.insideUnitCircle.normalized;
            }
            yield return new WaitForSeconds(changeDirectionTime);
        }
    }

    private void FixedUpdate()
    {
        if(isDashing){
rb.velocity = roamDirection*dashSpeed;
        }else{
            rb.velocity = roamDirection;
        }
            
    }

    private void AimAtPlayer()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.rotation = angle;
    }
private IEnumerator AttackRoutine()
{
    while (true)
    {

        List<IEnumerator> attacks = new List<IEnumerator>()
        {
            DashAttack(),
            RingAttackCoroutine(),
            ShotgunBurst(),
            SpawnDronesCoroutine()
        };
        for (int i = 0; i < attacks.Count; i++)
        {
            int j = Random.Range(i, attacks.Count);
            var temp = attacks[i];
            attacks[i] = attacks[j];
            attacks[j] = temp;
        }
        foreach (IEnumerator attack in attacks)
        {
            yield return StartCoroutine(attack);
            if (attack == attacks.Find(a => a == SpawnDronesCoroutine()))
            {
                yield return new WaitForSeconds(spawnCooldown);
            }
            else if (attack == attacks.Find(a => a == DashAttack()))
            {
                yield return new WaitForSeconds(dashCooldown);
            }
            else if (attack == attacks.Find(a => a == RingAttackCoroutine()))
            {
                yield return new WaitForSeconds(ringCooldown);
            }
            else 
            {
                yield return new WaitForSeconds(fanCooldown);
            }
        }
    }
}

private IEnumerator DashAttack()
{
    Debug.Log("WHY");
    isDashing = true;

    roamDirection  = ((Vector2)(player.position - transform.position)).normalized;

    float elapsed = 0f;
    

    while (elapsed < dashDuration)
    {
        elapsed += Time.fixedDeltaTime; 
        yield return new WaitForFixedUpdate();
    }

    isDashing = false;
}

private void RotateToDirection(Vector2 dir)
{
    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    rb.rotation = angle;
}

private IEnumerator ShotgunBurst()
{
    int pelletCount = 5;    
    float spreadAngle = 180f;   
    float angleStep = 20f;

    Vector2 toPlayer = (player.position - transform.position).normalized;
    float baseAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
    float startAngle = baseAngle - spreadAngle / 2f;

    for (int i = 0; i < pelletCount; i++)
    {
        float angle = startAngle + i * angleStep;
        Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.right;
        ShootBullet(dir, fanBulletSpeed);
    }

    yield return null; 
}

private IEnumerator SpawnDronesCoroutine()
{
    for (int i = 0; i < spawnCount; i++)
    {
        Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;
        Instantiate(smallDronePrefab, spawnPos, Quaternion.identity);
        yield return new WaitForSeconds(0.1f);
    }
    yield return new WaitForSeconds(0.5f);
}

private IEnumerator RingAttackCoroutine()
{
    int initialBullets = 8;      
    float delayBeforeSplit = 0.5f; 
    int splitBullets = 3;       
    float splitSpread = 45f;    

    for (int i = 0; i < initialBullets; i++)
    {
        float angle = i * 360f / initialBullets;
        Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.right;
        ShootBullet(dir, ringBulletSpeed);
        StartCoroutine(SplitBulletAfterDelay(transform.position, dir, delayBeforeSplit, splitBullets, splitSpread));
    }

    yield return null;
}

private IEnumerator SplitBulletAfterDelay(Vector2 spawnPos, Vector2 baseDir, float delay, int splitBullets, float spreadAngle)
{
    yield return new WaitForSeconds(delay);

    float angleStep = spreadAngle / (splitBullets - 1);
    float startAngle = -spreadAngle / 2f;

    for (int i = 0; i < splitBullets; i++)
    {
        float angle = startAngle + i * angleStep;
        Vector2 dir = Quaternion.Euler(0, 0, angle) * baseDir;

        ShootBullet(dir, ringBulletSpeed); 
    }
}

    private void ShootBullet(Vector2 direction, float speed)
    {
GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
bullet.transform.localScale = new Vector3(2f, 2f, 1f);
Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
Collider2D bossCollider = GetComponent<Collider2D>();
float angel = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
bullet.transform.rotation = Quaternion.Euler(0, 0, angel);
if (bulletCollider != null && bossCollider != null)
{
    Physics2D.IgnoreCollision(bulletCollider, bossCollider);
}

        Rigidbody2D rbBullet = bullet.GetComponent<Rigidbody2D>();
        if (rbBullet != null)
        {
            rbBullet.velocity = direction.normalized * speed;
        }
    }
}
*/