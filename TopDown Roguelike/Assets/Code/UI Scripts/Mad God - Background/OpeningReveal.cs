using UnityEngine;
using System.Collections;

public class OpeningReveal : MonoBehaviour
{
    public Camera mainCam;
    public RectTransform border;
    public Animator curtainAnimator;

    public float startZoom = 8f;
    public float endZoom = 5f;

    public float revealDuration = 2f;
    public float borderScale = 3f;

    void Start()
    {
        // Freeze all gameplay
        Time.timeScale = 0;

        // Start camera zoomed out
        mainCam.orthographicSize = startZoom;

        // Play curtain animation
        curtainAnimator.Play("Curtains");
    }

    // Called by Animation Event at end of curtain animation
    public void StartReveal()
    {
        StartCoroutine(Reveal());
    }

    IEnumerator Reveal()
    {
        float t = 0;

        Vector3 startScale = border.localScale;
        Vector3 endScale = startScale * borderScale;

        while (t < 1f)
        {
            // Use unscaled time because the game is frozen
            t += Time.unscaledDeltaTime / revealDuration;

            float eased = Mathf.SmoothStep(0, 1, t);

            border.localScale = Vector3.Lerp(startScale, endScale, eased);
            mainCam.orthographicSize = Mathf.Lerp(startZoom, endZoom, eased);

            yield return null;
        }

        // Hide border
        border.gameObject.SetActive(false);

        // Start the game
        Time.timeScale = 1;
    }
}