using UnityEngine;

public class BossPillar : MonoBehaviour
{
    public float hp = 3f;
    public float damageToBoard = 1f;
    public BossAttackController boss;

public void TakeDamage(float amount)
{
    hp -= amount;
    Debug.Log($"Pillar hit! HP remaining: {hp}");
    if (hp <= 0)
    {
        Debug.Log("Pillar destroyed!");
        boss.TakeDamage(damageToBoard);
        Destroy(gameObject);
    }
}
    
}