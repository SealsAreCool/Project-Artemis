using UnityEngine;
using System.Collections;

/// <summary>
/// Attach to the BossAttackController GameObject (or any manager).
/// Call LaunchSweep() from BossAttackController to trigger the attack.
///
/// Behaviour:
///   1. Picks a random hand (left or right).
///   2. Swaps to the claw sprite and rotates the hand to face the player.
///   3. Brief telegraph pause (hand trembles in place).
///   4. Charges in a straight line through the arena in the player's direction.
///   5. Holds briefly at the end position, then slides back and restores idle.
/// </summary>
public class BossSweepAttack : MonoBehaviour
{
    [Header("References")]
    public BossHandAttack leftHand;
    public BossHandAttack rightHand;
    public Transform      playerTransform;   // assign in inspector or find at runtime
    public Transform      arenaCenter;

    [Header("Arena")]
    public Vector2 arenaSize = new Vector2(8f, 4f);

    [Header("Visuals")]
    public Sprite clawSprite;              // the claw sprite to swap to

    [Header("Timing")]
    public float telegraphDuration = 0.6f; // pause before charging
    public float chargeDuration    = 0.25f; // how fast the charge crosses the arena
    public float holdDuration      = 0.3f;  // pause at the end of the charge
    public float returnSpeed       = 5f;    // how fast the hand retreats

    [Header("Telegraph Shake")]
    public float shakeAmount = 0.08f;
    public float shakeSpeed  = 20f;

    [Header("Damage")]
    public float damageRadius = 0.6f;
    public LayerMask playerLayer;

    bool isAttacking = false;
    public bool IsAttacking => isAttacking;

    // -------------------------------------------------------------------------

    public IEnumerator LaunchSweep()
    {
        isAttacking = true;

        // --- choose hand ---
        bool useLeft = Random.value < 0.5f;
        BossHandAttack hand  = useLeft ? leftHand  : rightHand;
        BossHandAttack other = useLeft ? rightHand : leftHand;

        // --- freeze both hands ---
        leftHand.StopIdle();
        rightHand.StopIdle();
        leftHand.DisableAnimator();
        rightHand.DisableAnimator();

        // --- store state ---
        Vector3    startPos = hand.transform.position;
        Quaternion startRot = hand.transform.rotation;

        // --- swap to claw sprite ---
        hand.SetSprite(clawSprite);

        // --- face the player ---
        Vector3 toPlayer  = Vector3.zero;
        if (playerTransform != null)
        {
            toPlayer = (playerTransform.position - hand.transform.position).normalized;
            float angle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
            // offset 90° so the claw faces "forward" — adjust to match your sprite's orientation
            hand.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }

        // --- TELEGRAPH: shake in place ---
        float elapsed = 0f;
        Vector3 telegraphOrigin = hand.transform.position;
        while (elapsed < telegraphDuration)
        {
            elapsed += Time.deltaTime;
            float shake = Mathf.Sin(elapsed * shakeSpeed) * shakeAmount;
            hand.transform.position = telegraphOrigin + new Vector3(shake, shake * 0.5f, 0f);
            yield return null;
        }
        hand.transform.position = telegraphOrigin;

        // --- calculate charge end point ---
        // If we have a player, aim at them; otherwise charge straight across the arena.
        Vector3 chargeDir = playerTransform != null
            ? toPlayer
            : (useLeft ? Vector3.right : Vector3.left);

        // Push the end point far enough to always exit the arena bounds
        float diagonal = Mathf.Sqrt(arenaSize.x * arenaSize.x + arenaSize.y * arenaSize.y);
        Vector3 chargeEnd = telegraphOrigin + chargeDir * (diagonal + 2f);

        // --- CHARGE ---
        elapsed = 0f;
        bool hitThisCharge = false;
        while (elapsed < chargeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / chargeDuration);
            hand.transform.position = Vector3.Lerp(telegraphOrigin, chargeEnd, t);

            // damage — only deal once per charge
            if (!hitThisCharge)
            {
                Collider2D hit = Physics2D.OverlapCircle(hand.transform.position, damageRadius, playerLayer);
                if (hit != null)
                {
                    hit.SendMessage("TakeDamage", 1f, SendMessageOptions.DontRequireReceiver);
                    hitThisCharge = true;
                }
            }

            yield return null;
        }

        // --- hold at end ---
        yield return new WaitForSeconds(holdDuration);

        // --- return to start ---
        Vector3 returnFrom = hand.transform.position;
        elapsed = 0f;
        float returnDur = Vector3.Distance(returnFrom, startPos) / returnSpeed;
        while (elapsed < returnDur)
        {
            elapsed += Time.deltaTime;
            hand.transform.position = Vector3.Lerp(returnFrom, startPos, elapsed / returnDur);
            yield return null;
        }
        hand.transform.position = startPos;
        hand.transform.rotation = startRot;

        // --- restore sprite and idle ---
        hand.ResetSprite();

        leftHand.EnableAnimator();
        rightHand.EnableAnimator();
        leftHand.SyncIdleBaseline();
        rightHand.SyncIdleBaseline();
        leftHand.StartIdle();
        rightHand.StartIdle();

        isAttacking = false;
    }
}
