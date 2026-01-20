using UnityEngine;

[ExecuteAlways]
public class Shadow : MonoBehaviour
{
    public Vector2 offset = new Vector2(0, -0.2f);
    public Vector2 size = new Vector2(1, 0.5f);
    public Color color = new Color(0, 0, 0, 0.6f);
    private GameObject shadowObj;
    private SpriteRenderer shadowSR;

    void Awake()
    {
        if (shadowObj == null)
        {
            shadowObj = new GameObject("Shadow");
            shadowObj.transform.parent = transform;
            shadowObj.transform.localPosition = new Vector3(offset.x, offset.y, 0);
            shadowSR = shadowObj.AddComponent<SpriteRenderer>();
            SpriteRenderer parentSR = GetComponent<SpriteRenderer>();
            if (parentSR != null)
            {
                shadowSR.sprite = parentSR.sprite;
                shadowSR.sortingLayerID = parentSR.sortingLayerID;
                shadowSR.sortingOrder = parentSR.sortingOrder - 1;
            }
            shadowSR.color = color;
        }
    }

    void Update()
    {
        if (shadowObj != null)
        {
            shadowObj.transform.rotation = Quaternion.identity;
            shadowObj.transform.position = new Vector3(
                transform.position.x + offset.x,
                transform.position.y + offset.y,
                transform.position.z
            );
            shadowObj.transform.localScale = new Vector3(size.x, size.y, 1);
        }
    }
}
