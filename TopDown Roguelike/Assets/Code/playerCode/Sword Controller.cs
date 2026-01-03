using UnityEngine;

public class SwordAttackController : MonoBehaviour
{
    public OrbitingSwords orbitManager;

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            LaunchSwordAtMouse();
        }
    }

    void LaunchSwordAtMouse()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        Transform sword = orbitManager.LaunchSword(mousePos);
        if (sword == null) return;

        SwordProjectile projectile = sword.GetComponent<SwordProjectile>();
        projectile.Initialize(mousePos);
        
    }
}
