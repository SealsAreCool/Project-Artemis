using UnityEngine;
using System.Collections;

public class BossAttackController : MonoBehaviour
{
    public BossHandAttack leftHand;
    public BossHandAttack rightHand;

    public Transform arenaCenter;
    public Vector2 arenaSize = new Vector2(8f,4f);

    public float poundsPerClap = 5;
    public float timeBetweenPounds = 1.2f;

    [Header("Clap")]
    public Sprite leftClapSprite;
    public Sprite rightClapSprite;
    public float clapSpeed = 5f;
    public float clapHoldTime = 0.6f;

    [Header("Cards")]
    public Sprite[] cardSprites;
    public float cardSpacing = 2f;

    enum Card { Queen, King, Ace, Jack }

    Card cardA;
    Card cardB;

    GameObject cardObj1;
    GameObject cardObj2;

    void Start()
    {
        StartCoroutine(BossLoop());
    }

    IEnumerator BossLoop()
    {
        while(true)
        {
            yield return StartCoroutine(ClapSequence());

            for(int i=0;i<poundsPerClap;i++)
            {
                yield return StartCoroutine(DoGroundPound());
                yield return new WaitForSeconds(timeBetweenPounds);
            }
        }
    }

    IEnumerator ClapSequence()
    {
        leftHand.isAttacking = true;
        rightHand.isAttacking = true;

        SpriteRenderer leftSR = leftHand.GetComponent<SpriteRenderer>();
        SpriteRenderer rightSR = rightHand.GetComponent<SpriteRenderer>();

        Vector3 leftStart = leftHand.transform.localPosition;
        Vector3 rightStart = rightHand.transform.localPosition;

        float centerX = (leftStart.x + rightStart.x)/2f;

        Vector3 leftClap = new Vector3(centerX - 0.25f,leftStart.y,leftStart.z);
        Vector3 rightClap = new Vector3(centerX + 0.25f,rightStart.y,rightStart.z);

        if(leftClapSprite) leftSR.sprite = leftClapSprite;
        if(rightClapSprite) rightSR.sprite = rightClapSprite;

        float duration = Vector3.Distance(leftStart,leftClap)/clapSpeed;

        float t=0;
        while(t<1)
        {
            t+=Time.deltaTime/duration;

            leftHand.transform.localPosition = Vector3.Lerp(leftStart,leftClap,t);
            rightHand.transform.localPosition = Vector3.Lerp(rightStart,rightClap,t);

            yield return null;
        }

        Vector3 cardCenter = (leftHand.transform.position+rightHand.transform.position)/2;

        SpawnCards(cardCenter);

        yield return new WaitForSeconds(clapHoldTime);

        t=0;
        while(t<1)
        {
            t+=Time.deltaTime/duration;

            leftHand.transform.localPosition = Vector3.Lerp(leftClap,leftStart,t);
            rightHand.transform.localPosition = Vector3.Lerp(rightClap,rightStart,t);

            yield return null;
        }

        leftHand.ResetSprite();
        rightHand.ResetSprite();

        leftHand.isAttacking=false;
        rightHand.isAttacking=false;

        if(cardObj1) Destroy(cardObj1);
        if(cardObj2) Destroy(cardObj2);
    }

    void SpawnCards(Vector3 pos)
    {
        int a = Random.Range(0,4);
        int b = Random.Range(0,4);

        cardA = (Card)a;
        cardB = (Card)b;

        cardObj1 = CreateCard(cardSprites[a],pos + Vector3.left*cardSpacing*0.5f);
        cardObj2 = CreateCard(cardSprites[b],pos + Vector3.right*cardSpacing*0.5f);
    }

    GameObject CreateCard(Sprite sprite,Vector3 pos)
    {
        GameObject g = new GameObject("Card");

        SpriteRenderer sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        sr.sortingLayerName = "Environment";
        sr.sortingOrder = 6;

        g.transform.position = pos;
        g.transform.localScale = Vector3.one*5f;

        return g;
    }

    IEnumerator DoGroundPound()
    {
        bool useLeft = Random.value<0.5f;

        BossHandAttack hand = useLeft ? leftHand : rightHand;

        while(hand.isAttacking)
            yield return null;

        Vector3 target = RandomPoint(useLeft);

        yield return StartCoroutine(hand.SmashRoutine(target));

        ApplyCard(cardA,hand,target);
        ApplyCard(cardB,hand,target);
    }

    void ApplyCard(Card card,BossHandAttack hand,Vector3 pos)
    {
        switch(card)
        {
            case Card.Queen:
                hand.SpawnQueenRing(pos);
                break;

            case Card.King:
                hand.SpawnMiniHands(pos);
                break;

            case Card.Ace:
                hand.SpawnTremors(pos);
                break;

            case Card.Jack:
                hand.SmallSmash(pos + Vector3.left*1.5f);
                hand.SmallSmash(pos + Vector3.right*1.5f);
                break;
        }
    }

    Vector3 RandomPoint(bool leftSide)
    {
        float minX = leftSide ? arenaCenter.position.x-arenaSize.x/2 : arenaCenter.position.x;
        float maxX = leftSide ? arenaCenter.position.x : arenaCenter.position.x+arenaSize.x/2;

        float x = Random.Range(minX,maxX);
        float y = Random.Range(arenaCenter.position.y-arenaSize.y/2,arenaCenter.position.y+arenaSize.y/2);

        return new Vector3(x,y,0);
    }
}