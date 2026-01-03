using UnityEngine;
using System.Collections;

public class OrbitingSwords : MonoBehaviour
{
    public GameObject swordPrefab;
    public int swordCount = 3;
    public float baseOrbitRadius = 1.5f;
    public float orbitSpeed = 90f;
    public float pixelsPerUnit = 16f;
    public int playerSortingOrder = 0;
    public float respawnDelay = 0f;

    [HideInInspector] public Transform[] swords;
    private SpriteRenderer[] swordRenderers;
    private bool[] swordActive;
    private Vector3[] orbitOffsets;
    private float currentAngle = 0f;

    private void Start()
    {
        swords = new Transform[swordCount];
        swordRenderers = new SpriteRenderer[swordCount];
        swordActive = new bool[swordCount];
        orbitOffsets = new Vector3[swordCount];

        for (int i = 0; i < swordCount; i++)
        {
            float angle = i * (360f / swordCount);
            Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0) * baseOrbitRadius;
            orbitOffsets[i] = offset;

            Vector3 spawnPos = transform.position + offset;
            GameObject sword = Instantiate(swordPrefab, spawnPos, Quaternion.identity, transform);
            SwordProjectile proj = sword.GetComponent<SwordProjectile>();
            if (proj != null)
                proj.orbitManager = this;

            swords[i] = sword.transform;
            swordRenderers[i] = sword.GetComponent<SpriteRenderer>();
            swordActive[i] = true;
            swordRenderers[i].sortingOrder = (offset.y < 0) ? playerSortingOrder + 1 : playerSortingOrder - 1;
        }
    }

    private void Update()
    {
        currentAngle += orbitSpeed * Time.deltaTime;

        for (int i = 0; i < swordCount; i++)
        {
            if (!swordActive[i]) continue;

            float angle = currentAngle + i * (360f / swordCount);
            Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0) * baseOrbitRadius;

            Vector3 snappedOffset = new Vector3(
                Mathf.Round(offset.x * pixelsPerUnit) / pixelsPerUnit,
                Mathf.Round(offset.y * pixelsPerUnit) / pixelsPerUnit,
                0
            );

            swords[i].localPosition = snappedOffset;
            swordRenderers[i].sortingOrder = (offset.y < 0) ? playerSortingOrder + 1 : playerSortingOrder - 1;
        }
    }

    public Transform LaunchSword(Vector3 targetPos)
    {
        for (int i = 0; i < swordCount; i++)
        {
            if (swordActive[i])
            {
                swordActive[i] = false;
                swords[i].SetParent(null);
                SwordProjectile proj = swords[i].GetComponent<SwordProjectile>();
                if (proj != null)
                    proj.Initialize(targetPos);
                return swords[i];
            }
        }
        return null;
    }

    public Vector3 GetNextSwordPosition(Transform sword)
    {
        for (int i = 0; i < swordCount; i++)
        {
            if (swords[i] == sword)
                return transform.position + orbitOffsets[i];
        }
        return transform.position;
    }

    public void OnSwordReturned(Transform sword)
    {
        StartCoroutine(RespawnSwordRoutine(sword));
    }

    private IEnumerator RespawnSwordRoutine(Transform sword)
    {
        if (respawnDelay > 0f)
            yield return new WaitForSeconds(respawnDelay);

        sword.SetParent(transform);

        for (int i = 0; i < swordCount; i++)
        {
            if (swords[i] == sword)
            {
                swordActive[i] = true;
                swords[i].rotation = Quaternion.Euler(0, 0, 0);
                SpriteRenderer sr = sword.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.sortingOrder = (orbitOffsets[i].y < 0) ? playerSortingOrder + 1 : playerSortingOrder - 1;
                break;
            }
        }
    }
}
