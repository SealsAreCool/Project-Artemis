using UnityEngine;
using System.Collections;

public class Spawn : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public float respawnCooldown = 5f;
    public int maxAlive = 1;

    [Header("Spawn Behavior")]
    public bool spawnImmediatelyOnEnter = true;
    public bool requirePlayerExitBeforeRespawn = false;

    private bool playerInside;
    private bool onCooldown;
    private int aliveCount;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;

        if (spawnImmediatelyOnEnter)
            TrySpawn();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
    }

    void TrySpawn()
    {
        if (onCooldown) return;
        if (!playerInside) return;
        if (aliveCount >= maxAlive) return;

        GameObject enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        aliveCount++;

        enemy.GetComponent<Enemy>().OnDeathCallback += () => OnEnemyDeath();
    }

    void OnEnemyDeath()
    {
        aliveCount--;
        StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown()
    {
        onCooldown = true;
        yield return new WaitForSeconds(respawnCooldown);
        onCooldown = false;

        if (playerInside)
            TrySpawn();
    }
}
