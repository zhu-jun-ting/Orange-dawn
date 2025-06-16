using System.Runtime.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents instance;

    void Awake()
    {
        instance = this;
    }

    public event Action<float, EnemyMaster> onHitEnemy;
    public void HitEnemy(float damage_, EnemyMaster enemy_) {
        if (onHitEnemy != null) {
            onHitEnemy(damage_, enemy_);
        }
    }

    public enum DamageType {Normal, Crit, Heal, DotDamage}

    public event Action<int, PawnMaster, DamageType, Vector2> onShowNumberUI;
    public void ShowNumberUI(int damage_, PawnMaster reciever_, DamageType damage_type_, Vector2 location_) {
        if (onHitEnemy != null) {
            onShowNumberUI(damage_, reciever_, damage_type_, location_);
        }
    }
}
