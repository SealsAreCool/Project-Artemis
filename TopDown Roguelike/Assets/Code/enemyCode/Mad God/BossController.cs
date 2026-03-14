using UnityEngine;
using System.Collections;

public class BossAttackController : MonoBehaviour
{
    public BossHandAttack leftHand;
    public BossHandAttack rightHand;

    public Transform arenaCenter;
    public Vector2 arenaSize = new Vector2(8f,4f);

    public int poundsPerClap = 5;
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

    GameObject card1;
    GameObject card2;

    void Start()
    {
        StartCoroutine(BossLoop());
    }

    IEnumerator BossLoop()
    {
        while(true)
        {
            yield return StartCoroutine(Clap());

            for(int i=0;i<poundsPerClap;i++)
            {
                yield return StartCoroutine(DoGroundPound());
                yield return new WaitForSeconds(timeBetweenPounds);
            }
        }
    }

    IEnumerator Clap()
    {
        SpriteRenderer leftSR = leftHand.GetComponent<SpriteRenderer>();
        SpriteRenderer rightSR = rightHand.GetComponent<SpriteRenderer>();

        Animator leftAnim = leftHand.GetComponent<Animator>();
        Animator rightAnim = rightHand.GetComponent<Animator>();

        if(leftAnim) leftAnim.enabled = false;
        if(rightAnim) rightAnim.enabled = false;

        Vector3 leftStart = leftHand.transform.localPosition;
        Vector3 rightStart = rightHand.transform.localPosition;

        float centerX = (leftStart.x + rightStart.x)/2f;

        Vector3 leftTarget = new Vector3(centerX-0.25f,leftStart.y,leftStart.z);
        Vector3 rightTarget = new Vector3(centerX+0.25f,rightStart.y,rightStart.z);

        leftSR.sprite = leftClapSprite;
        rightSR.sprite = rightClapSprite;

        float duration = Vector3.Distance(leftStart,leftTarget)/clapSpeed;

        float t=0;

        while(t<1f)
        {
            t+=Time.deltaTime/duration;

            leftHand.transform.localPosition = Vector3.Lerp(leftStart,leftTarget,t);
            rightHand.transform.localPosition = Vector3.Lerp(rightStart,rightTarget,t);

            // rotation animation restored
            leftHand.transform.rotation = Quaternion.Euler(0,0,Mathf.Lerp(30f,0f,t));
            rightHand.transform.rotation = Quaternion.Euler(0,0,Mathf.Lerp(-30f,0f,t));

            yield return null;
        }

        Vector3 center = (leftHand.transform.position + rightHand.transform.position)/2;

        SpawnCards(center);

        yield return new WaitForSeconds(clapHoldTime);

        t=0;

        while(t<1f)
        {
            t+=Time.deltaTime/duration;

            leftHand.transform.localPosition = Vector3.Lerp(leftTarget,leftStart,t);
            rightHand.transform.localPosition = Vector3.Lerp(rightTarget,rightStart,t);

            leftHand.transform.rotation = Quaternion.Euler(0,0,Mathf.Lerp(0f,30f,t));
            rightHand.transform.rotation = Quaternion.Euler(0,0,Mathf.Lerp(0f,-30f,t));

            yield return null;
        }

        leftHand.transform.rotation = Quaternion.identity;
        rightHand.transform.rotation = Quaternion.identity;

        if(leftAnim) leftAnim.enabled = true;
        if(rightAnim) rightAnim.enabled = true;

        if(card1) Destroy(card1);
        if(card2) Destroy(card2);
    }

    void SpawnCards(Vector3 center)
    {
        int a = Random.Range(0,4);
        int b = Random.Range(0,4);

        cardA = (Card)a;
        cardB = (Card)b;

        card1 = CreateCard(cardSprites[a],center+Vector3.left*cardSpacing*0.5f);
        card2 = CreateCard(cardSprites[b],center+Vector3.right*cardSpacing*0.5f);
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
        BossHandAttack hand = Random.value<0.5f ? leftHand : rightHand;

        Vector3 target = RandomPoint(hand==leftHand);

        yield return StartCoroutine(hand.SmashRoutine(target));

        ApplyCard(cardA,hand,target);
        ApplyCard(cardB,hand,target);
    }

    void ApplyCard(Card card,BossHandAttack hand,Vector3 pos)
    {
        switch(card)
        {
            case Card.Queen: hand.SpawnQueenRing(pos); break;
            case Card.King: hand.SpawnMiniHands(pos); break;
            case Card.Ace: hand.SpawnTremors(pos); break;
            case Card.Jack:
                hand.SmallSmash(pos+Vector3.left*1.5f);
                hand.SmallSmash(pos+Vector3.right*1.5f);
                break;
        }
    }

    Vector3 RandomPoint(bool leftSide)
    {
        float minX = leftSide ?
            arenaCenter.position.x-arenaSize.x/2 :
            arenaCenter.position.x;

        float maxX = leftSide ?
            arenaCenter.position.x :
            arenaCenter.position.x+arenaSize.x/2;

        float x = Random.Range(minX,maxX);

        float y = Random.Range(
            arenaCenter.position.y-arenaSize.y/2,
            arenaCenter.position.y+arenaSize.y/2);

        return new Vector3(x,y,0);
    }
}