using UnityEngine;

public class AutoBoxCollider : MonoBehaviour
{
    SpriteRenderer sr;
    BoxCollider2D bc;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        bc = GetComponent<BoxCollider2D>();
    }

    void LateUpdate()
    {
        bc.size = sr.sprite.bounds.size;
    }
}
