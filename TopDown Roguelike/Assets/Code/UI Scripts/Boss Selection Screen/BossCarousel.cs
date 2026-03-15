using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BossCarousel : MonoBehaviour
{
    [Header("Images")]
    public Image center;
    public Image left;
    public Image right;

    [Header("Boss Data")]
    public Sprite[] bosses;
    public string[] bossScenes;

    int index = 0;

    void Start()
    {
        UpdateDisplay();
    }

    public void Next()
    {
        index = (index + 1) % bosses.Length;
        UpdateDisplay();
    }

    public void Previous()
    {
        index = (index - 1 + bosses.Length) % bosses.Length;
        UpdateDisplay();
    }

    public void StartBoss()
    {
        SceneManager.LoadScene(bossScenes[index]);
    }

    void UpdateDisplay()
    {
        center.sprite = bosses[index];
        left.sprite = bosses[(index - 1 + bosses.Length) % bosses.Length];
        right.sprite = bosses[(index + 1) % bosses.Length];
    }
}