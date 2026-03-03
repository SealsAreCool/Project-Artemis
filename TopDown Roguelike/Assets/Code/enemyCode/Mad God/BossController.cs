using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    public BossHandAttack leftHand;
    public BossHandAttack rightHand;

    public float attackInterval = 3f;

    void Start()
    {
        StartCoroutine(AttackLoop());
    }

    IEnumerator AttackLoop()
    {
        yield return new WaitForSeconds(2f);

        while (true)
        {
            yield return new WaitForSeconds(attackInterval);

            if (Random.value < 0.5f)
            {
                if (!leftHand.IsBusy())
                    leftHand.StartSmash();
            }
            else
            {
                if (!rightHand.IsBusy())
                    rightHand.StartSmash();
            }
        }
    }
}