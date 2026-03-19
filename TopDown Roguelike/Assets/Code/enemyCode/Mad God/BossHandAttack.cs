using UnityEngine;
using System.Collections;

public class BossHandAttack : MonoBehaviour
{
    [Header("Ground Pound Settings")]
    public float spawnHeight = 6f;
    public float gravity = 40f;

    public Sprite fallingSprite;
    public Sprite jackSprite;
    public GameObject miniHandPrefab;
    public Sprite tremorSprite;
    public Sprite queenCardSprite;

    public int queenBulletCount = 16;
    public float queenBulletSpeed = 6f;

    public float tremorSpacing = 1.2f;
    public int tremorSteps = 8;
    public LayerMask wallMask;

    [Header("Idle Motion")]
    public float idleAmplitude = 0.3f;
    public float idleSpeed = 1.5f;

    SpriteRenderer sr;
    Animator anim;
    Sprite originalSprite;

    bool smashing = false;
    bool idleActive = false;
    bool isClapping = false;
    Vector3 idleStartPos;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        originalSprite = sr.sprite;
        idleStartPos = transform.localPosition;
    }

    void Update()
    {
        if(idleActive && !smashing && !isClapping)
        {
            transform.localPosition = idleStartPos + Vector3.up * Mathf.Sin(Time.time * idleSpeed) * idleAmplitude;
        }
    }

    public void StartIdle() => idleActive = true;
    public void StopIdle() => idleActive = false;
    public void StartClapOrSmash() => isClapping = true;
    public void EndClapOrSmash() => isClapping = false;

    public void DisableAnimator() { if(anim) anim.enabled = false; }
    public void EnableAnimator() { if(anim) anim.enabled = true; }
    public void SetSprite(Sprite s) => sr.sprite = s;
    public void ResetSprite() => sr.sprite = originalSprite;

    // Smash (ground pound) is fully local, independent
    public IEnumerator Smash(Vector3 targetWorld, bool isLeftHand)
    {
        if(smashing) yield break;
        smashing = true;
        StopIdle();
        StartClapOrSmash();

        if(anim) anim.SetTrigger("MakeFist");
        yield return new WaitForSeconds(0.5f);

        if(anim) anim.enabled = false;

        Vector3 targetLocal = transform.parent.InverseTransformPoint(targetWorld);
        Vector3 startPos = transform.localPosition;
        Vector3 aboveTarget = new Vector3(targetLocal.x, targetLocal.y + spawnHeight, 0f);
        transform.localPosition = aboveTarget;

        sr.sprite = fallingSprite;
        transform.localEulerAngles = new Vector3(0,0,isLeftHand?30f:-30f);

        float vel = 0f;
        while(transform.localPosition.y > targetLocal.y)
        {
            vel += gravity * Time.deltaTime;
            transform.localPosition += Vector3.down * vel * Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetLocal;
        yield return new WaitForSeconds(0.3f);

        // Reset rotation and position
        transform.localEulerAngles = Vector3.zero;
        transform.localPosition = startPos;
        sr.sprite = originalSprite;
        if(anim) anim.enabled = true;

        // Sync idle to this baseline to prevent drift
        idleStartPos = startPos;

        smashing = false;
        EndClapOrSmash();
        StartIdle();
    }

    // Mini hands spawn independent of other hand
    public void SpawnMiniHands(Vector3 pos, bool isLeftHand)
    {
        if(!miniHandPrefab) return;
        float rotation = isLeftHand ? 30f : -30f;
        for(int i=0;i<2;i++)
        {
            Vector3 offset = new Vector3(Random.Range(-2f,2f), Random.Range(-1f,1f),0);
            Instantiate(miniHandPrefab, pos + offset, Quaternion.Euler(0,0,rotation));
        }
    }

    public void SpawnTremors(Vector3 center)
    {
        Vector2[] dirs = {
            Vector2.up, Vector2.down, Vector2.left, Vector2.right,
            new Vector2(1,1).normalized, new Vector2(-1,1).normalized,
            new Vector2(1,-1).normalized, new Vector2(-1,-1).normalized
        };
        foreach(var d in dirs)
            StartCoroutine(TremorChain(center,d));
    }

    IEnumerator TremorChain(Vector3 start, Vector2 dir)
    {
        Vector3 pos = start;
        for(int i=0;i<tremorSteps;i++)
        {
            if(Physics2D.Raycast(pos, dir, tremorSpacing, wallMask)) yield break;
            pos += (Vector3)dir * tremorSpacing;

            GameObject g = new GameObject("Tremor");
            var s = g.AddComponent<SpriteRenderer>();
            s.sprite = tremorSprite;
            s.sortingLayerID = sr.sortingLayerID;
            s.sortingOrder = sr.sortingOrder + 1;

            g.transform.position = pos;
            Destroy(g,1.2f);
            yield return new WaitForSeconds(0.07f);
        }
    }

    public void SpawnQueenRing(Vector3 center)
    {
        if(!queenCardSprite) return;
        for(int i=0;i<queenBulletCount;i++)
        {
            float a = i * Mathf.PI*2 / queenBulletCount;
            Vector2 dir = new(Mathf.Cos(a), Mathf.Sin(a));

            GameObject b = new GameObject("CardBullet");
            var s = b.AddComponent<SpriteRenderer>();
            s.sprite = queenCardSprite;
            s.sortingLayerID = sr.sortingLayerID;
            s.sortingOrder = sr.sortingOrder + 1;

            b.transform.position = center;
            StartCoroutine(CardMove(b, dir));
        }
    }

    IEnumerator CardMove(GameObject obj, Vector2 dir)
    {
        float life = 3f;
        while(life>0)
        {
            obj.transform.position += (Vector3)dir * queenBulletSpeed * Time.deltaTime;
            life -= Time.deltaTime;
            yield return null;
        }
        Destroy(obj);
    }

    public void JackStrike(Vector3 pos)
    {
        StartCoroutine(JackFall(pos));
    }

    IEnumerator JackFall(Vector3 pos)
    {
        GameObject g = new GameObject("JackStrike");
        SpriteRenderer s = g.AddComponent<SpriteRenderer>();
        s.sprite = jackSprite;
        s.sortingLayerID = sr.sortingLayerID;
        s.sortingOrder = sr.sortingOrder + 1;

        Vector3 start = pos + Vector3.up * spawnHeight;
        g.transform.position = start;

        float vel = 0f;
        while(g.transform.position.y > pos.y)
        {
            vel += gravity * Time.deltaTime;
            g.transform.position += Vector3.down * vel * Time.deltaTime;
            yield return null;
        }

        Destroy(g,0.5f);
    }
}