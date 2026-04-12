using UnityEngine;
using System.Collections;

public class BossLaserAttack : MonoBehaviour
{
    [Header("References")]
    public BossHandAttack leftHand;
    public BossHandAttack rightHand;
    public Transform      arenaCenter;

    [Header("Arena")]
    public Vector2 arenaSize = new Vector2(8f, 4f);

    [Header("Laser Visuals")]
    public Sprite  laserHandSprite;
    public Color   laserColor    = new Color(1f, 0.1f, 0.1f, 1f);
    public float   laserWidth    = 0.25f;
    public float   laserLength   = 20f;

    [Header("Timing")]
    public float telegraphDuration = 0.8f;
    public float sweepDuration     = 1.4f;
    public float returnSpeed       = 6f;

    [Header("Damage")]
    public float    damageRadius = 0.4f;
    public LayerMask playerLayer;

    public bool IsAttacking { get; private set; }

    public IEnumerator LaunchLaser()
    {
        IsAttacking = true;

        // --- choose hand and edge ---
        bool useLeft        = Random.value < 0.5f;
        BossHandAttack hand = useLeft ? leftHand : rightHand;

        int edge = Random.Range(0, 4);
        Vector3 handStart, handEnd, laserDir;
        CalculateEdgePositions(edge, out handStart, out handEnd, out laserDir);

        // --- save original world positions before touching anything ---
        Vector3 leftOrigin  = leftHand.transform.position;
        Vector3 rightOrigin = rightHand.transform.position;

        // --- freeze both hands ---
        leftHand.StopIdle();
        rightHand.StopIdle();
        leftHand.DisableAnimator();
        rightHand.DisableAnimator();

        if (laserHandSprite != null)
            hand.SetSprite(laserHandSprite);

        // --- build laser using SpriteRenderer quad instead of LineRenderer ---
        // LineRenderer requires a special material setup; a scaled quad is simpler
        // and picks up your existing sprite pipeline automatically.
        GameObject laserGO = new GameObject("BossLaser");
        SpriteRenderer laserSR = laserGO.AddComponent<SpriteRenderer>();
        laserSR.sprite           = MakePixelSprite();   // 1×1 white pixel
        laserSR.color            = laserColor;
        laserSR.sortingLayerName = "Environment";       // match your boss sorting layer
        laserSR.sortingOrder     = 5;

        // --- TELEGRAPH: hand at start, thin flickering beam ---
        hand.transform.position = handStart;
        float elapsed = 0f;
        while (elapsed < telegraphDuration)
        {
            elapsed += Time.deltaTime;
            float flicker = 0.3f + Mathf.PingPong(elapsed * 8f, 1f) * 0.3f;
            laserSR.color = new Color(laserColor.r, laserColor.g, laserColor.b, flicker);
            PositionLaserQuad(laserGO, hand.transform.position, laserDir, laserLength, laserWidth * 0.3f);
            yield return null;
        }

        // --- SWEEP: full beam, hand slides across edge ---
        laserSR.color = laserColor;
        elapsed = 0f;
        while (elapsed < sweepDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / sweepDuration);

            hand.transform.position = Vector3.Lerp(handStart, handEnd, t);
            PositionLaserQuad(laserGO, hand.transform.position, laserDir, laserLength, laserWidth);

            Collider2D hit = Physics2D.OverlapCircle(hand.transform.position, damageRadius, playerLayer);
            if (hit != null)
                hit.SendMessage("TakeDamage", 1f, SendMessageOptions.DontRequireReceiver);

            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        Destroy(laserGO);

        hand.ResetSprite();

        // --- return each hand to where it actually was ---
        Vector3 handOrigin = useLeft ? leftOrigin : rightOrigin;
        Vector3 returnFrom = hand.transform.position;
        elapsed = 0f;
        float returnDur = Vector3.Distance(returnFrom, handOrigin) / returnSpeed;
        // avoid divide-by-zero if hand is already there
        if (returnDur > 0.01f)
        {
            while (elapsed < returnDur)
            {
                elapsed += Time.deltaTime;
                hand.transform.position = Vector3.Lerp(returnFrom, handOrigin, elapsed / returnDur);
                yield return null;
            }
        }
        hand.transform.position = handOrigin;

        // --- restore both hands ---
        leftHand.EnableAnimator();
        rightHand.EnableAnimator();
        leftHand.SyncIdleBaseline();
        rightHand.SyncIdleBaseline();
        leftHand.StartIdle();
        rightHand.StartIdle();

        IsAttacking = false;
    }

    // -------------------------------------------------------------------------
    // Position a quad to represent the laser beam.
    // Origin is the hand position; the quad extends in laserDir for length units.
    // -------------------------------------------------------------------------
    void PositionLaserQuad(GameObject go, Vector3 origin, Vector3 dir, float length, float width)
    {
        // Centre the quad halfway along the beam
        go.transform.position = origin + dir * (length * 0.5f);

        // Rotate so the quad's local Y axis points along laserDir
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        go.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Scale: X = width, Y = length (sprite is 1×1 pixel so scale == world size)
        go.transform.localScale = new Vector3(width, length, 1f);
    }

    // Creates a tiny 1×1 white pixel sprite at runtime so no asset is needed.
    Sprite MakePixelSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    void CalculateEdgePositions(int edge, out Vector3 handStart, out Vector3 handEnd, out Vector3 laserDir)
    {
        Vector3 c  = arenaCenter.position;
        float   hw = arenaSize.x / 2f;
        float   hh = arenaSize.y / 2f;
        float   pad = 1.2f;

        switch (edge)
        {
            case 0: // top — sweeps left→right, fires down
                laserDir  = Vector3.down;
                handStart = new Vector3(c.x - hw - pad, c.y + hh + pad, c.z);
                handEnd   = new Vector3(c.x + hw + pad, c.y + hh + pad, c.z);
                break;
            case 1: // bottom — sweeps left→right, fires up
                laserDir  = Vector3.up;
                handStart = new Vector3(c.x - hw - pad, c.y - hh - pad, c.z);
                handEnd   = new Vector3(c.x + hw + pad, c.y - hh - pad, c.z);
                break;
            case 2: // left — sweeps top→bottom, fires right
                laserDir  = Vector3.right;
                handStart = new Vector3(c.x - hw - pad, c.y + hh + pad, c.z);
                handEnd   = new Vector3(c.x - hw - pad, c.y - hh - pad, c.z);
                break;
            default: // right — sweeps top→bottom, fires left
                laserDir  = Vector3.left;
                handStart = new Vector3(c.x + hw + pad, c.y + hh + pad, c.z);
                handEnd   = new Vector3(c.x + hw + pad, c.y - hh - pad, c.z);
                break;
        }
    }
}