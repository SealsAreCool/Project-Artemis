using UnityEngine;

public class StarTwinkle : MonoBehaviour
{
    public float speed = 2f;     
    public float intensity = 0.5f; 
    private SpriteRenderer sr;
    private Color originalColor;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
    }

    void Update()
    {
        float factor = 1 + Mathf.Sin(Time.time * speed) * intensity;
        sr.color = originalColor * factor; 
    }
}
