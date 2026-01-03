using UnityEngine;
using System.Collections;

public class BreakableObject : MonoBehaviour
{
    [Header("Break Settings")]
    public float fadeDuration = 2f;
    public float destroyDelay = 0.5f;

    private Animator animator;
    private Collider col;
    private Renderer rend;
    private bool broken = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        col = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
    }


    public void Break()
    {
        if (broken) return;
        broken = true;

        if (col != null)
            col.enabled = false;

        if (animator != null)
            animator.SetBool("isBroken", true);


        StartCoroutine(FadeAndDestroy());
    }

    IEnumerator FadeAndDestroy()
    {
        yield return new WaitForSeconds(destroyDelay);

        Material mat = rend.material;
        Color startColor = mat.color;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            mat.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }


    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 2f)
        {
            Break();
        }
    }
}
