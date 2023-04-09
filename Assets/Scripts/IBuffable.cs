using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBuffable
{
    public void ApplyBuff(Buff buff_);
    // void BuffDamage(float amount_);
    void Damage(float _amount, GameEvents.DamageType damage_type_, float _hit_back_factor, Transform instigator);
    void UpdatePlayerContinuousAOE(ContiniousAOEStat stat_);
    void AddLifeStealPercent(float percent_);
    
}