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
    public Vector2 startLocalOffset = new Vector2(1.17f, -0.3f);
    public Vector2 middleLocalOffset = new Vector2(1.8f, -0.3f);
    public float middleExtra = 2f;
    public float endInset = 0.02f;

    [HideInInspector] public float currentAngle;
    [HideInInspector] public bool firing;

    GameObject startInstance;
    GameObject middleInstance;
    GameObject endInstance;

    bool telegraphing;
    bool hitApplied;
    float telegraphTimer;

    float startLength;
    float middleOriginalLength;
    float endLength;

    public void BeginTelegraph()
    {
        telegraphing = true;
        firing = false;
        hitApplied = false;
        telegraphTimer = 0f;
    }

    public void EndLaser()
    {
        firing = false;
        telegraphing = false;
        if (startInstance) startInstance.SetActive(false);
        if (middleInstance) middleInstance.SetActive(false);
        if (endInstance) endInstance.SetActive(false);
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

                if (!startInstance)
                {
                    startInstance = Instantiate(startPrefab, eye.position, eye.rotation, eye);
                    startLength = startInstance.GetComponent<SpriteRenderer>().bounds.size.x;
                }

                if (!middleInstance)
                {
                    middleInstance = Instantiate(middlePrefab, eye.position, eye.rotation, eye);
                    middleOriginalLength = middleInstance.GetComponent<SpriteRenderer>().bounds.size.x;
                }

                if (!endInstance)
                {
                    endInstance = Instantiate(endPrefab, eye.position, eye.rotation, eye);
                    endLength = endInstance.GetComponent<SpriteRenderer>().bounds.size.x;
                }

                startInstance.SetActive(true);
                middleInstance.SetActive(true);
                endInstance.SetActive(true);
            }
            return;
        }

        if (!firing) return;

        currentAngle += sweepSpeed * Time.deltaTime;
        Vector2 origin = eye.position;
        Quaternion rot = Quaternion.Euler(0, 0, currentAngle);
        Vector2 dir = rot * Vector2.right;

        RaycastHit2D wallHit = Physics2D.Raycast(origin, dir, 100f, wallLayer);
        float distance = wallHit.collider ? wallHit.distance : 20f;
        Vector2 hitPoint = wallHit.collider ? wallHit.point : origin + dir * distance;

        RaycastHit2D playerHit = Physics2D.Raycast(origin, dir, distance, playerLayer);
        if (playerHit.collider && !hitApplied)
        {
            playerHit.collider.GetComponent<MaskHealth>()?.TakeDamage((int)damagePerHit);
            hitApplied = true;
        }

        Vector2 startPos = origin + (Vector2)(rot * startLocalOffset);
        startInstance.transform.position = startPos;
        startInstance.transform.rotation = rot;

        Vector2 middlePos = origin + (Vector2)(rot * middleLocalOffset);
        float middleLength = distance - startLength;
        middleInstance.transform.position = middlePos;
        middleInstance.transform.rotation = rot;
        Vector3 middleScale = middleInstance.transform.localScale;
        middleScale.x = middleExtra*middleLength / middleOriginalLength ;
        middleInstance.transform.localScale = middleScale;

        if (wallHit.collider)
        {
            float angle = Mathf.Atan2(wallHit.normal.y, wallHit.normal.x) * Mathf.Rad2Deg + 180f;
            endInstance.transform.rotation = Quaternion.Euler(0, 0, angle);
            Vector2 beamCenter = origin + dir * distance;
            Vector2 endPos = beamCenter;
            float a = currentAngle % 360f;
            if (a >= 45f && a <= 135f || a >= 225f && a <= 315f)
                endPos.x = hitPoint.x;
            else
                endPos.y = hitPoint.y;

            endInstance.transform.position = endPos - wallHit.normal * endInset;
        }
        else
        {
            endInstance.transform.position = origin + dir * distance;
            endInstance.transform.rotation = rot;
        }
    }
}
