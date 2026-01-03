using UnityEngine;
using System.Collections;

public class SwordProjectile : MonoBehaviour
{
    public OrbitingSwords orbitManager;
    public float speed = 100f;
    public float maxDistance = 50f;
    public int damage = 5;
    public float rotationSpeed = 360f;

    private Vector3 startPos;
    private Vector3 direction;
    private bool isLaunched = false;
    private bool returning = false;
    private float currentRotation = 0f;
    private Material swordMaterial;

    private void Awake()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        swordMaterial = Instantiate(sr.material);
        sr.material = swordMaterial;
    }

    public void Initialize(Vector3 targetPos)
    {
        if (isLaunched) return;

        startPos = transform.position;
        direction = (targetPos - startPos).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + 90);

        currentRotation = 0f;
        isLaunched = true;
        returning = false;
    }

    private void Update()
    {
        if (!isLaunched) return;

        if (!returning)
        {
            transform.position += direction * speed * Time.deltaTime;

            if (Vector3.Distance(startPos, transform.position) >= maxDistance)
            {
                if (orbitManager != null)
                    returning = true;
            }
        }
else
{
    Vector3 target = orbitManager.transform.position;
    direction = (target - transform.position).normalized;
    transform.position += direction * speed * Time.deltaTime;

    float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
    transform.rotation = Quaternion.RotateTowards(transform.rotation,
                                                  Quaternion.Euler(0, 0, targetAngle),
                                                  rotationSpeed * Time.deltaTime);

    if (Vector3.Distance(target, transform.position) < 0.5f)
    {
        isLaunched = false;
        returning = false;
        transform.position = orbitManager.GetNextSwordPosition(transform);
        orbitManager.OnSwordReturned(transform);
    }
}

    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Enemy") && isLaunched)
        {
            col.GetComponent<Enemy>()?.TakeDamage(damage);
        }
        else if (col.CompareTag("Wall") && isLaunched)
        {
            if (!returning && orbitManager != null)
            {
                returning = true;
            }
        }
    }
}
