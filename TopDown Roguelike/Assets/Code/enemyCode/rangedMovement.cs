using UnityEngine;

public class TeleportingRangedEnemy : MonoBehaviour
{
    public Transform player;
    public float minTeleportRadius = 3f;
    public float maxTeleportRadius = 7f;
    public float dashSpeed = 15f;
    public float teleportCooldown = 3f;
    public float disappearDuration = 0.3f;

    public GameObject projectilePrefab;
    public Transform firePoint;

    private float teleportTimer = 0f;
    private bool isInvisible = false;
    private bool isDashing = false;
    private Vector3 dashTarget;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    void Update()
    {
        teleportTimer += Time.deltaTime;

        if (!isDashing)
            RotateTowardsPlayer();

        if (isDashing)
        {
            transform.position = Vector3.MoveTowards(transform.position, dashTarget, dashSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, dashTarget) < 0.1f)
            {
                isDashing = false;
            }
        }
        else if (!isInvisible && teleportTimer >= teleportCooldown)
        {
            teleportTimer = 0f;
            StartCoroutine(TeleportAndAct());
        }
    }

    void RotateTowardsPlayer()
    {
        if (player == null) return;
        Vector3 lookDir = player.position - transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

System.Collections.IEnumerator TeleportAndAct()
{
    isInvisible = true;
    SpriteRenderer sr = GetComponent<SpriteRenderer>();
    if (sr != null)
        sr.enabled = false;

    yield return new WaitForSeconds(disappearDuration);

    float angle = Random.Range(0f, 360f);
    float radius = Random.Range(minTeleportRadius, maxTeleportRadius);
    Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0f) * radius;
    transform.position = player.position + offset;

    int action = Random.Range(0, 2); 

    if (action == 0 && projectilePrefab != null)
    {
        Vector3 spawnPos = transform.position;
        Vector3 shootDir = (player.position - transform.position).normalized; 
GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
if (rb != null)
    rb.velocity = shootDir * 10f;
float angel = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
proj.transform.rotation = Quaternion.Euler(0, 0, angel);

    }
    else 
    {
        RotateTowardsPlayer();
        dashTarget = player.position - offset;
        isDashing = true;
    }

    if (sr != null)
        sr.enabled = true;
    isInvisible = false;
}
}
