using UnityEngine;

public enum PhaseType { Normal, Alternating }

[CreateAssetMenu(menuName = "Boss/Phase")]
public class BossPhase : ScriptableObject
{
    [Header("Core")]
    public PhaseType type;
    public float phaseHp;

    [Header("Combat")]
    public bool useAllFourCards;
    public float poundDelay = 1f;
    public bool noClap;

    [Header("Top Hat Attack")]
    public bool topHatEnabled;

    [Header("Eye Background System")]
    public bool eyesEnabled = false;          // 👁️ NEW
    public float eyeSpawnMinDelay = 0.5f;     // 👁️ NEW
    public float eyeSpawnMaxDelay = 2f;       // 👁️ NEW
    public int maxEyesPerCycle = 3;           // 👁️ NEW

    [Header("Phase 3+ Attacks")]
    public bool laserEnabled;
    public bool sweepEnabled;
}