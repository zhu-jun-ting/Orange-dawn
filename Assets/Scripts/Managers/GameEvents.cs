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

    void OnEnable()
    {
        instance = this;
    }

    public event Action<float, EnemyMaster> onHitEnemy;
    public void HitEnemy(float damage_, EnemyMaster enemy_)
    {
        if (onHitEnemy != null)
        {
            onHitEnemy(damage_, enemy_);
        }
    }





    public event Action<float, PawnMaster, GameObject, DamageType, Transform, float, Gun> OnHitPawn;
    public void HitPawn(float damage_, PawnMaster reciever_, GameObject instigator_ = null, DamageType damage_type_ = DamageType.Normal, Transform location_ = null, float hit_back_factor_ = 0f, Gun source_ = null)
    {

        // calling the reciever's TakeDamage method
        if (reciever_ != null) reciever_.TakeDamage(damage_, reciever_, instigator_, damage_type_, location_, hit_back_factor_, source_);

        if (OnHitPawn != null)
        {
            OnHitPawn(damage_, reciever_, instigator_, damage_type_, location_,  hit_back_factor_, source_);
        }
        if (onShowNumberUI != null && location_ != null)
        {
            onShowNumberUI((int)damage_, reciever_, damage_type_, (Vector2)location_.position);
        }
    }




    public enum DamageType { Normal, Crit, Heal, DotDamage }

    public event Action<int, PawnMaster, DamageType, Vector2> onShowNumberUI;
    public void ShowNumberUI(int damage_, PawnMaster reciever_, DamageType damage_type_, Vector2 location_)
    {
        if (onHitEnemy != null)
        {
            onShowNumberUI(damage_, reciever_, damage_type_, location_);
        }
    }



    public event Action<int, int> OnUpdateMana;
    public void UpdateMana(int diffMana = 0, int maxMana = -1)
    {
        if (OnUpdateMana != null)
        {
            OnUpdateMana(diffMana, maxMana);
        }
    }

    public event Action OnLevelCleared;
    public void LevelCleared()
    {
        if (OnLevelCleared != null)
        {
            OnLevelCleared();
        }
    }

    public event Action<int> OnUpdateCoins;
    public void UpdateCoins(int diffCoin)
    {
        if (OnUpdateCoins != null)
        {
            OnUpdateCoins(diffCoin);
        }
    }
}