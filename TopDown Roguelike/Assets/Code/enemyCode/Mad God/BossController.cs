using UnityEngine;
using System.Collections;

public class BossAttackController : MonoBehaviour
{
    public BossHandAttack leftHand;
    public BossHandAttack rightHand;

    public Transform arenaCenter;
    public Vector2 arenaSize = new Vector2(8f, 4f);

    public int poundsPerClap = 5;
    public float poundDelay = 1.2f;

    public Sprite leftClapSprite;
    public Sprite rightClapSprite;

    public Sprite[] cardSprites;
    public float cardSpacing = 2f;
    public Sprite maskRectSprite;

    enum Card { Queen, King, Ace, Jack }
    Card cardA, cardB;

    GameObject card1, card2;
    GameObject mask;

    void Start()
    {
        leftHand.StartIdle();
        rightHand.StartIdle();
        StartCoroutine(BossLoop());
    }

    IEnumerator BossLoop()
    {
        while (true)
        {
            // Clap
            leftHand.StartClapOrSmash();
            rightHand.StartClapOrSmash();
            yield return Clap();
            leftHand.EndClapOrSmash();
            rightHand.EndClapOrSmash();

            // Ground pounds
            for (int i = 0; i < poundsPerClap; i++)
            {
                bool isLeft = Random.value < 0.5f;
                BossHandAttack hand = isLeft ? leftHand : rightHand;
                Vector3 target = RandomPoint(isLeft);

                yield return hand.Smash(target, isLeft);

                ApplyCard(cardA, hand, target);
                ApplyCard(cardB, hand, target);

                yield return new WaitForSeconds(poundDelay);
            }
        }
    }

   IEnumerator Clap()
    {
        Vector3 lStart = leftHand.transform.localPosition;
        Vector3 rStart = rightHand.transform.localPosition;

        float centerX = (lStart.x + rStart.x) / 2f;
        Vector3 lTarget = new Vector3(centerX - 0.25f, lStart.y, lStart.z);
        Vector3 rTarget = new Vector3(centerX + 0.25f, rStart.y, rStart.z);

        leftHand.DisableAnimator();
        rightHand.DisableAnimator();
        leftHand.SetSprite(leftClapSprite);
        rightHand.SetSprite(rightClapSprite);

        // Move hands to center slowly
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            leftHand.transform.localPosition = Vector3.Lerp(lStart, lTarget, t);
            rightHand.transform.localPosition = Vector3.Lerp(rStart, rTarget, t);
            yield return null;
        }

        // Pause in the middle
        yield return new WaitForSeconds(0.5f);

        // Spawn cards at center
        Vector3 center = (leftHand.transform.position + rightHand.transform.position) / 2f;
        SpawnCards(center);

        // Reveal cards while hands move back
        StartCoroutine(RevealMaskWithHands(leftHand.transform, rightHand.transform));

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            leftHand.transform.localPosition = Vector3.Lerp(lTarget, lStart, t);
            rightHand.transform.localPosition = Vector3.Lerp(rTarget, rStart, t);
            yield return null;
        }

        leftHand.ResetSprite();
        rightHand.ResetSprite();
        leftHand.EnableAnimator();
        rightHand.EnableAnimator();

        // Cleanup
        if (card1) Destroy(card1);
        if (card2) Destroy(card2);
        if (mask) Destroy(mask);
    }


    GameObject CreateCard(Sprite sprite, Vector3 pos)
    {
        GameObject g = new GameObject("Card");
        var sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Environment";
        sr.sortingOrder = 6;
        sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        g.transform.position = pos;
        g.transform.localScale = Vector3.one * 3f;
        return g;
    }

    void SpawnCards(Vector3 center)
    {
        cardA = (Card)Random.Range(0, 4);
        cardB = (Card)Random.Range(0, 4);

        card1 = CreateCard(cardSprites[(int)cardA], center + Vector3.left * cardSpacing * 0.5f);
        card2 = CreateCard(cardSprites[(int)cardB], center + Vector3.right * cardSpacing * 0.5f);

        mask = new GameObject("CardMask");
        var m = mask.AddComponent<SpriteMask>();
        m.sprite = maskRectSprite;
        mask.transform.position = center;
        mask.transform.localScale = new Vector3(0.1f, 12f, 1f);
    }

    IEnumerator RevealMaskWithHands(Transform left, Transform right)
    {
        while (mask != null && Vector3.Distance(left.localPosition, right.localPosition) > 0.01f)
        {
            Vector3 center = (left.position + right.position) / 2f;
            mask.transform.position = center;

            float width = Vector3.Distance(left.position, right.position) + 0.1f;
            mask.transform.localScale = new Vector3(width, mask.transform.localScale.y, 1f);
            yield return null;
        }
    }

    void ApplyCard(Card card, BossHandAttack hand, Vector3 pos)
    {
        switch (card)
        {
            case Card.Queen: hand.SpawnQueenRing(pos); break;
            case Card.King: hand.SpawnMiniHands(pos, hand == leftHand); break;
            case Card.Ace: hand.SpawnTremors(pos); break;
            case Card.Jack:
                hand.JackStrike(pos + Vector3.left * 1.5f);
                hand.JackStrike(pos + Vector3.right * 1.5f);
                break;
        }
    }

    Vector3 RandomPoint(bool left)
    {
        float minX = left ? -arenaSize.x / 2 : 0f;
        float maxX = left ? 0f : arenaSize.x / 2;
        float x = Random.Range(minX, maxX);
        float y = Random.Range(-arenaSize.y / 2, arenaSize.y / 2);
        return arenaCenter.localPosition + new Vector3(x, y, 0);
    }
}