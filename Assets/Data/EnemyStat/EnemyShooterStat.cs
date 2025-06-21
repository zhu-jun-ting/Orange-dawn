using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyShooterStat", menuName = "Stat/EnemyShooterStat", order = 3)]
public class EnemyShooterStat : EnemyStat {

    // [Header("Shooter Enemies")]
    public float shoot_damage; 
    public float shoot_range;
    public float shoot_interval;
    public GameObject bullet_prefab;
}