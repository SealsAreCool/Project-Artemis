using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 18f;

    [Header("Dash")]
    public float dashSpeed = 36f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.6f;

    [Header("Afterimages")]
    public int poolSize = 5;
    public float spawnRate = 0.08f;
    public float ghostLifetime = 0.5f;

    Rigidbody2D rb;
    Animator animator;
    SpriteRenderer sr;

    Vector2 move;
    Vector2 lastMove;

    bool isDashing;
    bool canDash = true;

    Vector2 dashDir;

    SpriteRenderer[] ghosts;
    float[] ghostTimers;
    int ghostIndex;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        rb.freezeRotation = true;

        // Create ghost pool once
        ghosts = new SpriteRenderer[poolSize];
        ghostTimers = new float[poolSize];

        GameObject container = new GameObject("DashGhostPool");

        for (int i = 0; i < poolSize; i++)
        {
            GameObject g = new GameObject("ghost_" + i);
            g.transform.parent = container.transform;

            SpriteRenderer gsr = g.AddComponent<SpriteRenderer>();
            gsr.enabled = false;

            ghosts[i] = gsr;
        }
    }

    void Update()
    {
        if (!isDashing)
        {
            move.x = Input.GetAxisRaw("Horizontal");
            move.y = Input.GetAxisRaw("Vertical");
            move = move.normalized;

            if (move != Vector2.zero)
                lastMove = move;
        }

        animator.SetFloat("Horizontal", move.x);
        animator.SetFloat("Vertical", move.y);
        animator.SetFloat("Speed", move.sqrMagnitude);

        animator.SetFloat("LastHorizontal", lastMove.x);
        animator.SetFloat("LastVertical", lastMove.y);

        if (Input.GetKeyDown(KeyCode.Q) && canDash)
            StartCoroutine(Dash());

        UpdateGhosts();
    }

    void FixedUpdate()
    {
        if (isDashing)
            rb.velocity = dashDir * dashSpeed;
        else
            rb.velocity = move * speed;
    }

    IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;

        dashDir = lastMove == Vector2.zero ? Vector2.down : lastMove;

        float timer = dashDuration;
        float spawnTimer = 0;

        while (timer > 0)
        {
            spawnTimer -= Time.deltaTime;

            if (spawnTimer <= 0)
            {
                SpawnGhost();
                spawnTimer = spawnRate;
            }

            timer -= Time.deltaTime;
            yield return null;
        }

        isDashing = false;
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void SpawnGhost()
    {
        SpriteRenderer g = ghosts[ghostIndex];

        g.sprite = sr.sprite;
        g.flipX = sr.flipX;
        g.flipY = sr.flipY;

        g.transform.position = sr.transform.position;
        g.transform.rotation = sr.transform.rotation;
        g.transform.localScale = sr.transform.lossyScale;

        g.sortingLayerID = sr.sortingLayerID;
        g.sortingOrder = sr.sortingOrder - 1;

        g.color = new Color(0.7f, 0.9f, 1f, 0.6f);

        g.enabled = true;

        ghostTimers[ghostIndex] = ghostLifetime;

        ghostIndex++;
        if (ghostIndex >= poolSize)
            ghostIndex = 0;
    }

    void UpdateGhosts()
    {
        for (int i = 0; i < ghosts.Length; i++)
        {
            if (!ghosts[i].enabled) continue;

            ghostTimers[i] -= Time.deltaTime;

            float t = ghostTimers[i] / ghostLifetime;

            Color c = ghosts[i].color;
            c.a = t * 0.6f;
            ghosts[i].color = c;

            if (ghostTimers[i] <= 0)
                ghosts[i].enabled = false;
        }
    }
}