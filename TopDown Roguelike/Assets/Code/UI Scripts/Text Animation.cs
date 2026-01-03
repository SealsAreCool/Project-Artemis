using UnityEngine;
using UnityEngine.UI;

public class UITitleAnimation : MonoBehaviour
{
    public float speed = 2f;    
    public float scaleAmount = 0.1f; 

    private Vector3 originalScale;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }

    void Update()
    {
        float scale = 1 + Mathf.Sin(Time.time * speed) * scaleAmount;
        rectTransform.localScale = originalScale * scale;
    }
}
