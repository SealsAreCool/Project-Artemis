using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    public Transform target;     
    private Vector3 fixedPos;     
    private bool followMode;      

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            Vector3 newPos = target.position;
            newPos.z = -10f;
            transform.position = newPos;
        }
    }
}
