using UnityEngine;

[CreateAssetMenu(fileName = "NPCLaserStat", menuName = "NPC/NPCLaserStat", order = 1)]
public class NPCLaserStat : ScriptableObject
{
    [Header("Basic Stats")]
    public float move_speed = 2f;
    public float max_health = 100f;
    public float melee_damage = 10f;
    public float hurtDuration = 0.2f;

    [Header("Laser Settings")]
    public float laser_range = 5f;
    public float laser_interval = 0.2f;
    public NPCLasergun npcLaserGunPrefab;
}
