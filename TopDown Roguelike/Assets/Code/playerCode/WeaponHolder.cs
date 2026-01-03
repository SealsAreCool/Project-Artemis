using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    public Transform weaponSlot;
    private Gun currentGun;
    private Gun nearbyGun;
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Weapon"))
        {
            Gun gun = col.GetComponent<Gun>();
            if (gun != null)
            {
                nearbyGun = gun;
            }
        }
    }
    void OnTriggerExit2D(Collider2D col){
        if(col.CompareTag("Weapon")){
            Gun gun = col.GetComponent<Gun>();
            if(nearbyGun == gun){
                nearbyGun = null;
            }
        }
    }
    void Update(){
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg-90;
        weaponSlot.rotation=Quaternion.Euler(0,0,angle);
        if(currentGun!=null){
            currentGun.transform.rotation = Quaternion.Euler(0,0,angle);
        }
        if(nearbyGun != null && Input.GetKeyDown(KeyCode.E)){
            PickUpGun(nearbyGun);
        }
    }
    void PickUpGun(Gun newGun)
    {
        if (currentGun != null)
        {
            currentGun.transform.parent = null;
            Rigidbody2D rb = currentGun.GetComponent<Rigidbody2D>();
            rb.simulated = true; 
            rb.velocity = Vector2.zero;
            currentGun.isHeld = false;
            currentGun.transform.localPosition = new Vector2(newGun.transform.localPosition.x, newGun.transform.localPosition.y);
            currentGun.sr.sortingOrder = 0;
        }
        
        currentGun = newGun;
        currentGun.sr.sortingOrder = 2;
        currentGun.isHeld = true;
        currentGun.transform.SetParent(weaponSlot);
        currentGun.transform.localPosition = Vector3.zero;
        currentGun.transform.localRotation = Quaternion.identity;

        Rigidbody2D newRb = currentGun.GetComponent<Rigidbody2D>();
        newRb.simulated = false; 
        newRb.velocity = Vector2.zero;
    }

}

