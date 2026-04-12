using UnityEngine;
using System.Collections;

public class EyeTwinkleSpawner : MonoBehaviour
{
    public GameObject eyePrefab;

    public float minDelay = 0.5f;
    public float maxDelay = 2f;

    public Vector2 minBounds;
    public Vector2 maxBounds;

    public int maxSimultaneous = 5;
    private int currentEyes = 0;

    // OnEnable instead of Start so the loop restarts every time the component
    // is re-enabled (e.g. after being disabled between phases).
    void OnEnable()
    {
        StartCoroutine(SpawnLoop());
    }

    void OnDisable()
    {
        StopAllCoroutines();
        // Reset count so eyes don't stay artificially capped if objects
        // were mid-flight when the spawner was disabled.
        currentEyes = 0;
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            if (currentEyes >= maxSimultaneous) continue;

            Vector2 pos = new Vector2(
                Random.Range(minBounds.x, maxBounds.x),
                Random.Range(minBounds.y, maxBounds.y)
            );

            GameObject eye = Instantiate(eyePrefab, pos, Quaternion.identity);
            currentEyes++;

            float scale = Random.Range(0.6f, 1.2f);
            eye.transform.localScale = Vector3.one * scale;

            StartCoroutine(DestroyAfterAnim(eye, 2f));
        }
    }

    IEnumerator DestroyAfterAnim(GameObject eye, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (eye != null)
            Destroy(eye);
        currentEyes--;
    }
}