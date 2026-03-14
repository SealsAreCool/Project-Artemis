using UnityEngine;
using System.Collections;

public class BossHandAttack : MonoBehaviour
{
    public float spawnHeight = 6f;
    public float gravity = 40f;

    public float disappearDelay = 0.3f;
    public float preFallDelay = 0.15f;
    public float impactDelay = 0.2f;

    public Sprite fallingSprite;

    public GameObject miniHandPrefab;
    public Sprite tremorSprite;
    public Sprite queenCardSprite;

    public int queenBulletCount = 16;
    public float queenBulletSpeed = 6f;

    SpriteRenderer sr;
    Collider2D col;
    Animator animator;

    Vector3 originalPos;
    Sprite originalSprite;

    public float tremorSpeed = 4f;
public float tremorSpacing = 1.2f;
public int tremorSteps = 8;
public LayerMask wallMask;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();

        originalPos = transform.localPosition;
        originalSprite = sr.sprite;
    }

    public IEnumerator SmashRoutine(Vector3 landingSpot)
    {
        if(animator)
            animator.SetTrigger("MakeFist");

        yield return new WaitForSeconds(0.5f);

        if(animator) animator.enabled=false;

        if(col) col.enabled=false;

        Vector3 start = new Vector3(
            landingSpot.x,
            landingSpot.y+spawnHeight,
            transform.position.z);

        sr.enabled=false;
        yield return new WaitForSeconds(disappearDelay);

        transform.position=start;
        sr.enabled=true;

        if(fallingSprite)
            sr.sprite=fallingSprite;

        yield return new WaitForSeconds(preFallDelay);

        float vel=0;

        while(transform.position.y>landingSpot.y)
        {
            vel+=gravity*Time.deltaTime;

            float newY=Mathf.Max(
                transform.position.y-vel*Time.deltaTime,
                landingSpot.y);

            transform.position=new Vector3(
                landingSpot.x,newY,transform.position.z);

            yield return null;
        }

        if(col) col.enabled=true;

        yield return new WaitForSeconds(impactDelay);

        transform.localPosition=originalPos;

        sr.sprite=originalSprite;

        if(animator) animator.enabled=true;
    }

    public void SmallSmash(Vector3 pos)
    {
        StartCoroutine(SmashRoutine(pos));
    }

    public void SpawnMiniHands(Vector3 pos)
    {
        if(!miniHandPrefab) return;

        Instantiate(miniHandPrefab,pos+Vector3.left*2,Quaternion.identity);
        Instantiate(miniHandPrefab,pos+Vector3.right*2,Quaternion.identity);
    }

public void SpawnTremors(Vector3 center)
{
    Vector2[] dirs =
    {
        Vector2.up,Vector2.down,Vector2.left,Vector2.right,
        new Vector2(1,1).normalized,
        new Vector2(-1,1).normalized,
        new Vector2(1,-1).normalized,
        new Vector2(-1,-1).normalized
    };

    foreach(Vector2 dir in dirs)
    {
        StartCoroutine(TremorChain(center,dir));
    }
}

IEnumerator TremorChain(Vector3 start, Vector2 dir)
{
    Vector3 pos = start;

    for(int i=0;i<tremorSteps;i++)
    {
        RaycastHit2D hit = Physics2D.Raycast(pos,dir,tremorSpacing,wallMask);

        if(hit.collider != null)
            yield break;

        pos += (Vector3)dir * tremorSpacing;

        GameObject g = new GameObject("Tremor");

        SpriteRenderer s = g.AddComponent<SpriteRenderer>();
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
            float angle=i*Mathf.PI*2/queenBulletCount;

            Vector2 dir=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));

            GameObject bullet=new GameObject("CardBullet");

            SpriteRenderer s=bullet.AddComponent<SpriteRenderer>();
            s.sprite=queenCardSprite;

            s.sortingLayerID=sr.sortingLayerID;
            s.sortingOrder=sr.sortingOrder+1;

            bullet.transform.position=center;

            StartCoroutine(CardMove(bullet,dir));
        }
    }

    IEnumerator CardMove(GameObject obj,Vector2 dir)
    {
        float life=3;

        while(life>0)
        {
            obj.transform.position+=(Vector3)dir*queenBulletSpeed*Time.deltaTime;
            life-=Time.deltaTime;
            yield return null;
        }

        Destroy(obj);
    }
}
