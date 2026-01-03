using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float recoil = 1f;
    public float recoilDuration = 0.5f;
    public float fireRate = 0.5f; 
    public int damage = 1;
    public float bulletSpeed = 10f;
    public Vector3 originalLocalPosition = Vector3.zero;
    [HideInInspector] public bool isHeld = false;
    private Coroutine recoilCoroutine;
    private float nextFireTime = 0f;
    public SpriteRenderer sr;
    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime&& isHeld)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Fire()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = firePoint.right * bulletSpeed;

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.damage = damage;
        originalLocalPosition=transform.localPosition;
         StartCoroutine(RecoilEffect());
    }
           

    IEnumerator RecoilEffect()
    {
        Vector3 recoilOffset =  transform.InverseTransformDirection(((firePoint.right).normalized)) * recoil * 0.1f;
        transform.localPosition = originalLocalPosition - recoilOffset;
        float elapsed = 0f;
        while (elapsed < recoilDuration)
        {
            transform.localPosition = Vector3.Lerp(
                originalLocalPosition - recoilOffset,
                originalLocalPosition,
                elapsed / recoilDuration
            );
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalLocalPosition;
    }
}
