using UnityEngine;

public class BossLaser : MonoBehaviour
{
    public Transform eye;
    public GameObject startPrefab;
    public GameObject middlePrefab;
    public GameObject endPrefab;
    public LayerMask wallLayer;
    public LayerMask playerLayer;
    public float damagePerHit = 1f;
    public float sweepSpeed = 120f;
    public float telegraphDuration = 1f;

    [HideInInspector] public float currentAngle;
    [HideInInspector] public bool firing;

    GameObject startInstance;
    GameObject middleInstance;
    GameObject endInstance;

    bool telegraphing;
    bool hitApplied;
    float telegraphTimer;

    float middleOriginalLength;

    public void BeginTelegraph()
    {
        if (startPrefab == null || middlePrefab == null || endPrefab == null || eye == null)
            return;

        if (startInstance == null)
            startInstance = Instantiate(startPrefab, eye.position, eye.rotation, eye);

        if (middleInstance == null)
        {
            middleInstance = Instantiate(middlePrefab, eye.position, eye.rotation, eye);

            SpriteRenderer sr = middleInstance.GetComponent<SpriteRenderer>();
            middleOriginalLength = sr.bounds.size.x;
        }

        if (endInstance == null)
            endInstance = Instantiate(endPrefab, eye.position, eye.rotation, eye);

        startInstance.SetActive(true);
        middleInstance.SetActive(true);
        endInstance.SetActive(true);

        telegraphing = true;
        firing = false;
        hitApplied = false;
        telegraphTimer = 0f;
    }

    public void EndLaser()
    {
        firing = false;
        telegraphing = false;
        hitApplied = false;

        if (startInstance != null) startInstance.SetActive(false);
        if (middleInstance != null) middleInstance.SetActive(false);
        if (endInstance != null) endInstance.SetActive(false);
    }

    void Update()
    {
        if (telegraphing)
        {
            telegraphTimer += Time.deltaTime;

            if (telegraphTimer >= telegraphDuration)
            {
                telegraphing = false;
                firing = true;
                currentAngle = eye.eulerAngles.z;
            }

            return;
        }

        if (!firing) return;

        currentAngle += sweepSpeed * Time.deltaTime;

        Vector2 origin = eye.position;
        Vector2 dir = Quaternion.Euler(0, 0, currentAngle) * Vector2.right;

        RaycastHit2D wallHit = Physics2D.Raycast(origin, dir, 100f, wallLayer);

        float distance = wallHit.collider != null ? wallHit.distance : 20f;

        Vector2 endPos = wallHit.collider != null
            ? wallHit.point
            : origin + dir * distance;

        RaycastHit2D playerHit = Physics2D.Raycast(origin, dir, distance, playerLayer);

        if (playerHit.collider != null && !hitApplied)
        {
            playerHit.collider.GetComponent<MaskHealth>()?.TakeDamage((int)damagePerHit);
            hitApplied = true;
        }

        if (startInstance != null)
        {
            startInstance.transform.position = origin;
            startInstance.transform.rotation = Quaternion.Euler(0, 0, currentAngle);
        }

        if (middleInstance != null)
        {
            middleInstance.transform.position = origin;
            middleInstance.transform.rotation = Quaternion.Euler(0, 0, currentAngle);

            Vector3 scale = middleInstance.transform.localScale;
            scale.x = distance / middleOriginalLength;
            middleInstance.transform.localScale = scale;
        }

        if (endInstance != null)
        {
            endInstance.transform.position = endPos;
            endInstance.transform.rotation = Quaternion.Euler(0, 0, currentAngle);
        }
    }
}
