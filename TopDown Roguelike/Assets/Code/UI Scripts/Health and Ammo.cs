using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MaskHealth : MonoBehaviour
{
    [Header("UI Elements")]
    public Transform maskPanel; 
    public Image maskPrefab;   
    public Sprite maskFull;
    public Sprite maskHalf;
    public Sprite maskEmpty;

    [Header("Player Health")]
    public int maxHealth = 10;
    public int currentHealth;

    private List<Image> masks = new List<Image>();

    void Start()
    {
        currentHealth = maxHealth;
        InitializeMasks();
        UpdateMasks();
    }

    void InitializeMasks()
    {

        foreach (Transform child in maskPanel)
            Destroy(child.gameObject);
        masks.Clear();
        int fullMasks = Mathf.CeilToInt(maxHealth / 2f);
        for (int i = 0; i < fullMasks; i++)
        {
            Image img = Instantiate(maskPrefab, maskPanel);
            masks.Add(img);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;
        UpdateMasks();
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateMasks();
    }

    void UpdateMasks()
    {
        for (int i = 0; i < masks.Count; i++)
        {
            int maskIndex = i * 2;

            if (currentHealth >= maskIndex + 2)
                masks[i].sprite = maskFull;
            else if (currentHealth == maskIndex + 1)
                masks[i].sprite = maskHalf;
            else
                masks[i].sprite = maskEmpty;
        }
    }
}
