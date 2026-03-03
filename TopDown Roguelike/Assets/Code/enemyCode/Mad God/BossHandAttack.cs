using UnityEngine;
using System.Collections;

public class BossHandAttack : MonoBehaviour
{
    public Transform arenaCenter;
    public Vector2 arenaSize = new Vector2(8f, 4f);

    public float smashHeight = 6f;
    public float gravity = 40f;
    public float impactDelay = 0.25f;

    public float floatAmplitude = 0.3f;
    public float floatSpeed = 1.5f;

    public Sprite fallingSprite;
    public float fallingRotation;
    public Vector2 fallingScale = Vector2.one;

    public GameObject shadowObject;
    public Vector2 shadowScale = new Vector2(1f, 1f);

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    private Animator animator;
    private SpriteRenderer sr;
    private Collider2D col;

    private bool isAttacking = false;

    private float idleDirection = 1f;
    private float idleOffset;

    void Start()
    {
        originalPosition = transform.position;
        originalScale = transform.localScale;
        originalRotation = transform.rotation;

        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        idleOffset = Random.Range(-floatAmplitude, floatAmplitude);

        if (shadowObject != null)
            shadowObject.SetActive(false);
    }

    void Update()
    {
        if (!isAttacking)
            LinearIdle();
    }

    void LinearIdle()
    {
        idleOffset += idleDirection * floatSpeed * Time.deltaTime;

        if (idleOffset > floatAmplitude)
        {
            idleOffset = floatAmplitude;
            idleDirection = -1f;
        }
        else if (idleOffset < -floatAmplitude)
        {
            idleOffset = -floatAmplitude;
            idleDirection = 1f;
        }

        transform.position = new Vector3(
            originalPosition.x,
            originalPosition.y + idleOffset,
            originalPosition.z
        );
    }

    public void StartSmash()
    {
        if (!isAttacking)
            StartCoroutine(SmashRoutine());
    }

    IEnumerator SmashRoutine()
    {
        isAttacking = true;

        animator.SetTrigger("MakeFist");
        yield return new WaitForSeconds(0.5f);

        if (col != null) col.enabled = false;

        float randomX = Random.Range(
            arenaCenter.position.x - arenaSize.x / 2,
            arenaCenter.position.x + arenaSize.x / 2
        );

        float randomY = Random.Range(
            arenaCenter.position.y - arenaSize.y / 2,
            arenaCenter.position.y + arenaSize.y / 2
        );

        Vector3 landingSpot = new Vector3(randomX, randomY, 0);

        if (shadowObject != null)
        {
            shadowObject.transform.position = landingSpot;
            shadowObject.transform.localScale = new Vector3(shadowScale.x, shadowScale.y, 1f);
            shadowObject.SetActive(true);
        }

        Vector3 startPos = new Vector3(
            landingSpot.x,
            landingSpot.y + smashHeight,
            landingSpot.z
        );

        transform.position = startPos;

        animator.enabled = false;

        if (fallingSprite != null)
            sr.sprite = fallingSprite;

        transform.localScale = new Vector3(fallingScale.x, fallingScale.y, 1f);
        transform.rotation = Quaternion.Euler(0f, 0f, fallingRotation);

        float velocity = 0f;

        while (transform.position.y > landingSpot.y)
        {
            velocity += gravity * Time.deltaTime;
            transform.position -= new Vector3(0, velocity * Time.deltaTime, 0);
            yield return null;
        }

        transform.position = landingSpot;

        if (col != null) col.enabled = true;

        yield return new WaitForSeconds(impactDelay);

        if (shadowObject != null)
            shadowObject.SetActive(false);

        transform.position = originalPosition;
        transform.localScale = originalScale;
        transform.rotation = originalRotation;

        animator.enabled = true;

        isAttacking = false;
    }
}