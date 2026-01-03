using UnityEngine;
using System.Collections;

public class RoomController : MonoBehaviour
{
    public GameObject meleePrefab;
    public GameObject rangedPrefab;
    public Transform[] spawnPoints;
    public Collider2D[] exitColliders;
    public SpriteRenderer[] exitSprites;
    public float spawnIndicatorTime = 1f;
    public GameObject spawnIndicatorPrefab;

    private bool roomActive = false;
    private int enemiesAlive = 0;
    private BoxCollider2D box;

private void Awake(){
    box = GetComponent<BoxCollider2D>();
}

private void OnTriggerStay2D(Collider2D col)
{
    if (roomActive || !col.CompareTag("Player")) return;

    Bounds playerBounds = col.bounds;      
    Bounds roomBounds = box.bounds; 

    if (roomBounds.Contains(playerBounds.min) && roomBounds.Contains(playerBounds.max))
    {
        roomActive = true;
        LockExits();
        StartCoroutine(SpawnEnemies());
    }
}

    private IEnumerator SpawnEnemies()
    {
        GameObject[] indicators = new GameObject[spawnPoints.Length];
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Vector3 indicatorPos = spawnPoints[i].position;
            indicatorPos.z = 0f; 
            if (spawnIndicatorPrefab != null)
            {
                indicators[i] = Instantiate(spawnIndicatorPrefab, indicatorPos, Quaternion.identity);
            }
        }

        yield return new WaitForSeconds(spawnIndicatorTime);
        for (int i = 0; i < indicators.Length; i++)
        {
            if (indicators[i] != null)
                Destroy(indicators[i]);
        }

      
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Vector3 spawnPos = spawnPoints[i].position;
            spawnPos.z = 0f; 
            GameObject prefab = rangedPrefab;
            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
            if(enemy.GetComponent<Enemy>().isBoss){
                enemy.GetComponent<BossTitleCard>().Play();

            }
            enemy.SetActive(true);
            enemy.GetComponent<Enemy>()?.AddDeathCallback(OnEnemyDeath);
            enemiesAlive++;
        }
    }

    private void OnEnemyDeath()
    {
        enemiesAlive--;
        if (enemiesAlive <= 0)
        {
            UnlockExits();
        }
    }

    private void LockExits()
    {
        foreach (var exit in exitColliders)
            exit.enabled = true;
        foreach(var sr in exitSprites)
            sr.enabled = true;
    }

    private void UnlockExits()
    {
        foreach (var exit in exitColliders)
            exit.enabled = false;
        foreach(var sr in exitSprites)
            sr.enabled = false;
    }
}
