using UnityEngine;
using UnityEngine.UI;

public class UIPlaceAnimation : MonoBehaviour
{
    public float speed = 2f; 
    public float moveAmount = 50f;  

    private RectTransform rectTransform;
    private Vector2 originalPos;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPos = rectTransform.anchoredPosition;
    }

    void Update()
    {
        float newY = originalPos.y + Mathf.Sin(Time.time * speed) * moveAmount;
        rectTransform.anchoredPosition = new Vector2(originalPos.x, newY);
    }
}
