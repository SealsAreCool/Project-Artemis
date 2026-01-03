using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BossTitleCard : MonoBehaviour
{
    [Header("Boss Info")]
    public string bossName = "Ω";
    public string subtitle = "The Eternal Sentinel";
    private readonly string glitchChars = "!@#$%^&*<>?/\\|[]{}-=+";

    private Image fadeOverlay;
    private TMP_Text bootText;
    private TMP_Text nameText;
    private TMP_Text subtitleText;

    [Header("Settings")]
    public float fadeDuration = 0.7f;
    public float typeSpeed = 0.001f;

    void Awake()
    {

        GameObject canvas = GameObject.Find("Canvas");

        if (canvas == null)
        {
            Debug.LogError("ERROR: No 'TitleCardCanvas' found in scene!");
            return;
        }

        fadeOverlay = canvas.transform.Find("FadeOverlay").GetComponent<Image>();
        bootText = canvas.transform.Find("BootText").GetComponent<TMP_Text>();
        nameText = canvas.transform.Find("NameText").GetComponent<TMP_Text>();
        subtitleText = canvas.transform.Find("SubtitleText").GetComponent<TMP_Text>();
    }

    public void Play()
    {
        StartCoroutine(TitleRoutine());
    }

    IEnumerator TitleRoutine()
    {
        Time.timeScale = 0f;

        fadeOverlay.color = new Color(0, 0, 0, 0);
        bootText.text = "";
        nameText.text = "";
        subtitleText.text = "";

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeDuration;
            fadeOverlay.color = new Color(0, 0, 0, t);
            yield return null;
        }

        string[] bootLines =
        {
            ">> SYSTEM WAKE...",
            ">> CORE DRIVERS LOADED",
            ">> SECURITY SHELL ONLINE",
            ">> PROTOCOL Ω UNLOCKED",
            ">> UNIT STATUS: HOSTILE"
        };

        foreach (string line in bootLines)
        {
            yield return StartCoroutine(GlitchTypeText(bootText, line, typeSpeed));
            bootText.text +="\n";
        }

        yield return new WaitForSecondsRealtime(0.2f);
        bootText.text = "";
        yield return new WaitForSecondsRealtime(0.1f);

        foreach (char c in bossName)
        {
            nameText.text += c;
            yield return new WaitForSecondsRealtime(typeSpeed*10);
        }


        foreach (char c in subtitle)
        {
            subtitleText.text += c;
            yield return new WaitForSecondsRealtime(typeSpeed*5);
        }

        yield return new WaitForSecondsRealtime(2f);

        t = 1f;
        while (t > 0f)
        {
            t -= Time.unscaledDeltaTime / fadeDuration;
            fadeOverlay.color = new Color(0, 0, 0, t);
            subtitleText.color = new Color(255,255,255,t);
            bootText.color = new Color(255,255,255,t);
            nameText.color = new Color(255,255,255,t);
            yield return null;
        }

        Time.timeScale = 1f;
        fadeOverlay.color = new Color(0, 0, 0, 0);
        bootText.text = "";
        nameText.text = "";
        subtitleText.text = "";
    }

IEnumerator GlitchTypeText(TMP_Text text, string finalText, float charDelay)
{
    string typedText = "";

    foreach (char c in finalText)
    {
        int flickerCount = Random.Range(3, 6);
        for (int i = 0; i < flickerCount; i++)
        {
            char glitchChar = glitchChars[Random.Range(0, glitchChars.Length)];
            text.text = typedText + glitchChar; 
            yield return new WaitForSecondsRealtime(charDelay * 0.1f);
        }

        typedText += c;
        text.text = typedText;
        yield return new WaitForSecondsRealtime(charDelay);
    }
}



}
