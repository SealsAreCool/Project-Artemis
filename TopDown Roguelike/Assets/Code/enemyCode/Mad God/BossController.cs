using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossAttackController : MonoBehaviour
{
    public BossHandAttack leftHand;
    public BossHandAttack rightHand;

[Header("Phase 3 Attacks")]
public BossLaserAttack laserAttack;
public BossSweepAttack sweepAttack;

    [Header("Phase 3 FX")]
    public EyeTwinkleSpawner eyeSpawner;

    [Header("Top Hat")]
    public TopHatMinigun topHatMinigun;

    [Header("Phases")]
    public BossPhase[] phases;

    public Transform arenaCenter;
    public Vector2 arenaSize = new Vector2(8f, 4f);

    public int poundsPerClap = 3;
    public float poundDelay = 1.2f;

    public Sprite leftClapSprite;
    public Sprite rightClapSprite;

    public float clapInSpeed = 6f;
    public float clapOutSpeed = 2f;

    public Sprite[] cardSprites;
    public float cardSpacing = 2f;
    public Sprite maskRectSprite;

    [Header("Pillars")]
    public GameObject pillarPrefab;
    public int pillarsPerWave = 3;

    enum Card { Jack, Queen, King, Ace }

    int currentPhaseIndex = 0;
    float currentPhaseHp;
    BossPhase currentPhase;
    Coroutine activeLoop;

    Card cardA, cardB;
    readonly List<GameObject> spawnedCards = new List<GameObject>();
    GameObject mask;

    bool pendingTransition = false;
    BossPhase pendingPhase = null;
    bool isInCinematic = false;

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    void Start()
    {
        currentPhaseIndex = 0;
        currentPhase = phases[0];
        currentPhaseHp = phases[0].phaseHp;
        leftHand.StartIdle();
        rightHand.StartIdle();
        activeLoop = StartCoroutine(BossLoop());
    }

    // -------------------------------------------------------------------------
    // Damage & Phase Transitions
    // -------------------------------------------------------------------------

    public void TakeDamage(float amount)
    {
        currentPhaseHp -= amount;
        Debug.Log($"Phase {currentPhaseIndex} HP: {currentPhaseHp}");

        if (currentPhaseHp <= 0 && !pendingTransition)
        {
            int nextIndex = currentPhaseIndex + 1;
            if (nextIndex < phases.Length)
            {
                pendingTransition = true;
                pendingPhase = phases[nextIndex];
                currentPhaseIndex = nextIndex;
            }
            else
            {
                Debug.Log("Boss defeated!");
            }
        }
    }

    IEnumerator TransitionToPhase(BossPhase next)
    {
        isInCinematic = true;

        if (activeLoop != null)
            StopCoroutine(activeLoop);

        currentPhaseHp = next.phaseHp;

        yield return StartCoroutine(PlayPhaseIntro(currentPhaseIndex));

        currentPhase = next;
        // Only apply eye settings if the intro didn't already set them (phase 3 intro sets them itself)
        if (currentPhaseIndex != 2)
            ApplyEyeSettings(currentPhase);

        isInCinematic = false;

        activeLoop = StartCoroutine(BossLoop());
    }

    void ApplyEyeSettings(BossPhase phase)
    {
        if (eyeSpawner == null) return;

        if (!phase.eyesEnabled)
        {
            eyeSpawner.enabled = false;
            return;
        }

        eyeSpawner.enabled = true;
        eyeSpawner.minDelay = phase.eyeSpawnMinDelay;
        eyeSpawner.maxDelay = phase.eyeSpawnMaxDelay;
        eyeSpawner.maxSimultaneous = phase.maxEyesPerCycle;
    }

    // -------------------------------------------------------------------------
    // Phase Intros
    // -------------------------------------------------------------------------

    IEnumerator PlayPhaseIntro(int phaseIndex)
    {
        leftHand.StopIdle();
        rightHand.StopIdle();
        leftHand.DisableAnimator();
        rightHand.DisableAnimator();

        if (phaseIndex == 1)
            yield return StartCoroutine(Phase2Intro());
        else if (phaseIndex == 2)
            yield return StartCoroutine(Phase3Intro());

        leftHand.EnableAnimator();
        rightHand.EnableAnimator();
        leftHand.SyncIdleBaseline();
        rightHand.SyncIdleBaseline();
        leftHand.StartIdle();
        rightHand.StartIdle();
    }

    IEnumerator Phase2Intro()
    {
        Vector3 lStart = leftHand.transform.localPosition;
        Vector3 rStart = rightHand.transform.localPosition;
        Quaternion lStartRot = leftHand.transform.localRotation;
        Quaternion rStartRot = rightHand.transform.localRotation;

        float centerX = (lStart.x + rStart.x) / 2f;
        Vector3 lTarget = new Vector3(centerX - 0.3f, lStart.y, lStart.z);
        Vector3 rTarget = new Vector3(centerX + 0.3f, rStart.y, rStart.z);

        leftHand.SetSprite(leftClapSprite);
        rightHand.SetSprite(rightClapSprite);
        leftHand.transform.localRotation  = Quaternion.Euler(0, 0, -45f);
        rightHand.transform.localRotation = Quaternion.Euler(0, 0,  45f);

        // Slam in
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * clapInSpeed * 1.5f;
            leftHand.transform.localPosition  = Vector3.Lerp(lStart, lTarget, t);
            rightHand.transform.localPosition = Vector3.Lerp(rStart, rTarget, t);
            yield return null;
        }

        // At impact: spawn all four cards with mask closed
        Vector3 center = (leftHand.transform.position + rightHand.transform.position) / 2f;
        SpawnIntroCards(center, new List<Card> { Card.Jack, Card.Queen, Card.King, Card.Ace });

        yield return new WaitForSeconds(0.15f); // brief pause at impact

        // Slide back out — mask reveals cards as hands pull apart
        StartCoroutine(RevealMaskWithHands(leftHand.transform, rightHand.transform));
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * clapOutSpeed;
            leftHand.transform.localPosition  = Vector3.Lerp(lTarget, lStart, t);
            rightHand.transform.localPosition = Vector3.Lerp(rTarget, rStart, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.4f); // let player read the cards

        CleanupCards();

        // Restore positions/rotations — animator/idle restored by PlayPhaseIntro
        leftHand.transform.localPosition  = lStart;
        rightHand.transform.localPosition = rStart;
        leftHand.transform.localRotation  = lStartRot;
        rightHand.transform.localRotation = rStartRot;
        leftHand.ResetSprite();
        rightHand.ResetSprite();
    }

    IEnumerator Phase3Intro()
    {
        Vector3 lStart = leftHand.transform.localPosition;
        Vector3 rStart = rightHand.transform.localPosition;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 1.5f;
            leftHand.transform.localPosition  = Vector3.Lerp(lStart, lStart + Vector3.right * 0.5f, t);
            rightHand.transform.localPosition = Vector3.Lerp(rStart, rStart + Vector3.left  * 0.5f, t);
            yield return null;
        }

        // Burst: flood the screen with eyes immediately for dramatic effect.
        // We set these directly rather than via ApplyEyeSettings so TransitionToPhase
        // won't overwrite them after the intro finishes.
        if (eyeSpawner != null)
        {
            eyeSpawner.minDelay       = 0.05f;
            eyeSpawner.maxDelay       = 0.15f;
            eyeSpawner.maxSimultaneous = 20;
            eyeSpawner.enabled        = true;
        }

        yield return new WaitForSeconds(3f);

        // Restore positions — animator/idle restored by PlayPhaseIntro
        leftHand.transform.localPosition  = lStart;
        rightHand.transform.localPosition = rStart;
    }

    // -------------------------------------------------------------------------
    // Cards — shared helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Spawns cards laid out around <paramref name="center"/> with a closed mask.
    /// Caller must start RevealMaskWithHands and call CleanupCards when done.
    /// </summary>
    void SpawnIntroCards(Vector3 center, List<Card> forcedCards)
    {
        int count = forcedCards.Count;
        float totalWidth = cardSpacing * (count - 1);
        for (int i = 0; i < count; i++)
        {
            int spriteIndex = (int)forcedCards[i];
            if (spriteIndex >= cardSprites.Length || cardSprites[spriteIndex] == null)
            {
                Debug.LogWarning($"SpawnIntroCards: no sprite for card index {spriteIndex}");
                continue;
            }
            float xOffset = -totalWidth * 0.5f + i * cardSpacing;
            spawnedCards.Add(CreateCard(cardSprites[spriteIndex], center + new Vector3(xOffset, 0f, 0f)));
        }
        SpawnMask(center); // starts at scale 0.1 — RevealMaskWithHands opens it as hands pull apart
    }

    void SpawnMask(Vector3 center)
    {
        mask = new GameObject("CardMask");
        var m = mask.AddComponent<SpriteMask>();
        m.sprite = maskRectSprite;
        mask.transform.position   = center;
        mask.transform.localScale = new Vector3(0.1f, 12f, 1f);
    }

    void CleanupCards()
    {
        foreach (var c in spawnedCards)
            if (c) Destroy(c);
        spawnedCards.Clear();
        if (mask) Destroy(mask);
        mask = null;
    }

    GameObject CreateCard(Sprite sprite, Vector3 pos)
    {
        GameObject g = new GameObject("Card");
        var sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = "Environment";
        sr.sortingOrder = 6;
        sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        g.transform.position   = pos;
        g.transform.localScale = Vector3.one * 3f;
        return g;
    }

    /// <summary>
    /// Runs every frame updating the mask width to match the gap between the hands.
    /// Stops automatically when the mask is destroyed (CleanupCards).
    /// </summary>
    IEnumerator RevealMaskWithHands(Transform left, Transform right)
    {
        SpriteRenderer leftSR  = left.GetComponent<SpriteRenderer>();
        SpriteRenderer rightSR = right.GetComponent<SpriteRenderer>();

        while (mask != null)
        {
            float leftEdge  = leftSR.bounds.max.x;
            float rightEdge = rightSR.bounds.min.x;
            float width = Mathf.Max(0f, rightEdge - leftEdge);

            mask.transform.position = new Vector3(
                (leftEdge + rightEdge) * 0.5f,
                mask.transform.position.y,
                mask.transform.position.z
            );
            mask.transform.localScale = new Vector3(width, mask.transform.localScale.y, 1f);

            yield return null;
        }
    }

    // -------------------------------------------------------------------------
    // Boss Loop
    // -------------------------------------------------------------------------

    IEnumerator BossLoop()
    {
        while (true)
        {
            if (isInCinematic)
            {
                yield return null;
                continue;
            }

            if (!currentPhase.noClap)
                yield return Clap();

            SpawnPillars();

// roll a special attack if any are enabled this phase
bool didSpecial = false;
yield return StartCoroutine(laserAttack.LaunchLaser());
if (currentPhase.topHatEnabled && Random.value < 0.5f)
{
    topHatMinigun.LaunchAttack();
    yield return new WaitUntil(() => !topHatMinigun.IsAttacking);
    didSpecial = true;
}
else if (currentPhase.laserEnabled && Random.value < 0.5f)
{
    yield return StartCoroutine(laserAttack.LaunchLaser());
    didSpecial = true;
}
else if (currentPhase.sweepEnabled && Random.value < 0.5f)
{
    yield return StartCoroutine(sweepAttack.LaunchSweep());
    didSpecial = true;
}

// always follow up with a normal pattern
if (currentPhase.type == PhaseType.Alternating)
    yield return AlternatingPattern();
else
    yield return NormalPattern();

            if (pendingTransition && pendingPhase != null)
            {
                pendingTransition = false;
                BossPhase next = pendingPhase;
                pendingPhase = null;
                yield return StartCoroutine(TransitionToPhase(next));
            }
        }
    }

    // -------------------------------------------------------------------------
    // Clap (gameplay — picks two random cards and stores them for the round)
    // -------------------------------------------------------------------------

    IEnumerator Clap()
    {
        isInCinematic = true;

        leftHand.StopIdle();
        rightHand.StopIdle();

        Vector3    lStart    = leftHand.transform.localPosition;
        Vector3    rStart    = rightHand.transform.localPosition;
        Quaternion lStartRot = leftHand.transform.localRotation;
        Quaternion rStartRot = rightHand.transform.localRotation;

        float centerX = (lStart.x + rStart.x) / 2f;
        Vector3 lTarget = new Vector3(centerX - 0.4f, lStart.y, lStart.z);
        Vector3 rTarget = new Vector3(centerX + 0.4f, rStart.y, rStart.z);

        leftHand.DisableAnimator();
        rightHand.DisableAnimator();

        leftHand.SetSprite(leftClapSprite);
        rightHand.SetSprite(rightClapSprite);
        leftHand.transform.localRotation  = Quaternion.Euler(0, 0, -45f);
        rightHand.transform.localRotation = Quaternion.Euler(0, 0,  45f);

        // Slam in
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * clapInSpeed;
            leftHand.transform.localPosition  = Vector3.Lerp(lStart, lTarget, t);
            rightHand.transform.localPosition = Vector3.Lerp(rStart, rTarget, t);
            yield return null;
        }

        // Pick two random cards and remember them for the upcoming attack round
        cardA = (Card)Random.Range(0, System.Enum.GetValues(typeof(Card)).Length);
        cardB = (Card)Random.Range(0, System.Enum.GetValues(typeof(Card)).Length);

        Vector3 center = (leftHand.transform.position + rightHand.transform.position) / 2f;
        spawnedCards.Add(CreateCard(cardSprites[(int)cardA], center + Vector3.left  * cardSpacing * 0.5f));
        spawnedCards.Add(CreateCard(cardSprites[(int)cardB], center + Vector3.right * cardSpacing * 0.5f));
        SpawnMask(center);
        StartCoroutine(RevealMaskWithHands(leftHand.transform, rightHand.transform));

        yield return new WaitForSeconds(0.15f);

        // Slide out
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * clapOutSpeed;
            leftHand.transform.localPosition  = Vector3.Lerp(lTarget, lStart, t);
            rightHand.transform.localPosition = Vector3.Lerp(rTarget, rStart, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.4f); // let player read the cards

        CleanupCards();

        // Snap to exact start before restoring animator
        leftHand.transform.localPosition  = lStart;
        rightHand.transform.localPosition = rStart;
        leftHand.transform.localRotation  = lStartRot;
        rightHand.transform.localRotation = rStartRot;

        leftHand.ResetSprite();
        rightHand.ResetSprite();

        leftHand.EnableAnimator();
        rightHand.EnableAnimator();

        leftHand.SyncIdleBaseline();
        rightHand.SyncIdleBaseline();

        leftHand.StartIdle();
        rightHand.StartIdle();

        isInCinematic = false;
    }

    // -------------------------------------------------------------------------
    // Attack Patterns
    // -------------------------------------------------------------------------

    IEnumerator AlternatingPattern()
    {
        Vector3 leftTarget = RandomPoint(true);
        yield return leftHand.Smash(leftTarget, true);
        ApplyCardsForCurrentPhase(leftHand, leftTarget);
        yield return new WaitForSeconds(currentPhase.poundDelay);

        Vector3 rightTarget = RandomPoint(false);
        yield return rightHand.Smash(rightTarget, false);
        ApplyCardsForCurrentPhase(rightHand, rightTarget);
        yield return new WaitForSeconds(currentPhase.poundDelay);

        Vector3 bothLeft  = RandomPoint(true);
        Vector3 bothRight = RandomPoint(false);
        Coroutine l = StartCoroutine(leftHand.Smash(bothLeft,  true));
        Coroutine r = StartCoroutine(rightHand.Smash(bothRight, false));
        yield return l;
        yield return r;
        ApplyCardsForCurrentPhase(leftHand,  bothLeft);
        ApplyCardsForCurrentPhase(rightHand, bothRight);
        yield return new WaitForSeconds(currentPhase.poundDelay);
    }

    IEnumerator NormalPattern()
    {
        for (int i = 0; i < 3; i++)
        {
            bool isLeft = Random.value < 0.5f;
            BossHandAttack hand   = isLeft ? leftHand : rightHand;
            Vector3        target = RandomPoint(isLeft);

            yield return hand.Smash(target, isLeft);
            ApplyCardsForCurrentPhase(hand, target);

            yield return new WaitForSeconds(currentPhase.poundDelay);
        }
    }

    void ApplyCardsForCurrentPhase(BossHandAttack hand, Vector3 pos)
    {
        if (currentPhase.useAllFourCards)
        {
            hand.SpawnQueenRing(pos);
            hand.SpawnMiniHands(pos, hand == leftHand);
            hand.SpawnTremors(pos);
            hand.JackStrike(pos + Vector3.left  * 1.5f);
            hand.JackStrike(pos + Vector3.right * 1.5f);
        }
        else
        {
            ApplyCard(cardA, hand, pos);
            ApplyCard(cardB, hand, pos);
        }
    }

    void ApplyCard(Card card, BossHandAttack hand, Vector3 pos)
    {
        switch (card)
        {
            case Card.Queen: hand.SpawnQueenRing(pos);                    break;
            case Card.King:  hand.SpawnMiniHands(pos, hand == leftHand);  break;
            case Card.Ace:   hand.SpawnTremors(pos);                      break;
            case Card.Jack:
                hand.JackStrike(RandomPoint(true));
                hand.JackStrike(RandomPoint(false));
                break;
        }
    }

    // -------------------------------------------------------------------------
    // Pillars
    // -------------------------------------------------------------------------

    public void SpawnPillars()
    {
        for (int i = 0; i < pillarsPerWave; i++)
        {
            Vector3 pos = arenaCenter.position + new Vector3(
                Random.Range(-arenaSize.x / 2f, arenaSize.x / 2f),
                Random.Range(-arenaSize.y / 2f, arenaSize.y / 2f),
                0f
            );
            GameObject p = Instantiate(pillarPrefab, pos, Quaternion.identity);
            p.GetComponent<BossPillar>().boss = this;
        }
    }

    // -------------------------------------------------------------------------
    // Utilities
    // -------------------------------------------------------------------------

    Vector3 RandomPoint(bool left)
    {
        float minX = left ? -arenaSize.x / 2f : 0f;
        float maxX = left ?  0f : arenaSize.x / 2f;
        float x = Random.Range(minX, maxX);
        float y = Random.Range(-arenaSize.y / 2f, arenaSize.y / 2f);
        return arenaCenter.localPosition + new Vector3(x, y, 0f);
    }
}