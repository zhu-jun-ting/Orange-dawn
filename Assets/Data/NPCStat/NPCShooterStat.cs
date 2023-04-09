using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCShooterStat", menuName = "Stat/NPCShooterStat", order = 4)]
public class NPCShooterStat : ScriptableObject {

    [Header("the initial setup of the Enemy stats, may be modified dynamically in the game.")]
    public float max_health;
    public float move_speed;
    public float melee_damage;
    public float hurtDuration;

    // [Header("Shooter Enemies")]
    public float shoot_range;
    public float shoot_interval;
    public GameObject bullet_prefab;
}