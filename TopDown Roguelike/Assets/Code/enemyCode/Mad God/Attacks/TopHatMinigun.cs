using UnityEngine;
using System.Collections;
 
// Attach to the boss root or a dedicated manager object.
// Call LaunchAttack() from your boss controller after checking phase.topHatEnabled.
public class TopHatMinigun : MonoBehaviour
{
    [Header("Arena Bounds")]
    public float arenaHalfWidth  = 8f;   // distance from centre to left/right wall
    public float arenaTop        = 5f;   // world-space Y for the hat spawn row
 bool spawnLeft;
public bool IsAttacking { get; private set; }

    [Header("Hat")]
    public GameObject hatPrefab;
    public float      hatScale   = 1f;
 
    [Header("Sweep")]
    public float sweepDuration   = 3.5f; // total time the hat fires while sweeping
    public float fireRate        = 0.07f; // seconds between bird spawns (minigun cadence)
    // Sweep goes from the hat's side toward the opposite wall.
    // Angle range (degrees) relative to straight-down (0 = straight down).
    public float sweepAngleStart = -50f;  // starts aimed toward far side
    public float sweepAngleEnd   =  50f;  // ends aimed toward near side
 
    [Header("Birds")]
    public GameObject birdPrefab;
    public float      birdSpeed  = 10f;
    public float      birdLifetime = 4f;
    public Sprite     birdSprite;         // fallback if prefab has no sprite set
 
    [Header("Timing")]
    public float windupTime  = 0.6f;  // pause after hat appears before firing starts
    public float lingerTime  = 0.4f;  // pause after firing before hat disappears
 
    // ── Public entry point ────────────────────────────────────────────────────
    public void LaunchAttack()
    {
        StartCoroutine(RunAttack());
    }
 
    // ── Core coroutine ────────────────────────────────────────────────────────
    IEnumerator RunAttack()
    {
        IsAttacking = true;
        // Pick left or right spawn at random
        spawnLeft = Random.value < 0.5f;
        float spawnX   = spawnLeft ? -arenaHalfWidth + 1f : arenaHalfWidth - 1f;
Vector3 hatPos = transform.TransformPoint(new Vector3(spawnX, arenaTop, 0f));
 
        // Instantiate hat
        GameObject hat = hatPrefab
            ? Instantiate(hatPrefab, hatPos, Quaternion.identity)
            : CreateFallbackRect(hatPos, Color.black, new Vector2(1.2f, 0.9f), "Hat");
 
        hat.transform.localScale = Vector3.one * hatScale;
 
        // Windup – hat appears, brief pause before the barrage
        yield return new WaitForSeconds(windupTime);
 
        // Fire sweep
        float elapsed    = 0f;
        float nextFire   = 0f;
 
        // Sweep direction: if hat is on the left, sweep angle goes from negative (left) to positive (right)
        // relative to straight-down. Flip when on the right so it sweeps across the arena.
float angleA = 0f; // start straight down
float angleB = spawnLeft ? 90f : -90f;
// left hat sweeps toward right
// right hat sweeps toward left
// left hat sweeps down → right
// right hat sweeps down → left
 
        while (elapsed < sweepDuration)
        {
            elapsed  += Time.deltaTime;
            nextFire -= Time.deltaTime;
 
            if (nextFire <= 0f)
            {
                float t        = Mathf.Clamp01(elapsed / sweepDuration);
float angleDeg = Mathf.SmoothStep(angleA, angleB, t);
                FireBird(hat.transform.position, angleDeg);
                nextFire = fireRate;
            }
 
            yield return null;
        }
 
        // Linger then vanish
        yield return new WaitForSeconds(lingerTime);
            Destroy(hat);
    IsAttacking = false;
    }
 
    // ── Helpers ───────────────────────────────────────────────────────────────
 
    void FireBird(Vector3 origin, float angleDeg)
{
    // Rotate from straight down
    Vector2 dir = Quaternion.Euler(0f, 0f, angleDeg) * Vector2.down;

    GameObject bird;

    if (birdPrefab)
    {
        bird = Instantiate(birdPrefab, origin, Quaternion.identity);
    }
    else
    {
        bird = CreateFallbackRect(origin, new Color(0.15f, 0.1f, 0.05f),
                                  new Vector2(0.4f, 0.25f), "Bird");
        if (birdSprite)
            bird.GetComponent<SpriteRenderer>().sprite = birdSprite;
    }

float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

// Base rotation (tune this once)
bird.transform.rotation = Quaternion.Euler(0f, 0f, angle - (spawnLeft?225f:135f));

// Fix inversion using flip instead of rotation hacks
var sr = bird.GetComponent<SpriteRenderer>();
if (sr != null)
{
    sr.flipY = dir.x > 0f; // flip when going left
}


    var rb = bird.GetComponent<Rigidbody2D>();
    if (!rb) rb = bird.AddComponent<Rigidbody2D>();

    rb.gravityScale = 0f;
    rb.velocity = dir * birdSpeed;

    Destroy(bird, birdLifetime);
}
    // Creates a coloured sprite-renderer placeholder when no prefab is assigned
    GameObject CreateFallbackRect(Vector3 pos, Color color, Vector2 size, string label)
    {
        var go = new GameObject(label);
        go.transform.position = pos;
 
        var sr    = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreatePixelSprite(color);
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
 
        return go;
    }
 
    static Sprite CreatePixelSprite(Color color)
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
 